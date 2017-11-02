using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagPlane : MonoBehaviour {
    public GameObject BagPlaneSelf;

    void Awake()
    {
        BagPlaneSelf = this.gameObject;
    }

	// Use this for initialization
	void Start () {
        BagPlaneSelf.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool isPanelOpen { get { return BagPlaneSelf.activeSelf; } }

    public void OnClick()
    {
        if (isPanelOpen)
            BagPlaneSelf.SetActive(false);
        else
            BagPlaneSelf.SetActive(true);
    }
}
