using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {

    public GameObject Robot = null;
    private Vector3 delta = new Vector3(-0.31f, 7.679f, -6.95f);

    // Use this for initialization
    void Start () {
        // Robot = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
        if (Robot)
            transform.position = new Vector3(Robot.transform.position.x + delta.x, delta.y, Robot.transform.position.z + delta.z);
	}
}
