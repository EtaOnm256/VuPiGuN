using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBase : Pausable
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool sprint_once;
    public bool fire;
    public bool fire_forcedispatch;
    public bool down;
    public bool slash;
    public bool slash_forcedispatch;
    public bool subfire;
    public bool lockswitch;
    public RobotController.RingMenuDir ringMenuDir = RobotController.RingMenuDir.Center;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // AI�p
    public virtual void OnTakeDamage(Vector3 pos, Vector3 dir, int damage, RobotController.KnockBackType knockBackType, RobotController dealer)
    {

    }
}
