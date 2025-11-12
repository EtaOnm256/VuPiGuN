using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI bigResultText;
    [SerializeField] TextMeshProUGUI summaryLabel;
    [SerializeField] RectTransform summaryLabelRectTm;
    [SerializeField] TextMeshProUGUI summaryValue;
    [SerializeField] TextMeshProUGUI summaryGold;
    [SerializeField] RectTransform summaryGoldRectTm;
    [SerializeField] Image background;
    [SerializeField] GameState gameState;
    [SerializeField] Image blackout;
    [SerializeField] GameObject proceedButton;
    [SerializeField] GameObject retryButton;
    [SerializeField] GameObject endButton;
    [SerializeField] GameObject sumLineObject;
    public bool victory;

    public int power;
    public int dealeddamage;

    public int result_gold;
    public int power_gold;
    public int dealeddamage_gold;

    public int currentgold;

    bool finished = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        

        if (victory)
        {
            summaryLabel.text = "çÏêÌåãâ " + "\n" + "écêÌóÕ" + "\n" + "ó^É_ÉÅÅ[ÉW" + "\n" + "älìæéëã‡" + "\n \n" + "çáåvéëã‡";
            summaryValue.text = "èüóò"+"\n" + power.ToString() + "\n" + dealeddamage.ToString() + "\n \n \n ";
            summaryGold.text = "$" + result_gold.ToString() + "\n" + "$" + power_gold.ToString() + "\n" + "$" + dealeddamage_gold.ToString() + "\n" + "$" + (result_gold + power_gold + dealeddamage_gold).ToString() + "\n" + "\n " + "$" + currentgold;

            bigResultText.text = resultText.text = "çÏêÌê¨å˜";
            background.color = new Color(1.0f, 1.0f, 1.0f, 0.125f);
            sumLineObject.SetActive(true);
            proceedButton.SetActive(true);
            retryButton.SetActive(false);
            endButton.SetActive(false);
        }
        else
        {
            summaryLabel.text = "çÏêÌåãâ " + "\n" + "écêÌóÕ" + "\n" + "ó^É_ÉÅÅ[ÉW" + "\n";
            summaryValue.text = "";
            summaryGold.text = "îsñk" + "\n" + power.ToString() + "\n" + dealeddamage.ToString();

            summaryLabelRectTm.sizeDelta = new Vector2(400, summaryLabelRectTm.sizeDelta.y);
            summaryGoldRectTm.sizeDelta = new Vector2(400, summaryLabelRectTm.sizeDelta.y);

            bigResultText.text = resultText.text = "çÏêÌé∏îs";
            background.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
            sumLineObject.SetActive(false);
            proceedButton.SetActive(false);
            retryButton.SetActive(true);
            endButton.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void GoSummaryScreen()
    {
        bigResultText.gameObject.SetActive(false);
        background.gameObject.SetActive(true);
    }

    public void OnClickProceed()
    {
        if (finished)
            return;

        if (victory)
        {
            gameState.destination = GameState.Destination.WorldMap;
            gameState.progress++;
            gameState.progressStage++;
            StartCoroutine("Blackout");
        }
        else
        {
            gameState.destination = GameState.Destination.Title;
            gameState.progress = -1;
            gameState.progressStage = -1;
            StartCoroutine("Blackout");
        }

        finished = true;
    }

    public void OnClickRetry()
    {
        if (finished)
            return;

        gameState.destination = GameState.Destination.Garage;
        StartCoroutine("Blackout");
  
        finished = true;
    }

    IEnumerator Blackout()
    {

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time - start < 1.0f)
        {
            yield return wait;

            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 1.0f);


        }

        switch(gameState.destination)
        {
            case GameState.Destination.WorldMap:
            if (gameState.progress <= gameState.GetMaxProgress())
                SceneManager.LoadScene("WorldMap");
            else
                SceneManager.LoadScene("Ending");
                break;
            case GameState.Destination.Garage:
                SceneManager.LoadScene("Intermission");
                break;
            case GameState.Destination.Title:
                SceneManager.LoadScene("Title");
                break;
        }
    }
}
