using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotMotion : MonoBehaviour {

	public Vector2 constVec; //(1.0, 0.0);

	void Awake () {
		// constVec = Vector2(1.0, 0.0);
		constVec.Set(1.0, 0.0);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void JoystickMove(Vector2 move)
	{
		Debug.Log(move);
		float angle = Vector3.Angle(move, constVec);
		transform.rotation = Quaternion.Euler(0, angle, 0);
	}
}
