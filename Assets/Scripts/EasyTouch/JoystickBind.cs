using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickBind : MonoBehaviour {

    private bool hasBinded = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool getBinded()
    {
        return hasBinded;
    }

    public void bindJoystick()
    {
        hasBinded = true;
    }
}
