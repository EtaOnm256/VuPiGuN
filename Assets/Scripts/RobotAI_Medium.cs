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

    bool overheating = false;

    public enum State
    {
        Ground,
        Ascend,
        Dash,
        Decend
    }

    public State state = State.Ground;

    // Update is called once per frame
    void FixedUpdate()
    {
        float mindist = float.MaxValue;
        
        RobotController nearest_robot = null;

        foreach (var team in robotController.worldManager.teams)
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

            if (robotController.upperBodyState == RobotController.UpperBodyState.FIRE)
            {
                
            }
            else
            {
                bool Aim_blocked;
                {
                    RaycastHit hit;
                    Aim_blocked = Physics.Raycast(robotController.GetCenter(), targetQ * Vector3.forward, out hit, mindist, 1 << 3);

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
                            if (hit.distance < 10.0f)
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
                    jump = false;

                    switch (state)
                    {
                        case State.Ground:
                            {
                                bool dodge = false;

                                foreach (var team in robotController.worldManager.teams)
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
                                    sprint = true;
                                    moveDirChangeTimer = 60;
                                }
                                else
                                {
                                    if (mindist > 75.0f)
                                    {
                                        move.y = 1.0f;
                                        move.x = 0.0f;
                                        //moveDirChangeTimer = 60;
                                    }
                                    else
                                    {


                                        if (moveDirChangeTimer <= 0)
                                        {
                                            move = VectorUtil.rotate(new Vector2(1.0f, 0.0f), Random.Range(0, 360.0f));
                                            moveDirChangeTimer = 60;
                                        }
                                    }

                                    if (robotController.boost >= robotController.Boost_Max)
                                    {
                                        state = State.Ascend;
                                    }
                                }

                                if (!robotController.Grounded)
                                    state = State.Decend;
                            }
                            break;
                        case State.Ascend:
                            {
                                jump = true;

                                RaycastHit hit;
                                bool ground = Physics.Raycast(robotController.GetCenter(), Vector3.down, out hit, 50.0f, 1 << 3);

                                if (hit.distance > 15.0f)
                                {
                                    state = State.Dash;
                                }

                                if (overheating)
                                    state = State.Decend;

                                if (robotController.Grounded)
                                    state = State.Ground;

                            }
                            break;
                        case State.Dash:
                            {
                                sprint = true;

                                if (mindist > 75.0f)
                                {
                                    move.y = 1.0f;
                                    move.x = 0.0f;
                                    //moveDirChangeTimer = 60;
                                }
                                else
                                {


                                    if (moveDirChangeTimer <= 0)
                                    {
                                        move = VectorUtil.rotate(new Vector2(1.0f, 0.0f), Random.Range(0, 360.0f));
                                        moveDirChangeTimer = 60;
                                    }
                                }

                                if (overheating)
                                    state = State.Decend;

                                if (robotController.Grounded)
                                    state = State.Ground;
                            }
                            break;
                        case State.Decend:
                            {
                                if (robotController.Grounded)
                                    state = State.Ground;
                            }
                            break;
                    }
               

                    fire_wait--;
                    moveDirChangeTimer--;
                }
            }
        }


        return;
    }
}
