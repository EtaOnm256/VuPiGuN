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
    [SerializeField] TextMeshProUGUI summaryGold;
    [SerializeField] Image background;
    [SerializeField] GameState gameState;
    [SerializeField] Image blackout;

    public bool victory;

    public int power;
    public int dealeddamage;

    public int power_gold;
    public int dealeddamage_gold;

    public int currentgold;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        summaryValue.text = power.ToString()+"\n"+ dealeddamage.ToString()+ "\n \n \n ";
        summaryGold.text = "$"+power_gold.ToString() + "\n" + "$" + dealeddamage_gold.ToString() + "\n" + "$" + (power_gold+ dealeddamage_gold).ToString()+  "\n" + "\n " + "$" +currentgold;

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
        if (victory)
        {

            gameState.stage++;
            StartCoroutine("Blackout");
        }
        else
        {
            gameState.stage = -1;
            StartCoroutine("Blackout");
        }
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

        gameState.loadingDestination = GameState.LoadingDestination.Intermission;

        SceneManager.LoadScene("Loading");
    }
}
