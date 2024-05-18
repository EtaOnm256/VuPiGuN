using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatAxe : InfightWeapon
{
    override public bool emitting
    {
        set
        {
            if (_emitting != value)
            {

                _emitting = value;

                if (_emitting)
                    material.SetFloat(powerID, 5.0f);
                else
                    material.SetFloat(powerID, 0.75f);
            }
        }
        get { return _emitting; }
    }

    Material material;
    int powerID;
    private void Awake()
    {
        material = GetComponent<MeshRenderer>().materials[1];
        powerID = Shader.PropertyToID("_Power");

        _slashMotionInfo = new Dictionary<RobotController.LowerBodyState, SlashMotionInfo>
        {
            { RobotController.LowerBodyState.GroundSlash,new SlashMotionInfo(2) },
            { RobotController.LowerBodyState.AirSlash,new SlashMotionInfo(1) },
            { RobotController.LowerBodyState.LowerSlash,new SlashMotionInfo(1) },
            { RobotController.LowerBodyState.QuickSlash,new SlashMotionInfo(1) },
            { RobotController.LowerBodyState.DashSlash,new SlashMotionInfo(1) },
        };

        foreach (var slashmotion in slashMotionInfo)
        {
            for (int i = 0; i < slashmotion.Value.num; i++)
            {
                switch (slashmotion.Key)
                {
                    case RobotController.LowerBodyState.GroundSlash:
                    case RobotController.LowerBodyState.AirSlash:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"{slashmotion.Key.ToString()}2_{i}");
                        break;
                    case RobotController.LowerBodyState.LowerSlash:
                    case RobotController.LowerBodyState.DashSlash:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"{slashmotion.Key.ToString()}_{i}");
                        break;
                    case RobotController.LowerBodyState.QuickSlash:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"GroundSlash2_{i}");
                        break;

                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        material.SetFloat(powerID, 0.75f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
