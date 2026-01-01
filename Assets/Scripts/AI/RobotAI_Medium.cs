using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Medium : RobotAI_Base
{
    int moveDirChangeTimer = 0;

    void Awake()
    {
        robotController = GetComponent<RobotController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        attack_reload = Random.Range(fire_wait_min, fire_wait_max);
        attack_prepare = fire_prepare_max;

        if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.InfightBoost))
            infight_dist *= 1.5f;
    }

    bool ascending = false;

    public int ascend_margin = 0;

    public int attack_reload;

    [SerializeField] int fire_wait_min = 60;
    [SerializeField] int fire_wait_max = 120;

    public int attack_prepare = 15;
    [SerializeField] int fire_prepare_max = 15;

    //public int infight_reload = 0;
    //public int infight_wait = 0;
    public int jumpslash_reload = 0;
    public int dashslash_reload = 0;
    public int horizon_reload = 0;
    bool overheating = false;

    bool prev_slash = false;
    bool prev_sprint = false;

    public int ground_step_remain = 2;
    public enum State
    {
        Ground,
        Ascend,
        Dash,
        Decend
    }

    public State state = State.Ground;

    //public float movedirection_range = 90.0f;

    //public float movedirection_range = 180.0f;

    public float lock_range = 75.0f;

    enum SubweaponState
    {
        READY,
        FIRING,
        CHARGING
    }

    [SerializeField]SubweaponState subweaponState = SubweaponState.READY;
    float DetermineMoveDirection(float dist)
    {
        if (robotController.rightWeapon == null || dist > robotController.rightWeapon.optimal_range_max)
            return 45.0f;

        if (dist < robotController.rightWeapon.optimal_range_min)
            return 135.0f;

        return 90.0f;
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        //return;
        float mindist = float.MaxValue;
        float mindist_y = float.MaxValue;
        Vector2 stepMove;
        DetermineTarget();

        if (current_target != null && current_target)
        {
            mindist = (current_target.GetCenter() - robotController.GetCenter()).magnitude;
            mindist_y = (current_target.GetCenter() - robotController.GetCenter()).y;
        }

        ringMenuDir = RobotController.RingMenuDir.Center;

        if (current_target == null)
        {
           
        }
        else
        {

            Vector3 cameraAxis = robotController.GetTargetQuaternionForView(current_target).eulerAngles;

            robotController._cinemachineTargetYaw = cameraAxis.y;
            robotController._cinemachineTargetPitch = cameraAxis.x;

            Quaternion targetQ = Quaternion.LookRotation(current_target.GetCenter() - robotController.GetCenter(), Vector3.up);

            fire = false;
            sprint = false;
            slash = false;
            subfire = false;
            if (overheating)
            {
                if (robotController.boost >= robotController.robotParameter.Boost_Max)
                    overheating = false;
            }
            else
            {
                if (robotController.boost <= 0)
                    overheating = true;
            }

            //if (robotController.upperBodyState == RobotController.UpperBodyState.FIRE)
            //{
                
            //}
            //else
            {
                RaycastHit aimblockhit;
                bool Aim_blocked = Physics.Raycast(robotController.GetCenter(), targetQ * Vector3.forward, out aimblockhit, mindist, WorldManager.layerPattern_Building);

                bool climbing = Aim_blocked && aimblockhit.collider.gameObject.layer == 3;
                bool tallbuilding = Aim_blocked && aimblockhit.collider.gameObject.layer == 7;

                if (ascending)
                {
                    if (!climbing)
                        ascend_margin--;
                    else
                        ascend_margin = 60;

                    if (ascend_margin <= 0)
                    {
                        ascending = false;
                    }

                }
                else
                {
                    if (climbing)
                    {
                        if (aimblockhit.distance < 10.0f)
                        {
                            ascending = true;
                            ascend_margin = 60;
                        }
                    }

                }

                if (Aim_blocked)
                {
                    if (tallbuilding)
                    {
                        move.y = 1.0f;
                        move.x = 1.0f;

                        if (!overheating)
                            sprint = true;

                        jump = false;
                    }
                    else
                    {
                        move.y = 1.0f;
                        move.x = 0.0f;

                        if (ascending)
                        {
                            if (overheating || !ascending)
                            {
                                jump = false;

                                if (!overheating)
                                    sprint = true;
                            }
                            else
                            {
                                jump = true;
                            }
                        }
                    }
                }
                else
                {
                    RaycastHit floorhit;

                    bool ground = Physics.Raycast(robotController.GetCenter(), Vector3.down, out floorhit, float.MaxValue, WorldManager.layerPattern_Building);
                    float target_angle = Vector3.Angle(current_target.GetTargetedPosition() - robotController.chest_hint.transform.position, transform.forward);

                    jump = false;

                    bool allow_fire = false;
                    bool force_fire = false;
                    bool allow_infight = false;
                    bool force_infight = false;
                    bool allow_jumpslash = false;
                    bool allow_dashslash = false;
                    bool allow_horizon = false;


                    bool dodge = false;

                    ProcessDodge(out dodge, out stepMove, targetQ);

                    switch (state)
                    {
                        case State.Ground:
                            {
                                if (dodge)
                                {
                                    move = stepMove;

                                    if (robotController.lowerBodyState == RobotController.LowerBodyState.STEP
                                        && !IsStepDirectionCrossed(RobotController.determineStepDirection(stepMove), robotController.stepDirection))
                                        sprint = true;
                                    else
                                        sprint = !prev_sprint;

                                    if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.RollingShoot) && Mathf.Abs(stepMove.x) > Mathf.Abs(stepMove.y))
                                    {
                                        allow_fire = true;
                                        ringMenuDir = Random.Range(0, 2) == 0 ? RobotController.RingMenuDir.Left : RobotController.RingMenuDir.Right;
                                    }

                                    moveDirChangeTimer = 60;
                                }
                                else
                                {
                                    if (ground_step_remain > 0)
                                    {
                                        if(robotController.team.orderToAI != WorldManager.OrderToAI.EVADE && mindist > infight_dist &&
                                            robotController.shoulderWeapon != null && robotController.shoulderWeapon.allrange && robotController.shoulderWeapon.canHold
                                            && subweaponState != SubweaponState.CHARGING
                                            )
                                        {
                                            subfire = true;
                                            subweaponState = SubweaponState.FIRING;
                                        }

                                        if (robotController.team.orderToAI == WorldManager.OrderToAI.EVADE)
                                        {
                                            move.y = -1.0f;
                                            move.x = 0.0f;
                                        }
                                        else
                                        {
                                            if (mindist > lock_range / 2 && (robotController.rightWeapon == null || mindist < robotController.rightWeapon.limit_range_max))
                                            {
                                                move.y = 1.0f;
                                                move.x = 0.0f;
                                            }
                                            else
                                            {
                                                move.x = 1.0f;
                                                move.y = 0.0f;
                                            }
                                        }

                                        if (robotController.lowerBodyState == RobotController.LowerBodyState.STEP)
                                            sprint = true;
                                        else
                                            sprint = !prev_sprint;

                                        moveDirChangeTimer = 60;

                                        if (ground_step_remain != 1 && robotController.lowerBodyState == RobotController.LowerBodyState.STEP)
                                        {
                                            ground_step_remain = 1;
                                        }
                                        if (ground_step_remain == 1 && robotController.lowerBodyState == RobotController.LowerBodyState.STEPGROUND)
                                        {
                                            ground_step_remain = 0;
                                        }
                                    }
                                    else
                                    {
                                        bool far = false;

                                        if (robotController.team.orderToAI == WorldManager.OrderToAI.EVADE)
                                            far = mindist > 150.0f;
                                        else
                                            far = mindist > lock_range;

                                        if (far)
                                        {
                                            move.y = 1.0f;
                                            move.x = 0.0f;
                                            //moveDirChangeTimer = 60;
                                        }
                                        else
                                        {
                                            if (robotController.team.orderToAI == WorldManager.OrderToAI.EVADE)
                                            {
                                                if (moveDirChangeTimer <= 0)
                                                {
                                                    move = VectorUtil.rotate(new Vector2(0.0f, -1.0f), Random.Range(-45.0f * Mathf.Deg2Rad, 45.0f * Mathf.Deg2Rad));
                                                    moveDirChangeTimer = 60;
                                                }
                                            }
                                            else
                                            {
                                                float movedirection_range = DetermineMoveDirection(mindist);

                                                if (moveDirChangeTimer <= 0)
                                                {
                                                    move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(-movedirection_range * Mathf.Deg2Rad, movedirection_range * Mathf.Deg2Rad));
                                                    moveDirChangeTimer = 60;
                                                }
                                            }
                                        }

                                        if (robotController.boost >= robotController.robotParameter.Boost_Max)
                                        {
                                            jump = true;
                                        }
                                    }

                                    if (robotController.team.orderToAI != WorldManager.OrderToAI.EVADE)
                                    {
                                        if (mindist < jumpslash_dist)
                                            allow_jumpslash = true;

                                        if (mindist < dashslash_dist)
                                            allow_dashslash = true;

                                        if (mindist < horizon_dist && Mathf.Abs(mindist_y) < mindist * 0.5f)
                                            allow_horizon = true;
                                            

                                        if ( (current_target.Grounded || robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.IaiSlash) || robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.SeedOfArts))
                                            && (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.VoidShift) || mindist < infight_dist))
                                            allow_infight = true;

                                        if (target_angle <= 90)
                                            allow_fire = true;

                                  
                                        if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.RollingShoot) && robotController.rightWeapon != null && (!(robotController.rightWeapon.canHold && Aiming_Precise()) && robotController.fire_followthrough > 0) && !robotController.rollingfire_followthrough)
                                        {
                                            allow_fire = true;
                                            ringMenuDir = Random.Range(0, 2) == 0 ? RobotController.RingMenuDir.Left : RobotController.RingMenuDir.Right;
                                        }
                                        else if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.ExtremeSlide))
                                        {
                                           


                                            if (
                                                        (
                                                        (robotController.rightWeapon != null && (!(robotController.rightWeapon.canHold && Aiming_Precise()) && robotController.fire_followthrough > 0)) // 射撃キャンセル
                                                        || (robotController.Sword != null && ((robotController.lowerBodyState == RobotController.LowerBodyState.SLASH || robotController.lowerBodyState == RobotController.LowerBodyState.SLASH_DASH) && !Slash_Precise(mindist))) //格闘キャンセル
                                                        || robotController.lowerBodyState == RobotController.LowerBodyState.JumpSlash_Ground
                                                        || (robotController.lowerBodyState == RobotController.LowerBodyState.SWEEP && (mindist > horizon_dist/* || robotController.Sword.hitHistoryRCCount > 0*/))
                                                        )
                                               )
                                            {
                                                sprint = !prev_sprint;
                                            }
                                        }
                                    }
                                }

                                if (!robotController.Grounded)
                                    state = State.Ascend;
                            }
                            break;
                        case State.Ascend:
                            {
                                jump = true;

                              

                                if (floorhit.distance > 15.0f || dodge)
                                {
                                    state = State.Dash;
                                }

                                if (overheating)
                                    state = State.Decend;

                                if (robotController.Grounded)
                                    state = State.Ground;

                                if (robotController.team.orderToAI != WorldManager.OrderToAI.EVADE)
                                {
                                    if (!dodge)
                                    {
                                        if (mindist < jumpslash_dist)
                                            allow_jumpslash = true;

                                        if (mindist < dashslash_dist)
                                            allow_dashslash = true;

                                        if (mindist < horizon_dist && Mathf.Abs(mindist_y) < mindist * 0.5f)
                                            allow_horizon = true;

                                        if ((robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.VoidShift) || mindist < infight_dist))
                                            allow_infight = true;
                                    }
                                }
                            }
                            break;
                        case State.Dash:
                            {
                                sprint = true;

                                bool far = false;

                                if (robotController.team.orderToAI == WorldManager.OrderToAI.EVADE)
                                    far = mindist > 150.0f;
                                else
                                    far = mindist > lock_range;


                                if (dodge)
                                {
                                   
                                  
                                    move = stepMove;

                                    Vector3 stepMove_horizon = new Vector3(stepMove.x, 0.0f, stepMove.y);

                                    Vector3 rel = Quaternion.Inverse(targetQ) * transform.forward;

                                    if(Vector3.Dot(stepMove_horizon,rel) < Mathf.Cos(45.0f*Mathf.Deg2Rad))
                                    {
                                        if (robotController.lowerBodyState == RobotController.LowerBodyState.DASH && prev_sprint)
                                            sprint = false;
                                    }

                                    if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.RollingShoot) && Mathf.Abs(stepMove.x) > Mathf.Abs(stepMove.y))
                                    {
                                        allow_fire = true;
                                        ringMenuDir = Random.Range(0, 2) == 0 ? RobotController.RingMenuDir.Left : RobotController.RingMenuDir.Right;
                                    }

                                }
                                else
                                {
                                    if (robotController.team.orderToAI == WorldManager.OrderToAI.EVADE)
                                    {
                                        if (moveDirChangeTimer <= 0)
                                        {
                                            move = VectorUtil.rotate(new Vector2(0.0f, -1.0f), Random.Range(-45.0f * Mathf.Deg2Rad, 45.0f * Mathf.Deg2Rad));
                                            moveDirChangeTimer = 60;
                                        }
                                    }
                                    else
                                    {
                                        if (far)
                                        {
                                            move.y = 1.0f;
                                            move.x = 0.0f;
                                        }
                                        else
                                        {
                                            if (moveDirChangeTimer <= 0)
                                            {
                                                float movedirection_range = DetermineMoveDirection(mindist);

                                                move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(-movedirection_range * Mathf.Deg2Rad, movedirection_range * Mathf.Deg2Rad));
                                                moveDirChangeTimer = 60;
                                            }

                                            if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.RollingShoot) && robotController.rightWeapon != null && (!(robotController.rightWeapon.canHold && Aiming_Precise()) && robotController.fire_followthrough > 0) && !robotController.rollingfire_followthrough)
                                            {
                                                allow_fire = true;
                                                ringMenuDir = Random.Range(0, 2) == 0 ? RobotController.RingMenuDir.Left : RobotController.RingMenuDir.Right;
                                            }
                                            else if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.NextDrive))
                                            {
                                                if (
                                                    robotController.team.orderToAI != WorldManager.OrderToAI.EVADE && mindist > infight_dist &&
                                                    robotController.shoulderWeapon != null && robotController.shoulderWeapon.allrange && robotController.shoulderWeapon.canHold
                                                    && subweaponState != SubweaponState.CHARGING
                                                    )
                                                {
                                                    subfire = true;
                                                    subweaponState = SubweaponState.FIRING;
                                                }
                                                else
                                                {
                                                 

                                                    if(

                                                               (
                                                        (robotController.rightWeapon != null && (!(robotController.rightWeapon.canHold && Aiming_Precise()) && robotController.fire_followthrough > 0)) // 射撃キャンセル
                                                        || (robotController.Sword != null && ((robotController.lowerBodyState == RobotController.LowerBodyState.SLASH || robotController.lowerBodyState == RobotController.LowerBodyState.SLASH_DASH) && !Slash_Precise(mindist))) //格闘キャンセル
                                                        || robotController.lowerBodyState == RobotController.LowerBodyState.JumpSlash_Ground
                                                        || (robotController.lowerBodyState == RobotController.LowerBodyState.SWEEP && (mindist > horizon_dist/* || robotController.Sword.hitHistoryRCCount > 0*/))
                                                        )
                                                        && prev_sprint)
                                                    {
                                                        sprint = false;
                                                    }
                                                }
                                             }
                                        }
                                    }
                                }

                                if (overheating)
                                    state = State.Decend;


                                if (robotController.Grounded)
                                    state = State.Ground;

                                if (floorhit.distance < 10.0f && !dodge)
                                    state = State.Ascend;

                                if (robotController.team.orderToAI != WorldManager.OrderToAI.EVADE)
                                {
                                    if (target_angle <= 90)
                                        allow_fire = true;

                                    if (!dodge)
                                    {

                                        if (mindist < jumpslash_dist)
                                            allow_jumpslash = true;

                                        if (mindist < dashslash_dist)
                                            allow_dashslash = true;

                                        if (mindist < horizon_dist && Mathf.Abs(mindist_y) < mindist * 0.5f)
                                            allow_horizon = true;

                                        if ((robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.VoidShift) || mindist < infight_dist))
                                            allow_infight = true;
                                    }

                                    if ( robotController.shoulderWeapon != null && !robotController.shoulderWeapon.allrange
                                  && (robotController.shoulderWeapon.energy == robotController.shoulderWeapon.MaxEnergy
                                  || (robotController.upperBodyState == RobotController.UpperBodyState.SUBFIRE && robotController.fire_followthrough > 0 && robotController.shoulderWeapon.canHold)))
                                    {
                                        subfire = true;
                                    }
                                }
                              
                            }
                            break;
                        case State.Decend:
                            {
                                if (robotController.Grounded)
                                {
                                    state = State.Ground;
                                    ground_step_remain = 2;
                                }

                                if (robotController.team.orderToAI != WorldManager.OrderToAI.EVADE)
                                {
                                    if (floorhit.distance > 10.0f)
                                    {
                                        if (robotController.shoulderWeapon != null
                                        && (robotController.shoulderWeapon.energy == robotController.shoulderWeapon.MaxEnergy
                                        || (/*robotController.upperBodyState == RobotController.UpperBodyState.SUBFIRE && */robotController.fire_followthrough > 0 && robotController.shoulderWeapon.canHold)))
                                        {
                                            subfire = true;
                                        }
                                        allow_fire = true;
                                    }

                                    if (!dodge)
                                    {

                                        if (mindist < jumpslash_dist)
                                            allow_jumpslash = true;

                                        if (mindist < dashslash_dist)
                                            allow_dashslash = true;

                                        if (mindist < horizon_dist && Mathf.Abs(mindist_y) < mindist * 0.5f)
                                            allow_horizon = true;

                                        if ((robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.VoidShift) || mindist < infight_dist))
                                            allow_infight = true;
                                    }

                                   
                                }
                            }
                            break;
                    }

                    bool infight_now = false;
       
                    if (robotController.lowerBodyState == RobotController.LowerBodyState.SLASH)
                    {
                        //if (robotController.slash_count == robotController.Sword.slashMotionInfo[robotController.subState_Slash].num - 1)
                        if(robotController.subState_Slash != RobotController.SubState_Slash.AirSlash)
                        {
                            attack_reload = 60;

                            if (type == AI_Type.FRANK)
                            {
                                TargetNearest(current_target);
                            }
                        }
                    }
                    else if(attack_reload > 0)
                        attack_reload--;

                    if (robotController.lowerBodyState == RobotController.LowerBodyState.SLASH
                     || robotController.lowerBodyState == RobotController.LowerBodyState.SLASH_DASH)
                        infight_now = true;

                    if (robotController.lowerBodyState == RobotController.LowerBodyState.JumpSlash)
                        jumpslash_reload = 90;
                    else if(jumpslash_reload > 0)
                        jumpslash_reload--;

                    if(robotController.lowerBodyState == RobotController.LowerBodyState.SLASH_DASH && robotController.subState_Slash == RobotController.SubState_Slash.DashSlash)
                        dashslash_reload = 90;
                    else if (dashslash_reload > 0)
                        dashslash_reload--;

                    if(robotController.lowerBodyState == RobotController.LowerBodyState.SWEEP)
                        horizon_reload = 60;
                    else if (horizon_reload > 0)
                        horizon_reload--;

                    if (robotController.Sword == null || attack_reload > 0)
                        allow_infight = allow_dashslash = allow_jumpslash = allow_horizon = false;

                    if (robotController.rightWeapon == null)
                        allow_fire = false;

                    if (!robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.DashSlash) || dashslash_reload > 0 || robotController.boost < 80 || infight_now)
                        allow_dashslash = false;
                    if (!robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.JumpSlash) || jumpslash_reload > 0 || robotController.boost < 80 || infight_now)
                        allow_jumpslash = false;
                    if (!robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.HorizonSweep) || horizon_reload > 0 || robotController.boost < 80 || infight_now)
                        allow_horizon = false;


                    if (current_target.mirage_time > 0 && (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.NextDrive) || robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.ExtremeSlide)))
                        allow_infight = allow_jumpslash = allow_dashslash = false;

                    if (robotController.lowerBodyState == RobotController.LowerBodyState.SWEEP && robotController.Sword.hitHistoryRCCount > 0)
                    {
                        allow_fire = allow_jumpslash = allow_dashslash = allow_horizon = false;
                        allow_infight = force_infight = true;
                    }

                    if (infight_now)
                    {
                        allow_jumpslash = allow_dashslash = allow_horizon = false;

                        if(robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.QuickDraw))
                        {
                            if( robotController.slash_count == robotController.Sword.slashMotionInfo[robotController.subState_Slash].num - 1
                                && robotController.Sword.hitHistoryRCCount > 0)
                            {
                                if (!dodge)
                                {
                                    allow_fire = force_fire = true;
                                    allow_infight = false;
                                }
                            }

                        }
                 
                        if (robotController.slash_count < robotController.Sword.slashMotionInfo[robotController.subState_Slash].num - 1)
                        {
                            if (!dodge)
                            {
                                allow_infight = true;
                                force_infight = true;
                            }
                        }

                    }
                    else
                    {
                        if (allow_fire && ringMenuDir != RobotController.RingMenuDir.Center)
                        {
                            allow_infight = allow_jumpslash = allow_dashslash = allow_horizon = false;
                        }
                        else if (allow_fire && (allow_infight || allow_jumpslash || allow_dashslash || allow_horizon))
                        {
                            //if (Random.Range(0, 2) != 0)
                            //    allow_fire = false;
                            //else
                            //    allow_infight = allow_jumpslash = allow_dashslash = allow_horizon = false;
                        }

                        //if(allow_infight && (allow_jumpslash || allow_dashslash || allow_horizon))
                        //{
                            //if (Random.Range(0, 2) != 0)
                            //    allow_infight = false;
                            //else
                            //    allow_jumpslash = allow_dashslash = allow_horizon = false;
                        //}
                    }

                    if(robotController.upperBodyState == RobotController.UpperBodyState.FIRE || 
                        robotController.upperBodyState == RobotController.UpperBodyState.HEAVYFIRE || 
                        robotController.upperBodyState == RobotController.UpperBodyState.ROLLINGFIRE ||
                        robotController.upperBodyState == RobotController.UpperBodyState.ROLLINGHEAVYFIRE)
                    {
                        attack_reload = Random.Range(fire_wait_min, fire_wait_max);
                        attack_prepare = fire_prepare_max;
                    }

                    if (robotController.rightWeapon == null || attack_reload > 0 || mindist > robotController.rightWeapon.limit_range_max)
                        allow_fire = false;


                    if (allow_fire || allow_infight || allow_dashslash || allow_jumpslash || allow_horizon)
                    {
                        if (attack_prepare > 0)
                        {
                            attack_prepare--;
                            allow_fire = allow_infight = allow_dashslash = allow_jumpslash = allow_horizon = false;
                        }
                    }
                    else
                    {
                        attack_prepare = fire_prepare_max;
                    }


                    if (robotController.upperBodyState == RobotController.UpperBodyState.FIRE && (!robotController.fire_done || robotController.rightWeapon.canHold))
                        allow_jumpslash = allow_dashslash = false;

                    if (force_fire)
                        allow_fire = true;

                    if (force_infight)
                        allow_infight = true;

                    int size = 0;
                    int[] cmd = new int[5];

                    if (allow_fire)
                        cmd[size++] = 0;
                    if (allow_infight)
                        cmd[size++] = 1;
                    if (allow_dashslash)
                        cmd[size++] = 2;
                    if (allow_jumpslash)
                        cmd[size++] = 3;
                    if (allow_horizon)
                        cmd[size++] = 4;

                    int selected_cmd = -1;

                    {

                        if (size > 0)
                        {
                            int select_idx;
                            select_idx = Random.Range(0, size);
                            selected_cmd = cmd[select_idx];
                        }
                    }

                    



                    if (selected_cmd == 4 && !prev_slash)
                    {
                        if(Random.Range(0, 2) == 0)
                            ringMenuDir = RobotController.RingMenuDir.Left;
                        else
                            ringMenuDir = RobotController.RingMenuDir.Right;
                        slash = true;
                    }
                    else if (selected_cmd == 3 && !prev_slash)
                    {
                       ringMenuDir = RobotController.RingMenuDir.Down;
                       slash = true;
                    }
                    else if(selected_cmd == 2 && !prev_slash)
                    {
                       ringMenuDir = RobotController.RingMenuDir.Up;
                       slash = true;
                    }
                    else if (selected_cmd == 1)
                    {
                        //if (infight_wait <= 0)
                        //{
                            if (!prev_slash)
                            {
                                slash = true;

                                int rand = Random.Range(0, 4);

                                if (rand == 0)
                                    move = Vector2.left;
                                else if (rand == 3)
                                    move = Vector2.right;
                                else
                                    move = Vector2.zero;
                            }
                        //}
                        //else
                        //    infight_wait--;
                    }
                    else
                    {
                        //infight_wait = 15;

                        if (robotController.rightWeapon != null && robotController.fire_followthrough > 0 && robotController.rightWeapon.canHold)
                        {
                            fire = true;
                        }
                        else
                        {
                            if (allow_fire)
                            {
                                fire = true;
                            }
                        }
                    }


                    attack_reload--;
                    //infight_wait--;
                    moveDirChangeTimer--;
                    prev_dodge = dodge;
                }
            }
        }

        prev_slash = slash;
        prev_sprint = sprint;
        //
        //fire = false;
        //slash = false;
        //subfire = false;
        //

        if(subweaponState == SubweaponState.FIRING && !subfire)
        {
            subweaponState = SubweaponState.CHARGING;
        }

        if(subweaponState == SubweaponState.CHARGING && robotController.shoulderWeapon != null)
        {
            if (robotController.shoulderWeapon.energy == robotController.shoulderWeapon.MaxEnergy && robotController.shoulderWeapon.canHold)
                subweaponState = SubweaponState.READY;
        }

        return;
    }
}
