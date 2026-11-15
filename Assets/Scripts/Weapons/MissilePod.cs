using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WorldManager;

public class MissilePod : Weapon
{
    [SerializeField]GameObject missile_prefab;
    GameObject beamemit_prefab;


    private const int Max_Ammo = 6;

    public override int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    private const int Reload_Time = 600;

    int _energy = 0;

    public List<GameObject> firePoints;
    int Duration_Time = 0;
    [SerializeField] int fire_interval = 5;
    [SerializeField] float disperse = 10.0f;
    private int current_cycle = 0;

    override public int fire_followthrough
    {
        get { return 30; }
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
        beamemit_prefab = Resources.Load<GameObject>("Effects/MissileEmit");

        weaponPanelItem.iconImage.sprite = Resources.Load<Sprite>("UI/BeamRifle");
    }

    // Start is called before the first frame update
    protected override void OnStart()
    {
        weaponPanelItem.ammoSlider.maxValue = MaxEnergy;
        energy = MaxEnergy;

        Missile beam = missile_prefab.GetComponent<Missile>();
        projectile_speed = beam.speed;
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + (int)(10 * reloadfactor));

        if(trigger && Duration_Time <= 0)
        {
            if (energy >= Reload_Time)
            {

                GameObject beam_obj = GameObject.Instantiate(missile_prefab, firePoints[0].transform.position, firePoints[0].transform.rotation);

                Missile beam = beam_obj.GetComponent<Missile>();
                Vector3 dir_rel = Quaternion.Euler((Random.value * 2.0f-1.0f)*disperse, (Random.value * 2.0f - 1.0f)*disperse, 0.0f) * Vector3.forward;

                if (current_cycle >= firePoints.Count)
                    current_cycle = 0;

                beam.direction = firePoints[current_cycle++].transform.rotation * dir_rel;
                beam.target = Target_Robot;
                //beam.transform.localScale = Vector3.one;
                 GameObject beamemit_obj = GameObject.Instantiate(beamemit_prefab, firePoints[0].transform.position, firePoints[0].transform.rotation);
                beam.team = owner.team;
                beam.owner = owner;
                beam.itemFlag = owner.robotParameter.itemFlag;
                energy -= Reload_Time;

                Duration_Time = fire_interval;
            }
        }

        if(Duration_Time > 0)
            Duration_Time--;
    }


}
