using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SceneProfiler.UI {

public class ProfilingButtonBehaviour : MonoBehaviour {
    public void OnClick() {
        if(!DataCollector.started) {
            ProfilerSettingsButtonBehaviour psb = GameObject.Find("ProfilerSettingsButton")
                .GetComponent<ProfilerSettingsButtonBehaviour>();

            if(psb.isPanelOpen)
                psb.close();

            DataCollector.startProfiling();
        } else {
            DataCollector.stopProfiling();
        }
    }

    void Update() {
        if(DataCollector.uploadProgress == -1) {
            gameObject.GetComponent<Button>().enabled = true;
            gameObject.GetComponentInChildren<Text>().text =
                DataCollector.started ? "Stop Profiling" : "Start Profiling";
        } else {
            gameObject.GetComponent<Button>().enabled = false;
            gameObject.GetComponentInChildren<Text>().text = string.Format(
                DataCollector.uploadProgress == 0 && DataCollector.compress ? "Compressing" : "Uploading in {0}%",
                (int) DataCollector.uploadProgress);
        }
    }

    void Start() {
        // TODO: Move it somewhere else?
        if(Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) {
            GameObject go_es = new GameObject();
            go_es.name = "EventSystem";
            go_es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go_es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            DontDestroyOnLoad(go_es);
        }
    }
}
}
