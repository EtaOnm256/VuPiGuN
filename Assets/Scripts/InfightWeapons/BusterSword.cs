using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BusterSword : InfightWeapon
{
    override public bool slashing
    {
        set
        {
            _slashing = value;

            if (_slashing)
                _emitting = true;

            if (!_slashing)
            {
                hitHistoryCount = 0;
                hitHistoryRCCount = 0;

                System.Array.Fill(hitHistory, null);
                System.Array.Fill(hitHistoryRC, null);
            }

        }
        get { return _slashing; }
    }

    override public bool emitting
    {
        set
        {
            if (_emitting != value)
            {

                _emitting = value;

                if (!_emitting)
                    _slashing = false;

                meshRenderer.enabled = _emitting;
            }
        }
        get { return _emitting; }
    }

    override public bool can_dash_slash
    {
        get { return true; }
    }

    override public bool can_jump_slash
    {
        get { return true; }
    }

    //Material material;
    //int powerID;
    MeshRenderer meshRenderer;

    private void Awake()
    {
        //material = GetComponent<MeshRenderer>().materials[1];
        meshRenderer = GetComponent<MeshRenderer>();
        //powerID = Shader.PropertyToID("_Power");

        _slashMotionInfo = new Dictionary<RobotController.SubState_Slash, SlashMotionInfo>
        {
            { RobotController.SubState_Slash.GroundSlash,new SlashMotionInfo(2) },
            { RobotController.SubState_Slash.AirSlash,new SlashMotionInfo(1) },
            { RobotController.SubState_Slash.LowerSlash,new SlashMotionInfo(1) },
            { RobotController.SubState_Slash.QuickSlash,new SlashMotionInfo(2) },
            { RobotController.SubState_Slash.DashSlash,new SlashMotionInfo(1) },
               { RobotController.SubState_Slash.JumpSlash,new SlashMotionInfo(1) },
            { RobotController.SubState_Slash.JumpSlash_Jump,new SlashMotionInfo(1) },
            { RobotController.SubState_Slash.JumpSlash_Ground,new SlashMotionInfo(1) },
        };

        _motionProperty = new Dictionary<RobotController.SubState_Slash, MotionProperty>
            {
                { RobotController.SubState_Slash.GroundSlash,new MotionProperty{DashSpeed = 25.0f ,DashDuration = 45 ,SlashDistance=7.0f,RotateSpeed=4.0f,SlashDistance_Min = float.MinValue} },
                { RobotController.SubState_Slash.AirSlash,new MotionProperty{DashSpeed = 37.5f ,DashDuration = 45/2,SlashDistance=7.0f,RotateSpeed=4.0f,SlashDistance_Min = float.MinValue } },
              { RobotController.SubState_Slash.LowerSlash,new MotionProperty{DashSpeed = 25.0f ,DashDuration = 45 ,SlashDistance=7.0f,RotateSpeed=4.0f,SlashDistance_Min = float.MinValue} },
                 { RobotController.SubState_Slash.QuickSlash,new MotionProperty{DashSpeed = 37.5f ,DashDuration = 45/2 ,SlashDistance=7.0f,RotateSpeed=4.0f,SlashDistance_Min = float.MinValue} },
                 { RobotController.SubState_Slash.AirSlashSeed,new MotionProperty{DashSpeed = 37.5f ,DashDuration = 30 ,SlashDistance=7.0f,RotateSpeed=4.0f,SlashDistance_Min = float.MinValue} },
                 { RobotController.SubState_Slash.SlideSlashSeed,new MotionProperty{DashSpeed = 37.5f ,DashDuration = 30 ,SlashDistance=7.0f,RotateSpeed=4.0f,SlashDistance_Min = float.MinValue} },
                 { RobotController.SubState_Slash.RollingSlash,new MotionProperty{DashSpeed = 25.0f ,DashDuration = 45/2 ,SlashDistance=7.0f,RotateSpeed=4.0f,SlashDistance_Min = float.MinValue,AlwaysForward=true,ForceProceed=true} },
                 { RobotController.SubState_Slash.DashSlash,new MotionProperty{DashSpeed = 72.0f,DashDuration = 45,SlashDistance=7.0f,RotateSpeed=0.75f,SlashDistance_Min = float.MinValue } },
                  { RobotController.SubState_Slash.JumpSlash_Jump,new MotionProperty{DashSpeed = 60.0f ,DashDuration = 20, SlashDistance=7.0f,RotateSpeed=6.0f,SlashDistance_Min = 5.0f} },
            };


        foreach (var slashmotion in slashMotionInfo)
        {
            for (int i = 0; i < slashmotion.Value.num; i++)
            {
                switch (slashmotion.Key)
                {
                    case RobotController.SubState_Slash.GroundSlash:
                    case RobotController.SubState_Slash.AirSlash:
                    case RobotController.SubState_Slash.QuickSlash:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"{slashmotion.Key.ToString()}2_{i}");
                        break;
                    case RobotController.SubState_Slash.LowerSlash:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"{slashmotion.Key.ToString()}_{i}");

                        break;
                    case RobotController.SubState_Slash.JumpSlash:
                    case RobotController.SubState_Slash.JumpSlash_Jump:
                    case RobotController.SubState_Slash.JumpSlash_Ground:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"{slashmotion.Key.ToString()}2_{i}");
                        slashmotion.Value.damage[i] = 200;
                        break;
                    default:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"{slashmotion.Key.ToString()}_{i}");

                        if (slashmotion.Key == RobotController.SubState_Slash.GroundSlash
                            || slashmotion.Key == RobotController.SubState_Slash.AirSlashSeed)
                        {
                            if (i == slashmotion.Value.num - 1)
                                slashmotion.Value.damage[i] = 150;
                        }

                        if (slashmotion.Key == RobotController.SubState_Slash.RollingSlash)
                            slashmotion.Value.damage[i] = 50;

                        if (slashmotion.Key == RobotController.SubState_Slash.LowerSlash)
                        {
                            slashmotion.Value.damage[i] = 150;
                        }

                        break;

                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //material.SetFloat(powerID, 0.75f);


        if (autovanish && !_emitting)
        {
            meshRenderer.enabled = _emitting;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    const int num_points = 3;

    Vector3[] prev_points = new Vector3[num_points];
    Vector3[] points = new Vector3[num_points];

    //public GameObject hitpoint;
    public GameObject[] hitpoints;

    RaycastHit[] rayCastHit = new RaycastHit[8];

    public GameObject hitEffect_prefab;


    protected override void OnFixedUpdate()
    {

        for (int i = 0; i < num_points; i++)
        {
            //points[i] = hitpoint.transform.position;
            points[i] = hitpoints[i].transform.position;
        }



        if (slashing)
        {
            for (int idx_point = 0; idx_point < num_points; idx_point++)
            {
                EvalHit(points[idx_point], prev_points[idx_point]);
            }
        }

        points.CopyTo(prev_points, 0);
    }

    private void EvalHit(Vector3 p1, Vector3 p2)
    {
        Ray ray = new Ray(p1, p2 - p1);

        float length = (p2 - p1).magnitude;

        int numhit = Physics.RaycastNonAlloc(ray, rayCastHit, length, 1 << 6);

        for (int idx_hit = 0; idx_hit < numhit; idx_hit++)
        {
            if (hitHistory.Contains(rayCastHit[idx_hit].collider.gameObject))
                continue;

            hitHistory[hitHistoryCount++] = rayCastHit[idx_hit].collider.gameObject;



            RobotController robotController = rayCastHit[idx_hit].collider.gameObject.GetComponentInParent<RobotController>();

            if (robotController != null)
            {
                if (robotController.team == owner.team)
                    continue;

                if (!robotController.has_hitbox)
                    continue;

                if (hitHistoryRC.Contains(robotController))
                    continue;

                hitHistoryRC[hitHistoryRCCount++] = robotController;

                robotController.TakeDamage(rayCastHit[idx_hit].point,dir, damage, knockBackType, owner) ;
                if (knockBackType == RobotController.KnockBackType.Finish || knockBackType == RobotController.KnockBackType.KnockUp)
                {
                    robotController.DoHitSlow(10);
                    owner.DoHitSlow(10);
                }
                else
                {
                    robotController.DoHitSlow(5);
                    owner.DoHitSlow(5);
                }

                GameObject.Instantiate(hitEffect_prefab, rayCastHit[idx_hit].point, Quaternion.identity);
            }


        }
    }
}
