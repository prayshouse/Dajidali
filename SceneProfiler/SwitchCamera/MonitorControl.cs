using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneProfiler {

public class MonitorControl : MonoBehaviour {

    private OverdrawMonitor overdrawMonitor;

	// Use this for initialization
	void Start () {
        overdrawMonitor = OverdrawMonitor.Instance;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
}