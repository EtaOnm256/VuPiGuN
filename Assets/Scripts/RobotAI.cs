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
        
    }

    bool boosting = false;
    bool ascending = false;

    // Update is called once per frame
    void FixedUpdate()
    {

        //return;
        //fire = true;
        slash = true;
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
