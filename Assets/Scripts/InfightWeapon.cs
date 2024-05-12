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

    Material material;
    int powerID;
    
    virtual public bool emitting
    {
        set
        {
            if (_emitting != value)
            {

                _emitting = value;

                if(_emitting)
                    material.SetFloat(powerID,5.0f);
                else
                    material.SetFloat(powerID, 0.75f);
            }
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

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().materials[1];
        powerID = Shader.PropertyToID("_Power");


    }

    // Start is called before the first frame update
    void Start()
    {
        material.SetFloat(powerID, 0.75f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
