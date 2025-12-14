using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Easy : RobotAI_Base
{
    [SerializeField] bool dummy;

    bool dodge_ready;

    int moveDirChangeTimer = 0;



  

    // Start is called before the first frame update
    void Start()
    {
        fire_wait = Random.Range(30, 60);
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

                        //if (!overheating)
                        //    sprint = true;

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

                                //if (!overheating)
                                //    sprint = true;
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
                    jump = false;

                    if (fire_wait <= 0 && robotController.team.orderToAI != WorldManager.OrderToAI.EVADE)
                    {
                        if (mindist < 100.0f)
                        {
                            if (fire_prepare <= 0)
                            {
                                fire = true;
                                fire_wait = Random.Range(30, 60);
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
                            fire_wait = Random.Range(30, 60);
                            fire_prepare = 15;
                        }
                    }
                    else
                    {

                        bool dodge = false;
                        ProcessDodge(out dodge, out stepMove, targetQ);

                        if (dummy)
                        {
                            if (robotController.lowerBodyState == RobotController.LowerBodyState.STEPGROUND)
                                dodge_ready = false;

                            if (!dodge_ready)
                                dodge = false;
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
                                if(robotController.team.orderToAI == WorldManager.OrderToAI.EVADE)
                                {
                                    if (moveDirChangeTimer <= 0)
                                    {
                                         move = VectorUtil.rotate(new Vector2(0.0f, -1.0f), Random.Range(-45.0f * Mathf.Deg2Rad, 45.0f * Mathf.Deg2Rad));
                                         moveDirChangeTimer = 60;
                                    }
                                }
                                else
                                {
                                    if (moveDirChangeTimer <= 0)
                                    {
                                        move = VectorUtil.rotate(new Vector2(1.0f, 0.0f), Random.Range(0, 360.0f*Mathf.Deg2Rad));
                                        moveDirChangeTimer = 60;
                                    }
                                }
                               
                            }
                        }

                        fire_wait--;
                        moveDirChangeTimer--;
                        prev_dodge = dodge;
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

        bool coli = Physics.Raycast(robotController.transform.position, robotController.cameraRotation*move_pos, 10.0f,WorldManager.layerPattern_Building);

        if(coli)
        {
            move = -move;
        }

        moveDirChangeTimer--;

        if (robotController.Target_Robot != null)
            robotController.cameraRotation = robotController.GetTargetQuaternionForView(robotController.Target_Robot);*/
    }

    public override void OnTakeDamage(Vector3 pos, Vector3 dir, int damage, RobotController.KnockBackType knockBackType, RobotController dealer)
    {
        if (dummy)
            dodge_ready = true;

        base.OnTakeDamage(pos, dir, damage, knockBackType, dealer);
    }
}
