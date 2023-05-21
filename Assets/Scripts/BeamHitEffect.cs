using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamHitEffect : MonoBehaviour
{
    int MaxTime = 60;

    int time;

    float initialscale;

    // Start is called before the first frame update
    void Start()
    {
        initialscale = transform.localScale.x;
        time = MaxTime;
    }

    // Update is called once per frame
    void Update()
    {
        float scale = initialscale * (time) / MaxTime;

        transform.localScale = new Vector3(scale, scale, scale);

        time--;

        if(time==0)
        {
            GameObject.Destroy(gameObject);
        }    
    }
}
