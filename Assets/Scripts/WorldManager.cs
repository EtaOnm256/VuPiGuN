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
    public GameObject enemy_weapon_prefab = null;


    // Start is called before the first frame update
    void Start()
    {
        Team friend_team = new Team { robotControllers = new List<RobotController>() };
        Team enemy_team = new Team { robotControllers = new List<RobotController>() };
  

        SpawnEnemy(new Vector3(-20, 0, 40), Quaternion.Euler(0.0f, 180.0f, 0.0f), enemy_team);
        SpawnEnemy(new Vector3(20, 0, 40), Quaternion.Euler(0.0f, 180.0f, 0.0f), enemy_team);
     

        teams.Add(friend_team);
        teams.Add(enemy_team);


    }

    public void HandleEnemyAdd(RobotController robotController)
    {
        foreach (var team in teams)
        {
            if (team == robotController.team) continue;

            foreach (var robot in team.robotControllers)
            {
                robot.OnEnemyAdded(robotController);
                robotController.OnEnemyAdded(robot);
            }
        }
    }

    public void HandleEnemyRemove(RobotController robotController)
    {
        foreach (var team in teams)
        {
            if (team == robotController.team) continue;

            foreach (var robot in team.robotControllers)
            {
                robot.OnEnemyRemoved(robotController);
                robotController.OnEnemyRemoved(robot);
            }
        }
    }


    public void AssignToTeam(RobotController robotController)
    {
        player = robotController;

        robotController.team = teams[0];

        teams[0].robotControllers.Add(robotController);
        HandleEnemyAdd(robotController);


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(player==null)
        {
            GameObject player_obj = GameObject.Instantiate(enemy_prefab, new Vector3(0, 0, -40), Quaternion.Euler(0.0f, 0.0f, 0.0f));


            player = player_obj.GetComponent<RobotController>();

            player.worldManager = this;
          
            player.team = teams[0];

            teams[0].robotControllers.Add(player);
            HandleEnemyAdd(player);
        }

       
    }

    RobotController player;

    public void HandleRemoveUnit(RobotController robotController)
    {
        HandleEnemyRemove(robotController);


        robotController.team.robotControllers.Remove(robotController);

        if (player == robotController)
            player = null;
    }

    private void SpawnEnemy(Vector3 pos, Quaternion rot, Team team)
    {
        RaycastHit raycastHit;

        Physics.Raycast(pos+new Vector3(0.0f,100.0f,0.0f), -Vector3.up,out raycastHit, float.MaxValue, 1 << 3);

        GameObject enemy = GameObject.Instantiate(enemy_prefab, raycastHit.point, rot);
        GameObject enemyweapon = GameObject.Instantiate(enemy_weapon_prefab, raycastHit.point, rot);

        RobotController robotController = enemy.GetComponent<RobotController>();

        robotController.worldManager = this;
        robotController._input = enemy.AddComponent<RobotAI>();

        enemyweapon.transform.parent = robotController.RHand.transform;
        enemyweapon.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
        enemyweapon.transform.localEulerAngles = new Vector3(-90, 0, 180);
        enemyweapon.transform.localScale = new Vector3(1, 1, 1);

        robotController.rightWeapon = enemyweapon.GetComponent<Weapon>();

        DestroyImmediate(enemy.GetComponent<HumanInput>());
        DestroyImmediate(enemy.GetComponent<UnityEngine.InputSystem.PlayerInput>());

        robotController.team = team;

        team.robotControllers.Add(robotController);
        HandleEnemyAdd(robotController);
    }
}
