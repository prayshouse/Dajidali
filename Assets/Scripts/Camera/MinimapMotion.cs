using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapMotion : MonoBehaviour {
    public GameObject Robot = null;

	// Use this for initialization
	void Start () {
        // Robot = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
        if (Robot)
            transform.position = new Vector3(Robot.transform.position.x, 80.0f, Robot.transform.position.z);
	}
}
