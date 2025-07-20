using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
	//　非同期動作で使用するAsyncOperation
	private AsyncOperation async;
	//　シーンロード中に表示するUI画面
	//[SerializeField]
	//private GameObject loadUI;
	//　読み込み率を表示するスライダー
	//[SerializeField]
	//private Slider slider;

	[SerializeField] GameState gameState;

	public void Start()
	{
		//　ロード画面UIをアクティブにする
		//loadUI.SetActive(true);

		//　コルーチンを開始
		StartCoroutine("LoadData");
	}

	IEnumerator LoadData()
	{
		if (gameState.stage <= 7)
		{
			switch(gameState.loadingDestination)
            {
				case GameState.LoadingDestination.Intermission:
				case GameState.LoadingDestination.Intermission_Garage:
					async = SceneManager.LoadSceneAsync($"Intermission");
					break;
				case GameState.LoadingDestination.Mission:
				case GameState.LoadingDestination.TestingRoom:
					if (gameState.shoulderWeapon_name != null && gameState.shoulderWeapon_name != "")
					{
						if (gameState.subWeaponType == IntermissionButton.ShopItemWeapon.Type.Shoulder)
							gameState.player_variant = (GameObject)Resources.Load($"Robots/Robot6_Variant/Robot6 Shoulder");
						else
							gameState.player_variant = (GameObject)Resources.Load($"Robots/Robot6_Variant/Robot6 Back");
					}
					else
						gameState.player_variant = (GameObject)Resources.Load($"Robots/Robot6_Variant/Robot6 Std");

					RobotController.RobotParameter robotParameter = gameState.player_variant.GetComponent<RobotController>().robotParameter;

					if (gameState.rightWeapon_name != null && gameState.rightWeapon_name != "")
						robotParameter.rweapon_prefab = (GameObject)Resources.Load($"Weapons/{gameState.rightWeapon_name}");
					else
						robotParameter.rweapon_prefab = null;

					if (gameState.shoulderWeapon_name != null && gameState.shoulderWeapon_name != "")
						robotParameter.subweapon_prefab = (GameObject)Resources.Load($"Weapons/{gameState.shoulderWeapon_name}");
					else
						robotParameter.subweapon_prefab = null;

					robotParameter.itemFlag = gameState.itemFlag;

					if (gameState.loadingDestination == GameState.LoadingDestination.TestingRoom)
						async = SceneManager.LoadSceneAsync($"TestingRoom");
					else
						async = SceneManager.LoadSceneAsync($"Stage{gameState.stage}");
					break;
				case GameState.LoadingDestination.Title:
					async = SceneManager.LoadSceneAsync("Title");
					break;
			}
		}
		else
		{
			async = SceneManager.LoadSceneAsync("Ending");
		}

		//　読み込みが終わるまで進捗状況をスライダーの値に反映させる
		while (!async.isDone)
		{
			var progressVal = Mathf.Clamp01(async.progress / 0.9f);
			//slider.value = progressVal;
			yield return null;
		}
	}
}
