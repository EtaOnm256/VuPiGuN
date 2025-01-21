using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CanvasControl : MonoBehaviour
{
    public ResultCanvas resultCanvas;
    public GameObject HUDCanvas;
    public GameObject ResultCanvas;
    [SerializeField] Image blackout;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Blackin");
    }

    IEnumerator Blackin()
    {

        var wait = new WaitForSeconds(Time.deltaTime);

        int count = 60;
        while (count-- >= 0)
        {
            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)count) / 60.0f);
            yield return wait;
        }
    }
}
