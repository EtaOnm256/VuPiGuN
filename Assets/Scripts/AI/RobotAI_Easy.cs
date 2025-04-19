using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Easy : RobotAI_Base
{
    int moveDirChangeTimer = 0;



  

    // Start is called before the first frame update
    void Start()
    {
        fire_wait = Random.Range(60, 120);
    }

    bool boosting = false;
    bool ascending = false;

    public int ascend_margin = 0;

    public int fire_wait;
    public int fire_prepare = 15;

    bool overheating = false;
  

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

            /*Vector3 move_dir = robotController.cameraRotation * Vector3.forward;

            move_dir.y = 0.0f;
            move_dir = move_dir.normalized;*/

            fire = false;
            sprint = false;

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

            if (robotController.upperBodyState == RobotController.UpperBodyState.FIRE)
            {
                move = Vector2.zero;
            }
            else
            {
                RaycastHit hit;
                bool Aim_blocked = Physics.Raycast(robotController.GetCenter(), targetQ * Vector3.forward, out hit, mindist, 1 << 3);

                if(ascending)
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

                if (Aim_blocked)
                {
                    move.y = 1.0f;
                    move.x = 0.0f;

                    if (overheating || !ascending)
                    {
                        jump = false;
                    }
                    else
                    {
                        jump = true;
                    }
                }
                else
                {
                    jump = false;

                    if (fire_wait <= 0)
                    {
                        if (mindist < 100.0f)
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
                    else
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
                        }

                        fire_wait--;
                        moveDirChangeTimer--;
                    }
                }
            }
        }


        return;
      /*  fire = true;
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
            robotController.cameraRotation = robotController.GetTargetQuaternionForView(robotController.Target_Robot);*/
    }
}
