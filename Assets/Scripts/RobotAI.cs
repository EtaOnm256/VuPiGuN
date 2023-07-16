using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAI : InputBase
{
    int moveDirChangeTimer = 60;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //fire = true;
        slash = true;
        if(moveDirChangeTimer==0)
        {
            move = VectorUtil.rotate(new Vector2(1.0f, 0.0f), Random.Range(0, 360.0f));
            moveDirChangeTimer = 60;
        }
        //moveDirChangeTimer--;
    }
}
