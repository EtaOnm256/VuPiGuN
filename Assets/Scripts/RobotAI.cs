using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI : InputBase
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

    bool boosting = false;
    bool ascending = false;

    public int fire_wait;
    public int fire_prepare = 15;

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
                float dist = (robotController.GetCenter() - robot.GetCenter()).sqrMagnitude;

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
            //robotController.cameraRotation = robotController.GetTargetQuaternionForView(nearest_robot);

            Vector3 cameraAxis = robotController.GetTargetQuaternionForView(nearest_robot).eulerAngles;

            robotController._cinemachineTargetYaw = cameraAxis.y;
            robotController._cinemachineTargetPitch = cameraAxis.x;

            
            fire = false;
            sprint = false;
            if (robotController.upperBodyState == RobotController.UpperBodyState.FIRE)
            {
                move = Vector2.zero;
            }
            else
            {
                move = Vector2.zero;
                /*if (fire_wait <= 0)
                {
                    if (System.Math.Sqrt(mindist) < 100.0f)
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

                        move = Vector2.zero;
                        moveDirChangeTimer = 0;
                    }
                    else
                    {
                        fire_wait = Random.Range(60, 120);
                        fire_prepare = 15;
                    }
                }
                else*/
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
                                && dist/projectile.speed < 20.0f
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
                        sprint = true;
                        moveDirChangeTimer = 60;
                    }
                    /*else
                    {
                        if (System.Math.Sqrt(mindist) > 75.0f)
                        {
                            move.y = 1.0f;
                            //moveDirChangeTimer = 60;
                        }
                        else
                        {


                            if (moveDirChangeTimer == 0)
                            {
                                move = VectorUtil.rotate(new Vector2(1.0f, 0.0f), Random.Range(0, 360.0f));
                                moveDirChangeTimer = 60;
                            }
                        }
                    }*/
                    fire_wait--;
                    //moveDirChangeTimer--;
                }
            }
        }


        return;
        fire = true;
        //slash = true;
        subfire = false;
        if (boosting)
        {
            sprint = false;

            if (robotController.boost == 0)
            {
                boosting = false;
                //subfire = true;
            }

            if (ascending)
            {
                jump = true;
                if (robotController.transform.position.y > 10.0f)
                {
                    ascending = false;
                }
            }
            else
            {
                if (robotController.transform.position.y < 7.5f)
                {
                    ascending = true;
                }
                else
                {
                    jump = false;
                    sprint = true;
                }
            }
            
        }
        else
        {
            sprint = false;
            jump = false;

            if (robotController.boost == robotController.Boost_Max)
                boosting = true;
        }

       if(moveDirChangeTimer==0)
        {
            move = VectorUtil.rotate(new Vector2(1.0f, 0.0f), Random.Range(0, 360.0f));
            moveDirChangeTimer = 60;
        }



        Vector3 move_pos = new Vector3(move.x, 0.0f, move.y);

        bool coli = Physics.Raycast(robotController.transform.position, robotController.cameraRotation*move_pos, 10.0f,1 << 3);

        if(coli)
        {
            move = -move;
        }

        moveDirChangeTimer--;

        if (robotController.Target_Robot != null)
            robotController.cameraRotation = robotController.GetTargetQuaternionForView(robotController.Target_Robot);
    }
}
