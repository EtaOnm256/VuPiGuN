using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WorldManager;

public class MissilePod : Weapon
{
    [SerializeField]GameObject missile_prefab;
    [SerializeField] GameObject beamemit_prefab;


    [SerializeField] private int Max_Ammo = 6;

    public override int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    [SerializeField] private int Reload_Time = 600;

    int _energy = 0;

    public List<GameObject> firePoints;
    int Duration_Time = 0;
    [SerializeField] int fire_interval = 5;
    [SerializeField] float disperse = 10.0f;
    [SerializeField] float disperse_min = 0.0f;
    [SerializeField] int bundle = 1;
    [SerializeField] int MaxMagazine = 6;
    [SerializeField] bool _forcehold = false;
    int magazine;
    private int current_cycle = 0;

    [SerializeField] GameObject[] cover_close;
    [SerializeField] GameObject[] cover_open;

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

    public override bool forceHold
    {
        get { return _forcehold; }
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

        Missile beam = missile_prefab.GetComponent<Missile>();
        projectile_speed = beam.speed;
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + (int)(10 * reloadfactor));

        if ((_energy / Reload_Time) <= 0 || magazine <= 0)
        {
            canHold = false;
        }
        else
        {

            canHold = true;
        }

        if (trigger && Duration_Time <= 30 - fire_interval)
        {
            bool fired = false;


            for (int i = 0; i < bundle; i++)
            {
                if (energy >= Reload_Time)
                {
                    if (!fired)
                    {
                        GameObject beamemit_obj = GameObject.Instantiate(beamemit_prefab, firePoints[current_cycle].transform.position, firePoints[current_cycle].transform.rotation);
                        fired = true;
                    }

                    GameObject beam_obj = GameObject.Instantiate(missile_prefab, firePoints[current_cycle].transform.position, firePoints[current_cycle].transform.rotation);
                    Missile beam = beam_obj.GetComponent<Missile>();

                    Quaternion rotate = Quaternion.AngleAxis(((float)current_cycle) / firePoints.Count * 360.0f, Vector3.forward);

                    Vector3 dir_rel = rotate * Quaternion.Euler((Random.value) * disperse + disperse_min, 0.0f, 0.0f) * Vector3.forward;


                    beam.direction = firePoints[current_cycle++].transform.rotation * dir_rel;
                    beam.target = Target_Robot;
                    //beam.transform.localScale = Vector3.one;

                    beam.team = owner.team;
                    beam.owner = owner;
                    beam.itemFlag = owner.robotParameter.itemFlag;
                    energy -= Reload_Time;


                    if (current_cycle >= firePoints.Count)
                        current_cycle = 0;
                }
                magazine--;
            }
            Duration_Time = 30;
        }

        if (Duration_Time > 0)
        {
            Duration_Time--;
        }

        foreach (var cover in cover_close)
        {
            cover.SetActive(Duration_Time <= 0);
        }
        foreach (var cover in cover_open)
        {
            cover.SetActive(Duration_Time > 0);
        }
    }

    public override void ResetCycle()
    {
        magazine = MaxMagazine;
    }
}
