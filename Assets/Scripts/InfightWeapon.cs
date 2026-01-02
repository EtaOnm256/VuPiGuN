using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfightWeapon : Pausable
{
    public struct SlashMotionInfo
    {
        public int num;
        public int[] _animID;
        public int[] damage;

        public SlashMotionInfo(int _num)
        {
            num = _num;
            _animID = new int[_num];
            damage = System.Linq.Enumerable.Repeat(100,_num).ToArray();
            
        }
    }

    protected Dictionary<RobotController.SubState_Slash, SlashMotionInfo> _slashMotionInfo;
      

    virtual public Dictionary<RobotController.SubState_Slash, SlashMotionInfo> slashMotionInfo
    {
        get { return _slashMotionInfo; }
    }

    protected bool _emitting = false;
    protected bool _slashing = false;
    public Vector3 dir;
    public RobotController.KnockBackType knockBackType;
    public int damage = 250;
    public bool autovanish = false;
    public int hitHistoryCount = 0;
    public int hitHistoryRCCount = 0;
    [System.NonSerialized] public GameObject[] hitHistory = new GameObject[64];
    [System.NonSerialized] public RobotController[] hitHistoryRC = new RobotController[16];
    public RobotController owner;

    public InfightWeapon another = null;
    public bool this_is_slave = false;

    public bool sweep = false;

    virtual public bool paired
    {
        get { return false; }
    }
    virtual public bool emitting
    {
        set
        {
            _emitting = value;

            if (!_emitting)
                _slashing = false;
        }
        get { return _emitting; }
    }


    virtual public bool slashing
    {
        set
        {
            _slashing = value;
        }
        get { return _slashing; }
    }

    virtual public bool can_dash_slash
    {
        get { return false; }
    }

    virtual public bool dashslash_cutthrough
    {
        get { return true; }
    }

    virtual public bool can_jump_slash
    {
        get { return false; }
    }
    virtual public bool dualwielded
    {
        get { return false; }
    }

    virtual public bool haslowerslash
    {
        get { return true; }
    }

    public struct MotionProperty
    { 
        public float DashSpeed;
        public int DashDuration;
        public float SlashDistance;
        public float SlashDistance_Min;
        public float RotateSpeed;
        public bool ForceProceed;
    }

    virtual public Dictionary<RobotController.SubState_Slash, MotionProperty> motionProperty
    {
        get { return _motionProperty; }
    }

    protected Dictionary<RobotController.SubState_Slash, MotionProperty> _motionProperty;

   /* virtual public float SlashDistance
    {
        get { return 6.0f; }
    }*/

    // Update is called once per frame
    void Update()
    {
        
    }
}
