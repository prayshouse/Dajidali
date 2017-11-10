using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetWeaponBind : MonoBehaviour {

    private static GetWeaponBind instance;
    public static GetWeaponBind Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject tmp = GameObject.Find("GetWeapon");
                instance = tmp.AddComponent<GetWeaponBind>();
            }
            return instance;
        }
    }

    GameObject weaponBox_;
    static GameObject weaponBox
    {
        get { return Instance.weaponBox_; }
        set { Instance.weaponBox_ = value; }
    }

    GameObject mainPlayer_;
    static GameObject mainPlayer
    {
        get { return Instance.mainPlayer_; }
        set { Instance.mainPlayer_ = value; }
    }

    // Use this for initialization
    void Start () {
        mainPlayer = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void bindWeaponBox(GameObject wb)
    {
        weaponBox = wb;
    }

    public void freeWeaponBoxBind()
    {
        weaponBox = null;
    }

    public bool checkWeaponBox(GameObject wb)
    {
        bool result = false;
        if (weaponBox == wb) result = true;
        return result;
    }

    public void OnUp()
    {
        if (weaponBox != null)
        {
            // 处理获得装备的逻辑
            // 武器盒：weaponBox
            // 玩家:mainPlayer
            // 
            Debug.Log("getWeapon!");
        }
    }
}
