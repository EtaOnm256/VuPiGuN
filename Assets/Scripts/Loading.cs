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
		if (gameState.stage >= 1 && gameState.stage <= 6)
		{
			async = SceneManager.LoadSceneAsync($"Stage{gameState.stage}");
		}
		else
		{
			async = SceneManager.LoadSceneAsync("Title");
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
