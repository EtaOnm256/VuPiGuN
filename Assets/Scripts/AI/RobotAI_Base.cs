using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Base : InputBase
{
    protected RobotController robotController = null;
    protected bool prev_dodge = false;
    protected Vector2 stepMove = Vector2.zero;
    void Awake()
    {
        robotController = GetComponent<RobotController>();
    }

    protected Vector2 ThreatPosToStepMove(Vector3 pos, Quaternion targetQ)
    {
        Vector2 stepMove = Vector2.zero;
        Vector3 rel = Quaternion.Inverse(targetQ) * (pos - robotController.GetCenter());

        if (Mathf.Abs(rel.x) > Mathf.Abs(rel.z))
        {
            //if (rel.x > 0.0f)
            //    stepMove.y = 1.0f;
            //else
            //    stepMove.y = -1.0f;

            stepMove.y = Random.Range(0, 2) == 0 ? stepMove.y = 1.0f : stepMove.y = -1.0f;
        }
        else
        {
            //if (rel.z > 0.0f)
            //    stepMove.x = -1.0f;
            //else
            //    stepMove.x = 1.0f;

            stepMove.x = Random.Range(0, 2) == 0 ? stepMove.x = 1.0f : stepMove.x = -1.0f;
        }

        return stepMove;
    }

    protected RobotController current_target = null;

    public enum AI_Type
    {
        NORMAL,
        FRANK
    }

    [SerializeField] protected AI_Type type = AI_Type.NORMAL;

    protected void DetermineTarget()
    {
        switch (robotController.team.orderToAI)
        {
            case WorldManager.OrderToAI.NORMAL:

                /*if (type == AI_Type.FRANK)
                {
                    TargetBackstab();

                    if (current_target == null || !current_target)
                    {
                        TargetNearest(null);
                    }
                }
                else*/
                {
                    if (current_target == null || !current_target)
                    {
                        TargetNearest(null);
                    }
                }
                break;
            case WorldManager.OrderToAI.FOCUS:

                current_target = robotController.team.target_by_commander;

                if (current_target == null || !current_target)
                {
                    TargetNearest(null);
                }
                break;
            case WorldManager.OrderToAI.SPREAD:

                

                TargetNearest(robotController.team.target_by_commander);

                if (current_target == null || !current_target)
                {
                    TargetNearest(null);
                }
                break;
            case WorldManager.OrderToAI.EVADE:
                TargetNearest(null);
                break;
        }
    }
    public override void OnTakeDamage(Vector3 pos, Vector3 dir, int damage, RobotController.KnockBackType knockBackType, RobotController dealer)
    {
        if (robotController.team.orderToAI == WorldManager.OrderToAI.NORMAL)
        {
            if (dealer != null && dealer && dealer.team != robotController.team)
                current_target = dealer;
        }
    }

    protected void TargetNearest(RobotController exclude)
    {
        float mindist = float.MaxValue;

        foreach (var team in WorldManager.current_instance.teams)
        {
            if (team == robotController.team)
                continue;

            foreach (var robot in team.robotControllers)
            {
                if (robot == exclude)
                    continue;

                float dist = (robotController.GetCenter() - robot.GetCenter()).magnitude;

                if (dist < mindist)
                {
                    mindist = dist;
                    current_target = robot;
                }
            }

        }
    }

    protected void TargetBackstab()
    {
        current_target = null;

        float mindist = float.MaxValue;

        foreach (var team in WorldManager.current_instance.teams)
        {
            if (team == robotController.team)
                continue;

            foreach (var robot in team.robotControllers)
            {
                if (robot.Target_Robot == robotController)
                    continue;

                float dist = (robotController.GetCenter() - robot.GetCenter()).magnitude;

                if (dist < mindist)
                {
                    mindist = dist;
                    current_target = robot;
                }
            }

        }
    }

    protected bool IsStepDirectionCrossed(RobotController.StepDirection l,RobotController.StepDirection r)
    {
        if(l == RobotController.StepDirection.FORWARD || l== RobotController.StepDirection.BACKWARD)
            return r != RobotController.StepDirection.FORWARD && r != RobotController.StepDirection.BACKWARD;
        else
            return r == RobotController.StepDirection.FORWARD || r == RobotController.StepDirection.BACKWARD;
    }
}
