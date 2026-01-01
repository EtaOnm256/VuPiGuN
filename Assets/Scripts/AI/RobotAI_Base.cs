using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI_Base : InputBase
{
    protected RobotController robotController = null;
    protected bool prev_dodge = false;
    protected Vector2 prev_stepMove = Vector2.zero;

    void Awake()
    {
        robotController = GetComponent<RobotController>();
    }

    protected Vector2 ThreatPosToStepMove_Strafe(Vector3 pos, Quaternion targetQ)
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

    protected Vector2 ThreatPosToStepMove_Back(Vector3 pos, Quaternion targetQ)
    {
        Vector2 stepMove = Vector2.zero;
        Vector3 rel = Quaternion.Inverse(targetQ) * (pos - robotController.GetCenter());

        if (Mathf.Abs(rel.x) > Mathf.Abs(rel.z))
        {
            if (rel.x > 0.0f)
                stepMove.x = -1.0f;
            else
                stepMove.x = 1.0f;
        }
        else
        {
            if (rel.z > 0.0f)
                stepMove.y = -1.0f;
            else
                stepMove.y = 1.0f;
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

    protected bool Aiming_Precise()
    {
        Vector3 firing = (robotController.virtual_targeting_position_forBody - robotController.GetCenter()).normalized;
        Vector3 target = current_target.GetCenter() - robotController.GetCenter();

        return Vector3.Cross(firing, target).magnitude < 5.0f;
    }

    protected float infight_dist = 20.0f;
    protected float jumpslash_dist = 40.0f;
    protected float dashslash_dist = 40.0f;
    protected float horizon_dist = 20.0f;
    protected bool Slash_Precise(float mindist)
    {
        if(robotController.subState_Slash != RobotController.SubState_Slash.DashSlash)
            return current_target.mirage_time <= 0 && mindist < infight_dist;
        else
            return current_target.mirage_time <= 0 && mindist < dashslash_dist;
    }

    protected void ProcessDodge(out bool dodge,out Vector2 stepMove,Quaternion targetQ)
    {
        dodge = false;
        stepMove = Vector2.zero;

        float evade_thresh;

        if (!robotController.Grounded)
            evade_thresh = 40.0f;
        else
            evade_thresh = 30.0f;

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
                    && (shift < 3.0f || projectile.trajectory == Weapon.Trajectory.Curved)
                    && dist / projectile.speed < evade_thresh
                    )
                {
                    dodge = true;

                    if (!prev_dodge)
                        prev_stepMove = ThreatPosToStepMove_Strafe(projectile.transform.position, targetQ);
                    
                    stepMove = prev_stepMove;

                    break;
                }
            }

            foreach (var robot in team.robotControllers)
            {
                if (robot.dead || robot.Target_Robot != robotController || robot.event_acceptnextslash)
                    continue;

                if ((robot.GetCenter() - robotController.GetCenter()).magnitude > 20.0f)
                    continue;

                if (robot.lowerBodyState == RobotController.LowerBodyState.SWEEP)
                {
                    dodge = true;

                    if (!prev_dodge)
                        prev_stepMove = ThreatPosToStepMove_Back(robot.GetCenter(), targetQ);

                    stepMove = prev_stepMove;

                    break;
                }

                if ((robot.GetCenter() - robotController.GetCenter()).magnitude > 10.0f)
                    continue;

                if (robot.lowerBodyState == RobotController.LowerBodyState.SLASH
                    || robot.lowerBodyState == RobotController.LowerBodyState.SLASH_DASH
                    || robot.lowerBodyState == RobotController.LowerBodyState.JumpSlash
                    || robot.lowerBodyState == RobotController.LowerBodyState.JumpSlash_Jump)
                {
                    dodge = true;

                    if (!prev_dodge)
                        prev_stepMove = ThreatPosToStepMove_Strafe(robot.GetCenter(), targetQ);

                    stepMove = prev_stepMove;
                    break;
                }
            }
        }
    }
}
