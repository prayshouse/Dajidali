using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HedgehogTeam.EasyTouch;

public class TouchBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // Subscribe to events
    void OnEnable()
    {
        EasyTouch.On_TouchStart += On_TouchStart;
    }
    // Unsubscribe
    void OnDisable()
    {
        EasyTouch.On_TouchStart -= On_TouchStart;
    }
    // Unsubscribe
    void OnDestroy()
    {
        EasyTouch.On_TouchStart -= On_TouchStart;
    }
    // Touch start event
    public void On_TouchStart(Gesture gesture)
    {
        Debug.Log("Touch in " + gesture.position);
    }
}
