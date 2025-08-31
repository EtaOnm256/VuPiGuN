using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Leopard : RobotAI_Base
{
    int moveDirChangeTimer = 0;
    int stepDirChangeTimer = 30;

    void Awake()
    {
        robotController = GetComponent<RobotController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        fire_wait = Random.Range(60, 120);
    }

    bool ascending = false;

    public int ascend_margin = 0;

    public int fire_wait;
    public int fire_prepare = 15;

    public int infight_reload = 0;
    public int infight_wait = 0;
    public int jumpinfight_reload = 0;

    bool overheating = false;

    bool prev_slash = false;
    bool prev_sprint = false;

    Vector2 stepdir;

    public int ground_step_remain = 2;
    public enum State
    {
        Ground,
        Ascend,
        Dash,
        Decend
    }

    public State state = State.Ground;

    //public float movedirection_range = 0.0f;
    public float movedirection_range = 180.0f;

    //public float lock_range = 75.0f;
    public float lock_range = 150.0f;

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        //return;

        float mindist = float.MaxValue;

        TargetNearest(null);

        if (current_target != null && current_target)
            mindist = (current_target.GetCenter() - robotController.GetCenter()).magnitude;

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
                    bool allow_infight = false;

                    switch (state)
                    {
                        case State.Ground:
                            {
                                bool dodge = false;
                                /*if (nearest_robot != null && mindist < 10.0f)
                                {
                                    if ( (nearest_robot.upperBodyState == RobotController.UpperBodyState.FIRE
                                         || nearest_robot.upperBodyState == RobotController.UpperBodyState.SUBFIRE
                                         || nearest_robot.upperBodyState == RobotController.UpperBodyState.HEAVYFIRE) 
                                         && !nearest_robot.fire_done)
                                    {
                                        dodge = true;
                                    }
                                }*/

                                if (!dodge)
                                {
                                    foreach (var team in WorldManager.current_instance.teams)
                                    {
                                        if (team == robotController.team)
                                            continue;

                                        foreach (var robot in team.robotControllers)
                                        {
                                            if (robot.dead || robot.Target_Robot != robotController)
                                                continue;

                                            if ((robot.GetCenter() - robotController.GetCenter()).magnitude > 10.0f)
                                                continue;

                                            if (robot.lowerBodyState == RobotController.LowerBodyState.SLASH
                                                || robot.lowerBodyState == RobotController.LowerBodyState.SLASH_DASH
                                             
                                                || robot.lowerBodyState == RobotController.LowerBodyState.JumpSlash
                                                || robot.lowerBodyState == RobotController.LowerBodyState.JumpSlash_Jump
                                                )
                                            {
                                                dodge = true;
                                                stepMove = ThreatPosToStepMove(robot.GetCenter(), targetQ);
                                                break;
                                            }
                                        }

                                        float evade_thresh;

                                        if (state != State.Ground)
                                            evade_thresh = 40.0f;
                                        else
                                            evade_thresh = 30.0f;

                                        foreach (var projectile in team.projectiles)
                                        {
                                            if (projectile.dead)
                                                continue;

                                            //if ( Vector3.Dot(.normalized,projectile.direction.normalized) > Mathf.Cos(Mathf.PI/4))

                                            float shift = Vector3.Cross(projectile.direction.normalized, (robotController.GetCenter() - projectile.position)).magnitude;

                                            float dist = (robotController.GetCenter() - projectile.position).magnitude;

                                            if (Vector3.Dot((robotController.GetCenter() - projectile.transform.position).normalized, projectile.direction.normalized) > Mathf.Cos(Mathf.PI / 4)
                                                && (shift < 3.0f || projectile.trajectory == Weapon.Trajectory.Curved)
                                                && dist / projectile.speed < evade_thresh
                                                )
                                            {
                                                dodge = true;
                                                stepMove = ThreatPosToStepMove(projectile.transform.position, targetQ);
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (dodge)
                                {
                                    //if (stepDirChangeTimer == 0)
                                    //{
                                    //    stepdir = VectorUtil.rotate(stepMove, Random.Range(0, 2) != 0 ? 0.0f * Mathf.Deg2Rad : 180.0f * Mathf.Deg2Rad);
                                    //}

                                    if (!robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.ExtremeSlide) && robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.RollingShoot))
                                    {
                                        if (robotController.lowerBodyState == RobotController.LowerBodyState.HEAVYFIRE)
                                        {
                                            allow_fire = true;
                                            ringMenuDir = Random.Range(0, 2) == 0 ? RobotController.RingMenuDir.Left : RobotController.RingMenuDir.Right;
                                        }
                                    }
                                    else
                                    {
                                        move = stepMove;

                                        if (robotController.lowerBodyState == RobotController.LowerBodyState.STEP
                                          && !IsStepDirectionCrossed(RobotController.determineStepDirection(stepMove), robotController.stepDirection))
                                            sprint = true;
                                        else
                                            sprint = !prev_sprint;

                                        stepDirChangeTimer = 30;
                                    }
                                }
                                else
                                {
                                    if (stepDirChangeTimer > 0)
                                        stepDirChangeTimer--;

                                    if (ground_step_remain > 0 &&
                                        (
                                        robotController.lowerBodyState == RobotController.LowerBodyState.WALK
                                        || robotController.lowerBodyState == RobotController.LowerBodyState.STEP
                                        || robotController.lowerBodyState == RobotController.LowerBodyState.STEPGROUND
                                        )                                        
                                        
                                        && robotController.boost >= robotController.robotParameter.Boost_Max /2

                                        )
                                    {
                                        if (mindist > lock_range)
                                        {
                                            move.y = 1.0f;
                                            move.x = 0.0f;
                                        }
                                        else
                                        {
                                            if (moveDirChangeTimer <= 0)
                                            {
                                                move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(-movedirection_range * Mathf.Deg2Rad, movedirection_range * Mathf.Deg2Rad));
                                                moveDirChangeTimer = 60;
                                            }
                                        }

                                        if (robotController.lowerBodyState == RobotController.LowerBodyState.STEP)
                                            sprint = true;
                                        else
                                            sprint = !prev_sprint;


                                        if (ground_step_remain != 1 && robotController.lowerBodyState == RobotController.LowerBodyState.STEP)
                                        {
                                            ground_step_remain = 1;
                                        }
                                        if (ground_step_remain == 1 && robotController.lowerBodyState == RobotController.LowerBodyState.STEPGROUND)
                                        {
                                            ground_step_remain = 2;
                                        }
                                    }
                                    else if(robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.RollingShoot) && robotController.lowerBodyState == RobotController.LowerBodyState.HEAVYFIRE && robotController.fire_done)
                                    {
                                        allow_fire = true;
                                        ringMenuDir = Random.Range(0,2) == 0 ? RobotController.RingMenuDir.Left : RobotController.RingMenuDir.Right;
                                    }
                                    else
                                    {
                                        if (mindist > lock_range)
                                        {
                                            move.y = 1.0f;
                                            move.x = 0.0f;
                                            moveDirChangeTimer = 60;
                                        }
                                        else
                                        {
                                            if (moveDirChangeTimer <= 0)
                                            {
                                                move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(-movedirection_range * Mathf.Deg2Rad, movedirection_range * Mathf.Deg2Rad));

                                                moveDirChangeTimer = 60;
                                            }
                                        }
                                    }

                                    if (current_target.Grounded && mindist < 20.0f)
                                        allow_infight = true;


                                    //if (robotController.boost >= robotController.Boost_Max / 2)
                                    if(robotController.lowerBodyState == RobotController.LowerBodyState.STEPGROUND)
                                    {
                                        if (robotController.shoulderWeapon != null
                                          && (robotController.shoulderWeapon.energy == robotController.shoulderWeapon.MaxEnergy)
                                          && robotController.shoulderWeapon.limit_range_max > mindist)
                                        {
                                            subfire = true;
                                        }
                                        else
                                            allow_fire = true;
                                    }

                                    if (robotController.lowerBodyState == RobotController.LowerBodyState.SUBFIRE)
                                    {
                                        if (robotController.shoulderWeapon != null
                                         && robotController.fire_followthrough > 0 && robotController.shoulderWeapon.canHold)
                                        {
                                            subfire = true;
                                        }
                                    }
                                }
                                prev_dodge = dodge;
                            }
                            break;
                        case State.Ascend:
                            {
                                jump = true;



                                if (floorhit.distance > 15.0f)
                                {
                                    state = State.Dash;
                                }

                                if (overheating)
                                    state = State.Decend;

                                if (robotController.Grounded)
                                    state = State.Ground;

                                if (mindist < 20.0f)
                                    allow_infight = true;
                            }
                            break;
                        case State.Dash:
                            {
                                sprint = true;

                                if (mindist > lock_range)
                                {
                                    move.y = 1.0f;
                                    move.x = 0.0f;
                                    moveDirChangeTimer = 60;
                                }
                                else
                                {


                                    if (moveDirChangeTimer <= 0)
                                    {
                                        move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(-movedirection_range * Mathf.Deg2Rad, movedirection_range * Mathf.Deg2Rad));
                                        moveDirChangeTimer = 60;
                                    }
                                }

                                if (overheating)
                                    state = State.Decend;

                                if (robotController.Grounded)
                                    state = State.Ground;

                                //if (target_angle <= 60)
                                //    allow_fire = true;

                                if (mindist < 20.0f)
                                    allow_infight = true;

                            }
                            break;
                        case State.Decend:
                            {
                                if (robotController.Grounded)
                                {
                                    state = State.Ground;
                                    ground_step_remain = 2;
                                }

                                //if (floorhit.distance > 10.0f)
                                //    allow_fire = true;

                                if (mindist < 20.0f)
                                    allow_infight = true;

                              
                            }
                            break;
                    }

                    if (robotController.lowerBodyState == RobotController.LowerBodyState.SLASH  )
                    {
                        if (robotController.slash_count == robotController.Sword.slashMotionInfo[robotController.subState_Slash].num - 1)
                            infight_reload = 60;
                    }
                    else if (infight_reload > 0)
                        infight_reload--;

                    if (robotController.lowerBodyState == RobotController.LowerBodyState.JumpSlash)
                        jumpinfight_reload = 30;
                    else if (jumpinfight_reload > 0)
                        jumpinfight_reload--;

                    /*if(robotController.itemFlag.HasFlag(RobotController.ItemFlag.ExtremeSlide) && robotController.lowerBodyState == RobotController.LowerBodyState.JUMPSLASH_GROUND)
                    {
                        robotController.
                    }*/

                    if (robotController.Sword == null)
                        allow_infight = false;
                    if (robotController.rightWeapon == null)
                        allow_fire = false;


                    if (allow_infight)
                    {
                        if (infight_wait <= 0)
                        {

                            if (robotController.Sword.can_jump_slash && !prev_slash && jumpinfight_reload <= 0)
                            {
                                move.x = 0.0f;
                                move.y = -1.0f;
                                slash = true;
                            }
                            else if (!prev_slash && infight_reload <= 0)
                            {
                                move.x = 0.0f;
                                move.y = 0.0f;
                                slash = true;
                            }
                        }
                        else
                            infight_wait--;
                    }
                    else
                    {
                        infight_wait = 15;

                        if (/*fire_wait <= 0 && */allow_fire)
                        {
                            //if (mindist < lock_range)
                            {
                                //if (fire_prepare <= 0)
                                {
                                    fire = true;
                                    fire_wait = Random.Range(60, 120);
                                    fire_prepare = 15;
                                }
                                /*else
                                {
                                    fire_prepare--;
                                }*/

                                move = Vector2.zero;
                                moveDirChangeTimer = 0;
                            }
                            /*else
                            {
                                fire_wait = Random.Range(60, 120);
                                fire_prepare = 15;
                            }*/
                        }
                    }


                    fire_wait--;
                    infight_wait--;
                    moveDirChangeTimer--;
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

        //robotController.boost = robotController.Boost_Max;

        return;
    }
}
