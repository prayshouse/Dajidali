using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneProfiler {

public class OverdrawMonitor : MonoBehaviour
{
    private static OverdrawMonitor instance;
    public static OverdrawMonitor Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<OverdrawMonitor>();
                if (instance == null)
                {
                    var go = new GameObject("OverdrawMonitor");
                    instance = go.AddComponent<OverdrawMonitor>();
                }
            }
            return instance;
        }
    }

    private new Camera camera;
    private Shader replacementShader;

    public void Awake()
    {
        if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        replacementShader = Shader.Find("Debug/OverdrawInt");

        camera = GetComponent<Camera>();
        if (camera == null) camera = gameObject.AddComponent<Camera>();
        camera.CopyFrom(Camera.main);
        camera.SetReplacementShader(replacementShader, null);
    }


    // Use this for initialization
    void Start()
    {
        camera.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LateUpdate()
    {
        Camera main = Camera.main;
        camera.CopyFrom(main);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.SetReplacementShader(replacementShader, null);

        transform.position = main.transform.position;
        transform.rotation = main.transform.rotation;
    }
}
}