using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public class Team
    {
        public List<RobotController> robotControllers;
    }

    public List<Team> teams = new List<Team>();

    public GameObject enemy_prefab = null;

    // Start is called before the first frame update
    void Start()
    {
        Team friend_team = new Team { robotControllers = new List<RobotController>() };
        Team enemy_team = new Team { robotControllers = new List<RobotController>() };

       
        GameObject enemy = GameObject.Instantiate(enemy_prefab, new Vector3(20, 0, 40), Quaternion.Euler(0.0f, 180.0f, 0.0f));

       
        RobotController robotController = enemy.GetComponent<RobotController>();

        robotController.worldManager = this;
        robotController._input = enemy.AddComponent<RobotAI>();

        robotController.team = enemy_team;

        enemy_team.robotControllers.Add(robotController);

        enemy = GameObject.Instantiate(enemy_prefab, new Vector3(-20, 0, 40), Quaternion.Euler(0.0f, 180.0f, 0.0f));


        robotController = enemy.GetComponent<RobotController>();

        robotController.worldManager = this;
        robotController._input = enemy.AddComponent<RobotAI>();

        robotController.team = enemy_team;

        enemy_team.robotControllers.Add(robotController);

        teams.Add(friend_team);
        teams.Add(enemy_team);


    }

    public void AssignToTeam(RobotController robotController)
    {
        robotController.team = teams[0];

        teams[0].robotControllers.Add(robotController);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var team in teams)
        {
            
        }
    }
}
