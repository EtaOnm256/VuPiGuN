using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TitleButton : MonoBehaviour
{
    [SerializeField] GameState gameState;

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

        SceneManager.LoadScene("Stage1");
    }
}
