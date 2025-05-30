using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolidRifle : Weapon
{
    [SerializeField]GameObject bullet_prefab;
    GameObject solidemit_prefab;
    [SerializeField] GameObject barrel_origin;

    private const int Max_Ammo = 6;

    public override int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }

    private const int Reload_Time = 60;

    int _energy = 0;

    public GameObject firePoint;
    //public override bool dualwielded
    //{
    //    get { return true; }
    //}

    override public float firing_multiplier
    {
        get { return 1.15f; }
    }
    override public float lockon_multiplier
    {
        get { return 1.15f; }
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

    protected override void OnAwake()
    {
        solidemit_prefab = Resources.Load<GameObject>("Effects/SolidEmit");

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

            GameObject bullet_obj = GameObject.Instantiate(bullet_prefab, firePoint.transform.position, firePoint.transform.rotation);

            Bullet bullet = bullet_obj.GetComponent<Bullet>();

            bullet.direction = gameObject.transform.forward;
            bullet.target = Target_Robot;
            bullet.team = owner.team;
            //bullet.worldManager = owner.worldManager;
            bullet.owner = owner;
            bullet.itemFlag = owner.robotParameter.itemFlag;
            bullet.chargeshot = chargeshot;
            bullet.barrel_origin = barrel_origin.transform.position;
            GameObject solidemit_obj = GameObject.Instantiate(solidemit_prefab, firePoint.transform.position, firePoint.transform.rotation);

            solidemit_obj.transform.localScale = Vector3.one;

            energy -= Reload_Time;
        }
    }

 }
