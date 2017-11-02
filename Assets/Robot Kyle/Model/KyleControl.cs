using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KyleControl : MonoBehaviour
{

    private Animator anim;
    public enum state_ { Idle, Run, Walk };
    public state_ robotState { get; set; }

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        anim.SetBool("Idling", true);
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
            case state_.Walk:
                anim.SetInteger("Speed", 1);
                break;
            default:
                break;
        }
    }
}
