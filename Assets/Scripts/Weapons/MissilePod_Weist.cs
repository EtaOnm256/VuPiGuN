using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissilePod_Weist : Weapon
{
    GameObject missile_prefab;
    //GameObject beamemit_prefab;


    private const int Max_Ammo = 2;

    private const int MaxEnergy = Max_Ammo* Reload_Time;
    private const int Reload_Time = 120;

    int _energy = 0;

    public List<GameObject> firePoints;

    public Weapon another;
    MissilePod_Weist anothermissilepod;

    public bool this_is_slave = false;

    private int current_cycle = 0;

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
                    canHold = false;
                }
                else
                {
                    weaponPanelItem.ammoText.color = Color.white;
                    weaponPanelItem.iconImage.color = Color.white;
                    canHold = true;
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
        missile_prefab = Resources.Load<GameObject>("Projectile/Missile Variant");
        //beamemit_prefab = Resources.Load<GameObject>("BeamEmit");

        weaponPanelItem.iconImage.sprite = Resources.Load<Sprite>("UI/BeamRifle");
    }

    // Start is called before the first frame update
    protected override void OnStart()
    {
        weaponPanelItem.ammoSlider.maxValue = MaxEnergy;
        energy = MaxEnergy;

        anothermissilepod = (MissilePod_Weist)another;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!this_is_slave)
        {

            energy = Mathf.Min(MaxEnergy, energy + 1);

            if (trigger && fire_followthrough <= 20)
            {
                if (energy >= Reload_Time)
                {
                    Vector3 pos;
                    Quaternion rot;

                    if(current_cycle++ == 0)
                    {
                        pos = firePoints[0].transform.position;
                        rot = firePoints[0].transform.rotation;
                    }
                    else
                    {
                        pos = anothermissilepod.firePoints[0].transform.position;
                        rot = anothermissilepod.firePoints[0].transform.rotation;
                    }

                    //current_cycle++;

                    if (current_cycle >= 2)
                        current_cycle = 0;


                    GameObject beam_obj = GameObject.Instantiate(missile_prefab, pos, rot);

                    Missile beam = beam_obj.GetComponent<Missile>();

                    beam.direction = gameObject.transform.forward;
                    beam.target = Target_Robot;
                    //beam.transform.localScale = Vector3.one;
                    // GameObject beamemit_obj = GameObject.Instantiate(beamemit_prefab, firePoints[0].transform.position, firePoints[0].transform.rotation);

                    energy -= Reload_Time;

                    fire_followthrough = 30;
                }
            }

            if (fire_followthrough > 0)
                fire_followthrough--;
        }
    }


}
