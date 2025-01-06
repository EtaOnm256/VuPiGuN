using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pausable : MonoBehaviour
{
    //public WorldManager worldManager;

    void FixedUpdate()
    {
        OnFixedUpdateForce();

        if(!WorldManager.current_instance.pausing)
           OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {

    }

    protected virtual void OnFixedUpdateForce()
    {

    }

    public virtual void OnPause()
    {

    }

    public virtual void OnUnpause()
    {

    }
}
