using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effekseer;

public class Thruster : MonoBehaviour
{
    public EffekseerEmitter effekseerEmitter;

    bool _emitting;

    public bool emitting
    {
        set
        {
            bool prev = _emitting;

            _emitting = value;

            if (prev != _emitting)
            {
                if(emitting)
                {
                    effekseerEmitter.Play();
                }
                else
                {
                    effekseerEmitter.Stop();
                }
            }
        }
        get
        {
            return _emitting;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        effekseerEmitter.speed = 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
