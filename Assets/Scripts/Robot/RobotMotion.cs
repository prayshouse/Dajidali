using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotMotion : MonoBehaviour {

    private Animator anim;
    private Vector2 constVec;
    private Rigidbody rigidbody;

	void Awake () {
		constVec.Set(0.0f, 1.0f);
        anim = gameObject.GetComponent<Animator>();
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

	// Use this for initialization
	void Start ()
    {
        anim.SetBool("Idleing", true);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	public void JoystickMove(Vector2 move)
	{
        float angle = (move.x < 0.0f) ?
            360.0f - Vector3.Angle(move, constVec) : Vector3.Angle(move, constVec);
		transform.rotation = Quaternion.Euler(0, angle, 0);
        transform.position =
            new Vector3(transform.position.x + move.x / 10, transform.position.y, transform.position.z + move.y / 10);
    }

    public void JoystickMoveStart()
    {
        anim.SetBool("Idling", false);
        rigidbody.freezeRotation = true;
    }

    public void JoysitckMoveEnd()
    {
        anim.SetBool("Idling", true);
        rigidbody.freezeRotation = false;
    }
}
