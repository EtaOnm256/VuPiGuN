using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Medium : InputBase
{
    int moveDirChangeTimer = 0;

    RobotController robotController = null;

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

    public int ground_step_remain = 2;
    public enum State
    {
        Ground,
        Ascend,
        Dash,
        Decend
    }

    public State state = State.Ground;

    public float movedirection_range = 90.0f;
    //public float movedirection_range = 180.0f;

    public float lock_range = 75.0f;
    //public float lock_range = 150.0f;

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        //return;
        float mindist = float.MaxValue;
        
        RobotController nearest_robot = null;

        foreach (var team in WorldManager.current_instance.teams)
        {
            if (team == robotController.team)
                continue;

            foreach (var robot in team.robotControllers)
            {
                float dist = (robotController.GetCenter() - robot.GetCenter()).magnitude;

                if (dist < mindist)
                {
                    mindist = dist;
                    nearest_robot = robot;
                }
            }

        }

        ringMenuDir = RobotController.RingMenuDir.Center;

        if (nearest_robot == null)
        {
           
        }
        else
        {

            Vector3 cameraAxis = robotController.GetTargetQuaternionForView(nearest_robot).eulerAngles;

            robotController._cinemachineTargetYaw = cameraAxis.y;
            robotController._cinemachineTargetPitch = cameraAxis.x;

            Quaternion targetQ = Quaternion.LookRotation(nearest_robot.GetCenter() - robotController.GetCenter(), Vector3.up);

            fire = false;
            sprint = false;
            slash = false;
            subfire = false;
            if (overheating)
            {
                if (robotController.boost >= robotController.Boost_Max)
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
                    float target_angle = Vector3.Angle(nearest_robot.Chest.transform.position - transform.position, transform.forward);

                    jump = false;

                    bool allow_fire = false;
                    bool allow_infight = false;

                    switch (state)
                    {
                        case State.Ground:
                            {
                                bool dodge = false;

                                foreach (var team in WorldManager.current_instance.teams)
                                {
                                    if (team == robotController.team)
                                        continue;

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
                                            break;
                                        }
                                    }
                                }

                                if (dodge)
                                {
                                    move.x = 1.0f;
                                    move.y = 0.0f;

                                    if (robotController.lowerBodyState == RobotController.LowerBodyState.STEP)
                                        sprint = true;
                                    else
                                        sprint = !prev_sprint;

                                    moveDirChangeTimer = 60;
                                }
                                else
                                {
                                    if (ground_step_remain > 0)
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
                                                move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(-movedirection_range * 2 * Mathf.PI / 360.0f, movedirection_range * 2 * Mathf.PI / 360.0f));
                                                moveDirChangeTimer = 60;
                                            }
                                        }

                                        if (robotController.boost >= robotController.Boost_Max)
                                        {
                                            jump = true;
                                        }
                                    }

                                    if (nearest_robot.Grounded && mindist < 20.0f)
                                        allow_infight = true;

                                    if (target_angle <= 90)
                                        allow_fire = true;
                                }

                                if (!robotController.Grounded)
                                    state = State.Ascend;
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
                                    //moveDirChangeTimer = 60;
                                }
                                else
                                {


                                    if (moveDirChangeTimer <= 0)
                                    {
                                        move = VectorUtil.rotate(new Vector2(0.0f, 1.0f), Random.Range(-movedirection_range * 2 * Mathf.PI / 360.0f, movedirection_range * 2 * Mathf.PI / 360.0f));
                                         moveDirChangeTimer = 60;
                                    }
                                }

                                if (overheating)
                                    state = State.Decend;

                                if (robotController.Grounded)
                                    state = State.Ground;

                                if (target_angle <= 90)
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
                                    ground_step_remain = 2;
                                }

                                if(floorhit.distance > 10.0f)
                                    allow_fire = true;

                                if (mindist < 20.0f)
                                    allow_infight = true;

                                if (robotController.shoulderWeapon != null
                                   && (robotController.shoulderWeapon.energy == robotController.shoulderWeapon.MaxEnergy
                                   || robotController.fire_followthrough > 0 && robotController.shoulderWeapon.canHold))
                                {
                                    subfire = true;
                                }
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
                                ringMenuDir = RobotController.RingMenuDir.Down;
                                slash = true;
                            }
                            else if (!prev_slash && infight_reload <= 0)
                            {
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

                            if (fire_wait <= 0 && allow_fire)
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
