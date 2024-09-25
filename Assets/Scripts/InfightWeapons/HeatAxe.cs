using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class HeatAxe : InfightWeapon
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

                if (_emitting)
                    material.SetFloat(powerID, 5.0f);
                else
                    material.SetFloat(powerID, 0.75f);

                if(autovanish)
                {
                    meshRenderer.enabled = _emitting;
                }
            }
        }
        get { return _emitting; }
    }

   

    Material material;
    int powerID;
    MeshRenderer meshRenderer;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().materials[1];
        meshRenderer = GetComponent<MeshRenderer>();
        powerID = Shader.PropertyToID("_Power");

        _slashMotionInfo = new Dictionary<RobotController.LowerBodyState, SlashMotionInfo>
        {
            { RobotController.LowerBodyState.GroundSlash,new SlashMotionInfo(2) },
            { RobotController.LowerBodyState.AirSlash,new SlashMotionInfo(1) },
            { RobotController.LowerBodyState.LowerSlash,new SlashMotionInfo(1) },
            { RobotController.LowerBodyState.QuickSlash,new SlashMotionInfo(2) },
            { RobotController.LowerBodyState.DashSlash,new SlashMotionInfo(1) },
        };

            _motionProperty = new Dictionary<RobotController.LowerBodyState, MotionProperty>
            {
                { RobotController.LowerBodyState.GROUNDSLASH_DASH,new MotionProperty{DashSpeed = 20.0f ,DashLength = 67 ,SlashDistance = 4.5f,SlashDistance_Min = float.MinValue} },
                { RobotController.LowerBodyState.AIRSLASH_DASH,new MotionProperty{DashSpeed = 30.0f ,DashLength = 67/2,SlashDistance = 4.5f,SlashDistance_Min = float.MinValue } },
              //  { RobotController.LowerBodyState.LowerSlash,new SlashMotionInfo(1) },
                 { RobotController.LowerBodyState.QUICKSLASH_DASH,new MotionProperty{DashSpeed = 30.0f ,DashLength = 67/2,SlashDistance = 4.5f,SlashDistance_Min = float.MinValue } },
                 { RobotController.LowerBodyState.DASHSLASH_DASH,new MotionProperty{DashSpeed = 40.0f ,DashLength = 67/2,SlashDistance = 4.5f,SlashDistance_Min = float.MinValue } },
            };


        foreach (var slashmotion in slashMotionInfo)
        {
            for (int i = 0; i < slashmotion.Value.num; i++)
            {
                switch (slashmotion.Key)
                {
                    case RobotController.LowerBodyState.GroundSlash:
                    case RobotController.LowerBodyState.AirSlash:
                    case RobotController.LowerBodyState.QuickSlash:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"{slashmotion.Key.ToString()}2_{i}");
                        break;
                    case RobotController.LowerBodyState.LowerSlash:
                    case RobotController.LowerBodyState.DashSlash:
                        slashmotion.Value._animID[i] = Animator.StringToHash($"{slashmotion.Key.ToString()}_{i}");
                        break;

                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        material.SetFloat(powerID, 0.75f);


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

    GameObject[] hitHistory = new GameObject[16];
    RobotController[] hitHistoryRC = new RobotController[16];
    int hitHistoryCount = 0;

    void FixedUpdate()
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

                if (hitHistoryRC.Contains(robotController))
                    continue;

                hitHistoryRC[hitHistoryRCCount++] = robotController;

                robotController.DoDamage(dir, /*damage*/0, knockBackType) ;

                GameObject.Instantiate(hitEffect_prefab, rayCastHit[idx_hit].point, Quaternion.identity);
            }


        }
    }
}
