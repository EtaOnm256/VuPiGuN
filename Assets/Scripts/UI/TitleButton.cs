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

    bool finished = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnClickStart()
    {
        if (finished)
            return;

        gameState.stage = 1;

        audioSource.PlayOneShot(audioClip_Start);

        StartCoroutine("Blackout");

        finished = true;
    }

    IEnumerator Blackout()
    {

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time-start < 1.0f || audioSource.isPlaying)
        {
            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 1.0f);
            yield return wait;

        }

        gameState.Reset();
        SceneManager.LoadScene("Loading");
    }
}