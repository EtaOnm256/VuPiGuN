using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotPrepare : MonoBehaviour
{
    [ContextMenu("Execute")]
    private void Execute()
    {
        Camera.main.transform.SetPositionAndRotation(transform.position, transform.rotation);

        foreach(var team in WorldManager.current_instance.teams)
        {
            foreach(var robot in team.robotControllers)
            {
                robot.gameObject.SetActive(false);
            }
        }

        GameObject.Find("Canvas").SetActive(false);
    }
}
