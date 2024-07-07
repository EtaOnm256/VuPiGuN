using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamRifle : Weapon
{
    GameObject beam_prefab;
    GameObject beamemit_prefab;


    private const int Max_Ammo = 6;

    private const int MaxEnergy = Max_Ammo* Reload_Time;
    private const int Reload_Time = 150;

    int _energy = 0;

    public GameObject firePoint;

    int energy
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
        beam_prefab = Resources.Load<GameObject>("Projectile/Beam");
        beamemit_prefab = Resources.Load<GameObject>("BeamEmit");

        weaponPanelItem.iconImage.sprite = Resources.Load<Sprite>("UI/BeamRifle");
    }

    // Start is called before the first frame update
    protected override void OnStart()
    {
        weaponPanelItem.ammoSlider.maxValue = MaxEnergy;
        energy = MaxEnergy;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + 1);

        if (energy >= Reload_Time && trigger)
        {

            GameObject beam_obj = GameObject.Instantiate(beam_prefab, firePoint.transform.position, firePoint.transform.rotation);

            Beam beam = beam_obj.GetComponent<Beam>();

            beam.direction = gameObject.transform.forward;
            beam.target = Target_Robot;
            beam.team = owner.team;
            beam.worldManager = owner.worldManager;

            GameObject beamemit_obj = GameObject.Instantiate(beamemit_prefab, firePoint.transform.position, firePoint.transform.rotation);

            energy -= Reload_Time;

            fire_followthrough = 45;
        }

        if (fire_followthrough > 0)
            fire_followthrough--;
    }

 }
