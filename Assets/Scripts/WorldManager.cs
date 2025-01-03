using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour
{
    public class Team
    {
        public List<RobotController> robotControllers;
        public List<Projectile> projectiles;

        public Slider powerslider;

        public int _power;

        public int power
        {
            set
            {
                _power = value;
                if(powerslider != null)
                {
                    powerslider.value = _power;
                }
            }

            get { return _power; }
        }
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

    RobotController player;

    [SerializeField]GameObject spawn_prefab;

    [System.Serializable]
    public class Sequence
    {
        [System.Serializable]
        public class OneSpawn
        {
            public GameObject variant;
            //public Vector3 pos;
            //public Quaternion rot;
            public int squadCount;
            public bool loop;
        }

  

        public List<OneSpawn> spawns;

        public int currentSpawn = 0;
        public int loop_index = -1;
        public bool spawned = false;
    }

    public Sequence sequence_enemy;
    public Sequence sequence_friend;

    public Canvas HUDCanvas;
    public Canvas ResultCanvas;
    public bool finished = false;
    public bool victory = false;

    [SerializeField] GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        Slider friendPowerSlider = HUDCanvas.gameObject.transform.Find("FriendTeamPower").GetComponent<Slider>();
        Slider enemyPowerSlider = HUDCanvas.gameObject.transform.Find("EnemyTeamPower").GetComponent<Slider>();

        Team friend_team = new Team { robotControllers = new List<RobotController>(), projectiles = new List<Projectile>(),power = 1000,powerslider = friendPowerSlider };
        Team enemy_team = new Team { robotControllers = new List<RobotController>(), projectiles = new List<Projectile>(), power = 1000, powerslider = enemyPowerSlider };

        teams.Add(friend_team);
        teams.Add(enemy_team);

		SpawnPlayer(new Vector3(0, 0, -40),Quaternion.Euler(0.0f, 0.0f, 0.0f), friend_team);

        //SpawnNPC(player_variant, new Vector3(20, 0, -40), Quaternion.Euler(0.0f, 0.0f, 0.0f), friend_team);
        //SpawnEnemy(sequence_enemy.waves[0].spawns[0].variant, new Vector3(-20, 0, -40), Quaternion.Euler(0.0f, 0.0f, 0.0f), friend_team);

        for (int i = 0; i < sequence_enemy.spawns.Count; i++)
        {
            if (sequence_enemy.spawns[i].loop)
            {
                sequence_enemy.loop_index = i;
                break;
            }
        }

        for (int i = 0; i < sequence_friend.spawns.Count; i++)
        {
            if (sequence_friend.spawns[i].loop)
            {
                sequence_friend.loop_index = i;
                break;
            }
        }


        //SpawnEnemy(new Vector3(-20, 0, 40), Quaternion.Euler(0.0f, 180.0f, 0.0f), enemy_team);
        //SpawnEnemy(new Vector3(20, 0, 40), Quaternion.Euler(0.0f, 180.0f, 0.0f), enemy_team);





    }

    public void HandleRobotAdd(RobotController robotController)
    {
        foreach (var team in teams)
        {
            //if (team == robotController.team) continue;

            foreach (var robot in team.robotControllers)
            {
                robot.OnRobotAdded(robotController);
                robotController.OnRobotAdded(robot);
            }
        }
    }

    public void HandleRobotRemove(RobotController robotController)
    {
        foreach (var team in teams)
        {
            //if (team == robotController.team) continue;

            foreach (var robot in team.robotControllers)
            {
                robot.OnRobotRemoved(robotController);
                robotController.OnRobotRemoved(robot);
            }
        }
    }


    public void AssignToTeam(RobotController robotController)
    {
        robotController.team = teams[0];

        teams[0].robotControllers.Add(robotController);
        HandleRobotAdd(robotController);


    }

    void ProcessSpawn(Sequence sequence,Team team)
    {
        if (sequence.spawned)
        {
            if (team.robotControllers.Count < sequence.spawns[sequence.currentSpawn].squadCount)
            {
                if (sequence.currentSpawn < sequence.spawns.Count - 1)
                {
                    sequence.currentSpawn++;
                    sequence.spawned = false;
                }
                else
                {
                    if (sequence.loop_index != -1)
                    {
                        sequence.currentSpawn = sequence.loop_index;
                        sequence.spawned = false;
                    }
                }



            }
        }
        else
        {
            if (team.robotControllers.Count < sequence.spawns[sequence.currentSpawn].squadCount)
            {
                Sequence.OneSpawn spawn = sequence.spawns[sequence.currentSpawn];

                float distance;
                Quaternion rot;

                if (team == teams[0])
                {
                    distance = 25.0f;
                }
                else
                {
                    distance = 100.0f;
                }

                Vector3 pos;
                

                if(player && !player.dead)
                {
                    pos = player.GetCenter()+Quaternion.Euler(0.0f, Random.value * 360.0f, 0.0f)*Vector3.forward* distance;

                    if (team == teams[0])
                        rot = player.transform.rotation;
                    else
                        rot = Quaternion.LookRotation(player.GetCenter() - pos, Vector3.up);
                }
                else
                {
                    pos = Quaternion.Euler(0.0f, Random.value * 360.0f, 0.0f) * Vector3.forward * distance;
                    rot = Quaternion.LookRotation( - pos, Vector3.up);
                }

                if(pos.x >= 150.0f)
                {
                    pos.x -= distance*2;
                }
                else if (pos.x <= -150.0f)
                {
                    pos.x += distance * 2f;
                }

                if (pos.z >= 150.0f)
                {
                    pos.z -= distance * 2;
                }
                else if (pos.z <= -150.0f)
                {
                    pos.z += distance * 2;
                }

                SpawnNPC(spawn.variant,pos, rot, team);
                sequence.spawned = true;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (finished)
        {
            if(!ResultCanvas.gameObject.activeSelf)
            {
                ResultCanvas resultCanvas = ResultCanvas.GetComponent<ResultCanvas>();

                resultCanvas.power = teams[0].power;
                resultCanvas.victory = victory;
                ResultCanvas.gameObject.SetActive(true);

                HUDCanvas.gameObject.SetActive(false);
            }
        }
        else
        {
            if (player == null)
            {

                SpawnPlayer(new Vector3(0, 0, -40), Quaternion.Euler(0.0f, 0.0f, 0.0f), teams[0]);
            }

            ProcessSpawn(sequence_friend, teams[0]);
            ProcessSpawn(sequence_enemy, teams[1]);
        }




    }

    //RobotController player;

    public void HandleRemoveUnit(RobotController robotController)
    {
        HandleRobotRemove(robotController);

        robotController.team.power = System.Math.Max(0, robotController.team.power - robotController.Cost);

        if(robotController.team.power <= 0)
        {
            finished = true;

            victory = robotController.team != teams[0];
            
        }

        robotController.team.robotControllers.Remove(robotController);

        if (player == robotController)
            player = null;
    }

    private void SpawnPlayer(Vector3 pos, Quaternion rot, Team team)
    {
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

        GameObject.Instantiate(spawn_prefab, robot.transform.position,Quaternion.identity);

        player = robot;

        team.robotControllers.Add(robot);
        HandleRobotAdd(robot);
    }

    private void SpawnNPC(GameObject variant_obj,Vector3 pos, Quaternion rot, Team team)
    {
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

        GameObject.Instantiate(spawn_prefab, robot.transform.position, Quaternion.identity);

        team.robotControllers.Add(robot);
        HandleRobotAdd(robot);
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
