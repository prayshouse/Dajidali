using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAnim : MonoBehaviour {

    public Animator anim;

	// Use this for initialization
	void Start () {
        anim = gameObject.GetComponent<Animator>();
        anim.SetBool("Idling", true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
