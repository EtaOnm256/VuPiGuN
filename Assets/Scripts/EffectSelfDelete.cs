using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSelfDelete : Pausable
{
    public Effekseer.EffekseerEmitter effekseerEmitter = null;
    public float speed = 1.0f;
    public AudioSource audioSource = null;
    public bool ignorePause = false;
    public Animator animator = null;
    private void Awake()
    {
        WorldManager.current_instance.pausables.Add(this);

    }
    // Start is called before the first frame update
    void Start()
    {
        if(effekseerEmitter)
            effekseerEmitter.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if((!effekseerEmitter || !effekseerEmitter.exists) && (!audioSource || !audioSource.isPlaying) && (!animator || animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f))
        {
            GameObject.Destroy(gameObject);
        }
    }

    public override void OnPause()
    {
        if (effekseerEmitter && !ignorePause)
            effekseerEmitter.paused = true;
    }

    public override void OnUnpause()
    {
        if (effekseerEmitter && !ignorePause)
            effekseerEmitter.paused = false;
    }

    private void OnDestroy()
    {
        WorldManager.current_instance.pausables.Remove(this);
    }
}
