using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayBeamCannon : Weapon
{
    GameObject beam_prefab;
    GameObject beamemit_prefab;


    private const int Max_Ammo = 6;

    override public int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    private const int Reload_Time = 150;

    int _energy = 0;

    public GameObject firePoint;

    override public float firing_multiplier
    {
        get { return 1.3f; }
    }
    override public float lockon_multiplier
    {
        get { return 1.3f; }
    }

    override public int fire_followthrough
    {
        get { return 45; }
    }

    public override int energy
    {
        set
        {
            if (_energy != value)
            {
                _energy = value;

                weaponPanelItem.ammoText.text = (_energy/ Reload_Time).ToString();
                weaponPanelItem.ammoSlider.value = _energy;

                if((_energy / Reload_Time) <= 0)
                {
                    weaponPanelItem.ammoText.color = Color.red;
                    weaponPanelItem.iconImage.color = Color.red;
                }
                else
                {
                    weaponPanelItem.ammoText.color = Color.white;
                    weaponPanelItem.iconImage.color = Color.white;
                }
            }
        }

        get
        {
            return _energy;
        }
    }

    protected override void OnAwake()
    {
        beam_prefab = Resources.Load<GameObject>("Projectile/SprayBeam");
        beamemit_prefab = Resources.Load<GameObject>("Effects/BeamEmit");

        weaponPanelItem.iconImage.sprite = Resources.Load<Sprite>("UI/BeamRifle");
    }

    // Start is called before the first frame update
    protected override void OnStart()
    {
        weaponPanelItem.ammoSlider.maxValue = MaxEnergy;
        energy = MaxEnergy;
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + 1);

        if (energy >= Reload_Time && trigger)
        {
            for (int i = 0; i < 32; i++)
            {
                GameObject beam_obj = GameObject.Instantiate(beam_prefab, firePoint.transform.position, firePoint.transform.rotation);

                SprayBeam beam = beam_obj.GetComponent<SprayBeam>();

                Vector3 dir_rel = Quaternion.Euler(Random.value * 30.0f - 15.0f, Random.value * 30.0f - 15.0f, 0.0f) * Vector3.forward;


                beam.direction = firePoint.transform.rotation * dir_rel;
                //beam.target = Target_Robot;
                beam.target = null;
                beam.team = owner.team;
                beam.owner = owner;
                beam.chargeshot = chargeshot;
            }


            GameObject beamemit_obj = GameObject.Instantiate(beamemit_prefab, firePoint.transform.position, firePoint.transform.rotation);

            energy -= Reload_Time;
        }
    }

 }
