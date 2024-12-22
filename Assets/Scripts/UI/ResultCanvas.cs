using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField]TextMeshProUGUI summaryLabel;
    [SerializeField] TextMeshProUGUI summaryValue;
    [SerializeField] Image background;

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
            resultText.text = "çÏêÌê¨å˜";
            background.color = new Color(1.0f, 1.0f, 1.0f, 0.125f);
        }
        else
        {
            resultText.text = "çÏêÌé∏îs";
            background.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        }


    }
}
