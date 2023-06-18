using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSelfDelete : MonoBehaviour
{
    public Effekseer.EffekseerEmitter effekseerEmitter;
    public float speed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        effekseerEmitter.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(!effekseerEmitter.exists)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
