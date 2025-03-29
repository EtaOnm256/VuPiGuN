using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCannon : Weapon
{
    GameObject cannonball_prefab;
    GameObject beamemit_prefab;


    private const int Max_Ammo = 6;

    override public int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    private const int Reload_Time = 60;

    int _energy = 0;

    public GameObject firePoint;

    override public bool heavy
    {
        get { return true; }
    }

    override public int energy
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
        cannonball_prefab = Resources.Load<GameObject>("Projectile/CannonBall Variant");
        beamemit_prefab = Resources.Load<GameObject>("Effects/CannonEmit");

        weaponPanelItem.iconImage.sprite = Resources.Load<Sprite>("UI/BeamRifle");
    }

    // Start is called before the first frame update
    protected override void OnStart()
    {
        weaponPanelItem.ammoSlider.maxValue = MaxEnergy;
        energy = MaxEnergy;

        trajectory = Trajectory.Curved;
        projectile_gravity = -CannonBall.Gravity / Time.deltaTime;
        projectile_speed = CannonBall.Speed / Time.deltaTime;
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + 1);

        if (energy >= Reload_Time && trigger)
        {

            GameObject beam_obj = GameObject.Instantiate(cannonball_prefab, firePoint.transform.position, firePoint.transform.rotation);

            CannonBall beam = beam_obj.GetComponent<CannonBall>();

            beam.direction = gameObject.transform.forward;
            beam.target = Target_Robot;
            beam.team = owner.team;
            //beam.worldManager = owner.worldManager;
            GameObject beamemit_obj = GameObject.Instantiate(beamemit_prefab, firePoint.transform.position, firePoint.transform.rotation);
            beam.owner = owner;
            beam.chargeshot = chargeshot;
            energy -= Reload_Time;
        }
       }

 }
