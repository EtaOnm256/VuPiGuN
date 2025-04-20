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
        summaryValue.text = "\n"+power.ToString()+"\n"+ dealeddamage.ToString()+ "\n \n \n ";
        summaryGold.text = "$" + result_gold.ToString() + "\n"+"$" +power_gold.ToString() + "\n" + "$" + dealeddamage_gold.ToString() + "\n" + "$" + (power_gold+ dealeddamage_gold).ToString()+  "\n" + "\n " + "$" +currentgold;

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
        if (finished)
            return;

        if (victory)
        {
            gameState.loadingDestination = GameState.LoadingDestination.Intermission;
            gameState.stage++;
            StartCoroutine("Blackout");
        }
        else
        {
            gameState.loadingDestination = GameState.LoadingDestination.Title;
            gameState.stage = -1;
            StartCoroutine("Blackout");
        }

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


        SceneManager.LoadScene("Loading");
    }
}
