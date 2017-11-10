using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotInit : MonoBehaviour {

    GameObject joystick;
    ETCJoystick etcJoystick;
    RobotMotion robotMotion;
    JoystickBind joystickBind;

    GameObject mainCamera;
    CameraMotion cameraMotion;

    GameObject minimapCamera;
    MinimapMotion minimapMotion;

	// Use this for initialization
	void Start () {
        joystick = GameObject.Find("MoveJoystick");
        joystickBind = joystick.GetComponent<JoystickBind>();
        if (!joystickBind.getBinded())
        {
            etcJoystick = joystick.GetComponent<ETCJoystick>();
            robotMotion = gameObject.GetComponent<RobotMotion>();
            etcJoystick.onMove.AddListener(robotMotion.JoystickMove);
            etcJoystick.onTouchStart.AddListener(robotMotion.JoystickMoveStart);
            etcJoystick.onTouchUp.AddListener(robotMotion.JoysitckMoveEnd);

            mainCamera = GameObject.Find("Main Camera");
            cameraMotion = mainCamera.GetComponent<CameraMotion>();
            cameraMotion.Robot = gameObject;

            minimapCamera = GameObject.Find("MinimapCamera");
            minimapMotion = minimapCamera.GetComponent<MinimapMotion>();
            minimapMotion.Robot = gameObject;

            joystickBind.bindJoystick();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
