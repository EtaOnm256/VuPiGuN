using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public RobotController Target_Robot;
    public UIController_Overlay uIController_Overlay = null;
    public GameObject weaponPanelItemObj = null;
    public GameObject weaponPanelItem_prefab = null;
    public WeaponPanelItem weaponPanelItem = null;

    private bool _trigger;

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

    private bool _canHold = false;

    protected int fire_followthrough = 0;

    public bool followthrough_now
    {
        get { return fire_followthrough != 0; }
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
}
