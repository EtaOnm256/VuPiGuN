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
    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
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
        gameState.stage = -1;
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

        gameState.intermission = true;

        SceneManager.LoadScene("Loading");
    }
}
