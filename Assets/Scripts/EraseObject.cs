using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseObject : MonoBehaviour
{
	//　操作キャラクターのTransform
	//private Transform characterTransform;
	private Renderer myRenderer;
	private Material[] materials;
	private Color[] colors_org;
	private Color[] colors_trans;
	//　オブジェクトのアルファ値を操作するカメラとの距離
	[SerializeField]
	private float cameraDistance = 15.0f;
	// カメラとの距離によってアルファ値に影響を与えるオフセット値
	//[SerializeField]
	//private float alphaOffset = 5f;
	//　距離を測るカメラ
	private Camera mainCamera;
	//　カメラ位置と自身の位置の距離
	private float distance;
	//　操作キャラクターとカメラの距離
	//private float distanceBetweenCharacterAndCamera;
	//　レンダラーがカメラの範囲内かどうか
	private bool isVisible;

	// Start is called before the first frame update
	void Start()
	{
		//characterTransform = GameObject.FindWithTag("Player").transform;
		mainCamera = Camera.main;
		myRenderer = GetComponent<Renderer>();
		materials = myRenderer.materials;

		colors_org = new Color[materials.Length];
		colors_trans = new Color[materials.Length];

		int idx = 0;

		foreach (var material in materials)
		{
			colors_org[idx] = material.color;
			colors_trans[idx] = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
			idx++;
		}
	}

	// Update is called once per frame
	void Update()
	{
		//　カメラの範囲内でなければそれ以降の処理を行わない
		if (!isVisible)
		{
			return;
		}

		//　カメラ位置と自身の位置の距離を計算
		//distance = Vector3.Distance(mainCamera.transform.position, transform.position);
		distance = Vector3.Dot(transform.position-mainCamera.transform.position, mainCamera.transform.forward);

		//　操作キャラクターとカメラの距離
		//distanceBetweenCharacterAndCamera = Vector3.Distance(mainCamera.transform.position, characterTransform.position);

		Debug.Log(distance);

		//　操作キャラクターとカメラの間に自身がいる場合、かつcameraDistanceより短い距離にいた場合は距離に応じたアルファ値にする
		if (distance < cameraDistance)
		{
			int idx = 0;
			foreach (var material in materials)
			{
				//material.SetFloat("_Alpha", Mathf.InverseLerp(0f, cameraDistance + alphaOffset, distance));
				material.color = colors_trans[idx++];
			}
		}
		else
		{
			int idx = 0;
			//　指定距離より遠い場合は標準
			foreach (var material in materials)
			{
				//material.SetFloat("_Alpha", 1f);
				material.color = colors_org[idx++];
			}
		}
	}

	private void OnBecameVisible()
	{
		isVisible = true;
		foreach (var material in materials)
		{
			material.SetFloat("_Alpha", 1f);
		}
	}
	private void OnBecameInvisible()
	{
		isVisible = false;
	}
}