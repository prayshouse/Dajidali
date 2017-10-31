#define ENABLE_PROFILER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using UnityEngine;

namespace SceneProfiler {

public enum CameraType { MAIN, OVERDRAW, MIPMAP }

interface IScreenshotPreset {
    string description { get; }
    IEnumerator takeScreenshot(string path);
    void waitForCompletion();
}

class NullPreset : IScreenshotPreset {
    public string description { get { return "Do Not Take Screenshot"; } }
    public IEnumerator takeScreenshot(string path) { yield return null; }
    public void waitForCompletion() { }
}

class BuiltInPreset : IScreenshotPreset {
    public string description { get { return "Built-in Capturer"; } }

    public IEnumerator takeScreenshot(string path) {
        DataCollector.insertProfilingTag("DiscardedTag");
        Application.CaptureScreenshot(path + ".png");

        yield return null; 
    }

    public void waitForCompletion() { }
}

class FetchFrameDataPreset : IScreenshotPreset {
    Texture2D screen_cap_cam_tex_;

    public string description { get { return "Fetch Screen Mode"; } }

    public IEnumerator takeScreenshot(string path) {
        yield return new WaitForEndOfFrame();
        DataCollector.insertProfilingTag("DiscardedTag");

        if(!screen_cap_cam_tex_ ||
            screen_cap_cam_tex_.width != Screen.width ||
            screen_cap_cam_tex_.height != Screen.height)
            screen_cap_cam_tex_ = new Texture2D(Screen.width, Screen.height);

        screen_cap_cam_tex_.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screen_cap_cam_tex_.Apply();

        FileStream fs = new FileStream(path + ".png", FileMode.Create);
        byte[] f_data = screen_cap_cam_tex_.EncodeToPNG();
        fs.Write(f_data, 0, f_data.Length);
        fs.Close();
        fs = null;

        yield return null;
        DataCollector.insertProfilingTag("DiscardedTag");
    }

    public void waitForCompletion() { }
}

class AndroidNativePreset : IScreenshotPreset {
    public string description { get { return "Android Native Mode (Not working)"; } }

    public IEnumerator takeScreenshot(string path) {
        DataCollector.insertProfilingTag("DiscardedTag");

        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject cur_act = jc.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject cur_win = cur_act.Call<AndroidJavaObject>("getWindow");
        AndroidJavaObject cur_dec = cur_win.Call<AndroidJavaObject>("getDecorView");
        AndroidJavaObject cur_viw = cur_dec.Call<AndroidJavaObject>("getRootView");
        cur_viw.Call("setDrawingCacheEnabled", true);
        AndroidJavaObject bmp = cur_viw.Call<AndroidJavaObject>("getDrawingCache");

        AndroidJavaObject file = new AndroidJavaObject("java.io.FileOutputStream", path + ".png");
        AndroidJavaClass formats = new AndroidJavaClass("android.graphics.Bitmap$CompressFormat");
        var compress_method = AndroidJNI.GetMethodID(bmp.GetRawClass(), "compress", "(Landroid/graphics/Bitmap$CompressFormat;ILjava/io/OutputStream;)Z");
        jvalue arg1 = new jvalue(); arg1.l = formats.GetStatic<AndroidJavaObject>("PNG").GetRawObject();
        jvalue arg2 = new jvalue(); arg2.i = 100;
        jvalue arg3 = new jvalue(); arg3.l = file.GetRawObject();
        AndroidJNI.CallBooleanMethod(bmp.GetRawObject(), compress_method, new jvalue[] {
            arg1, arg2, arg3
        });
        file.Call("close");
        cur_viw.Call("setDrawingCacheEnabled", false);

        yield return null;
    }

    public void waitForCompletion() { }
}

class FetchRenderTexturePreset : IScreenshotPreset {
    public string description { get { return "Fetch Texture Mode (Smaller)"; } }

    RenderTexture screen_cap_cam_rt_;
    Camera screen_cap_cam_;
    List<Thread> screen_cap_work_threads_;
    Texture2D screen_cap_cam_tex_;
    public CameraType camera_type_ = CameraType.MAIN;

    public static double zoomOutRatio = 4;

    static int ssWidth {
        get { return (int) (Screen.width / zoomOutRatio); }
    }

    static int ssHeight {
        get { return (int) (Screen.height / zoomOutRatio); }
    }

    public IEnumerator takeScreenshot(string path) {
        if(screen_cap_cam_ == null)
            switch (camera_type_) {
            case CameraType.OVERDRAW:
                screen_cap_cam_ = GameObject.Find("OverdrawMonitor").GetComponent<Camera>();
                break;
            case CameraType.MIPMAP:
                screen_cap_cam_ = GameObject.Find("MipmapMonitor").GetComponent<Camera>();
                break;
            case CameraType.MAIN:
            default:
                screen_cap_cam_ = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                break;
            }
            
        if(screen_cap_cam_rt_ == null)
            screen_cap_cam_rt_ = new RenderTexture(ssWidth, ssHeight, 16);
        if(screen_cap_cam_rt_.width != ssWidth || screen_cap_cam_rt_.height != ssHeight) {
            screen_cap_cam_rt_.Release(); // memory leak if not releasing!
            screen_cap_cam_rt_ = new RenderTexture(ssWidth, ssHeight, 16);
        }
        if(screen_cap_work_threads_ == null)
            screen_cap_work_threads_ = new List<Thread>();
        if(screen_cap_cam_tex_ == null)
            screen_cap_cam_tex_ = new Texture2D(screen_cap_cam_rt_.width, screen_cap_cam_rt_.height, TextureFormat.RGB24, false);
        if(screen_cap_cam_tex_.width != ssWidth || screen_cap_cam_tex_.height != ssHeight)
            screen_cap_cam_tex_.Resize(ssWidth, ssHeight, TextureFormat.RGB24, false);

        int prev = screen_cap_cam_.targetDisplay;
        screen_cap_cam_.targetTexture = screen_cap_cam_rt_;
        screen_cap_cam_.targetDisplay = -1;
        screen_cap_cam_.Render();
        screen_cap_cam_.targetTexture = null;
        screen_cap_cam_.targetDisplay = prev;

        DataCollector.insertProfilingTag("DiscardedTag");

        yield return null;

        DataCollector.insertProfilingTag("DiscardedTag");

        RenderTexture.active = screen_cap_cam_rt_;
        screen_cap_cam_tex_.ReadPixels(new Rect(0, 0, ssWidth, ssHeight), 0, 0);

        byte[] f_data = screen_cap_cam_tex_.GetRawTextureData();
        int w = screen_cap_cam_tex_.width, h= screen_cap_cam_tex_.height;

        waitForCompletion();
        screen_cap_work_threads_.Add(new Thread(() => {
            FileStream fs = new FileStream(path + ".ppm", FileMode.Create);
            string header = string.Format("P6\n{0} {1}\n255\n", w, h);
            byte[] raw_header = System.Text.Encoding.ASCII.GetBytes(header);
            fs.Write(raw_header, 0, raw_header.Length);
            fs.Write(f_data, 0, f_data.Length);
            fs.Close();

            System.GC.Collect();
        }));
        screen_cap_work_threads_.Last().Start();

        yield return null;

        DataCollector.insertProfilingTag("DiscardedTag");
    }

    public void waitForCompletion() {
        if(screen_cap_work_threads_ != null) {
            foreach(var st in screen_cap_work_threads_)
                if(st != null) st.Join();
            screen_cap_work_threads_.Clear();
        }
    }

    ~FetchRenderTexturePreset() {
        if(screen_cap_cam_rt_ != null)
            screen_cap_cam_rt_.Release();
    }
}

class RootedAndroidPreset : IScreenshotPreset {
    System.Diagnostics.Process su_proc_;

    public string description { get { return "Rooted Android Mode (Fastest)"; } }

    public IEnumerator takeScreenshot(string path) {
        if(su_proc_ == null) {
            System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo("su");
            si.UseShellExecute = false;
            si.RedirectStandardInput = true;
            su_proc_ = System.Diagnostics.Process.Start(si);
        }
        su_proc_.StandardInput.WriteLine(string.Format("screencap -p {0}.png\n", path));
        su_proc_.StandardInput.Flush();

        yield return null;
    }

    public void waitForCompletion() { }
}

}