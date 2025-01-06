using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotVariant : MonoBehaviour
{
    public GameObject robot_prefab = null;
    public GameObject rweapon_prefab = null;
    public GameObject lweapon_prefab = null;
    public GameObject subweapon_prefab = null;
    public bool weapon_chest_paired = false;
    public bool carrying_weapon = false;
    public bool dualwield_lightweapon = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ArmWeapon(RobotController robotController, WorldManager worldManager)
    {
        robotController.carrying_weapon = carrying_weapon;
        robotController.dualwield_lightweapon = dualwield_lightweapon;

        if (rweapon_prefab != null)
        {
            GameObject playerrweapon = GameObject.Instantiate(rweapon_prefab);

            playerrweapon.transform.parent = robotController.RHand.transform;
            playerrweapon.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
            playerrweapon.transform.localEulerAngles = new Vector3(-90, 0, 180);
            playerrweapon.transform.localScale = new Vector3(1, 1, 1);

            robotController.rightWeapon = playerrweapon.GetComponent<Weapon>();
        }

        if (lweapon_prefab != null)
        {
            GameObject playerlweapon = GameObject.Instantiate(lweapon_prefab);

            playerlweapon.transform.parent = robotController.LHand.transform;
            playerlweapon.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
            playerlweapon.transform.localEulerAngles = new Vector3(-90, 0, 180);
            playerlweapon.transform.localScale = new Vector3(1, 1, 1);

            robotController.Sword = playerlweapon.GetComponent<InfightWeapon>();
        }

        if (subweapon_prefab != null)
        {
            if (weapon_chest_paired)
            {
                GameObject playersubweapon_r = GameObject.Instantiate(subweapon_prefab);

                playersubweapon_r.transform.parent = robotController.chestWeapon_anchor[1].transform;
                playersubweapon_r.transform.localPosition = Vector3.zero;
                playersubweapon_r.transform.localEulerAngles = Vector3.zero;
                playersubweapon_r.transform.localScale = Vector3.one;

                playersubweapon_r.GetComponent<Weapon>().this_is_slave = true;
                
                GameObject playersubweapon_l = GameObject.Instantiate(subweapon_prefab);

                playersubweapon_l.transform.parent = robotController.chestWeapon_anchor[0].transform;
                playersubweapon_l.transform.localPosition = Vector3.zero;
                playersubweapon_l.transform.localEulerAngles = Vector3.zero;
                playersubweapon_l.transform.localScale = Vector3.one;
                playersubweapon_l.GetComponent<Weapon>().this_is_slave = false;
                playersubweapon_l.GetComponent<Weapon>().another = playersubweapon_r.GetComponent<Weapon>();

                robotController.shoulderWeapon = playersubweapon_l.GetComponent<Weapon>();


            }
            else
            {
                GameObject playersubweapon_l = GameObject.Instantiate(subweapon_prefab);

                playersubweapon_l.transform.parent = robotController.chestWeapon_anchor[0].transform;
                playersubweapon_l.transform.localPosition = Vector3.zero;
                playersubweapon_l.transform.localEulerAngles = Vector3.zero;
                playersubweapon_l.transform.localScale = Vector3.one;

                robotController.shoulderWeapon = playersubweapon_l.GetComponent<Weapon>();
            }

        }
    }

    public RobotController Spawn(Vector3 pos, Quaternion rot,WorldManager worldManager)
    {
      

        GameObject robot = GameObject.Instantiate(robot_prefab, pos, rot);

        RobotController robotController = robot.GetComponent<RobotController>();

        ArmWeapon(robotController, worldManager);

        return robotController;
    }
}
