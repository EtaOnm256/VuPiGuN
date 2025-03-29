using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMG : Weapon
{
    GameObject bullet_prefab;
    GameObject solidemit_prefab;


    private const int Max_Ammo = 30;

    override public int MaxEnergy
    {
        get
        {
            return Max_Ammo * Reload_Time;
        }
    }
    private const int Reload_Time = 12;

    int _energy = 0;

    public GameObject firePoint;

    const int MaxMagazine = 5;

    int magazine = MaxMagazine;

    int Duration_Time = 0;

    override public int fire_followthrough
    {
        get { return 30; }
    }

    //public override bool dualwielded
    //{
    //    get { return true; }
    //}

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

    override public float firing_multiplier
    {
        get { return 2.0f; }
    }
    override public float lockon_multiplier
    {
        get { return 3.0f; }
    }
    protected override void OnAwake()
    {
        bullet_prefab = Resources.Load<GameObject>("Projectile/SMGBullet");
        solidemit_prefab = Resources.Load<GameObject>("Effects/SMGEmit");

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

        if ((_energy / Reload_Time) <= 0 || magazine <= 0)
        {
            canHold = false;
        }
        else
        {
         
            canHold = true;
        }

        if (energy >= Reload_Time && trigger && Duration_Time <= 25)
        {

            GameObject bullet_obj = GameObject.Instantiate(bullet_prefab, firePoint.transform.position, firePoint.transform.rotation);

            SMGBullet bullet = bullet_obj.GetComponent<SMGBullet>();

            bullet.direction = gameObject.transform.forward;
            bullet.target = Target_Robot;
            bullet.team = owner.team;
            //bullet.worldManager = owner.worldManager;
            bullet.owner = owner;
            bullet.chargeshot = chargeshot;
            GameObject solidemit_obj = GameObject.Instantiate(solidemit_prefab, firePoint.transform.position, firePoint.transform.rotation);

            solidemit_obj.transform.localScale = Vector3.one/2.0f;

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
