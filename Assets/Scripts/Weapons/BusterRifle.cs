using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusterRifle : Weapon
{
    [SerializeField]GameObject beam_prefab;
    [SerializeField] GameObject beamemit_prefab = null;
    [SerializeField] GameObject barrel_origin;

    private const int Max_Ammo = 180;

    BusterBeam beam = null;

    override public int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    private const int Reload_Time = 2;
    [SerializeField] int MaxMagazine = 60;

    int magazine;


    int _energy = 0;

    public GameObject firePoint;

    [SerializeField] float _firing_multiplier = 1.3f;

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
        get { return true; }
    }

    override public float aiming_angle_speed
    {
        get { return 1.0f; }
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
        beam.chargeshot = chargeshot;
        beam.barrel_origin = barrel_origin.transform.position;

        beam.emitting = false;

        magazine = MaxMagazine;
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + 1);

        if ((_energy / Reload_Time) <= 0 || magazine <= 0)
        {
            canHold = false;
        }
        else
        {

            canHold = true;
        }

        if (energy >= Reload_Time && trigger)
        {
            beam.direction = gameObject.transform.forward;
            beam.transform.position = firePoint.transform.position;
            beam.barrel_origin = barrel_origin.transform.position;
            beam.emitting = true;
            beam.target = Target_Robot;
            energy -= Reload_Time;

            if (beamemit_prefab != null)
                GameObject.Instantiate(beamemit_prefab, firePoint.transform.position, firePoint.transform.rotation);

            magazine--;
        }
        else
            beam.emitting = false;
    }

    public override void ResetCycle()
    {
        magazine = MaxMagazine;
    }

}
