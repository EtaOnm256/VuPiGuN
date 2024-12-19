using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public class Team
    {
        public List<RobotController> robotControllers;
        public List<Projectile> projectiles;
    }

    public List<Team> teams = new List<Team>();

    /*public GameObject player_prefab = null;
    public GameObject player_rweapon_prefab = null;
    public GameObject player_lweapon_prefab = null;
    public GameObject player_subweapon_prefab = null;
    public bool player_weapon_chest_paired = false;*/

    public GameObject player_variant;

   /* public GameObject enemy_prefab = null;
    public GameObject enemy_rweapon_prefab = null;
    public GameObject enemy_lweapon_prefab = null;
    public GameObject enemy_subweapon_prefab = null;
    public bool enemy_weapon_chest_paired = false;*/

  
    [System.Serializable]
    public class Sequence
    {
        [System.Serializable]
        public class OneSpawn
        {
            public GameObject variant;
            public Vector3 pos;
            public Quaternion rot;
            public int squadCount;
        }

  

        public List<OneSpawn> spawns;
    }

    public Sequence sequence;
    public int currentSpawn = -1;
    public bool spawned = false;

    // Start is called before the first frame update
    void Start()
    {
        Team friend_team = new Team { robotControllers = new List<RobotController>(), projectiles = new List<Projectile>() };
        Team enemy_team = new Team { robotControllers = new List<RobotController>(), projectiles = new List<Projectile>() };

        teams.Add(friend_team);
        teams.Add(enemy_team);

        SpawnPlayer(new Vector3(0, 0, -40),Quaternion.Euler(0.0f, 0.0f, 0.0f), friend_team);

        SpawnEnemy(player_variant, new Vector3(20, 0, -40), Quaternion.Euler(0.0f, 0.0f, 0.0f), friend_team);
        //SpawnEnemy(sequence.waves[0].spawns[0].variant, new Vector3(-20, 0, -40), Quaternion.Euler(0.0f, 0.0f, 0.0f), friend_team);

        currentSpawn = 0;


        //SpawnEnemy(new Vector3(-20, 0, 40), Quaternion.Euler(0.0f, 180.0f, 0.0f), enemy_team);
        //SpawnEnemy(new Vector3(20, 0, 40), Quaternion.Euler(0.0f, 180.0f, 0.0f), enemy_team);





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
        //player = robotController;

        robotController.team = teams[0];

        teams[0].robotControllers.Add(robotController);
        HandleEnemyAdd(robotController);


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /* if(player==null)
         {
             GameObject player_obj = GameObject.Instantiate(enemy_prefab, new Vector3(0, 0, -40), Quaternion.Euler(0.0f, 0.0f, 0.0f));


             player = player_obj.GetComponent<RobotController>();

             player.worldManager = this;

             player.team = teams[0];

             teams[0].robotControllers.Add(player);
             HandleEnemyAdd(player);
         }*/

        if (currentSpawn < sequence.spawns.Count)
        {
            if (spawned)
            {
                if (teams[1].robotControllers.Count < sequence.spawns[currentSpawn].squadCount)
                {
                    currentSpawn++;
                    spawned = false;
                }
            }
            else
            {
                if (teams[1].robotControllers.Count < sequence.spawns[currentSpawn].squadCount)
                {
                    Sequence.OneSpawn spawn = sequence.spawns[currentSpawn];

                    SpawnEnemy(spawn.variant, spawn.pos, spawn.rot, teams[1]);
                    spawned = true;
                }
            }
        }
       
    }

    //RobotController player;

    public void HandleRemoveUnit(RobotController robotController)
    {
        HandleEnemyRemove(robotController);


        robotController.team.robotControllers.Remove(robotController);

        //if (player == robotController)
        //    player = null;
    }

    private void SpawnPlayer(Vector3 pos, Quaternion rot, Team team)
    {
        /*   RaycastHit raycastHit;

          Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, 1 << 3);

         GameObject player = GameObject.Instantiate(player_prefab, raycastHit.point, rot);



          RobotController robotController = player.GetComponent<RobotController>();

          robotController.HUDCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
          robotController.uIController_Overlay = robotController.HUDCanvas.GetComponent<UIController_Overlay>(); ;
          robotController.is_player = true;
          robotController.CinemachineCameraTarget = GameObject.Find("Main Camera");

          robotController.worldManager = this;

          if (player_rweapon_prefab != null)
          {
              GameObject playerrweapon = GameObject.Instantiate(player_rweapon_prefab, raycastHit.point, rot);

              playerrweapon.transform.parent = robotController.RHand.transform;
              playerrweapon.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
              playerrweapon.transform.localEulerAngles = new Vector3(-90, 0, 180);
              playerrweapon.transform.localScale = new Vector3(1, 1, 1);

              robotController.rightWeapon = playerrweapon.GetComponent<Weapon>();
          }

          if (player_lweapon_prefab != null)
          {
              GameObject playerlweapon = GameObject.Instantiate(player_lweapon_prefab, raycastHit.point, rot);

              playerlweapon.transform.parent = robotController.LHand.transform;
              playerlweapon.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
              playerlweapon.transform.localEulerAngles = new Vector3(-90, 0, 180);
              playerlweapon.transform.localScale = new Vector3(1, 1, 1);

              robotController.Sword = playerlweapon.GetComponent<InfightWeapon>();
          }

          if (player_subweapon_prefab != null)
          {
              if (player_weapon_chest_paired)
              {
                  GameObject playersubweapon_r = GameObject.Instantiate(player_subweapon_prefab, raycastHit.point, rot);

                  playersubweapon_r.transform.parent = robotController.chestWeapon_anchor[1].transform;
                  playersubweapon_r.transform.localPosition = Vector3.zero;
                  playersubweapon_r.transform.localEulerAngles = Vector3.zero;
                  playersubweapon_r.transform.localScale = Vector3.one;

                  playersubweapon_r.GetComponent<Weapon>().this_is_slave = true;

                  GameObject playersubweapon_l = GameObject.Instantiate(player_subweapon_prefab, raycastHit.point, rot);

                  playersubweapon_l.transform.parent = robotController.chestWeapon_anchor[0].transform;
                  playersubweapon_l.transform.localPosition = Vector3.zero;
                  playersubweapon_l.transform.localEulerAngles = Vector3.zero;
                  playersubweapon_l.transform.localScale = Vector3.one;
                  playersubweapon_l.GetComponent<Weapon>().this_is_slave = false;
                  playersubweapon_l.GetComponent<Weapon>().another = playersubweapon_l.GetComponent<Weapon>();

                  robotController.shoulderWeapon = playersubweapon_l.GetComponent<Weapon>();


              }
              else
              {
                  GameObject playersubweapon_l = GameObject.Instantiate(player_subweapon_prefab, raycastHit.point, rot);

                  playersubweapon_l.transform.parent = robotController.chestWeapon_anchor[0].transform;
                  playersubweapon_l.transform.localPosition = Vector3.zero;
                  playersubweapon_l.transform.localEulerAngles = Vector3.zero;
                  playersubweapon_l.transform.localScale = Vector3.one;

                  robotController.shoulderWeapon = playersubweapon_l.GetComponent<Weapon>();
              }

          }

          //DestroyImmediate(enemy.GetComponent<HumanInput>());
          //DestroyImmediate(enemy.GetComponent<UnityEngine.InputSystem.PlayerInput>());
      */

        RobotVariant variant = player_variant.GetComponent<RobotVariant>();

        RaycastHit raycastHit;

        Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, 1 << 3);

        RobotController robot = variant.Spawn(raycastHit.point, rot);

        robot.HUDCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
        robot.uIController_Overlay = robot.HUDCanvas.GetComponent<UIController_Overlay>(); ;
        robot.is_player = true;

        robot.CinemachineCameraTarget = GameObject.Find("Main Camera");

        robot.worldManager = this;
        robot.team = team;

        team.robotControllers.Add(robot);
        HandleEnemyAdd(robot);
    }

    private void SpawnEnemy(GameObject variant_obj,Vector3 pos, Quaternion rot, Team team)
    {
        /*RaycastHit raycastHit;

        Physics.Raycast(pos+new Vector3(0.0f,500.0f,0.0f), -Vector3.up,out raycastHit, float.MaxValue, 1 << 3);

        GameObject enemy = GameObject.Instantiate(enemy_prefab, raycastHit.point, rot);
      

        RobotController robotController = enemy.GetComponent<RobotController>();

        robotController.worldManager = this;
        robotController.HUDCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
        robotController.uIController_Overlay = robotController.HUDCanvas.GetComponent<UIController_Overlay>(); ;
        //robotController._input = enemy.AddComponent<RobotAI_Leopard>();
        robotController._input = enemy.AddComponent<RobotAI_Medium>();

        if (enemy_rweapon_prefab!=null)
        {
            GameObject enemyrweapon = GameObject.Instantiate(enemy_rweapon_prefab, raycastHit.point, rot);

            enemyrweapon.transform.parent = robotController.RHand.transform;
            enemyrweapon.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
            enemyrweapon.transform.localEulerAngles = new Vector3(-90, 0, 180);
            enemyrweapon.transform.localScale = new Vector3(1, 1, 1);

            robotController.rightWeapon = enemyrweapon.GetComponent<Weapon>();
        }

        if(enemy_lweapon_prefab != null)
        {
            GameObject enemylweapon = GameObject.Instantiate(enemy_lweapon_prefab, raycastHit.point, rot);

            enemylweapon.transform.parent = robotController.LHand.transform;
            enemylweapon.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
            enemylweapon.transform.localEulerAngles = new Vector3(-90, 0, 180);
            enemylweapon.transform.localScale = new Vector3(1, 1, 1);

            robotController.Sword = enemylweapon.GetComponent<InfightWeapon>();
        }

        if (enemy_subweapon_prefab != null)
        {
            if (enemy_weapon_chest_paired)
            {
                GameObject enemysubweapon_r = GameObject.Instantiate(enemy_subweapon_prefab, raycastHit.point, rot);

                enemysubweapon_r.transform.parent = robotController.chestWeapon_anchor[1].transform;
                enemysubweapon_r.transform.localPosition = Vector3.zero;
                enemysubweapon_r.transform.localEulerAngles = Vector3.zero;
                enemysubweapon_r.transform.localScale = Vector3.one;

                enemysubweapon_r.GetComponent<Weapon>().this_is_slave = true;

                GameObject enemysubweapon_l = GameObject.Instantiate(enemy_subweapon_prefab, raycastHit.point, rot);

                enemysubweapon_l.transform.parent = robotController.chestWeapon_anchor[0].transform;
                enemysubweapon_l.transform.localPosition = Vector3.zero;
                enemysubweapon_l.transform.localEulerAngles = Vector3.zero;
                enemysubweapon_l.transform.localScale = Vector3.one;
                enemysubweapon_l.GetComponent<Weapon>().this_is_slave = false;
                enemysubweapon_l.GetComponent<Weapon>().another = enemysubweapon_r.GetComponent<Weapon>();

                robotController.shoulderWeapon = enemysubweapon_l.GetComponent<Weapon>();

         
            }
            else
            {



                GameObject enemysubweapon_l = GameObject.Instantiate(enemy_subweapon_prefab, raycastHit.point, rot);

                enemysubweapon_l.transform.parent = robotController.chestWeapon_anchor[0].transform;
                enemysubweapon_l.transform.localPosition = Vector3.zero;
                enemysubweapon_l.transform.localEulerAngles = Vector3.zero;
                enemysubweapon_l.transform.localScale = Vector3.one;

                robotController.shoulderWeapon = enemysubweapon_l.GetComponent<Weapon>();
            }
            
        }
        DestroyImmediate(enemy.GetComponent<HumanInput>());
        DestroyImmediate(enemy.GetComponent<UnityEngine.InputSystem.PlayerInput>());
*/
        RobotVariant variant = variant_obj.GetComponent<RobotVariant>();

        RaycastHit raycastHit;

        Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, 1 << 3);

        RobotController robot = variant.Spawn(raycastHit.point, rot);

        robot.HUDCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
        robot.uIController_Overlay = robot.HUDCanvas.GetComponent<UIController_Overlay>(); ;
        robot.is_player = false;
        DestroyImmediate(robot.GetComponent<HumanInput>());
        DestroyImmediate(robot.GetComponent<UnityEngine.InputSystem.PlayerInput>());
        robot._input = robot.gameObject.AddComponent<RobotAI_Medium>();

        robot.worldManager = this;
        robot.team = team;

        team.robotControllers.Add(robot);
        HandleEnemyAdd(robot);
    }

    public void AddProjectile(Projectile projectile,Team team)
    {
        projectile.team = team;
        team.projectiles.Add(projectile);
    }

    public void RemoveProjectile(Projectile projectile)
    {
        projectile.team.projectiles.Remove(projectile);
    }
}
