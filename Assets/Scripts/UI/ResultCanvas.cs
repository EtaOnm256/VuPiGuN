using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultText;
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
            resultText.text = "��퐬��";
            background.color = new Color(1.0f, 1.0f, 1.0f, 0.125f);
        }
        else
        {
            resultText.text = "��편�s";
            background.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        }


    }

    public void OnClickProceed()
    {
        if (victory)
        {

            gameState.stage++;

            if (gameState.stage >= 1 && gameState.stage <= 6)
            {
                SceneManager.LoadScene($"Stage{gameState.stage}");
            }
            else
            {
                SceneManager.LoadScene($"Title");
            }
        }
        else
        {
            SceneManager.LoadScene($"Title");
        }
    }
}
