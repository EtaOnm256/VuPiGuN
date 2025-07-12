using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamRifle : Weapon
{
    [SerializeField]GameObject beam_prefab;
    [SerializeField] GameObject beamemit_prefab = null;
    [SerializeField] GameObject barrel_origin;

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
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + 1);

        if (energy >= Reload_Time && trigger)
        {

            GameObject beam_obj = GameObject.Instantiate(beam_prefab, firePoint.transform.position, firePoint.transform.rotation);

            Beam beam = beam_obj.GetComponent<Beam>();

            beam.direction = gameObject.transform.forward;
            beam.target = Target_Robot;
            beam.team = owner.team;
            beam.owner = owner;
            beam.itemFlag = owner.robotParameter.itemFlag;
            beam.chargeshot = chargeshot;
            beam.barrel_origin = barrel_origin.transform.position;

            if(beamemit_prefab!=null)
                GameObject.Instantiate(beamemit_prefab, firePoint.transform.position, firePoint.transform.rotation);

            energy -= Reload_Time;
        }
    }

 }
