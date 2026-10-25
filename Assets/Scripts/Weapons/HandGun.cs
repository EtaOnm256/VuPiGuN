using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGun : Weapon
{
    [SerializeField]GameObject bullet_prefab;
    [SerializeField] bool beam = false;
    [SerializeField] GameObject solidemit_prefab;


    [SerializeField] int Max_Ammo = 30;

    override public ShotType shottype
    {
        get { return ShotType.RAPID; }
    }

    public override bool dualwielded => true;

    override public int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    [SerializeField] int Reload_Time = 12;

    int _energy = 0;

    public GameObject firePoint;

    [SerializeField] int MaxMagazine = 4;

    int magazine;

    int Duration_Time = 0;

    [SerializeField] int fire_interval = 10;

    [SerializeField] int _fire_followthrough = 30;
    [SerializeField] GameObject barrel_origin;

    HandGun anothergun;
    private int current_cycle = 0;

    override public int fire_followthrough
    {
        get { return _fire_followthrough; }
    }

    [SerializeField] float _optimal_range_max = 20.0f;

    override public float optimal_range_max => _optimal_range_max;

    [SerializeField] float _optimal_range_min = float.MinValue;

    override public float optimal_range_min => _optimal_range_min;

    [SerializeField] float _firing_angle = 60.0f;

    public override float firing_angle => _firing_angle;

    override public int energy
    {
        set
        {
            if (_energy != value)
            {
                _energy = value;

                weaponPanelItem.ammoText.text = (_energy / Reload_Time).ToString();
                weaponPanelItem.ammoSlider.value = _energy;

                if ((_energy / Reload_Time) <= 0)
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

    [SerializeField] float _firing_multiplier = 2.0f;

    override public float firing_multiplier
    {
        get { return _firing_multiplier; }
    }
    override public float lockon_multiplier
    {
        get { return 3.0f; }
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
        magazine = MaxMagazine;

        anothergun = (HandGun)another;

        if (beam)
        {
            Beam bullet = bullet_prefab.GetComponent<Beam>();
            projectile_speed = bullet.speed / Time.deltaTime;

        }
        else
        {
            Bullet bullet = bullet_prefab.GetComponent<Bullet>();
            projectile_speed = bullet.speed / Time.deltaTime;
        }
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

        if ((shotModifier == ShotModifier.BURST3 || energy >= Reload_Time) && trigger && Duration_Time <= 30- fire_interval)
        {

            Vector3 pos;
            Quaternion rot;
            Vector3 barrelorigin_pos;

            if (current_cycle++ == 0)
            {
                pos = firePoint.transform.position;
                rot = firePoint.transform.rotation;
                barrelorigin_pos = barrel_origin.transform.position;
            }
            else
            {
                pos = anothergun.firePoint.transform.position;
                rot = anothergun.firePoint.transform.rotation;
                barrelorigin_pos = anothergun.barrel_origin.transform.position;
            }

          

            GameObject bullet_obj = GameObject.Instantiate(bullet_prefab, pos, rot);

            if (beam)
            {
                Beam bullet = bullet_obj.GetComponent<Beam>();

                bullet.direction = gameObject.transform.forward;
                bullet.target = Target_Robot;
                bullet.team = owner.team;
                //bullet.worldManager = owner.worldManager;
                bullet.owner = owner;
                bullet.itemFlag = owner.robotParameter.itemFlag;
                bullet.shotModifier = shotModifier;
                bullet.barrel_origin = barrelorigin_pos;
                GameObject solidemit_obj = GameObject.Instantiate(solidemit_prefab, pos, rot);

                solidemit_obj.transform.localScale = Vector3.one / 2.0f;
            }
            else
            {
                Bullet bullet = bullet_obj.GetComponent<Bullet>();

                bullet.direction = gameObject.transform.forward;
                bullet.target = Target_Robot;
                bullet.team = owner.team;
                //bullet.worldManager = owner.worldManager;
                bullet.owner = owner;
                bullet.itemFlag = owner.robotParameter.itemFlag;
                bullet.shotModifier = shotModifier;
                bullet.barrel_origin = barrel_origin.transform.position;
                GameObject solidemit_obj = GameObject.Instantiate(solidemit_prefab, pos, rot);
            }

            if (current_cycle >= 2)
                current_cycle = 0;

            if (shotModifier != ShotModifier.BURST3)
                energy -= Reload_Time;

            Duration_Time = 30;

            magazine--;
        }

        if (Duration_Time > 0)
            Duration_Time--;
    }

    public override void ResetCycle()
    {
        magazine = MaxMagazine;
    }

}
