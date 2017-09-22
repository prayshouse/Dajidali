using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotMotion : MonoBehaviour {

    private Animator anim;
    private Vector2 constVec;
    private KyleControl kyleControl;

	void Awake () {
		constVec.Set(0.0f, 1.0f);
    }

	// Use this for initialization
	void Start ()
    {
        anim = this.gameObject.GetComponent<Animator>();
        kyleControl = gameObject.GetComponent<KyleControl>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	public void JoystickMove(Vector2 move)
	{
		// Debug.Log(move);
		float angle = (move.x < 0.0f) ?
            360.0f - Vector3.Angle(move, constVec) : Vector3.Angle(move, constVec);
		transform.rotation = Quaternion.Euler(0, angle, 0);
        transform.position =
            new Vector3(transform.position.x + move.x / 10, transform.position.y, transform.position.z + move.y / 10);
    }

    public void JoystickMoveStart()
    {
        kyleControl.robotState = KyleControl.state_.Run;
    }

    public void JoysitckMoveEnd()
    {
        kyleControl.robotState = KyleControl.state_.Idle;
    }
}
