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
    [SerializeField]TextMeshProUGUI summaryLabel;
    [SerializeField] TextMeshProUGUI summaryValue;
    [SerializeField] Image background;
    [SerializeField] GameState gameState;

    public bool victory;

    public int power;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        summaryValue.text = power.ToString();

        if (victory)
        {

            bigResultText.text = resultText.text = "çÏêÌê¨å˜";
            background.color = new Color(1.0f, 1.0f, 1.0f, 0.125f);
        }
        else
        {
            bigResultText.text = resultText.text = "çÏêÌé∏îs";
            background.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        }


    }

    public void GoSummaryScreen()
    {
        bigResultText.gameObject.SetActive(false);
        background.gameObject.SetActive(true);
    }

    public void OnClickProceed()
    {
        if (victory)
        {

            gameState.stage++;

            SceneManager.LoadScene($"Loading");
        }
        else
        {
            gameState.stage = -1;
            SceneManager.LoadScene($"Loading");
        }
    }
}
