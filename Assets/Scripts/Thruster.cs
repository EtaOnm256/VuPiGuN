using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effekseer;

public class Thruster : Pausable
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
                    //effekseerEmitter.Play();
                    effekseerEmitter.Play();
                    effekseerEmitter.SendTrigger(0);
                }
                else
                {
                    //effekseerEmitter.Stop();
                    effekseerEmitter.SendTrigger(1);
                }
            }
        }
        get
        {
            return _emitting;
        }
    }

    private void Awake()
    {
        WorldManager.current_instance.effects.Add(this);

    }

    // Start is called before the first frame update
    void Start()
    {
        effekseerEmitter.speed = 2.0f;
    }

    public override void OnPause()
    {
        effekseerEmitter.paused = true;
    }

    public override void OnUnpause()
    {
        effekseerEmitter.paused = false;
    }

    private void OnDestroy()
    {
        WorldManager.current_instance.effects.Remove(this);
    }
}
