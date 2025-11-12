using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
	public enum Destination
	{
		Mission,
		TestingRoom,
		//Intermission, //重くなったら追加するかも
		//Intermission_Garage, //重くなったら追加するかも
		//WorldMap, //重くなったら追加するかも
		//Title //重くなったら追加するかも
	}

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
		//if (gameState.progress <= gameState.GetMaxProgress())
		//{
			switch(gameState.loadingDestination)
            {
				//case Destination.Intermission:
				//case Destination.Intermission_Garage:
				//	async = SceneManager.LoadSceneAsync($"Intermission");
				//	break;
				case Destination.Mission:
				case Destination.TestingRoom:
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

					if (gameState.loadingDestination == Destination.TestingRoom)
						async = SceneManager.LoadSceneAsync($"TestingRoom");
					else
						async = SceneManager.LoadSceneAsync(gameState.GetNextStage_UpdateSkyIndex().Item2);

					if (gameState.loadingDestination == Destination.Mission)
					{
						gameState.army_enemy = Resources.Load<Army>($"Armys/Army{gameState.progressStage}_enemy");
						gameState.army_friend = Resources.Load<Army>($"Armys/Army{gameState.progressStage}_friend");
					}

					if (gameState.loadingDestination == Destination.TestingRoom)
					{
						gameState.army_enemy = Resources.Load<Army>($"Armys/ArmyTest_enemy");
						gameState.army_friend = Resources.Load<Army>($"Armys/ArmyTest_friend");
					}

					break;
				//case GameState.LoadingDestination.Title:
				//	async = SceneManager.LoadSceneAsync("Title");
				//	break;
				//case GameState.LoadingDestination.WorldMap:
				//	async = SceneManager.LoadSceneAsync("WorldMap");
				//	break;
			}
		//}
		//else
		//{
		//	async = SceneManager.LoadSceneAsync("Ending");
		//}

		//　読み込みが終わるまで進捗状況をスライダーの値に反映させる
		while (!async.isDone)
		{
			var progressVal = Mathf.Clamp01(async.progress / 0.9f);
			//slider.value = progressVal;
			yield return null;
		}
	}
}
