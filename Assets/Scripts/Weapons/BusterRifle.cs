using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusterRifle : Weapon
{
    [SerializeField]GameObject beam_prefab;
    [SerializeField] GameObject beamemit_prefab = null;
    [SerializeField] GameObject barrel_origin;

    private const int Max_Ammo = 3;

    BusterBeam beam = null;

    override public int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    private const int Reload_Time = 120;
    [SerializeField] int MaxMagazine = 60;

    int magazine;

    int charge = 0;
    [SerializeField] int Charge_Max=15;
    int _energy = 0;

    [SerializeField] int TrackingLimit = 5;

    public GameObject firePoint;

    [SerializeField] float _firing_multiplier = 1.3f;
    [SerializeField] Effekseer.EffekseerEmitter effekseerEmitter;
    override public float firing_multiplier
    {
        get { return _firing_multiplier; }
    }
    override public float lockon_multiplier
    {
        get { return 1.3f; }
    }

    [SerializeField] int _fire_followthrough = 45;

    override public int fire_followthrough
    {
        get { return _fire_followthrough; }
    }

    public override bool forceHold
    {
        get { return charge > 0; }
    }
    public override bool tracking
    {
        get { return charge <= TrackingLimit; }
    }

    override public float optimal_range_max
    {
        get { return 20.0f; }
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

    [SerializeField] private bool _wrist_equipped = false;
    override public bool wrist_equipped
    {
        get { return _wrist_equipped; }
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

        GameObject beam_obj = GameObject.Instantiate(beam_prefab, firePoint.transform.position, firePoint.transform.rotation);

        beam = beam_obj.GetComponent<BusterBeam>();


        beam.team = owner.team;
        beam.owner = owner;
        beam.itemFlag = owner.robotParameter.itemFlag;
      
        beam.barrel_origin = barrel_origin.transform.position;

        beam.emitting = false;

        magazine = MaxMagazine;
    }

    bool prev_fire = false;

    bool prev_effect_play = false;

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + 1);

        if ( ((_energy / Reload_Time) <= 0 && !prev_fire) || magazine <= 0)
        {
            canHold = false;
        }
        else
        {

            canHold = true;
        }

        bool effect_play = false;

        if (trigger)
        {
            if (charge < Charge_Max)
            {
                charge++;
                prev_fire = false;
                effect_play = true;
            }
            else
            {
                if (!prev_fire)
                {
                    if (energy >= Reload_Time)
                    {
                        energy -= Reload_Time;
                        if (beamemit_prefab != null)
                            GameObject.Instantiate(beamemit_prefab, firePoint.transform.position, firePoint.transform.rotation);

                        prev_fire = true;
                    }
                }

                if (prev_fire)
                {

                    beam.direction = gameObject.transform.forward;
                    beam.transform.position = firePoint.transform.position;
                    beam.barrel_origin = barrel_origin.transform.position;
                    beam.chargeshot = chargeshot;
                    beam.emitting = true;
                    beam.target = Target_Robot;

                    magazine--;
                }
                
            }
        }
        else
        {
            beam.emitting = false;
            prev_fire = false;
            charge = 0;
        }
        effekseerEmitter.speed = 4.0f;
        if (effect_play && !prev_effect_play)
            effekseerEmitter.Play();

        //if (!effect_play && prev_effect_play)
        //    effekseerEmitter.Stop();

        prev_effect_play = effect_play;
    }

    public override void ResetCycle()
    {
        magazine = MaxMagazine;
    }

}
