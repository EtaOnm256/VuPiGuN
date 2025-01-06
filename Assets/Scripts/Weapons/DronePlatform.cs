using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePlatform : Weapon
{
    GameObject drone_prefab = null;
    public List<GameObject> drone_anchors;

    List<GameObject> drone_objs = new List<GameObject>();
    List<Drone> drones = new List<Drone>();
    private int current_cycle = 0;

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

    public List<GameObject> firePoints;
    int Duration_Time = 0;

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

    override public bool allrange
    {
        set
        {

        }

        get
        {
            return true;
        }
    }

    protected override void OnAwake()
    {
        drone_prefab = Resources.Load<GameObject>("Drone Variant");

        weaponPanelItem.iconImage.sprite = Resources.Load<Sprite>("UI/BeamRifle");
    }

    // Start is called before the first frame update
    protected override void OnStart()
    {
        weaponPanelItem.ammoSlider.maxValue = MaxEnergy;
        energy = MaxEnergy;

        foreach(var drone_anchor in drone_anchors)
        {
            GameObject drone_obj = GameObject.Instantiate(drone_prefab, drone_anchor.transform.position, drone_anchor.transform.rotation, drone_anchor.transform);

          

            Drone drone = drone_obj.GetComponent<Drone>();
            drone.anchor = drone_anchor;
            //drone.worldManager = worldManager;
            //drone.owner = owner; //Ç±Ç±ÇæÇ∆èáèòÇÃä÷åWÇ≈ê›íËÇ≥ÇÍÇ»Ç¢Ç±Ç∆Ç™Ç†ÇÈ
            drones.Add(drone);

            drone_obj.transform.localPosition = drone.offset;

            drone_objs.Add(drone_obj);

        }
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        energy = Mathf.Min(MaxEnergy, energy + 1);

     

        if(trigger && Duration_Time <= 0)
        {
            if (energy >= Reload_Time)
            {
                int search_duration = 0;
                bool found = false;

                while (true)
                {
                    if(drones[current_cycle].state == Drone.State.Ready)
                    {
                        found = true;
                        break;
                    }

                    current_cycle++;

                    if (current_cycle >= drones.Count)
                        current_cycle = 0;

                    search_duration++;

                    if (search_duration >= drones.Count)
                    {
                        break;
                    }
                }

                if (found)
                {
                    drones[current_cycle].owner = owner; //StartÇæÇ∆èáèòÇÃä÷åWÇ≈ê›íËÇ≥ÇÍÇƒÇ¢Ç»Ç¢Ç±Ç∆Ç™Ç†ÇÈÇÃÇ≈
                    drones[current_cycle].target = Target_Robot;
                    energy -= Reload_Time;

                    Duration_Time = 10;
                }
            }
        }

        if(Duration_Time > 0)
            Duration_Time--;
    }

    public override void OnKnockback()
    {
        foreach (var drone in drones)
        {
            if(drone.state == Drone.State.Going || drone.state == Drone.State.Firing)
            {
                drone.state = Drone.State.Homing;
            }
        }
    }

    protected override void OnDestroy_Called_By_Unit()
    {
        foreach (var drone_obj in drone_objs)
        {
            GameObject.Destroy(drone_obj);
        }
    }
}
