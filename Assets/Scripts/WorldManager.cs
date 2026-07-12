using Cinemachine.Utility;
using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using static WorldManager;
using static WorldManager.Team;

public class WorldManager : MonoBehaviour
{
    static public WorldManager current_instance = null;

    static public int layerPattern_Building = 1 << 3 | 1 << 7;

    public enum SpawnType
    {
        NORMAL,
        LARGESCALE
    }

    public class Team
    {
        public List<RobotController> robotControllers = new List<RobotController>();
        public List<Projectile> projectiles = new List<Projectile>();

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

        public OrderToAI orderToAI = OrderToAI.NORMAL;
        public RobotController target_by_commander = null;

        //public int affected_by_sensorarray = 0;
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
    public class ArmyInstance
    {
        public Army army;

        public int currentSpawn = 0;
        public int loop_index = -1;
        public bool spawned = false;

        public List<Spawning> spawnings_seq = new List<Spawning>();

        public class Group
        {
            public List<RobotController> controllers = new List<RobotController>();
            public List<Spawning> spawnings = new List<Spawning>();
            public int reinforce_count = 0;
        }

        public List<Group> groups = new List<Group>();

        public int allgroup_maxrobot = 0;
    }

    ArmyInstance armyInstance_enemy = new ArmyInstance();
    ArmyInstance armyInstance_friend = new ArmyInstance();

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
    //RobotController attention_target;
    //Vector3 attention_position;
    BoundingSphere attention_sphere;

    [SerializeField] public GameState gameState;

    [SerializeField] UnityEngine.InputSystem.InputActionAsset inputActions;

    [SerializeField] UnityEngine.InputSystem.PlayerInput playerInput;

    [SerializeField] HumanInput humanInput;

    [SerializeField] bool testingroom;

    public enum OrderToAI : int
    {
        NORMAL = 0,
        FOCUS,
        SPREAD,
        EVADE,
    }

    UIController_Overlay uIController_Overlay = null;

    bool prev_command = false;

    private void Awake()
    {
        current_instance = this;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if (gameState.army_friend != null)
            armyInstance_friend.army = gameState.army_friend;
        else
            armyInstance_friend.army = DetermineFriendArmyFromSceneName();

        if (gameState.army_enemy != null)
            armyInstance_enemy.army = gameState.army_enemy;
        else
            armyInstance_enemy.army = DetermineEnemyArmyFromSceneName();

        CinemachineCameraTarget = GameObject.Find("Main Camera");

        uIController_Overlay = canvasControl.HUDCanvas.GetComponent<UIController_Overlay>();

        Slider friendPowerSlider = canvasControl.HUDCanvas.gameObject.transform.Find("FriendTeamPower").GetComponent<Slider>();
        Slider enemyPowerSlider = canvasControl.HUDCanvas.gameObject.transform.Find("EnemyTeamPower").GetComponent<Slider>();

        Team friend_team = new Team {power = armyInstance_friend.army.power, powerslider = friendPowerSlider };
        Team enemy_team = new Team { power = armyInstance_enemy.army.power, powerslider = enemyPowerSlider };

        friendPowerSlider.value = friendPowerSlider.maxValue = armyInstance_friend.army.power;
        enemyPowerSlider.value = enemyPowerSlider.maxValue = armyInstance_enemy.army.power;

        teams.Add(friend_team);
        teams.Add(enemy_team);

        Vector3 pos = PlacePlayerSpawn(60);



        foreach (var group_tmpl in armyInstance_friend.army.groups)
        {
            armyInstance_friend.groups.Add(new ArmyInstance.Group());

            if (group_tmpl.condition.type == Army.UnitGroup.Condition.Type.None)
                armyInstance_friend.allgroup_maxrobot += group_tmpl.count;
        }

        while (ProcessSpawnSequence(armyInstance_friend, friend_team, true)) ;
        while (ProcessSpawnGroup(armyInstance_friend, friend_team, true)) ;

        foreach (var group_tmpl in armyInstance_enemy.army.groups)
        {
            armyInstance_enemy.groups.Add(new ArmyInstance.Group());

            if (group_tmpl.condition.type == Army.UnitGroup.Condition.Type.None)
                armyInstance_enemy.allgroup_maxrobot += group_tmpl.count;
        }


        while (ProcessSpawnSequence(armyInstance_enemy, enemy_team, true)) ;
        while (ProcessSpawnGroup(armyInstance_enemy, enemy_team, true)) ;

        PresetCameraTransform(pos);

        for (int i = 0; i < armyInstance_enemy.army.spawns.Count; i++)
        {
            if (armyInstance_enemy.army.spawns[i].loop)
            {
                armyInstance_enemy.loop_index = i;
                break;
            }
        }

        for (int i = 0; i < armyInstance_friend.army.spawns.Count; i++)
        {
            if (armyInstance_friend.army.spawns[i].loop)
            {
                armyInstance_friend.loop_index = i;
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

            if(team != robotController.team)
            {
                //if (robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.SensorArray))
                //{
                    //team.affected_by_sensorarray++;
                //}
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

        foreach (var group in armyInstance_friend.groups)
        {
            if (group.controllers.Remove(robotController)) break;
        }
        foreach (var group in armyInstance_enemy.groups)
        {
            if (group.controllers.Remove(robotController)) break;
        }
    }


    public void AssignToTeam(RobotController robotController)
    {
        robotController.team = teams[0];

        teams[0].robotControllers.Add(robotController);
        HandleRobotAdd(robotController);


    }

    void DetermineSpawnTransform(out Vector3 spawn_position,out Quaternion spawn_rotation,Team team,SpawnType spawnType)
    {
        if (spawnType == SpawnType.LARGESCALE)
        {
            while (true)
            {
                Vector3 pos;

                /*pos.y = 0.0f;
                pos.x = Random.value * 100.0f - 50.0f;
                if (team == teams[0])
                {
                    pos.z = -Random.value * 50.0f-100.0f;
                    spawn_rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                }
                else
                {
                    pos.z = Random.value * 50.0f+100.0f;
                    spawn_rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
                }*/


                pos.y = 0.0f;
                pos.x = UnityEngine.Random.value * 300.0f - 150.0f;
                pos.z = UnityEngine.Random.value * 300.0f - 150.0f;

                spawn_rotation = Quaternion.LookRotation(-pos, Vector3.up);

                RaycastHit raycastHit;
                Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, WorldManager.layerPattern_Building);

                if (raycastHit.collider.gameObject.layer != 7)
                {
                    spawn_position = raycastHit.point;
                    break;
                }
            }
        }
        else
        {
            float distance;

            if (team == teams[0])
            {
                distance = 25.0f;
            }
            else
            {
                distance = 100.0f;
            }

            while (true)
            {
                Vector3 pos;

                RaycastHit raycastHit;
                if (player)
                {
                    pos = player.GetCenter() + Quaternion.Euler(0.0f, UnityEngine.Random.value * 360.0f, 0.0f) * Vector3.forward * distance;

                    if (team == teams[0])
                        spawn_rotation = player.transform.rotation;
                    else
                        spawn_rotation = Quaternion.LookRotation(player.GetCenter() - pos, Vector3.up);
                }
                else if (armyInstance_friend.spawnings_seq.Find(x => x.player) != null)
                {
                    var spawning = armyInstance_friend.spawnings_seq.Find(x => x.player);

                    pos = spawning.pos + Quaternion.Euler(0.0f, UnityEngine.Random.value * 360.0f, 0.0f) * Vector3.forward * distance;

                    if (team == teams[0])
                        spawn_rotation = spawning.rot;
                    else
                        spawn_rotation = Quaternion.LookRotation(spawning.pos - pos, Vector3.up);
                }
                else
                {
                    pos = player_last_position + Quaternion.Euler(0.0f, UnityEngine.Random.value * 360.0f, 0.0f) * Vector3.forward * distance;
                    spawn_rotation = Quaternion.LookRotation(player_last_position - pos, Vector3.up);
                }

                if (pos.x >= 150.0f)
                {
                    pos.x -= distance * 2;
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

                Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, WorldManager.layerPattern_Building);

                if (raycastHit.collider.gameObject.layer != 7)
                {
                    spawn_position = raycastHit.point;
                    break;
                }
            }
        }
    }

    bool ProcessUnitGroup(ArmyInstance armyInst, Team team, bool instant, ref int grouping_force_num, bool hav_condition)
    {
        bool hav_progress = false;
        List<Target> tr = new List<Target>();
        int attention_inThisFrame = 0;
        for (int g_idx = 0; g_idx < armyInst.groups.Count; g_idx++)
        {
            var group_templ = armyInst.army.groups[g_idx];

            if ((group_templ.condition.type != Army.UnitGroup.Condition.Type.None) != hav_condition)
                continue;

            var group_inst = armyInst.groups[g_idx];

            //Vector3 attention_inThisFrame_avgPos = Vector3.zero;
         


            for (int i = 0; i < group_inst.spawnings.Count;)
            {
                if (group_inst.spawnings[i].wait <= 0)
                {
                    RobotController robot = SpawnNPC(group_inst.spawnings[i].variant, group_inst.spawnings[i].pos, group_inst.spawnings[i].rot, team);

                    group_inst.controllers.Add(robot);

                    if (group_inst.spawnings[i].boss)
                    {
                        attention_inThisFrame++;
                        //attention_inThisFrame_avgPos += robot.GetCenter();

                        tr.Add(new Target { target = robot.GetCenter(), radius = 7.0f, weight = 1.0f });
                    }

                    group_inst.spawnings.RemoveAt(i);
                }
                else
                    i++;
            }
        }


        if (attention_inThisFrame > 0)
        {
            attention = true;
            attention_timer = 0;
            attention_sphere = CalculateBoundingSphere(tr);
        }

        List<(Army.UnitGroup,ArmyInstance.Group)> spawn_determinePosRot = new List<(Army.UnitGroup, ArmyInstance.Group)> ();

        for (int g_idx = 0; g_idx < armyInst.groups.Count; g_idx++)
        {
            var group_templ = armyInst.army.groups[g_idx];

            if ((group_templ.condition.type != Army.UnitGroup.Condition.Type.None) != hav_condition)
                continue;

            var group_inst = armyInst.groups[g_idx];
            int spawn_count;

            spawn_count = group_templ.count - (group_inst.controllers.Count + group_inst.spawnings.Count);

            if (spawn_count > 0)
            {
                if (group_templ.reinforce_count > 0)
                {
                    spawn_count = System.Math.Min(spawn_count, group_templ.reinforce_count - group_inst.reinforce_count);

                    if (spawn_count <= 0)
                        continue;
                }

                switch (group_templ.condition.type)
                {
                    case Army.UnitGroup.Condition.Type.None:
                        //増援がいる分通常戦力の補充を減らす
                        spawn_count = System.Math.Min(spawn_count, armyInst.allgroup_maxrobot - grouping_force_num);
                        break;
                    case Army.UnitGroup.Condition.Type.PowerLessThan:
                        //通常戦力が最大いても増援は出す
                        if (team.power >= group_templ.condition.param)
                            spawn_count = 0;
                        break;
                    default:
                        throw new System.NotSupportedException();
                }

                if (spawn_count <= 0)
                    continue;

                for (int i = 0; i < spawn_count; i++)
                {
                    if (!hav_condition)
                    {
                        Vector3 spawn_position;
                        Quaternion spawn_rotation;

                        DetermineSpawnTransform(out spawn_position, out spawn_rotation, team, SpawnType.LARGESCALE);

                        if (instant)
                            group_inst.controllers.Add(SpawnNPC(group_templ.variant, spawn_position, spawn_rotation, team));
                        else
                            group_inst.spawnings.Add(new Team.Spawning { player = false, pos = spawn_position, rot = spawn_rotation, variant = group_templ.variant, wait = 60, boss = group_templ.boss });
                    }
                    else
                        spawn_determinePosRot.Add((group_templ,group_inst));

                    hav_progress = true;
                    group_inst.reinforce_count++;
                    grouping_force_num++;
                }
            }
        }

        if (hav_condition && spawn_determinePosRot.Count > 0)
        {
            Vector3 spawn_position_pivot = Vector3.zero;
            Quaternion spawn_rotation_pivot = Quaternion.identity;

            DetermineSpawnTransform(out spawn_position_pivot, out spawn_rotation_pivot, team, SpawnType.LARGESCALE);

            for (int i=0;i< spawn_determinePosRot.Count;i++)
            {
                var determinePosRot = spawn_determinePosRot[i];

                var group_templ = determinePosRot.Item1;
                var group_inst = determinePosRot.Item2;

                Vector3 spawn_position;
                Quaternion spawn_rotation;

                
                spawn_rotation = spawn_rotation_pivot;

                float distance = spawn_determinePosRot.Count == 1 ? 0.0f : 10.0f;
                Vector3 pos = spawn_position_pivot + Quaternion.Euler(0.0f, 360.0f*i/spawn_determinePosRot.Count, 0.0f) * Vector3.forward * distance;

                while (true)
                {
                
                    RaycastHit raycastHit;

                    if (pos.x >= 150.0f)
                    {
                        pos.x -= distance * 2;
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

                    Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, WorldManager.layerPattern_Building);

                    if (raycastHit.collider.gameObject.layer != 7)
                    {
                        spawn_position = raycastHit.point;
                        break;
                    }

                    distance = UnityEngine.Random.Range(10.0f, 20.0f);
                    pos = spawn_position_pivot + Quaternion.Euler(0.0f, UnityEngine.Random.value * 360.0f, 0.0f) * Vector3.forward * distance;
                }

                if (instant)
                    group_inst.controllers.Add(SpawnNPC(group_templ.variant, spawn_position, spawn_rotation, team));
                else
                    group_inst.spawnings.Add(new Team.Spawning { player = false, pos = spawn_position, rot = spawn_rotation, variant = group_templ.variant, wait = 60, boss = group_templ.boss });
            }
        }
        return hav_progress;
    }

    bool ProcessSpawnSequence(ArmyInstance armyInst,Team team,bool instant)
    {
        bool hav_progress = false;

        for(int i=0;i<armyInst.spawnings_seq.Count;)
        {
            if(armyInst.spawnings_seq[i].player)
            {
                i++;
                continue;
            }

            if(armyInst.spawnings_seq[i].wait <= 0)
            {
                RobotController robot = SpawnNPC(armyInst.spawnings_seq[i].variant, armyInst.spawnings_seq[i].pos, armyInst.spawnings_seq[i].rot, team);

                if (armyInst.spawnings_seq[i].boss)
                {
                    attention = true;
                    attention_timer = 0;
                    attention_sphere = new BoundingSphere(robot.GetCenter(),7.0f);
                }
                armyInst.spawnings_seq.RemoveAt(i);               
            }
            else
                i++;
        }



        // シーケンス処理

        if (armyInst.spawned) // 今のインデックスはスポーン済み。次に進むかの判定
        {
            if (team.robotControllers.Count+armyInst.spawnings_seq.Count < armyInst.army.spawns[armyInst.currentSpawn].squadCount)
            {
                if (armyInst.currentSpawn < armyInst.army.spawns.Count - 1)
                {
                    armyInst.currentSpawn++;
                    armyInst.spawned = false;
                    hav_progress = true;
                }
                else
                {
                    if (armyInst.loop_index != -1)
                    {
                        armyInst.currentSpawn = armyInst.loop_index;
                        armyInst.spawned = false;
                        hav_progress = true;
                    }
                }



            }
        }
        else　// 今のインデックスはスポーンまだ。スポーンするかの判定
        {
            bool do_spawn = false;

            if (armyInst.currentSpawn < armyInst.army.spawns.Count)
            {
                if (armyInst.army.spawns[armyInst.currentSpawn].burst)
                {
                    do_spawn = team.robotControllers.Count + armyInst.spawnings_seq.Count == 0;
                }
                else
                    do_spawn = team.robotControllers.Count + armyInst.spawnings_seq.Count < armyInst.army.spawns[armyInst.currentSpawn].squadCount;
            }

            if (do_spawn)
            {
                Vector3 spawn_position;
                Quaternion spawn_rotation;

                DetermineSpawnTransform(out spawn_position, out spawn_rotation, team, SpawnType.NORMAL);

                Army.OneSpawn spawn = armyInst.army.spawns[armyInst.currentSpawn];

                if (instant)
                {
                    RobotController robot = SpawnNPC(spawn.variant, spawn_position, spawn_rotation, team);

                    if (spawn.boss)
                    {
                        attention = true;
                        attention_timer = 0;
                        attention_sphere = new BoundingSphere(robot.GetCenter(), 7.0f);
                    }
                }
                else
                    armyInst.spawnings_seq.Add(new Team.Spawning { player = false, pos = spawn_position, rot = spawn_rotation, variant = spawn.variant, wait = 60, boss = spawn.boss });
                
                armyInst.spawned = true;
                hav_progress = true;
            }
        }

        return hav_progress;
    }

    bool ProcessSpawnGroup(ArmyInstance armyInst, Team team, bool instant)
    {
        // グループ処理
        bool hav_progress = false;

        // 最大数を無視して増援は出るけど、増援がいる分通常戦力の補充を減らすためのカウンタ
        int grouping_force_num = 0;

        // 中で計算しなおすのがちょっと嫌（今更だけど）、ループ内でスポーンしたら足してる
        foreach (var group_inst in armyInst.groups)
        {
            grouping_force_num += group_inst.controllers.Count + group_inst.spawnings.Count;
        }


        hav_progress |= ProcessUnitGroup(armyInst, team, instant, ref grouping_force_num,true);
        
        hav_progress |= ProcessUnitGroup(armyInst, team, instant, ref grouping_force_num,false);

        return hav_progress;
    }

    Vector3 PlacePlayerSpawn(int wait)
    {
        
        RaycastHit raycastHit;

        while (true)
        {
            Vector3 pos = new Vector3(UnityEngine.Random.value * 200.0f - 100.0f, 0, UnityEngine.Random.value * 200.0f - 100.0f);
            Physics.Raycast(pos + new Vector3(0.0f, 500.0f, 0.0f), -Vector3.up, out raycastHit, float.MaxValue, WorldManager.layerPattern_Building);

            if (raycastHit.collider.gameObject.layer != 7)
                break;
        }
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

        armyInstance_friend.spawnings_seq.Add(new Team.Spawning { player = true, pos = raycastHit.point, rot = quaternion, wait = 60 });

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

            foreach (var spawning in armyInstance_enemy.spawnings_seq)
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
            foreach (var spawning in armyInstance_friend.spawnings_seq)
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
                if (player_spawning.wait <= 0)
                {
                    SpawnPlayer(player_spawning.pos, player_spawning.rot, teams[0]);
                    armyInstance_friend.spawnings_seq.Remove(player_spawning);
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

            if (finish_timer >= 240) // とどめ刺したキャラへズーム
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
            if (finish_timer >= 180) // とどめ刺したキャラへ旋回
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
                        canvasControl.resultCanvas.result_gold = victory ? (gameState.progressStage-1) * 200+1000 : gameState.progressStage*1000;
                        canvasControl.resultCanvas.power_gold = teams[0].power;
                        canvasControl.resultCanvas.dealeddamage_gold = player_dealeddamage/4;

                        gameState.gold += canvasControl.resultCanvas.result_gold+canvasControl.resultCanvas.power_gold + canvasControl.resultCanvas.dealeddamage_gold;

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
                 Quaternion qTo = Quaternion.LookRotation(attention_sphere.position - CinemachineCameraTarget.transform.position);

                 float angle = Quaternion.Angle(qFrom, qTo);

                 float v = Mathf.Max(angle / 15.0f, 0.2f);

                 CinemachineCameraTarget.transform.rotation = Quaternion.RotateTowards(CinemachineCameraTarget.transform.rotation, qTo, v);
             }/*
             else if(attention_timer < 120)
             {
                 Vector3 rel = (attention_position - CinemachineCameraTarget.transform.position);

                 if (rel.magnitude > 15.0f)
                 {
                     CinemachineCameraTarget.transform.position += rel.normalized * (rel.magnitude - 15.0f) / 15.0f;
                 }

                 CinemachineCameraTarget.transform.rotation = Quaternion.LookRotation(attention_position - CinemachineCameraTarget.transform.position);
             }*/
            else if (attention_timer < 120)
            {
                Vector3 LookAt = attention_sphere.position;

                Vector3 From_Horiz = CinemachineCameraTarget.transform.position- LookAt;
                From_Horiz.y = 0.0f;
                From_Horiz = From_Horiz.normalized;

                Vector3 From = LookAt + new Vector3(From_Horiz.x, 0.5f, From_Horiz.z) *attention_sphere.radius*1.0f;
                Vector3 rel = From - CinemachineCameraTarget.transform.position;

                CinemachineCameraTarget.transform.position += rel.normalized * (rel.magnitude) / 15.0f;

                CinemachineCameraTarget.transform.rotation = Quaternion.LookRotation(LookAt - CinemachineCameraTarget.transform.position);
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

            if(humanInput.command && !prev_command)
            {
                teams[0].orderToAI++;
                if (teams[0].orderToAI > OrderToAI.EVADE)
                    teams[0].orderToAI = OrderToAI.NORMAL;
             
                uIController_Overlay.OnChangeOrderToAI(teams[0].orderToAI);
            }

            if(player != null && player && !player.dead)
            {
                teams[0].target_by_commander = player.Target_Robot;
            }

            ProcessPlayerSpawn();

            while (ProcessSpawnSequence(armyInstance_friend, teams[0], false)) ;
            while (ProcessSpawnGroup(armyInstance_friend, teams[0], false)) ;
            while (ProcessSpawnSequence(armyInstance_enemy, teams[1], false)) ;
            while (ProcessSpawnGroup(armyInstance_enemy, teams[1], false)) ;

            foreach (var spawning in armyInstance_friend.spawnings_seq)
                spawning.wait--;

            foreach (var spawning in armyInstance_enemy.spawnings_seq)
                spawning.wait--;

            foreach (var group in armyInstance_friend.groups)
            {
                foreach(var spawning in group.spawnings)
                {
                    spawning.wait--;
                }
            }

            foreach (var group in armyInstance_enemy.groups)
            {
                foreach (var spawning in group.spawnings)
                {
                    spawning.wait--;
                }
            }
        }

        prev_command = humanInput.command;
    }

    //RobotController player;

    public void HandleRemoveUnit(RobotController robotController, RobotController dealer,Vector3 dir)
    {
        HandleRobotRemove(robotController);

        if (!testingroom)
        {
            if (robotController.robotParameter.Cost < 0)
            {
                if(armyInstance_friend.allgroup_maxrobot == 0)
                    robotController.team.power = System.Math.Max(0, robotController.team.power - (armyInstance_friend.army.power / 3));
                else
                    robotController.team.power = System.Math.Max(0, robotController.team.power - (armyInstance_friend.army.power  * 2 / armyInstance_friend.allgroup_maxrobot / 3));
            }
            else
                robotController.team.power = System.Math.Max(0, robotController.team.power - robotController.robotParameter.Cost);

            if (robotController.team.power <= 0)
            {
                finished = true;

                finish_victim = robotController;
                finish_dealer = dealer;
                // 決着がついた後の動きで集計に影響が出ないように
                canvasControl.resultCanvas.dealeddamage = player_dealeddamage;
                finish_dir = dir;

                victory = robotController.team != teams[0];

            }
        }

        if(robotController.robotParameter.itemFlag.HasFlag(RobotController.ItemFlag.TrackingSystem))
        {
            foreach(var team in teams)
            {
                if (team == robotController.team)
                    continue;

                //team.affected_by_sensorarray--;

                //if (team.affected_by_sensorarray < 0)
                //    Debug.Log("team.affected_by_sensorarray < 0");
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
        GameObject robotObj = GameObject.Instantiate(gameState.player_variant, pos, rot);
        //RobotController robot = variant.Spawn(raycastHit.point, rot,this);

        robotObj.tag = "Player";

        RobotController robot = robotObj.GetComponent<RobotController>();

        robot.HUDCanvas = canvasControl.HUDCanvas;
        robot.uIController_Overlay = uIController_Overlay;
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

    private RobotController SpawnNPC(GameObject variant_obj,Vector3 pos, Quaternion rot, Team team)
    {
        //RobotVariant variant = variant_obj.GetComponent<RobotVariant>();
       

      

        GameObject robotObj = GameObject.Instantiate(variant_obj, pos, rot);
        //RobotController robot = variant.Spawn(raycastHit.point, rot,this);
        RobotController robot = robotObj.GetComponent<RobotController>();

        robot.HUDCanvas = canvasControl.HUDCanvas;
        robot.uIController_Overlay = uIController_Overlay;
        robot.is_player = false;
        robot.team = team;

        GameObject.Instantiate(spawn_prefab, robot.transform.position, Quaternion.identity);

        team.robotControllers.Add(robot);
        HandleRobotAdd(robot);

        return robot;
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
    BoundingSphere CalculateBoundingSphere(IList<Target> targets)
    {
        var averagePos = CalculateAveragePosition(targets);

        float maxweight = CalculateMaxWeight(targets);

        var sphere = WeightedMemberBoundsForValidMember(targets[0], averagePos, maxweight);
        for (int i = 1; i < targets.Count; ++i)
        {
            var s = WeightedMemberBoundsForValidMember(targets[i], averagePos, maxweight);
            var distance = (s.position - sphere.position).magnitude + s.radius;
            if (distance > sphere.radius)
            {
                // Point is outside current sphere: update
                sphere.radius = (sphere.radius + distance) * 0.5f;
                sphere.position = (sphere.radius * sphere.position + (distance - sphere.radius) * s.position) / distance;
            }
        }
        return sphere;
    }

    static BoundingSphere WeightedMemberBoundsForValidMember(Target t, Vector3 avgPos, float maxWeight)
    {
        var pos = t.target == null ? avgPos : /*TargetPositionCache.GetTargetPosition(t.target)*/t.target;
        var w = Mathf.Max(0, t.weight);
        if (maxWeight > UnityVectorExtensions.Epsilon && w < maxWeight)
            w /= maxWeight;
        else
            w = 1;
        return new BoundingSphere(Vector3.Lerp(avgPos, pos, w), t.radius * w);
    }

    Vector3 CalculateAveragePosition(IList<Target> targets)
    {
        //if (m_WeightSum < UnityVectorExtensions.Epsilon)
        //    return transform.position;

        var pos = Vector3.zero;
        var count = targets.Count;
        float weightsum = 0.0f;
        for (int i = 0; i < count; ++i)
        {
            var weight = targets[i].weight;
            pos += targets[i].target * weight;
            weightsum += weight;
        }
        if (weightsum < UnityVectorExtensions.Epsilon)
            return transform.position;
        else
            return pos / weightsum;
    }

    float CalculateMaxWeight(IList<Target> targets)
    {
        float result = 0.0f;

        foreach(var target in targets)
        {
            result = Mathf.Max(result, target.weight);
        }
        return result;
    }

    public struct Target
    {
        /// <summary>The target objects.  This object's position and orientation will contribute to the
        /// group's average position and orientation, in accordance with its weight</summary>
        [Tooltip("The target objects.  This object's position and orientation will contribute to the "
            + "group's average position and orientation, in accordance with its weight")]
        public Vector3 target;
        /// <summary>How much weight to give the target when averaging.  Cannot be negative</summary>
        [Tooltip("How much weight to give the target when averaging.  Cannot be negative")]
        public float weight;
        /// <summary>The radius of the target, used for calculating the bounding box.  Cannot be negative</summary>
        [Tooltip("The radius of the target, used for calculating the bounding box.  Cannot be negative")]
        public float radius;
    }

    Army DetermineFriendArmyFromSceneName()
    {
        String stageName = SceneManager.GetActiveScene().name;
        String armyName;
        SkySwitcher skySwitcher = GetComponent<SkySwitcher>();
        
        if(stageName == "TestingRoom")
        {
            armyName = "Test";
        }
        else if(skySwitcher == null)
        {
            armyName = $"{stageName.Substring(5, 1)}_0";
        }
        else
        {
            armyName = $"{stageName.Substring(5, 1)}_{skySwitcher.current}";
        }
        return Resources.Load<Army>($"Armys/Army{armyName}_friend");
    }
    Army DetermineEnemyArmyFromSceneName()
    {
        String stageName = SceneManager.GetActiveScene().name;
        String armyName;
        SkySwitcher skySwitcher = GetComponent<SkySwitcher>();

        if (stageName == "TestingRoom")
        {
            armyName = "Test";
        }
        else if (skySwitcher == null)
        {
            armyName = $"{stageName.Substring(5, 1)}_0";
        }
        else
        {
            armyName = $"{stageName.Substring(5, 1)}_{skySwitcher.current}";
        }
        return Resources.Load<Army>($"Armys/Army{armyName}_enemy");
    }
}
