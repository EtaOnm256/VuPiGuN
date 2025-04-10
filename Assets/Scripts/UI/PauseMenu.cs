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
    [SerializeField] bool ending = false;

    bool finished = false;
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
        if (finished)
            return;

        StartCoroutine("Blackout");

        finished = true;
    }

    IEnumerator Blackout()
    {

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float waittime;

        if (ending)
            waittime = 1.0f;
        else
            waittime = 0.5f;

        float start = Time.time;
        while (Time.time - start < waittime)
        {
            yield return wait;
            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / waittime);
        }

        if (!testingroom)
        {
            gameState.stage = -1;
            gameState.loadingDestination = GameState.LoadingDestination.Title;
        }
        else
            gameState.loadingDestination = GameState.LoadingDestination.Intermission_Garage;

        SceneManager.LoadScene("Loading");
    }
}
