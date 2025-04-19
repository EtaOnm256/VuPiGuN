using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Base : InputBase
{
    protected RobotController robotController = null;
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
            if (rel.x > 0.0f)
                stepMove.y = 1.0f;
            else
                stepMove.y = -1.0f;
        }
        else
        {
            if (rel.z > 0.0f)
                stepMove.x = -1.0f;
            else
                stepMove.x = 1.0f;
        }

        return stepMove;
    }

    protected RobotController current_target = null;

    protected void DetermineTarget()
    {
        if (current_target == null || !current_target)
        {
            TargetNearest();
        }
    }
    public override void OnTakeDamage(Vector3 pos, Vector3 dir, int damage, RobotController.KnockBackType knockBackType, RobotController dealer)
    {
        if (dealer != null && dealer)
            current_target = dealer;
    }

    protected void TargetNearest()
    {
        float mindist = float.MaxValue;

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
                    current_target = robot;
                }
            }

        }
    }
}
