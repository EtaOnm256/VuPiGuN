using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
	//�@�񓯊�����Ŏg�p����AsyncOperation
	private AsyncOperation async;
	//�@�V�[�����[�h���ɕ\������UI���
	//[SerializeField]
	//private GameObject loadUI;
	//�@�ǂݍ��ݗ���\������X���C�_�[
	//[SerializeField]
	//private Slider slider;

	[SerializeField] GameState gameState;

	public void Start()
	{
		//�@���[�h���UI���A�N�e�B�u�ɂ���
		//loadUI.SetActive(true);

		//�@�R���[�`�����J�n
		StartCoroutine("LoadData");
	}

	IEnumerator LoadData()
	{
		if (gameState.stage >= 1 && gameState.stage <= 6)
		{
			if(gameState.intermission)
            {
				async = SceneManager.LoadSceneAsync($"Intermission");
			}
			else
            {
				if (gameState.shoulderWeapon_name != null && gameState.shoulderWeapon_name != "")
				{
					if(gameState.subWeaponType == IntermissionButton.ShopItemWeapon.Type.Shoulder)
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

				async = SceneManager.LoadSceneAsync($"Stage{gameState.stage}");
			}

		
		}
		else
		{
			async = SceneManager.LoadSceneAsync("Title");
		}

		//�@�ǂݍ��݂��I���܂Ői���󋵂��X���C�_�[�̒l�ɔ��f������
		while (!async.isDone)
		{
			var progressVal = Mathf.Clamp01(async.progress / 0.9f);
			//slider.value = progressVal;
			yield return null;
		}
	}
}
