using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KyleControl : MonoBehaviour
{

    private Animator anim;
    //private float jumpTimer = 0;
    public enum state_ { Idle, Run };
    public state_ robotState { get; set; }

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        switch (robotState)
        {
            case state_.Idle:
                anim.SetInteger("Speed", 0);
                break;
            case state_.Run:
                anim.SetInteger("Speed", 2);
                break;
            default:
                break;
        }

        // Jump
        //if (Input.GetKey("3"))
        //{

        //    jumpTimer = 1;
        //    anim.SetBool("Jumping", true);

        //}

        //if (jumpTimer > 0.5) jumpTimer -= Time.deltaTime;
        //else if (anim.GetBool("Jumping") == true) anim.SetBool("Jumping", false);

    }
}
