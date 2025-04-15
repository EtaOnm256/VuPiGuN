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
}
