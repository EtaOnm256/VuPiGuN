using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfightWeapon : MonoBehaviour
{
    public struct SlashMotionInfo
    {
        public int num;
        public int[] _animID;

        public SlashMotionInfo(int _num)
        {
            num = _num;
            _animID = new int[_num];
        }
    }

    protected Dictionary<RobotController.LowerBodyState, SlashMotionInfo> _slashMotionInfo;
      

    virtual public Dictionary<RobotController.LowerBodyState, SlashMotionInfo> slashMotionInfo
    {
        get { return _slashMotionInfo; }
    }

    protected bool _emitting = false;
    protected bool _slashing = false;
    public Vector3 dir;
    public bool strong = false;
    public int damage = 250;

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

  


    // Update is called once per frame
    void Update()
    {
        
    }
}