using UnityEngine;
using System.Collections;

public class gameManager : MonoBehaviour {
	//创建子弹的预设体
	public GameObject mBulletPrefab;

	GameObject mainPlayer;

	GameObject currentBullet;

	void Awake()
	{
		mainPlayer = GameObject.FindGameObjectWithTag ("Player");
	}


	void Update () {
		//如果按下鼠标左键
		//if(Input.GetMouseButtonDown(0)){
			//调用单例脚本里面的从对象池中取对象的方法
		//	GameObjectPool.GetInstance().MyInstantiate(mBulletPrefab);
		//}

	}
	public void shoot()
	{
		
	
		currentBullet = GameObjectPool.GetInstance ().MyInstantiate (mBulletPrefab);
		currentBullet.transform.SetPositionAndRotation (mainPlayer.transform.position, mainPlayer.transform.rotation);
	}
}
