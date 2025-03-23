using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] Image blackout;
    [SerializeField] GameState gameState;
    [SerializeField] TMPro.TextMeshProUGUI exitButtonText;

    [SerializeField] bool testingroom = false;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        
        if(testingroom)
        {
            exitButtonText.text = "テストを終了";
        }
        else
        {
            exitButtonText.text = "タイトルに戻る";
        }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnClickContinue()
    {
        canvas.SetActive(false);
    }
    public void OnClickExit()
    {
        StartCoroutine("Blackout");
    }

    IEnumerator Blackout()
    {

        var wait = new WaitForSeconds(Time.deltaTime);

        int count = 0;
        while (count++ < 60)
        {
            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)count) / 60.0f);
            yield return wait;
        }

        if (!testingroom)
        {
            gameState.stage = -1;
            gameState.loadingDestination = GameState.LoadingDestination.Intermission;
        }
        else
            gameState.loadingDestination = GameState.LoadingDestination.Intermission_Garage;

        SceneManager.LoadScene("Loading");
    }
}
