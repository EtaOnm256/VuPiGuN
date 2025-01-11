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
			async = SceneManager.LoadSceneAsync($"Stage{gameState.stage}");
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
