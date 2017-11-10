using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBox : MonoBehaviour {

    public bool hasWeapon = true;

    public bool Bow = false;

    // L
    public bool Gladius1 = false;
    public bool Shield1 = false;
    public bool Shield2 = false;
    public bool Shield3 = false;

    // R
    public int Arrow = 0;
    public bool Broadsword1 = false;
    public bool Broadsword2 = false;
    public bool Gladius2 = false;
    public bool Hammer = false;
    public bool Longsword = false;
    public bool Rifle = false;
    public bool SpacePistol = false;
    public bool SpaceRifle = false;
    public bool Spear = false;

    Collider sphereCollider;

    GameObject mainPlayer;
    Collider mainPlayerCollider;

	// Use this for initialization
	void Start () {
        sphereCollider = gameObject.GetComponent<SphereCollider>();
        mainPlayer = GameObject.FindGameObjectWithTag("Player");
        mainPlayerCollider = mainPlayer.GetComponent<CapsuleCollider>();
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    void OnTriggerEnter(Collider other)
    {
        if (hasWeapon && other == mainPlayerCollider)
        {
            GetWeaponBind.Instance.bindWeaponBox(gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == mainPlayerCollider && GetWeaponBind.Instance.checkWeaponBox(gameObject))
        {
            GetWeaponBind.Instance.freeWeaponBoxBind();
        }
    }
}
