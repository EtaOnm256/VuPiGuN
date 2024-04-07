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
    // Start is called before the first frame update
    public virtual void Fire()
    {
    }

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
