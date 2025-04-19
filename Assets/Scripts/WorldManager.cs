using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour
{
    static public WorldManager current_instance = null;
    public class Team
    {
        public List<RobotController> robotControllers = new List<RobotController>();
        public List<Projectile> projectiles = new List<Projectile>();
        public List<Spawning> spawnings = new List<Spawning>();

        public Slider powerslider;

        public int _power;

        public int power
        {
            set
            {
                _power = value;
                if (powerslider != null)
                {
                    powerslider.value = _power;
                }
            }

            get { return _power; }
        }

        public class Spawning 
        {
            public Vector3 pos;
            public Quaternion rot;
            public bool player;
            public int wait;
            public GameObject variant;
            public bool boss;
        }
    }

    public List<Pausable> pausables = new List<Pausable>();

    public List<Team> teams = new List<Team>();

    /*public GameObject player_prefab = null;
    public GameObject player_rweapon_prefab = null;
    public GameObject player_lweapon_prefab = null;
    public GameObject player_subweapon_prefab = null;
    public bool player_weapon_chest_paired = false;*/

    //public GameObject player_variant;

    /* public GameObject enemy_prefab = null;
     public GameObject enemy_rweapon_prefab = null;
     public GameObject enemy_lweapon_prefab = null;
     public GameObject enemy_subweapon_prefab = null;
     public bool enemy_weapon_chest_paired = false;*/

    RobotController player;
    Vector3 player_last_position;
    //int player_spawn_wait = 60;

    GameObject CinemachineCameraTarget;

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
            public bool loop = false;
            public bool burst = false; // 0�̂ɂȂ�܂ŃX�|�[�������Ȃ�
            public bool boss = false;
        }

  

        public List<OneSpawn> spawns;

        public int currentSpawn = 0;
        public int loop_index = -1;
        public bool spawned = false;
    }

    public Sequence sequence_enemy;
    public Sequence sequence_friend;

    [SerializeField] int armypower_enemy = 600;
    [SerializeField] int armypower_friend = 600;

    [SerializeField] CanvasControl canvasControl;

    public bool finished = false;
    public bool victory = false;

    public int player_dealeddamage = 0;

    public int finish_timer = 0;

    public RobotController finish_dealer;
    public Vector3 finish_dir;
    public RobotController finish_victim;

    public bool attention;
    int attention_timer = 0;
    RobotController attention_target;

    [SerializeField] GameState gameState;

    [SerializeField] UnityEngine.InputSystem.InputActionAsset inputActions;

    [SerializeField] UnityEngine.InputSystem.PlayerInput playerInput;

    [SerializeField] HumanInput humanInput;

    [SerializeField] bool testingroom;

    private void Awake()
    {
        current_instance = this;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        CinemachineCameraTarget = GameObject.Find("Main Camera");

        Slider friendPowerSlider = canvasControl.HUDCanvas.gameObject.transform.Find("FriendTeamPower").GetComponent<Slider>();
        Slider enemyPowerSlider = canvasControl.HUDCanvas.gameObject.transform.Find("EnemyTeamPower").GetComponent<Slider>();

        Team friend_team = new Team {power = armypower_friend, powerslider = friendPowerSlider };
        Team enemy_team = new Team { power = armypower_enemy, powerslider = enemyPowerSlider };

        friendPowerSlider.value = friendPowerSlider.maxValue = armypower_friend;
        enemyPowerSlider.value = enemyPowerSlider.maxValue = armypower_enemy;

        teams.Add(friend_team);
        teams.Add(enemy_team);

        Vector3 pos = PlacePlayerSpawn(60);

        while (ProcessSpawn(sequence_friend, friend_team, true)) ;
        while (ProcessSpawn(sequence_enemy, enemy_team, true)) ;

        PresetCameraTransform(pos);

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

        Physics.autoSimulation = true;
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

    

    bool ProcessSpawn(Sequence sequence,Team team,bool instant)
    {
        bool hav_progress = false;

        for(int i=0;i<team.spawnings.Count;)
        {
            if(team.spawnings[i].player)
            {
                i++;
                continue;
            }

            if(team.spawnings[i].wait <= 0)
            {
                SpawnNPC(team.spawnings[i].variant, team.spawnings[i].pos, team.spawnings[i].rot, team, team.spawnings[i].boss);
                team.spawnings.RemoveAt(i);               
            }
            else
            {
                team.spawnings[i].wait--;
                i++;
            }
        }

        if (sequence.spawned) // ���̃C���f�b�N�X�̓X�|�[���ς݁B���ɐi�ނ��̔���
        {
            if (team.robotControllers.Count+team.spawnings.Count < sequence.spawns[sequence.currentSpawn].squadCount)
            {
                if (sequence.currentSpawn < sequence.spawns.Count - 1)
                {
                    sequence.currentSpawn++;
                    sequence.spawned = false;
                    hav_progress = true;
                }
                else
                {
                    if (sequence.loop_index != -1)
                    {
                        sequence.currentSpawn = sequence.loop_index;
                        sequence.spawned = false;
                        hav_progress = true;
                    }
                }



            }
        }
        else�@// ���̃C���f�b�N�X�̓X�|�[���܂��B�X�|�[�����邩�̔���
        {
            bool do_spawn;

            if (sequence.spawns[sequence.currentSpawn].burst)
            {
                do_spawn = team.robotControllers.Count + team.spawnings.Count == 0;
            }
            else
                do_spawn = team.robotControllers.Count + team.spawnings.Count < sequence.spawns[sequence.currentSpawn].squadCount;

            if (do_spawn)
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
                

                if(player)
                {
                    pos = player.GetCenter()+Quaternion.Euler(0.0f, Random.value * 360.0f, 0.0f)*Vector3.forward* distance;

                    if (team == teams[0])
                        rot = player.transform.rotation;
                    else
                        rot = Quaternion.LookRotation(player.GetCenter() - pos, Vector3.up);
                }
                else if(teams[0].spawnings.Find(x=>x.player)!=null)
                {
                    var spawning = teams[0].spawnings.Find(x => x.player);

                    pos = spawning.pos+ Quaternion.Euler(0.0f, Random.value * 360.0f, 0.0f) * Vector3.forward * distance;

                    if (team == teams[0])
                        rot = spawning.rot;
                    else
                        rot = Quaternion.LookRotation(spawning.pos - pos, Vector3.up);
                }
                else
                {
                    pos = player_last_position + Quaternion.Euler(0.0f, Random.value * 360.0f, 0.0f) * Vector3.forward * distance;
                    rot = Quaternion.LookRotation(player_last_position - pos, Vector3.up);
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

                if (instant)
                    SpawnNPC(spawn.variant, pos, rot, team,spawn.boss);
                else
                    team.spawnings.Add(new Team.Spawning { player = false, pos = pos, rot = rot, variant = spawn.variant, wait = 60 ,boss = spawn.boss });
                
                sequence.spawned = true;
                hav_progress = true;
            }
        }

        return hav_progress;
    }

    Vector3 PlacePlayerSpawn(int wait)
    {
        Vector3 pos = new Vector3(Random.value * 200.0f - 100.0f, 0, Random.value * 200.0f - 100.0f);
        RaycastHit raycastHit;

        Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, 1 << 3);
        float min_dist = float.MaxValue;
        RobotController nearest_robot = null;

        foreach (var team in teams)
        {
            if (team == teams[0]) continue;

            foreach (var robot in team.robotControllers)
            {
                float dist = (robot.transform.position - raycastHit.point).magnitude;

                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest_robot = robot;
                }
            }
        }

        Quaternion quaternion = Quaternion.identity;

        if (nearest_robot != null)
        {
            Vector3 rel = nearest_robot.transform.position - raycastHit.point;
            rel.y = 0.0f;

            quaternion = Quaternion.LookRotation(rel, Vector3.up);

            //CinemachineCameraTarget.transform.rotation = RobotController.GetTargetQuaternionForView_FromTransform(nearest_robot, new RobotController.Transform2 { position = raycastHit.point, rotation = quaternion });
        }

        //CinemachineCameraTarget.transform.position = raycastHit.point + CinemachineCameraTarget.transform.rotation * RobotController.offset;

        teams[0].spawnings.Add(new Team.Spawning { player = true, pos = raycastHit.point, rot = quaternion, wait = 60 });

        return raycastHit.point;
    }

    void PresetCameraTransform(Vector3 pos)
    {
        float min_dist = float.MaxValue;
        Vector3? nearest_robot_pos = null;

        foreach (var team in teams)
        {
            if (team == teams[0]) continue;

            foreach (var robot in team.robotControllers)
            {
                float dist = (robot.transform.position - pos).magnitude;

                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest_robot_pos = robot.GetCenter();
                }
            }

            foreach (var spawning in team.spawnings)
            {
                float dist = (spawning.pos - pos).magnitude;

                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest_robot_pos = spawning.pos;
                }
            }
        }
     

        Quaternion quaternion = Quaternion.identity;

        if (nearest_robot_pos != null)
        {
            Vector3 rel = nearest_robot_pos.Value - pos;
            rel.y = 0.0f;

            quaternion = Quaternion.LookRotation(rel, Vector3.up);

            CinemachineCameraTarget.transform.rotation = RobotController.GetTargetQuaternionForView_FromTransform(nearest_robot_pos.Value, new RobotController.Transform2 { position = pos, rotation = quaternion });
        }

        CinemachineCameraTarget.transform.position = pos + CinemachineCameraTarget.transform.rotation * RobotController.offset;
    }

    void ProcessPlayerSpawn()
    {
        if (player == null)
        {
            Team.Spawning player_spawning = null;
            foreach (var spawning in teams[0].spawnings)
            {
                if (spawning.player)
                {
                    player_spawning = spawning;
                    break;
                }
            }

            if (player_spawning == null)
            {
                PresetCameraTransform(PlacePlayerSpawn(60));
            }
            else
            {
                if (player_spawning.wait == 0)
                {
                    SpawnPlayer(player_spawning.pos, player_spawning.rot, teams[0]);
                    teams[0].spawnings.Remove(player_spawning);
                }
                else
                {
                    player_spawning.wait--;
                }
            }

        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (finished)
        {
            if (finish_timer >= 300)
            {
                if (!canvasControl.ResultCanvas.gameObject.activeSelf)
                {
                    canvasControl.resultCanvas.GoSummaryScreen();
                    canvasControl.ResultCanvas.gameObject.SetActive(true);
                }
            }

            if (finish_timer >= 240) // �Ƃǂߎh�����L�����փY�[��
            {
                if (finish_dealer)
                {
                    Vector3 rel = (finish_dealer.GetCenter() - CinemachineCameraTarget.transform.position);

                    if (rel.magnitude > 15.0f)
                    {
                        CinemachineCameraTarget.transform.position += rel.normalized * (rel.magnitude - 15.0f) / 15.0f;
                    }

                    CinemachineCameraTarget.transform.rotation = Quaternion.LookRotation(finish_dealer.GetCenter() - CinemachineCameraTarget.transform.position);
                }
            }
            if (finish_timer >= 180) // �Ƃǂߎh�����L�����֐���
            {
                Time.timeScale = 1.0f;

                if (finish_dealer)
                {
                    Quaternion qFrom = CinemachineCameraTarget.transform.rotation;
                    Quaternion qTo = Quaternion.LookRotation(finish_dealer.GetCenter() - CinemachineCameraTarget.transform.position);

                    float angle = Quaternion.Angle(qFrom, qTo);

                    float v = Mathf.Max(angle / 15.0f, 0.2f);

                    CinemachineCameraTarget.transform.rotation = Quaternion.RotateTowards(CinemachineCameraTarget.transform.rotation, qTo, v);
                }

            }
            else if (finish_timer >= 120)
            {
                if (canvasControl.ResultCanvas.gameObject.activeSelf)
                {
                    canvasControl.ResultCanvas.gameObject.SetActive(false);
                }

                Vector3 dir = finish_dir;

                dir.y = 0.0f;

                Quaternion view = Quaternion.FromToRotation(Vector3.left, dir);

                if (finish_timer == 120)
                    CinemachineCameraTarget.transform.position = finish_victim.GetCenter() + view * (new Vector3(0.0f, 5, -12));

                if (finish_victim)
                    CinemachineCameraTarget.transform.rotation = Quaternion.LookRotation(finish_victim.GetCenter() - CinemachineCameraTarget.transform.position);

                Unpause();
                Time.timeScale = 0.5f;
            }
            else
            {
                if (!pausing)
                {
                    Pause();

                    if (!canvasControl.ResultCanvas.gameObject.activeSelf)
                    {
                        canvasControl.resultCanvas.power = teams[0].power;
                        canvasControl.resultCanvas.dealeddamage = player_dealeddamage;
                        canvasControl.resultCanvas.power_gold = teams[0].power*(1+gameState.stage)*2;
                        canvasControl.resultCanvas.dealeddamage_gold = player_dealeddamage;

                        gameState.gold += canvasControl.resultCanvas.power_gold + canvasControl.resultCanvas.dealeddamage_gold;

                        canvasControl.resultCanvas.currentgold = gameState.gold;


                        canvasControl.resultCanvas.victory = victory;
                        canvasControl.ResultCanvas.gameObject.SetActive(true);

                        canvasControl.HUDCanvas.gameObject.SetActive(false);
                    }
                }
                else
                {

                }
            }

            finish_timer++;
        }
        else if (attention)
        {
            if (!pausing)
            {
                Pause();
                canvasControl.HUDCanvas.gameObject.SetActive(false);
            }

            if (attention_timer < 60)
            {
              

                Quaternion qFrom = CinemachineCameraTarget.transform.rotation;
                Quaternion qTo = Quaternion.LookRotation(attention_target.GetCenter() - CinemachineCameraTarget.transform.position);

                float angle = Quaternion.Angle(qFrom, qTo);

                float v = Mathf.Max(angle / 15.0f, 0.2f);

                CinemachineCameraTarget.transform.rotation = Quaternion.RotateTowards(CinemachineCameraTarget.transform.rotation, qTo, v);
            }
            else if(attention_timer < 120)
            {
                Vector3 rel = (attention_target.GetCenter() - CinemachineCameraTarget.transform.position);

                if (rel.magnitude > 15.0f)
                {
                    CinemachineCameraTarget.transform.position += rel.normalized * (rel.magnitude - 15.0f) / 15.0f;
                }

                CinemachineCameraTarget.transform.rotation = Quaternion.LookRotation(attention_target.GetCenter() - CinemachineCameraTarget.transform.position);
            }
            else
            {
                attention = false;
                canvasControl.HUDCanvas.gameObject.SetActive(true);
                Unpause();
            }

            attention_timer++;
        }
        else
        {
            if (humanInput.menu)
            {
                Pause();
                canvasControl.HUDCanvas.gameObject.SetActive(false);
                canvasControl.PauseMenu.SetActive(true);
            }

            if (pausing)
            {
                if (!canvasControl.PauseMenu.activeInHierarchy)
                {
                    Unpause();
                    canvasControl.HUDCanvas.gameObject.SetActive(true);
                }
                else
                    return;
            }

            ProcessPlayerSpawn();

            ProcessSpawn(sequence_friend, teams[0], false);
            ProcessSpawn(sequence_enemy, teams[1], false);
        }
    }

    //RobotController player;

    public void HandleRemoveUnit(RobotController robotController, RobotController dealer,Vector3 dir)
    {
        HandleRobotRemove(robotController);

        if (!testingroom)
        {
            if (robotController.robotParameter.Cost < 0)
                robotController.team.power = System.Math.Max(0, robotController.team.power - armypower_friend / 3);
            else
                robotController.team.power = System.Math.Max(0, robotController.team.power - robotController.robotParameter.Cost);

            if (robotController.team.power <= 0)
            {
                finished = true;

                finish_victim = robotController;
                finish_dealer = dealer;
                // ������������̓����ŏW�v�ɉe�����o�Ȃ��悤��
                canvasControl.resultCanvas.dealeddamage = player_dealeddamage;
                finish_dir = dir;

                victory = robotController.team != teams[0];

            }
        }

        robotController.team.robotControllers.Remove(robotController);

        if (player == robotController)
        {
            player_last_position = robotController.GetCenter();
            player = null;
        }
    }

    private void SpawnPlayer(Vector3 pos, Quaternion rot, Team team)
    {
        

        RaycastHit raycastHit;

        Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, 1 << 3);

        GameObject robotObj = GameObject.Instantiate(gameState.player_variant, raycastHit.point, rot);
        //RobotController robot = variant.Spawn(raycastHit.point, rot,this);

        RobotController robot = robotObj.GetComponent<RobotController>();

        robot.HUDCanvas = canvasControl.HUDCanvas;
        robot.uIController_Overlay = robot.HUDCanvas.GetComponent<UIController_Overlay>(); ;
        robot.is_player = true;
        DestroyImmediate(robot.GetComponent<InputBase>());
        robot._input = humanInput;
        robot.CinemachineCameraTarget = CinemachineCameraTarget;

        //robot.worldManager = this;
        robot.team = team;

        GameObject.Instantiate(spawn_prefab, robot.transform.position,Quaternion.identity);

        player = robot;

        team.robotControllers.Add(robot);
        HandleRobotAdd(robot);
    }

    private void SpawnNPC(GameObject variant_obj,Vector3 pos, Quaternion rot, Team team,bool boss)
    {
        //RobotVariant variant = variant_obj.GetComponent<RobotVariant>();
       

        RaycastHit raycastHit;

        Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, 1 << 3);

        GameObject robotObj = GameObject.Instantiate(variant_obj, raycastHit.point, rot);
        //RobotController robot = variant.Spawn(raycastHit.point, rot,this);
        RobotController robot = robotObj.GetComponent<RobotController>();

        robot.HUDCanvas = canvasControl.HUDCanvas;
        robot.uIController_Overlay = robot.HUDCanvas.GetComponent<UIController_Overlay>(); ;
        robot.is_player = false;
        robot.team = team;

        GameObject.Instantiate(spawn_prefab, robot.transform.position, Quaternion.identity);

        team.robotControllers.Add(robot);
        HandleRobotAdd(robot);

        if (boss)
        {
            attention = true;
            attention_timer = 0;
            attention_target = robot;
        }
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

    public bool pausing = false;

    public void Pause()
    {
        if (pausing)
            return;

      
        foreach(var pausable in pausables)
        {
            pausable.OnPause();
        }

        Physics.autoSimulation = false;

        pausing = true;
    }

    public void Unpause()
    {
        if (!pausing)
            return;

        foreach (var pausable in pausables)
        {
            pausable.OnUnpause();
        }

        Physics.autoSimulation = true;

        pausing = false;
    }
}
