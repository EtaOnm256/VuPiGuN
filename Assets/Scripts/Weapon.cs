using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public RobotController owner;
    public RobotController Target_Robot;
    public UIController_Overlay uIController_Overlay = null;
    public GameObject weaponPanelItemObj = null;
    public GameObject weaponPanelItem_prefab = null;
    public WeaponPanelItem weaponPanelItem = null;

    public Weapon another = null;
    public bool this_is_slave = false;
    private bool _trigger;

    public enum Trajectory
    {
        Straight,
        Curved
    }

    public Trajectory trajectory = Trajectory.Straight;
    public float projectile_gravity;
    public float projectile_speed;

    public bool trigger
    {
        get { return _trigger; }
        set { _trigger = value; }
    }

    public bool canHold
    {
        get { return _canHold; }
        protected set { _canHold = value; }
    }

    virtual public bool heavy
    {
        get { return false; }
    }

    virtual public float firing_multiplier
    {
        get { return 1.0f; }
    }

    virtual public float lockon_multiplier
    {
        get { return 1.0f; }
    }

    //virtual public bool dualwielded
    //{
    //    get { return false; }
    //}

    private bool _canHold = false;

    virtual public int fire_followthrough
    {
        get { return 45; }
    }

    //public bool followthrough_now
    //{
    //    get { return fire_followthrough != 0; }
    //}

    virtual public int MaxEnergy

    {
        set
        {

        }

        get
        {
            return 0;
        }
    }
 
    virtual public int energy
    {
        set
        {
            
        }

        get
        {
            return 0;
        }
    }

    // Start is called before the first frame update

    private void Awake()
    {
        weaponPanelItem_prefab = Resources.Load<GameObject>("UI/WeaponPanelItem");
        weaponPanelItemObj = Instantiate(weaponPanelItem_prefab);
        weaponPanelItemObj.SetActive(false);
        weaponPanelItem = weaponPanelItemObj.GetComponent<WeaponPanelItem>();
   

        OnAwake();
    }

    private void Start()
    {
        if (uIController_Overlay != null)
        {
            weaponPanelItemObj.SetActive(true);
            
            weaponPanelItemObj.transform.parent = uIController_Overlay.weaponPanel.transform;
            weaponPanelItemObj.transform.localScale = Vector3.one;
        }

        OnStart();

    }

    protected virtual void OnAwake() { }

    protected virtual void OnStart() { }

    public virtual void ResetCycle() { }

    public virtual void OnKnockback() { }

    public virtual void OnDestroy_Called_By_Unit() { }
}
