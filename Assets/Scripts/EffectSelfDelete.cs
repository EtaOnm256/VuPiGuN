using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSelfDelete : Pausable
{
    public Effekseer.EffekseerEmitter effekseerEmitter;
    public float speed = 1.0f;
    public AudioSource audioSource = null;
    private void Awake()
    {
        WorldManager.current_instance.pausables.Add(this);

    }
    // Start is called before the first frame update
    void Start()
    {
        effekseerEmitter.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(!effekseerEmitter.exists && (!audioSource || !audioSource.isPlaying))
        {
            GameObject.Destroy(gameObject);
        }
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
        WorldManager.current_instance.pausables.Remove(this);
    }
}
