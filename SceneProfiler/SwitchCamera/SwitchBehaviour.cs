using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneProfiler {

public class SwitchBehaviour : MonoBehaviour {

	private enum cameraType { Main, Mipmap, Overdraw };
	private static cameraType currentCamera;
	private Camera mainCamera;
	//private GameObject mipmapButton;
	private Shader overdrawShader;
	private Shader mipmapShader;
	private CameraClearFlags mainClearFlag;
	private Color mainBackground;
    

	// Use this for initialization
	void Start () {
		currentCamera = cameraType.Main;
		mainCamera = Camera.main;
		//mipmapButton = GameObject.Find("MipmapScreenshot");
        //mipmapButton.SetActive(false);

		overdrawShader = Shader.Find("Debug/OverdrawInt");
		mipmapShader = Shader.Find("Debug/MipmapLevelView");

		mainClearFlag = mainCamera.clearFlags;
		mainBackground = mainCamera.backgroundColor;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClick()
	{
		switch (currentCamera)
		{
			case cameraType.Main:
				mainCamera.clearFlags = CameraClearFlags.SolidColor;
				mainCamera.backgroundColor = Color.black;
				mainCamera.SetReplacementShader(mipmapShader, null);
				currentCamera = cameraType.Mipmap;
				//mipmapButton.SetActive(true);
				break;
			case cameraType.Mipmap:
				mainCamera.SetReplacementShader(overdrawShader, null);
				currentCamera = cameraType.Overdraw;
				//mipmapButton.SetActive(false);
				break;
			case cameraType.Overdraw:
				mainCamera.ResetReplacementShader();
                mainCamera.clearFlags = mainClearFlag;
				mainCamera.backgroundColor = mainBackground;
				currentCamera = cameraType.Main;
				break;
			default:
				break;
		}
	}
}
}