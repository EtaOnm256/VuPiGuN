using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI : InputBase
{
    int moveDirChangeTimer = 60;

    RobotController robotController = null;

    void Awake()
    {
        robotController = GetComponent<RobotController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool boosting = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        //fire = true;
        //slash = true;

        /*if(boosting)
        {
            if (robotController.boost == 0)
                boosting = false;
        }
        else
        {
            if (robotController.boost == robotController.Boost_Max)
                boosting = true;
        }
           
        jump = boosting;

        if(moveDirChangeTimer==0)
        {
            move = VectorUtil.rotate(new Vector2(1.0f, 0.0f), Random.Range(0, 360.0f));
            moveDirChangeTimer = 60;
        }
        moveDirChangeTimer--;*/
    }
}
