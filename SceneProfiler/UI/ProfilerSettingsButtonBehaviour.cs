using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SceneProfiler.UI {

public class ProfilerSettingsButtonBehaviour : MonoBehaviour {
    public GameObject go_panel;

    internal InputField uri_input;
    internal InputField local_input;
    internal InputField interval_input;
    internal InputField meta_input;
    internal Dropdown screenshot_combo;
    internal Toggle memory_toggle;
    internal Toggle auto_upload_toggle;
    internal Toggle compress_toggle;

    public void OnClick() {
        if(isPanelOpen)
            close();
        else
            open();
    }

    public void OnCleanUpClick() {
        close();
        DataCollector.cleanUp();
    }

    public void OnReUploadClick() {
        close();
        DataCollector.reUpload();
    }

    public bool isPanelOpen { get { return go_panel.activeSelf; } }

    public void close() {
        go_panel.SetActive(false);

        DataCollector.defaultUploadPath = uri_input.text;
        DataCollector.defaultLocalPath = local_input.text;
        DataCollector.metaInfo = meta_input.text;
        DataCollector.collectInterval = int.Parse(interval_input.text);
        DataCollector.autoUpload = auto_upload_toggle.isOn;
        DataCollector.memorySnapshot = memory_toggle.isOn;
        DataCollector.screenshotPresetIndex = screenshot_combo.value;
        DataCollector.compress = compress_toggle.isOn;

        DataCollector.saveConfig();
    }

    public void open() {
        go_panel.SetActive(true);

        uri_input.text = DataCollector.defaultUploadPath;
        local_input.text = DataCollector.defaultLocalPath;
        interval_input.text = DataCollector.collectInterval.ToString();
        meta_input.text = DataCollector.metaInfo;
        auto_upload_toggle.isOn = DataCollector.autoUpload;
        memory_toggle.isOn = DataCollector.memorySnapshot;
        compress_toggle.isOn = DataCollector.compress;

        if(screenshot_combo.options.Count != DataCollector.screenshotPresetsCount) {
            screenshot_combo.options.Clear();
            for(int i = 0; i < DataCollector.screenshotPresetsCount; i++) {
                screenshot_combo.options.Add(new Dropdown.OptionData(
                    DataCollector.screenshotPresetDescription(i)));
            }
        }

        screenshot_combo.value = DataCollector.screenshotPresetIndex;
    }

    void Start() {
        Transform tf_panel = go_panel.GetComponent<Transform>();

        GameObject go_ui = tf_panel.Find("UriInput").gameObject;
        GameObject go_li = tf_panel.Find("LocalInput").gameObject;
        GameObject go_ii = tf_panel.Find("IntervalInput").gameObject;
        GameObject go_mi = tf_panel.Find("MetaInput").gameObject;
        GameObject go_sc = tf_panel.Find("ScreenshotCombo").gameObject;
        GameObject go_mt = tf_panel.Find("MemoryToggle").gameObject;
        GameObject go_at = tf_panel.Find("AutoUploadToggle").gameObject;
        GameObject go_ct = tf_panel.Find("CompressionToggle").gameObject;

        uri_input = go_ui.GetComponent<InputField>();
        local_input = go_li.GetComponent<InputField>();
        interval_input = go_ii.GetComponent<InputField>();
        meta_input = go_mi.GetComponent<InputField>();
        screenshot_combo = go_sc.GetComponent<Dropdown>();
        memory_toggle = go_mt.GetComponent<Toggle>();
        auto_upload_toggle = go_at.GetComponent<Toggle>();
        compress_toggle = go_ct.GetComponent<Toggle>();

        go_panel.SetActive(false);
    }
}

}
