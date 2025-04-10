using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CanvasControl : MonoBehaviour
{
    public ResultCanvas resultCanvas;
    public GameObject HUDCanvas;
    public GameObject ResultCanvas;
    public GameObject PauseMenu;
    [SerializeField] Image blackout;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Blackin");

        Cursor.lockState = CursorLockMode.Locked;
    }

    IEnumerator Blackin()
    {

        blackout.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time - start < 0.5f)
        {
            yield return wait;
            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, 1.0f - ((float)fade / 0.5f));


        }
    }
}
