#define ENABLE_PROFILER

using UnityEngine;
using UnityEngine.Profiling;

using System;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace SceneProfiler {

/// <summary>
/// To use DataCollector, attach it to a gameobject.
/// </summary>
/// <example>
/// <code>
/// void ButtonStart_OnClick() {
///     DataCollector.autoUpload = true;
///     DataCollector.defaultLocalPath = ""; // PWD
///     DataCollector.screenshotFrameInterval = 30;
///     DataCollector.defaultUploadPath = "ftp://127.0.0.1";
///     DataCollector.startProfiling();
/// }
///
/// void ButtonStop_OnClick() {
///     DataCollector.stopProfiling();
/// }
/// </code>
/// </example>
public class DataCollector : MonoBehaviour {
    string dirname_ = "";
    static public string directoryName {
        get { return Path.Combine(defaultLocalPath, inst().dirname_); }
    }

    string metainfo_ = "";
    static public string metaInfo {
        get { return inst().metainfo_; }
        set { inst().metainfo_ = value ?? ""; }
    }

    int start_frame_ = -1;
    static public bool started {
        get { return inst().start_frame_ > 0; }
    }

    bool auto_upload_ = true;
    static public bool autoUpload {
        get { return inst().auto_upload_; }
        set { inst().auto_upload_ = value; }
    }

    bool memory_snapshot_ = true;
    static public bool memorySnapshot {
        get { return inst().memory_snapshot_; }
        set { inst().memory_snapshot_ = value; }
    }

    bool overdraw_screenshot_ = true;
    static public bool overdrawScreenshot {
        get { return inst().overdraw_screenshot_; }
        set { inst().overdraw_screenshot_ = value; }
    }
    
    public void invOverdrawScreenshot(bool value) {
        overdrawScreenshot = !overdrawScreenshot;
    }

    static public float uploadProgress {
        get { return inst().upload_manager_.uploadProgress; }
    }

    int collect_interval_ = 150;
    static public int collectInterval {
        get { return inst().collect_interval_; }
        set { inst().collect_interval_ = value; }
    }

    static public string defaultUploadPath {
        get { return inst().upload_manager_.remoteBaseDirectory; }
        set { inst().upload_manager_.remoteBaseDirectory = value ?? ""; }
    }

    private string default_local_path_ = "$PDP";
    /// <summary>
    /// Set "$PDP" to retrieve the default log path, i.e. UnityEngine.Application.persistentDataPath
    /// </summary>
    static public string defaultLocalPath {
        get {
            if(inst().default_local_path_ == "$PDP")
                return Application.persistentDataPath;
            return inst().default_local_path_;
        }
        set {
            inst().default_local_path_ = value == null ? "$PDP" : value == "" ? "$PDP" : value;
            inst().upload_manager_.localBaseDirectory = defaultLocalPath;
        }
    }

    static public bool compress {
        get { return inst().upload_manager_.compress; }
        set { inst().upload_manager_.compress = value; }
    }

    static DataCollector inst_;
    static DataCollector inst() {
        if(inst_ != null)
            return inst_;
        else throw new NullReferenceException("Hint: Please attach Data Collector to a gameobject first.");
    }

    private FetchRenderTexturePreset overdraw_screenshot_preset_;
    private BuiltInPreset mipmap_screenshot_preset_;
    private IScreenshotPreset[] screenshot_presets_;
    private int screenshot_preset_index_ = 3;
    public static int screenshotPresetIndex {
        get { return inst().screenshot_preset_index_; }
        set {
            if(value < inst().screenshot_presets_.Length)
                inst().screenshot_preset_index_ = value;
            else
                inst().screenshot_preset_index_ = 0;
        }
    }

    static public int screenshotPresetsCount {
        get { return inst().screenshot_presets_.Length; }
    }

    static public string screenshotPresetDescription(int i) {
        return inst().screenshot_presets_[i].description;
    }

    private UploadManager upload_manager_;

    static int change_log_interval_ {
        get {
            if(collectInterval < 250)
                return collectInterval;
            return 250;
        }
    }

	static public void startProfiling() {
        inst().dirname_ = "Profiler-" + metaInfo + "-" +
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "-" +
            DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        if(!Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName);

        inst().resource_log_ = new StreamWriter(
            new FileStream(Path.Combine(directoryName, "resource.log"), FileMode.Create));
        inst().extra_log_ = new StreamWriter(
            new FileStream(Path.Combine(directoryName, "extra.log"), FileMode.Create));

        Profiler.enabled = true;
        Profiler.enableBinaryLog = true;
        Profiler.logFile = Path.Combine(directoryName, "raw-data.0.log");

        inst().start_frame_ = Time.frameCount;
	}

    StreamWriter resource_log_;
    StreamWriter extra_log_;
    Dictionary<String, String> extra_data_;

    static private void reset() {
        inst().start_frame_ = -1;

        Profiler.enabled = false;
        Profiler.logFile = "";

        if(inst().resource_log_ != null) {
            inst().resource_log_.Close();
            inst().resource_log_ = null;
        }

        if(inst().extra_log_ != null) {
            inst().extra_log_.Close();
            inst().extra_log_ = null;
        }
    }
	
	static public void stopProfiling() {
        reset();

        string dn = inst().dirname_;
        inst().dirname_ = "";

        if(autoUpload) {
            inst().upload_manager_.targetName = dn;
            inst().upload_manager_.startToUpload();
        }
	}

    static void sample_pin_() { }
    static internal void insertProfilingTag(string s) {
        Profiler.BeginSample(s);
        sample_pin_();
        Profiler.EndSample();
    }

    public static void addExtra(string name, string value) {
        if(inst().extra_data_ == null)
            inst().extra_data_ = new Dictionary<string, string>();
        inst().extra_data_[name] = value;
    }

    void Start() {
        if(inst_ != null)
            throw new InvalidOperationException("Data Collector should be a singleton.");
        inst_ = this;

        screenshot_presets_ = new IScreenshotPreset[] {
            new NullPreset(),
            new BuiltInPreset(),
            new FetchFrameDataPreset(),
            new FetchRenderTexturePreset(),
            new RootedAndroidPreset(),
        };
        overdraw_screenshot_preset_ = new FetchRenderTexturePreset();
        mipmap_screenshot_preset_ = new BuiltInPreset();

        upload_manager_ = new UploadManager();

        loadConfig();

        DontDestroyOnLoad(this);

        reset();
    }

    void Update() {
        string sample_string = string.Format("FpsTag_{0}_{1}",
            Time.frameCount, (int)(Time.deltaTime * 1e6));
        insertProfilingTag(sample_string);
        int started_frame = Time.frameCount - start_frame_;

        if(!started) return;

        if(started_frame % collectInterval == 0) {
            StartCoroutine(screenshot_presets_[screenshotPresetIndex].takeScreenshot(
                Path.Combine(directoryName, "ScreenshotAtFrame" + (Time.frameCount))));

            if(memorySnapshot)
                takeResourceSnapshot(resource_log_);

            if (overdrawScreenshot) {
                overdraw_screenshot_preset_.camera_type_ = CameraType.OVERDRAW;
                StartCoroutine(overdraw_screenshot_preset_.takeScreenshot(
                    Path.Combine(directoryName, "OverdrawAtFrame" + (Time.frameCount))));
            }
        }

        if(extra_data_ != null)
            writeExtra();

        if(started_frame % change_log_interval_ == 0) {
            if(started_frame / change_log_interval_ > 0)
                Profiler.logFile = Path.Combine(directoryName,
                    "raw-data." + started_frame / change_log_interval_ + ".log");
        }
    }

    public void mipmapScreenshot() {
        if (!started) return;
        StartCoroutine(screenshot_presets_[screenshotPresetIndex].takeScreenshot(
            Path.Combine(directoryName, "MipmapAtFrame" + (Time.frameCount))));
    }

    void writeExtra() {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.Append(Time.frameCount);
        foreach(var entry in extra_data_) {
            sb.Append("|");
            sb.Append(entry.Key);
            sb.Append(":");
            sb.Append(entry.Value);
        }

        extra_log_.WriteLine(sb);
        extra_data_.Clear();
    }

    [Serializable]
    private class Config {
        public string metaInfo = "YourName-ProjectName";
        public bool autoUpload = true;
        public int collectInterval = 150;
        public bool compress = true;
        public string defaultLocalPath = "$PDP";
        public string defaultUploadPath = "ftp://127.0.0.1/";
        public bool memorySnapshot = true;
        public int screenshotPresetIndex = 3;
    }

    static public void loadConfig() {
        string fn = Path.Combine(Application.persistentDataPath, "ProfilerConfig.conf");

        Config conf = null;

        if(File.Exists(fn)) {
            using(FileStream fs = File.OpenRead(fn)) {
                try {
                    BinaryFormatter fm = new BinaryFormatter();
                    conf = fm.Deserialize(fs) as Config;
                } catch(SerializationException) {
                    conf = null;
                    Debug.Log("Unable to load config. Will config anew.");
                }
            }
        }

        conf = conf ?? new Config();

        Debug.Log(conf.defaultUploadPath);

        metaInfo = conf.metaInfo;
        autoUpload = conf.autoUpload;
        collectInterval = conf.collectInterval;
        compress = conf.compress;
        //defaultLocalPath = conf.defaultLocalPath;
        defaultLocalPath = "";
        defaultUploadPath = conf.defaultUploadPath;
        memorySnapshot = conf.memorySnapshot;
        screenshotPresetIndex = conf.screenshotPresetIndex;
    }

    static public void saveConfig() {
        string fn = Path.Combine(Application.persistentDataPath, "ProfilerConfig.conf");

        Config conf = new Config();

        conf.metaInfo = metaInfo;
        conf.autoUpload = autoUpload;
        conf.collectInterval = collectInterval;
        conf.compress = compress;
        //conf.defaultLocalPath = defaultLocalPath;
        conf.defaultUploadPath = defaultUploadPath;
        conf.memorySnapshot = memorySnapshot;
        conf.screenshotPresetIndex = screenshotPresetIndex;

        using(FileStream fs = File.Create(fn)) {
            BinaryFormatter fm = new BinaryFormatter();
            fm.Serialize(fs, conf);
        }
    }

    static void takeResourceSnapshot(StreamWriter f) {
        Type[] ts = new Type[] {
            typeof(Texture2D),
            typeof(Mesh),
            typeof(RenderTexture),
            typeof(AnimationClip),
            typeof(AudioClip),
            /*
            typeof(GameObject),
            typeof(Shader),
            typeof(Material),
            */
        };

        long total_size = 0;

        insertProfilingTag("DiscardedTag");
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach(Type t in ts) {
            sb.Append(Time.frameCount);
            sb.Append("|");
            sb.Append(t.Name);
            sb.Append("|");
            UnityEngine.Object[] texs = Resources.FindObjectsOfTypeAll(t);

            long type_size = 0;
            foreach(UnityEngine.Object tex in texs) {
                if(tex.name != "")
                    sb.Append(string.Format("{0}: {1}\n", tex.name, Profiler.GetRuntimeMemorySizeLong(tex)));
                else
                    sb.Append(string.Format("__UNNAMED: {0}\n", Profiler.GetRuntimeMemorySizeLong(tex)));

                total_size += Profiler.GetRuntimeMemorySizeLong(tex);
                type_size += Profiler.GetRuntimeMemorySizeLong(tex);
            }
            sb.Append(string.Format("__TOTAL: {0}\r\n", type_size));
        }

        f.Write(sb);
        f.Write(string.Format("{0}|Total|{1}\r\n", Time.frameCount, total_size));
    }

    void OnDestroy() {
        // Let it be...
        /*
        if(started)
            stopProfiling();
        saveConfig();
        */
    }

    static public void cleanUp() {
        string[] files = Directory.GetFiles(defaultLocalPath, "Profiler-*.zip");
        string[] dirs = Directory.GetDirectories(defaultLocalPath, "Profiler-*");

        foreach(string f in files)
            File.Delete(f);
        foreach(string d in dirs)
            Directory.Delete(d, true);
    }

    static public void reUpload() {
        string[] files = Directory.GetFiles(defaultLocalPath, "Profiler-*.zip");
        foreach(string f in files)
            File.Delete(f);

        string[] dirs = Directory.GetDirectories(defaultLocalPath, "Profiler-*");
        inst().StartCoroutine(reUpload_coroutine(dirs));
    }

    static private IEnumerator reUpload_coroutine(string[] dirs) {
        foreach(string d in dirs) {
            // wait until the upload thread idles
            while(true) {
                if(uploadProgress != -1)
                    yield return null;
                else break;
            }

            inst().upload_manager_.targetName = Path.GetFileNameWithoutExtension(d);
            inst().upload_manager_.startToUpload();
        }
    }

}

}