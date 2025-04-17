using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCannon : Weapon
{
    [SerializeField]GameObject cannonball_prefab;
    [SerializeField]GameObject beamemit_prefab;


    private const int Max_Ammo = 6;

    override public int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    private const int Reload_Time = 60;

    [SerializeField] int _fire_followthrough = 45;
    override public int fire_followthrough
    {
        get { return _fire_followthrough; }
    }

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

    override public float aiming_angle_speed
    {
        get { return 1.0f; }
    }
    protected override void OnAwake()
    {
        weaponPanelItem.iconImage.sprite = Resources.Load<Sprite>("UI/BeamRifle");
    }

    // Start is called before the first frame update
    protected override void OnStart()
    {
        weaponPanelItem.ammoSlider.maxValue = MaxEnergy;
        energy = MaxEnergy;

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

            if (trajectory == Trajectory.Curved)
            {

                CannonBall beam = beam_obj.GetComponent<CannonBall>();

                beam.direction = gameObject.transform.forward;
                beam.target = Target_Robot;
                beam.team = owner.team;
                beam.owner = owner;
                beam.chargeshot = chargeshot;

                if (beamemit_prefab)
                    GameObject.Instantiate(beamemit_prefab, firePoint.transform.position, firePoint.transform.rotation);

            }
            else
            {
                Beam beam = beam_obj.GetComponent<Beam>();

                beam.direction = gameObject.transform.forward;
                beam.target = Target_Robot;
                beam.team = owner.team;
                beam.owner = owner;
                beam.chargeshot = chargeshot;

                if (beamemit_prefab)
                    GameObject.Instantiate(beamemit_prefab, firePoint.transform.position, firePoint.transform.rotation);
            }
            energy -= Reload_Time;
        }
       }

 }
