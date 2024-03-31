using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamRifle : Weapon
{
    GameObject beam_prefab;
    GameObject beamemit_prefab;


    private void Awake()
    {
        beam_prefab = Resources.Load<GameObject>("Beam");
        beamemit_prefab = Resources.Load<GameObject>("BeamEmit");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Fire()
    {
        GameObject beam_obj = GameObject.Instantiate(beam_prefab, gameObject.transform.position, gameObject.transform.rotation);

        Beam beam = beam_obj.GetComponent<Beam>();

        beam.direction = gameObject.transform.forward;
        beam.target = Target_Robot;

        GameObject beamemit_obj = GameObject.Instantiate(beamemit_prefab, gameObject.transform.position, gameObject.transform.rotation);
    }
}
