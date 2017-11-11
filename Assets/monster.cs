using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class monster : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collision)
	{
		Debug.Log ("怪物发生碰撞");
		print (collision.collider.gameObject.name);

	}
}
