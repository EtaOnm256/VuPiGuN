using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Gargoyle : RobotAI_Base
{
    int moveDirChangeTimer = 0;

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

    public enum State
    {
        Ground,
        Ascend,
        Dash,
        Decend
    }

    public State state = State.Ground;

    public float movedirection_range = 135.0f;

    //public float lock_range = 75.0f;
    public float lock_range = 150.0f;

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        //return;
        float mindist = float.MaxValue;

        DetermineTarget();

        if (current_target != null && current_target)
            mindist = (current_target.GetCenter() - robotController.GetCenter()).magnitude;

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
                bool Aim_blocked;
                {
                    RaycastHit aimblockhit;
                    Aim_blocked = Physics.Raycast(robotController.GetCenter(), targetQ * Vector3.forward, out aimblockhit, mindist, 1 << 3);

                    if (ascending)
                    {
                        if (!Aim_blocked)
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
                        if (Aim_blocked)
                        {
                            if (aimblockhit.distance < 10.0f)
                            {
                                ascending = true;
                                ascend_margin = 60;
                            }
                        }

                    }
                }

                if (Aim_blocked)
                {
                    move.y = 1.0f;
                    move.x = 0.0f;

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
                else
                {
                    RaycastHit floorhit;

                    bool ground = Physics.Raycast(robotController.GetCenter(), Vector3.down, out floorhit, float.MaxValue, 1 << 3);
                    float target_angle = Vector3.Angle(current_target.Chest.transform.position - transform.position, transform.forward);

                    jump = false;

                    bool allow_fire = false;
                    bool allow_infight = false;

                    switch (state)
                    {
                        case State.Ground:
                            {
                                bool dodge = false;
                                Vector2 stepMove = Vector2.zero;
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

                                        if (robot.lowerBodyState == RobotController.LowerBodyState.AIRSLASH_DASH
                                            || robot.lowerBodyState == RobotController.LowerBodyState.AirSlash
                                            || robot.lowerBodyState == RobotController.LowerBodyState.DashSlash
                                            || robot.lowerBodyState == RobotController.LowerBodyState.DASHSLASH_DASH
                                            || robot.lowerBodyState == RobotController.LowerBodyState.GroundSlash
                                            || robot.lowerBodyState == RobotController.LowerBodyState.GROUNDSLASH_DASH
                                            || robot.lowerBodyState == RobotController.LowerBodyState.JumpSlash
                                            || robot.lowerBodyState == RobotController.LowerBodyState.JumpSlash_Jump
                                            || robot.lowerBodyState == RobotController.LowerBodyState.QUICKSLASH_DASH
                                            || robot.lowerBodyState == RobotController.LowerBodyState.QuickSlash
                                            || robot.lowerBodyState == RobotController.LowerBodyState.LowerSlash)
                                        {
                                            dodge = true;
                                            stepMove = ThreatPosToStepMove(robot.GetCenter(), targetQ);
                                            break;
                                        }
                                    }

                                    foreach (var projectile in team.projectiles)
                                    {
                                        if (projectile.dead)
                                            continue;

                                        //if ( Vector3.Dot(.normalized,projectile.direction.normalized) > Mathf.Cos(Mathf.PI/4))

                                        float shift = Vector3.Cross(projectile.direction.normalized, (robotController.GetCenter() - projectile.position)).magnitude;

                                        float dist = (robotController.GetCenter() - projectile.position).magnitude;

                                        if (Vector3.Dot((robotController.GetCenter() - projectile.transform.position).normalized, projectile.direction.normalized) > Mathf.Cos(Mathf.PI / 4)
                                            && (/*projectile.target == robotController || */shift < 3.0f)
                                            && dist / projectile.speed < 20.0f
                                            )
                                        {
                                            dodge = true;
                                            stepMove = ThreatPosToStepMove(projectile.transform.position, targetQ);
                                            break;
                                        }
                                    }
                                }

                                if (dodge)
                                {
                                    move = stepMove;

                                    if (robotController.lowerBodyState == RobotController.LowerBodyState.STEP)
                                        sprint = true;
                                    else
                                        sprint = !prev_sprint;

                                    moveDirChangeTimer = 60;
                                }
                                else
                                {
                                    if (robotController.boost >= robotController.robotParameter.Boost_Max)
                                    {
                                        if (mindist > lock_range / 2)
                                        {
                                            move.y = 1.0f;
                                            move.x = 0.0f;
                                        }
                                        else
                                        {
                                            move.x = 1.0f;
                                            move.y = 0.0f;
                                        }

                                        if (robotController.lowerBodyState == RobotController.LowerBodyState.STEP)
                                            sprint = true;
                                        else
                                            sprint = !prev_sprint;

                                        moveDirChangeTimer = 60;

                                        if (robotController.lowerBodyState == RobotController.LowerBodyState.STEPGROUND)
                                        {
                                            jump = true;
                                        }
                                    }
                                    else
                                    {
                                        if (mindist > lock_range)
                                        {
                                            move.y = 1.0f;
                                            move.x = 0.0f;
                                            //moveDirChangeTimer = 60;
                                        }
                                        else
                                        {


                                            if (moveDirChangeTimer <= 0)
                                            {
                                                move = VectorUtil.rotate(new Vector2(0.0f, -1.0f), Random.Range(-movedirection_range * 2 * Mathf.PI / 360.0f, movedirection_range * 2 * Mathf.PI / 360.0f));
                                                moveDirChangeTimer = 60;
                                            }
                                        }
                                    }

                                    if (current_target.Grounded && mindist < 20.0f)
                                        allow_infight = true;

                                    //if (target_angle <= 90)
                                    //    allow_fire = true;
                                }

                                if (!robotController.Grounded)
                                    state = State.Ascend;
                            }
                            break;
                        case State.Ascend:
                            {
                                bool firing_sub = false;

                                if (robotController.shoulderWeapon != null)
                                {
                                    if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.NextDrive))
                                    {
                                        if (robotController.upperBodyState == RobotController.UpperBodyState.SUBFIRE && robotController.shoulderWeapon.canHold)
                                        {
                                            subfire = true;
                                            firing_sub = true;
                                        }
                                    }

                                    if (robotController.shoulderWeapon.energy == robotController.shoulderWeapon.MaxEnergy)
                                    {
                                        firing_sub = true;
                                        jump = true;

                                        if(floorhit.distance > 25.0f && robotController._verticalVelocity > robotController.robotParameter.AscendingVelocity * 3 / 4)
                                        {
                                            subfire = true;
                                        }
                                    }

                                }

                                if (!firing_sub)
                                {
                                    if ( (floorhit.distance > 25.0f
                                            || robotController._verticalVelocity < -robotController.robotParameter.AscendingVelocity * 3 / 4)
                                            && ( (robotController.upperBodyState != RobotController.UpperBodyState.FIRE && robotController.upperBodyState != RobotController.UpperBodyState.SUBFIRE)
                                                  || robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.NextDrive)
                                                  ))
                                    {
                                        state = State.Dash;
                                        moveDirChangeTimer = 0;
                                        jump = false;
                                    }
                                    else
                                        jump = true;
                                }



                                if (mindist > lock_range * 3 / 4)
                                {
                                    move.y = 1.0f;
                                    move.x = 0.0f;
                                    //moveDirChangeTimer = 60;
                                }
                                else
                                {


                                    if (moveDirChangeTimer <= 0)
                                    {
                                        move = VectorUtil.rotate(new Vector2(0.0f, -1.0f), Random.Range(-movedirection_range * 2 * Mathf.PI / 360.0f, movedirection_range * 2 * Mathf.PI / 360.0f));
                                        moveDirChangeTimer = 60;
                                    }
                                }

                                if (overheating)
                                    state = State.Decend;

                                if (robotController.Grounded)
                                    state = State.Ground;

                                if (mindist < 20.0f)
                                    allow_infight = true;

                                //if (target_angle <= 90)
                                //    allow_fire = true;
                            }
                            break;
                        case State.Dash:
                            {
                                sprint = true;

                                if (mindist > lock_range*3/4)
                                {
                                    move.y = 1.0f;
                                    move.x = 0.0f;
                                    //moveDirChangeTimer = 60;
                                }
                                else
                                {
                                   

                                    if (moveDirChangeTimer <= 0)
                                    {
                                        //move = VectorUtil.rotate(new Vector2(0.0f, -1.0f), Random.Range(-movedirection_range * 2 * Mathf.PI / 360.0f, movedirection_range * 2 * Mathf.PI / 360.0f));
                                        move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(0,2) == 0 ? 90*2*Mathf.PI / 360.0f: -90 * 2 * Mathf.PI / 360.0f);
                                        moveDirChangeTimer = 60;
                                    }
                                }

                                //if (overheating)
                                if (robotController.boost < robotController.robotParameter.Boost_Max * 1 / 4)
                                {
                                    state = State.Decend;
                                }
                                else if (floorhit.distance < 15.0f)
                                {
                                    state = State.Ascend;
                                }
                                else
                                {
                                    if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.NextDrive))
                                    {
                                        if (!robotController.rightWeapon.canHold && robotController.fire_followthrough > 0 && prev_sprint)
                                        {
                                            {
                                                move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(-75 * 2 * Mathf.PI / 360.0f, 75 * 2 * Mathf.PI / 360.0f));
                                                moveDirChangeTimer = 60;
                                            }
                                            sprint = false;
                                        }
                                    }
                                }

                                if (robotController.Grounded)
                                    state = State.Ground;

                                if (target_angle <= 100)
                                    allow_fire = true;

                                if (mindist < 20.0f)
                                    allow_infight = true;
                              
                            }
                            break;
                        case State.Decend:
                            {
                                if (robotController.Grounded)
                                {
                                    state = State.Ground;
                                }

                                /*if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.NextDrive))
                                {
                                    if (robotController.boost > robotController.Boost_Max*3/4)
                                    {
                                        state = State.Ascend;
                                    }
                                }
                                else*/
                                {
                                    if (robotController.boost > robotController.robotParameter.Boost_Max / 2)
                                    {
                                        state = State.Ascend;
                                    }
                                }

                                if(floorhit.distance > 25.0f && target_angle <= 90)
                                    allow_fire = true;

                                if (mindist < 20.0f)
                                    allow_infight = true;

                               
                            }
                            break;
                    }

                    if (robotController.lowerBodyState == RobotController.LowerBodyState.AirSlash
                        || robotController.lowerBodyState == RobotController.LowerBodyState.GroundSlash
                        || robotController.lowerBodyState == RobotController.LowerBodyState.QuickSlash
                        || robotController.lowerBodyState == RobotController.LowerBodyState.LowerSlash
                        || robotController.lowerBodyState == RobotController.LowerBodyState.DashSlash)
                    {
                        if(robotController.slash_count == robotController.Sword.slashMotionInfo[robotController.lowerBodyState].num-1)
                            infight_reload = 60;
                    }
                    else if(infight_reload > 0)
                        infight_reload--;

                    if (robotController.lowerBodyState == RobotController.LowerBodyState.JumpSlash)
                        jumpinfight_reload = 90;
                    else if(jumpinfight_reload > 0)
                        jumpinfight_reload--;

                    /*if(robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.ExtremeSlide) && robotController.lowerBodyState == RobotController.LowerBodyState.JUMPSLASH_GROUND)
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
                        if (robotController.fire_followthrough > 0 && robotController.rightWeapon.canHold)
                        {
                            fire = true;
                        }
                        else
                        {
                            if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.NextDrive))
                            {
                                fire = allow_fire;
                            }
                            else if (fire_wait <= 0 && allow_fire)
                            {
                                if (mindist < lock_range)
                                {
                                    if (fire_prepare <= 0)
                                    {
                                        fire = true;
                                        fire_wait = Random.Range(60, 120);
                                        fire_prepare = 15;
                                    }
                                    else
                                    {
                                        fire_prepare--;
                                    }
                                }
                                else
                                {
                                    fire_wait = Random.Range(60, 120);
                                    fire_prepare = 15;
                                }
                            }
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

        return;
    }
}
