using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class TitleButton : MonoBehaviour
{
    [SerializeField] GameState gameState;
    [SerializeField] Image blackout;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip_Start;
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

        audioSource.PlayOneShot(audioClip_Start);

        StartCoroutine("Blackout");
    }

    IEnumerator Blackout()
    {

        var wait = new WaitForSeconds(Time.deltaTime);

        int count = 0;
        while (count++ < 90 || audioSource.isPlaying)
        {
            int fade = System.Math.Max(0, count);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 90.0f);
            yield return wait;
        }

        gameState.Reset();
        SceneManager.LoadScene("Loading");
    }
}