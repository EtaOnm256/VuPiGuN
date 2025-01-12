using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class TitleButton : MonoBehaviour
{
    [SerializeField] GameState gameState;
    [SerializeField] Image blackout;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnClickStart()
    {
        gameState.stage = 1;

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

        SceneManager.LoadScene("Loading");
    }
}