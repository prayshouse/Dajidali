using UnityEngine;
using System.Collections;
public class Bullet : MonoBehaviour {
	void OnEnable  () {
		//脚本可用的时候，重置子弹的位置。
		//如果不加这句代码，从对象池中取出的子弹就会从上一次消失的位置开始运动。而不是你设定的子弹生成位置
		transform.position = Vector3.zero;
		//开启协程方法
		StartCoroutine(DelayDisable(3f));
	}


	void Update () {
		//子弹生成，自动向前运动
		transform.Translate(Vector3.forward*Time.deltaTime*20);
	}
	void OnDisable(){
		Debug.Log("I'm over");
	}
	//子弹消失的方法
	IEnumerator DelayDisable(float time){
		//等待三秒
		yield return new WaitForSeconds(time);
		//调用单例中向对象池里面存对象的方法
		GameObjectPool.GetInstance().MyDisable(gameObject);
	}

}

