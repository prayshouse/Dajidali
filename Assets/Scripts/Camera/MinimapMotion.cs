using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapMotion : MonoBehaviour {
    GameObject klyeRobot;

	// Use this for initialization
	void Start () {
        klyeRobot = GameObject.Find("Robot Kyle");
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(klyeRobot.transform.position.x, 20.0f, klyeRobot.transform.position.z);
	}
}
