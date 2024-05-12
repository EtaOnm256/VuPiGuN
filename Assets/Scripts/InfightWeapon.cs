using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfightWeapon : MonoBehaviour
{
    protected bool _emitting = false;
    protected bool _slashing = false;
    public Vector3 dir;
    public bool strong = false;
    public int damage = 250;
    virtual public bool emitting
    {
        set
        {
            _emitting = value;
        }
        get { return _emitting; }
    }

    virtual public bool slashing
    {
        set
        {
            _slashing = value;

        }
        get { return _slashing; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
