using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {

    GameObject Robot;
    private Vector3 delta = new Vector3(-0.31f, 7.679f, -6.95f);

    // Use this for initialization
    void Start () {
        Robot = GameObject.Find("Robot Kyle");
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(Robot.transform.position.x + delta.x, delta.y, Robot.transform.position.z + delta.z);
	}
}
