using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

using UnityEngine.Animations.Rigging;
using System;
using System.Collections.Generic;
using System.Collections;
/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

using StarterAssets;

[RequireComponent(typeof(CharacterController))]
public class RobotController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    public float RotateSpeed = 0.2f;
    public float DashRotateSpeed = 0.05f;
    public float AirDashRotateSpeed = 0.05f;

    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float AirMoveSpeed = 1.0f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    // 上がマイナスなので注意
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = -30.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = 60.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    public float TerminalVelocity = 53.0f;
    public float AscendingVelocity = 20.0f;

    public Vector3 cameraPosition;
    public Quaternion cameraRotation;

    public int HP = 500;
    public int MaxHP = 500;

    public int StepLimit = 30;

    int stepremain = 0;

    private bool is_player;

    public Vector3 offset;

    public Vector3 slash_camera_offset;
    public bool slash_camera_offset_set = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;


    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDVerticalSpeed;
    private int _animIDGround;
    private int _animIDStand;
    private int _animIDWalk;
    private int _animIDJump;
    private int _animIDAir;

    private int _animIDStep_Left;
    private int _animIDStep_Right;
    private int _animIDStep_Front;
    private int _animIDStep_Back;

    private int _animIDDash;

    private int _animIDKnockback_Back;
    private int _animIDKnockback_Front;
    private int _animIDKnockback_Left;
    private int _animIDKnockback_Right;

    private int _animIDKnockback_Strong_Back;
    private int _animIDKnockback_Strong_Front;
    private int _animIDKnockback_Strong_Left;
    private int _animIDKnockback_Strong_Right;

    private int _animIDDown;
    private int _animIDGetup;

    private int _animIDStand2;

    private int _animIDSubFire;

    private int _animIDHeavyFire;

    int slash_count = 0;
    bool slash_reserved = false;

    /*   const int GroundSlash_Num = 3;
       private int[] _animIDGroundSlash;

       const int AirSlash_Num = 1;
       private int[] _animIDAirSlash = new int[AirSlash_Num];

       const int LowerSlash_Num = 1;
       private int[] _animIDLowerSlash = new int[LowerSlash_Num];*/

  

   

    Vector3 dashslash_offset;

    private Animator _animator;
    private CharacterController _controller;
    private Vector3 hitNormal;

    private float org_controller_height;
    private float min_controller_height;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    public InputBase _input;

    public MultiAimConstraint headmultiAimConstraint;
    public MultiAimConstraint chestmultiAimConstraint;
    public MultiAimConstraint rhandmultiAimConstraint;
    public RigBuilder rigBuilder;

    //public MultiAimConstraint rarmmultiAimConstraint;
    public OverrideTransform overrideTransform;
    public GameObject aimingBase;
    public GameObject shoulder_hint;
    public GameObject chest_hint;

    public GameObject aiming_hint
    {
        get
        {
            if (dualwielding) return chest_hint;
            else return shoulder_hint;
        }
    }

    public GameObject Head = null;
    public GameObject Chest = null;
    public GameObject RHand = null;
    public GameObject LHand = null;
    public GameObject LShoulder = null;

    //public bool dualwielding
    //{
    //    get { return rightWeapon.heavy || rightWeapon.dualwielded; }
    //}

    public bool dualwielding = false;

    public enum LockonState
    {
        FREE,
        SEEKING,
        LOCKON
    }

    [SerializeField]
    List<Thruster> thrusters = new List<Thruster>();

    LockonState lockonState = LockonState.FREE;

    public RobotController Target_Robot;

    private GameObject target_chest;
    private GameObject target_head;

    public List<RobotController> lockingEnemy = new List<RobotController>();

    private float _headaimwait = 0.0f;

    private float _chestaimwait = 0.0f;

    private float _rarmaimwait = 0.0f;

    public bool fire_done = false;

    private float _barmlayerwait = 0.0f;

    public Animator animator;

    public GameObject explode_prefab;

    public WorldManager worldManager;

    public WorldManager.Team team;

    //public bool firing = false;

    bool event_grounded = false;
    bool event_jumped = false;
    bool event_stepped = false;
    bool event_stepbegin = false;
    bool event_dashed = false;
    bool event_knockbacked = false;
    bool event_getup = false;
    bool event_downed = false;
    bool event_slash = false;
    bool event_subfired = false;
    bool event_heavyfired = false;

    public enum UpperBodyState
    {
        STAND,
        FIRE,
        KNOCKBACK,
        DOWN,
        GETUP,
        GROUNDSLASH_DASH,
        GroundSlash,
        AIRSLASH_DASH,
        AirSlash,
        LowerSlash,
        QUICKSLASH_DASH,
        QuickSlash,
        DASHSLASH_DASH,
        DashSlash,
        SUBFIRE,
        HEAVYFIRE,
    }

    public enum LowerBodyState
    {
        STAND,
        WALK,
        FIRE,
        AIR,
        GROUND,
        JUMP,
        AIRFIRE,
        STEP,
        DASH,
        AIRROTATE,
        KNOCKBACK,
        DOWN,
        GETUP,
        GROUNDSLASH_DASH,
        GroundSlash,
        AIRSLASH_DASH,
        AirSlash,
        LowerSlash,
        QUICKSLASH_DASH,
        QuickSlash,
        DASHSLASH_DASH,
        DashSlash,
        SUBFIRE,
        AIRSUBFIRE,
        HEAVYFIRE,
        AIRHEAVYFIRE,
    }

    public enum StepDirection
    {
        FORWARD, LEFT, BACKWARD, RIGHT
    }

    StepDirection stepDirection;

    public UpperBodyState upperBodyState = RobotController.UpperBodyState.STAND;
    public LowerBodyState lowerBodyState = RobotController.LowerBodyState.STAND;

    float steptargetrotation;

    public Canvas HUDCanvas;
    Slider boostSlider;

    Vector3 knockbackdir;

    public int Boost_Max = 200;

    int _boost;

    public bool dead = false;

    public int boost
    {
        get { return _boost; }
        set
        {
            _boost = value;

            if (HUDCanvas != null)
                boostSlider.value = _boost;
        }
    }




    public GameObject Rhand;

    public Weapon rightWeapon;

    public Weapon shoulderWeapon;

    public InfightWeapon Sword;

    public UIController_Overlay uIController_Overlay;

    private bool spawn_completed = false; // スポーンしたフレームにDoDamage呼ばれると落ちるので

    public GameObject AimHelper_Head = null;
    public GameObject AimHelper_Chest = null;
    public GameObject AimHelper_RHand = null;

    Quaternion AimTargetRotation_Head;
    Quaternion AimTargetRotation_Chest;

    public enum KnockBackType
    {
        None,
        Weak,
        Strong
    }

    public void DoDamage(Vector3 dir, int damage, KnockBackType knockBackType)
    {
        if (!spawn_completed)
            return;

        _animator.speed = 1.0f;

        if (knockBackType != KnockBackType.None)
        {

            if (lowerBodyState != LowerBodyState.DOWN)
            {
                if (lowerBodyState == LowerBodyState.AIR || lowerBodyState == LowerBodyState.AIRFIRE || lowerBodyState == LowerBodyState.AIRROTATE || lowerBodyState == LowerBodyState.DASH
                    || lowerBodyState == LowerBodyState.AIRSUBFIRE || lowerBodyState == LowerBodyState.AIRHEAVYFIRE)
                {
                    TransitLowerBodyState(LowerBodyState.DOWN);

                    knockbackdir = dir;
                    knockbackdir.y = 0.0f;
                    if (knockBackType == KnockBackType.Strong)
                    {
                        _speed = SprintSpeed;
                    }
                    else
                    {
                        _speed = SprintSpeed / 2;
                    }
                    _verticalVelocity = 0.0f;
                }
                else
                {
                    event_knockbacked = false;



                    upperBodyState = UpperBodyState.KNOCKBACK;
                    lowerBodyState = LowerBodyState.KNOCKBACK;

                    knockbackdir = dir;
                    knockbackdir.y = 0.0f;

                    float knockbackdegree = Mathf.Atan2(knockbackdir.x, knockbackdir.z) * Mathf.Rad2Deg;

                    float stepmotiondegree = Mathf.Repeat(knockbackdegree - transform.eulerAngles.y + 180.0f, 360.0f) - 180.0f;

                    if (knockBackType == KnockBackType.Strong)
                    {
                        if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
                            _animator.Play(_animIDKnockback_Strong_Right, 0, 0);
                        else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
                            _animator.Play(_animIDKnockback_Strong_Back, 0, 0);
                        else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
                            _animator.Play(_animIDKnockback_Strong_Left, 0, 0);
                        else
                            _animator.Play(_animIDKnockback_Strong_Front, 0, 0);

                        _speed = SprintSpeed * 2;
                    }
                    else
                    {

                        if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
                            _animator.Play(_animIDKnockback_Right, 0, 0);
                        else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
                            _animator.Play(_animIDKnockback_Back, 0, 0);
                        else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
                            _animator.Play(_animIDKnockback_Left, 0, 0);
                        else
                            _animator.Play(_animIDKnockback_Front, 0, 0);

                        _speed = SprintSpeed;
                    }

                    _controller.height = 7.0f;

                    _verticalVelocity = 0.0f;
                }

                Sword.emitting = false;

                // 射撃中にのけぞった場合に備えて
                if (dualwielding)
                {
                    _animator.CrossFadeInFixedTime(_animIDStand2, 0.5f, 2);
                    _animator.speed = 1.0f;
                }
            }
        }

        HP = Math.Max(0, HP - damage);

        if (HP <= 0)
        {
            dead = true;
        }
    }

    private void TargetEnemy(RobotController robotController)
    {
        UntargetEnemy();

        Target_Robot = robotController;

        target_chest = Target_Robot.Chest;
        target_head = Target_Robot.Head;

        //rigBuilder.Build();

        Target_Robot.lockingEnemy.Add(this);

        if (HUDCanvas != null)
            uIController_Overlay.target = Target_Robot;
    }

    public void UntargetEnemy()
    {
        if (Target_Robot != null)
        {
            Target_Robot.lockingEnemy.Remove(this);
        }
    }

    public void PurgeTarget(RobotController robotController)
    {
        Target_Robot = null;

        target_chest = null;
        target_head = null;

        //rigBuilder.Build();

        if (HUDCanvas != null)
            uIController_Overlay.target = null;
    }

    private void Awake()
    {


        //HUDCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
        if (HUDCanvas != null)
        {
            boostSlider = HUDCanvas.gameObject.transform.Find("BoostSlider").GetComponent<Slider>();


            uIController_Overlay = HUDCanvas.GetComponent<UIController_Overlay>();


            uIController_Overlay.origin = this;

            uIController_Overlay.AddWeapon(rightWeapon);

            if(shoulderWeapon != null)
                uIController_Overlay.AddWeapon(shoulderWeapon);
        }
    }

    private void Start()
    {
        if (CinemachineCameraTarget != null)
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        is_player = GetComponent<HumanInput>() != null;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();

        org_controller_height = _controller.height;
        min_controller_height = _controller.radius * 2;

        //_input = GetComponent<StarterAssetsInputs>();

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        if (HUDCanvas != null)
            boostSlider.value = boostSlider.maxValue = boost = Boost_Max;

        AimTargetRotation_Head = Head.transform.rotation;
        AimTargetRotation_Chest = Chest.transform.rotation;

        if (Target_Robot != null)
        {
            TargetEnemy(Target_Robot);
        }

        HP = MaxHP;

        if (team == null)
        {
            worldManager.AssignToTeam(this);
        }

        
        Sword.autovanish = dualwielding;
        Sword.emitting = false;

        spawn_completed = true;

    }

    private bool ConsumeBoost()
    {
        if (boost > 0)
        {
            boost -= 4;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void RegenBoost()
    {
        boost = Math.Min(boost + 12, Boost_Max);
    }

    public static float DistanceToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    public void OnEnemyAdded(RobotController robotController)
    {
        if (HUDCanvas != null)
            uIController_Overlay.AddRobot(robotController);
    }

    public void OnEnemyRemoved(RobotController robotController)
    {
        if (HUDCanvas != null)
            uIController_Overlay.RemoveRobot(robotController);
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            _hasAnimator = TryGetComponent(out _animator);

            float mindist = float.MaxValue;
            float minangle = float.MaxValue;
            RobotController nearest_robot = null;

            foreach (var team in worldManager.teams)
            {
                if (team == this.team)
                    continue;

                if (is_player)
                {
                    bool blocked = false;

                    if (lockonState == LockonState.FREE)
                    {


                        foreach (var robot in team.robotControllers)
                        {
                            Vector3 direction = GetCurrentAimQuaternion() * Vector3.forward;
                            // Vector3 startingPoint = transform.position;

                            Ray ray = new Ray(GetCenter(), direction);
                            float shift = Vector3.Cross(ray.direction, robot.GetCenter() - ray.origin).magnitude;

                            float dist = Vector3.Dot(ray.direction, robot.GetCenter() - ray.origin);

                            float angle = Vector3.Angle(ray.direction, robot.GetCenter() - ray.origin);

                            if (dist > 0.0f)
                            {
                                if (blocked)
                                {
                                    if (shift < 2.0f)
                                    {
                                        if (dist < mindist)
                                        {
                                            mindist = dist;
                                            nearest_robot = robot;
                                        }
                                    }
                                }
                                else
                                {
                                    if (shift < 2.0f)
                                    {
                                        blocked = true;
                                        mindist = dist;
                                        nearest_robot = robot;
                                    }
                                    else
                                    {
                                        if (angle < minangle)
                                        {
                                            minangle = angle;
                                            nearest_robot = robot;
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        nearest_robot = Target_Robot;

                        Vector3 direction = GetCurrentAimQuaternion() * Vector3.forward;
                        Vector3 startingPoint = transform.position;

                        Ray ray = new Ray(GetCenter(), direction);
                        float shift = Vector3.Cross(ray.direction, Target_Robot.GetCenter() - ray.origin).magnitude;

                        float dot = Vector3.Dot(ray.direction, Target_Robot.GetCenter() - ray.origin);

                        mindist = dot;
                        blocked = true;

                    }
                    if (blocked)
                    {
                        uIController_Overlay.distance = mindist;

                    }
                    else
                        uIController_Overlay.distance = 1000.0f;

                }
                else
                {
                    foreach (var robot in team.robotControllers)
                    {
                        float dist = (GetCenter() - robot.GetCenter()).sqrMagnitude;

                        if (dist < mindist)
                        {
                            mindist = dist;
                            nearest_robot = robot;
                        }
                    }
                }
            }

            if (nearest_robot == null)
            {
                UntargetEnemy();

                Target_Robot = null;

                target_chest = null;
                target_head = null;

                if (HUDCanvas != null)
                    uIController_Overlay.target = null;
            }
            else
            {
                if (Target_Robot != nearest_robot)
                    TargetEnemy(nearest_robot);
            }
            if (HUDCanvas != null)
                uIController_Overlay.lockonState = lockonState;

            rightWeapon.Target_Robot = Target_Robot;
            if(shoulderWeapon != null)
                shoulderWeapon.Target_Robot = Target_Robot;
            LowerBodyMove(); // 順番入れ替えるとHEAVYFIREの反動が処理できないので注意
            UpperBodyMove();

        }
        else
        {
            UntargetEnemy();

            for (int i = 0; i < lockingEnemy.Count; i++)
            {
                lockingEnemy[i].PurgeTarget(this);
            }

            if (uIController_Overlay != null)
                uIController_Overlay.origin = null;

            worldManager.HandleRemoveUnit(this);

            GameObject.Instantiate(explode_prefab, transform.position, Quaternion.identity);

            GameObject.Destroy(gameObject);
        }
    }



    private void AssignAnimationIDs()
    {
        _animIDVerticalSpeed = Animator.StringToHash("VerticalSpeed");
        _animIDGround = Animator.StringToHash("Ground");
        _animIDStand = Animator.StringToHash("Stand");
        _animIDWalk = Animator.StringToHash("Walk");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDAir = Animator.StringToHash("Air");

        _animIDStep_Left = Animator.StringToHash("Step_Left");
        _animIDStep_Right = Animator.StringToHash("Step_Right");
        _animIDStep_Front = Animator.StringToHash("Step_Front");
        _animIDStep_Back = Animator.StringToHash("Step_Back");

        _animIDDash = Animator.StringToHash("Dash");

        _animIDKnockback_Back = Animator.StringToHash("KnockBack_Back");
        _animIDKnockback_Front = Animator.StringToHash("KnockBack_Front");
        _animIDKnockback_Right = Animator.StringToHash("KnockBack_Right");
        _animIDKnockback_Left = Animator.StringToHash("KnockBack_Left");

        _animIDKnockback_Strong_Back = Animator.StringToHash("KnockBack_Strong_Back");
        _animIDKnockback_Strong_Front = Animator.StringToHash("KnockBack_Strong_Front");
        _animIDKnockback_Strong_Right = Animator.StringToHash("KnockBack_Strong_Right");
        _animIDKnockback_Strong_Left = Animator.StringToHash("KnockBack_Strong_Left");

        _animIDDown = Animator.StringToHash("Down");
        _animIDGetup = Animator.StringToHash("Getup");

      


        _animIDStand2 = Animator.StringToHash("Stand2");

        _animIDSubFire = Animator.StringToHash("SubFire");

        _animIDHeavyFire = Animator.StringToHash("HeavyFire");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition;

        if (lowerBodyState == LowerBodyState.DOWN)
            spherePosition = new Vector3(transform.position.x, transform.position.y + 3.4f,
                transform.position.z);
        else
            spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        if (_controller.isGrounded)
            Grounded = true;

        switch (lowerBodyState)
        {
            case LowerBodyState.AIR:
            case LowerBodyState.AIRFIRE:
            case LowerBodyState.DASH:
            case LowerBodyState.AIRROTATE:
            case LowerBodyState.AIRSUBFIRE:
            case LowerBodyState.AIRHEAVYFIRE:
                if (Grounded)
                {
                    TransitLowerBodyState(LowerBodyState.GROUND);
                }
                break;
            case LowerBodyState.STAND:
            case LowerBodyState.WALK:
            case LowerBodyState.STEP:
                if (!Grounded)
                {
                    TransitLowerBodyState(LowerBodyState.AIR);
                }
                break;
        }


    }

    private void OnEnable()
    {
        StartCoroutine(nameof(UpdateLateFixedUpdate));
    }

    private void OnDisable()
    {
        StopCoroutine(nameof(UpdateLateFixedUpdate));
    }

    private void LateFixedUpdate()
    {
        CameraAndLockon();

        if (CinemachineCameraTarget != null)
        {
            CinemachineCameraTarget.transform.position = cameraPosition;
            CinemachineCameraTarget.transform.rotation = cameraRotation;
        }
    }

    private IEnumerator UpdateLateFixedUpdate()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();


        while (true)
        {
            yield return waitForFixedUpdate;
            LateFixedUpdate();
        }
    }

    private void LateUpdate()
    {
        if (!(Target_Robot != null && lockonState != LockonState.FREE))
        {

            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                //float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                float deltaTimeMultiplier = 1.0f;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }
        }
    }

    // AIから呼びされるので
    public Quaternion GetTargetQuaternionForView(RobotController target)
    {
        Quaternion qtarget = Quaternion.LookRotation(target.GetCenter() - GetCenter(), Vector3.up);

        Vector3 vtarget = qtarget.eulerAngles;

        vtarget.x += 10.0f;

        return Quaternion.Euler(vtarget);
    }

    private Quaternion GetCurrentAimQuaternion()
    {
        Quaternion qtarget = cameraRotation;

        Vector3 vtarget = qtarget.eulerAngles;

        vtarget.x -= 10.0f;

        return Quaternion.Euler(vtarget);
    }


    private void CameraAndLockon()
    {
        float dist_enemy = float.MaxValue;

        if (Target_Robot != null &&
                        (lowerBodyState == LowerBodyState.AIRSLASH_DASH
                        || lowerBodyState == LowerBodyState.AirSlash
                        || lowerBodyState == LowerBodyState.DashSlash
                        || lowerBodyState == LowerBodyState.DASHSLASH_DASH
                        || lowerBodyState == LowerBodyState.GroundSlash
                        || lowerBodyState == LowerBodyState.GROUNDSLASH_DASH
                        || lowerBodyState == LowerBodyState.QuickSlash
                        || lowerBodyState == LowerBodyState.QUICKSLASH_DASH
                        || lowerBodyState == LowerBodyState.LowerSlash
                   ) && lockonState != LockonState.FREE
                   )
        {
            dist_enemy = (GetCenter() - Target_Robot.GetCenter()).magnitude;
        }

        if (dist_enemy < 20.0f)
        {
            if (!slash_camera_offset_set)
            {
                Vector3 off_hori = GetCenter() - Target_Robot.GetCenter();
                off_hori.y = 0.0f;

                slash_camera_offset = Quaternion.AngleAxis(90.0f, Vector3.up) * (off_hori).normalized * 20.0f;
            }

            slash_camera_offset_set = true;

            Vector3 lookat = (GetCenter() + Target_Robot.GetCenter()) / 2;

            cameraPosition = lookat + slash_camera_offset;

            cameraRotation = Quaternion.LookRotation(-slash_camera_offset);

            Quaternion q = GetTargetQuaternionForView(Target_Robot);

            _cinemachineTargetYaw = q.eulerAngles.y;
            _cinemachineTargetPitch = q.eulerAngles.x;

            if (_cinemachineTargetPitch > 180.0f)
                _cinemachineTargetPitch -= 360.0f;

            lockonState = LockonState.LOCKON;
        }
        else
        {
            slash_camera_offset_set = false;

            if (Target_Robot != null)
            {
                if (lockonState == LockonState.FREE)
                {

                }

                if (lockonState == LockonState.SEEKING)
                {

                    float angle = Quaternion.Angle(cameraRotation, GetTargetQuaternionForView(Target_Robot));

                    if (angle < 1.0f)
                    {
                        lockonState = LockonState.LOCKON;
                    }
                    else
                    {
                        Quaternion q = Quaternion.RotateTowards(cameraRotation, GetTargetQuaternionForView(Target_Robot), 1.0f);

                        _cinemachineTargetYaw = q.eulerAngles.y;
                        _cinemachineTargetPitch = q.eulerAngles.x;

                        if (_cinemachineTargetPitch > 180.0f)
                            _cinemachineTargetPitch -= 360.0f;
                    }
                }

                if (lockonState == LockonState.LOCKON)
                {
                    Quaternion q = GetTargetQuaternionForView(Target_Robot);

                    _cinemachineTargetYaw = q.eulerAngles.y;
                    _cinemachineTargetPitch = q.eulerAngles.x;

                    if (_cinemachineTargetPitch > 180.0f)
                        _cinemachineTargetPitch -= 360.0f;

                }
            }
            else
            {
                lockonState = LockonState.FREE;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, TopClamp, BottomClamp);


            // Cinemachine will follow this target
            cameraRotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);

            cameraPosition = transform.position + cameraRotation * offset * transform.lossyScale.x;


        }
    }

    private void UpperBodyMove()
    {
        bool head_no_aiming = false;
        bool chest_no_aiming = false;

        float rhandaimwait = 0.0f;

        bool chest_pitch_aim = false;

        switch (upperBodyState)
        {
            case UpperBodyState.FIRE:
                {
                    _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.10f);


                    _rarmaimwait = Mathf.Min(1.0f, _rarmaimwait + 0.04f);

                    _chestaimwait = Mathf.Min(1.0f, _chestaimwait + 0.04f);

                    _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f);

                    if (dualwielding)
                    {
                        rhandaimwait = Mathf.Clamp((animator.GetCurrentAnimatorStateInfo(2).normalizedTime - 0.70f) * 4, 0.0f, 1.0f);
                    }
                    else
                    {
                        rhandaimwait = Mathf.Clamp((animator.GetCurrentAnimatorStateInfo(1).normalizedTime - 0.70f) * 4, 0.0f, 1.0f);
                    }

                    if (!fire_done)
                    {
                        bool shoot = false;

                        if (dualwielding)
                        {
                            shoot = animator.GetCurrentAnimatorStateInfo(2).normalizedTime >= 1;
                        }
                        else
                        {
                            shoot = animator.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1;
                        }

                        if (shoot)
                        {
                            fire_done = true;
                            if (lockonState == LockonState.LOCKON)
                            {
                                rightWeapon.Target_Robot = Target_Robot;
                            }
                            else
                            {
                                rightWeapon.Target_Robot = null;
                                lockonState = LockonState.FREE;
                            }

                            rightWeapon.trigger = true;
                        }
                    }
                    else
                    {
                        if (rightWeapon.canHold)
                        {
                            rightWeapon.trigger = _input.fire;
                        }
                        else
                        {
                            rightWeapon.trigger = false;
                        }

                        if (!rightWeapon.followthrough_now)
                        {
                            upperBodyState = UpperBodyState.STAND;

                            if (lowerBodyState == LowerBodyState.AIRFIRE)
                            {
                                TransitLowerBodyState(LowerBodyState.AIR);
                            }
                            else if (lowerBodyState == LowerBodyState.FIRE)
                            {
                                TransitLowerBodyState(LowerBodyState.STAND);
                            }

                            if (dualwielding)
                            {
                                _animator.CrossFadeInFixedTime(_animIDStand2, 0.5f, 2);
                                _animator.speed = 1.0f;
                            }
                        }
                    }
                }
                break;
            case UpperBodyState.SUBFIRE:
                {
                    if (!fire_done)
                    {
                        if (event_subfired)
                        {
                            if (lockonState == LockonState.LOCKON)
                            {
                                shoulderWeapon.Target_Robot = Target_Robot;
                            }
                            else
                            {
                                shoulderWeapon.Target_Robot = null;
                                lockonState = LockonState.FREE;
                            }

                            shoulderWeapon.trigger = true;
                            fire_done = true;
                        }
                    }
                    else
                    {
                        if (shoulderWeapon.canHold)
                        {
                            shoulderWeapon.trigger = _input.subfire;
                        }
                        else
                        {
                            shoulderWeapon.trigger = false;
                        }
 
                        if (!shoulderWeapon.followthrough_now)
                        {

                            upperBodyState = UpperBodyState.STAND;
                            if (lowerBodyState == LowerBodyState.AIRSUBFIRE)
                            {
                                TransitLowerBodyState(LowerBodyState.AIR);
                            }
                            else if (lowerBodyState == LowerBodyState.SUBFIRE)
                                TransitLowerBodyState(LowerBodyState.STAND);
                            
                        }
                    }

                    _chestaimwait = Mathf.Min(1.0f, _chestaimwait + 0.08f);
                    chest_pitch_aim = true;
                    _headaimwait = 0.0f;
                    _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.16f);
                    _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f);
                }
                break;
            case UpperBodyState.HEAVYFIRE:
                {
                    if (!fire_done)
                    {
                        if (event_heavyfired)
                        {
                            if (lockonState == LockonState.LOCKON)
                            {
                                rightWeapon.Target_Robot = Target_Robot;
                            }
                            else
                            {
                                rightWeapon.Target_Robot = null;
                                lockonState = LockonState.FREE;
                            }

                            event_heavyfired = false; // 撃った瞬間をLowerBodyMove()で判定するため
                            rightWeapon.trigger = true;
                            fire_done = true;
                        }

                      

                        rhandaimwait = Mathf.Clamp((animator.GetCurrentAnimatorStateInfo(2).normalizedTime - 0.0f) * 4, 0.0f, 1.0f);
                    }
                    else
                    {
                        //if (rightWeapon.canHold)
                       // {
                        //    shoulderWeapon.trigger = _input.subfire;
                        //}
                        //else
                        {
                            rightWeapon.trigger = false;
                        }

                        if (!rightWeapon.followthrough_now)
                        {
                            upperBodyState = UpperBodyState.STAND;
                            if (lowerBodyState == LowerBodyState.AIRHEAVYFIRE)
                            {
                                TransitLowerBodyState(LowerBodyState.AIR);
                            }
                            else if (lowerBodyState == LowerBodyState.HEAVYFIRE)
                                TransitLowerBodyState(LowerBodyState.STAND);

                            if (dualwielding)
                            {
                                _animator.CrossFadeInFixedTime(_animIDStand2, 0.5f, 2);
                                _animator.speed = 1.0f;
                            }
                        }

                    }
                    _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.10f);
                    _rarmaimwait = Mathf.Min(1.0f, _rarmaimwait + 0.04f);
                    _chestaimwait = Mathf.Min(1.0f, _chestaimwait + 0.04f);
                    _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f);
                }
                break;
            case UpperBodyState.STAND:
                {
                    lockonState = LockonState.FREE;
                    float angle = 180.0f;

                    if (target_chest)
                    {
                        angle = Vector3.Angle(target_chest.transform.position - transform.position, transform.forward);

                        if (angle > 60)
                        {
                            _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.1f);
                            head_no_aiming = true;
                        }
                        else
                            _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.1f);
                    }
                    else
                    {
                        _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.1f);
                        head_no_aiming = true;
                    }


                    if (_input.fire)
                    {
                        if(rightWeapon.heavy)
                        {
                            animator.Play("HeavyFire", 2, 0.0f);

                            //_input.subfire = false;

                            if (lowerBodyState == LowerBodyState.AIR || lowerBodyState == LowerBodyState.DASH || lowerBodyState == LowerBodyState.AIRROTATE)
                            {
                                lowerBodyState = LowerBodyState.AIRHEAVYFIRE;
                            }
                            else
                            {
                                lowerBodyState = LowerBodyState.HEAVYFIRE;
                            }

                            upperBodyState = UpperBodyState.HEAVYFIRE;
                            event_heavyfired = false;
                            fire_done = false;
                            _animator.CrossFadeInFixedTime(_animIDHeavyFire, 0.25f, 0);
                            _animator.speed = 1.0f;

                            lockonState = LockonState.SEEKING;
                        }
                        else
                        {
                            upperBodyState = UpperBodyState.FIRE;
                            //_input.fire = false;

                            if (dualwielding)
                                animator.Play("Fire2", 2, 0.0f);
                            else
                                animator.Play("Fire", 1, 0.0f);

                            lockonState = LockonState.SEEKING;



                            if (angle > 100)
                            {
                                if (lowerBodyState == LowerBodyState.AIR || lowerBodyState == LowerBodyState.DASH || lowerBodyState == LowerBodyState.AIRROTATE)
                                {
                                    lowerBodyState = LowerBodyState.AIRFIRE;
                                    _animator.CrossFadeInFixedTime(_animIDAir, 0.5f, 0);
                                }
                                else
                                {
                                    lowerBodyState = LowerBodyState.FIRE;
                                    _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                                }
                                _animator.speed = 1.0f;
                            }
                        }
                      

                        fire_done = false;
                    }

                    if (_input.slash)
                    {
                        if (Grounded)
                        {

                            if (target_chest != null)
                            {
                                Vector3 target_dir = target_chest.transform.position - transform.position;

                                _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                                float rotation = _targetRotation;

                                // rotate to face input direction relative to camera position
                                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                            }

                            if (lowerBodyState == LowerBodyState.STAND || lowerBodyState == LowerBodyState.WALK)
                            {

                                //_input.slash = false;
                                lowerBodyState = LowerBodyState.GROUNDSLASH_DASH;
                                upperBodyState = UpperBodyState.GROUNDSLASH_DASH;
                                event_stepbegin = event_stepped = false;
                                _animator.CrossFadeInFixedTime(_animIDStep_Front, 0.0f, 0);
                                stepremain = Sword.motionProperty[lowerBodyState].DashLength;
                                slash_reserved = false;
                                Sword.emitting = true;
                                lockonState = LockonState.SEEKING;
                            }
                            else
                            {

                                //_input.slash = false;
                                lowerBodyState = LowerBodyState.QUICKSLASH_DASH;
                                upperBodyState = UpperBodyState.QUICKSLASH_DASH;
                                event_stepbegin = event_stepped = false;
                                _animator.CrossFadeInFixedTime(_animIDStep_Front, 0.0f, 0);
                                stepremain = Sword.motionProperty[lowerBodyState].DashLength;
                                slash_reserved = false;
                                Sword.emitting = true;
                                lockonState = LockonState.SEEKING;
                            }
                            _animator.speed = 1.0f;
                        }
                        else
                        {
                            if (lowerBodyState == LowerBodyState.DASH && Sword.can_dash_slash)
                            {
                                //_input.slash = false;
                                lowerBodyState = LowerBodyState.DASHSLASH_DASH;
                                upperBodyState = UpperBodyState.DASHSLASH_DASH;
                                event_stepbegin = event_stepped = false;
                                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.DashSlash]._animID[0], 0.0f, 0);
                                _animator.speed = 0.0f;
                                stepremain = Sword.motionProperty[lowerBodyState].DashLength;
                                slash_reserved = false;
                                Sword.emitting = true;
                                lockonState = LockonState.SEEKING;
                                if (target_chest != null)
                                {
                                    dashslash_offset = (Chest.transform.position - target_chest.transform.position);

                                    dashslash_offset.y = 0.0f;

                                    dashslash_offset = Quaternion.AngleAxis(-45, new Vector3(0.0f, 1.0f, 0.0f)) * dashslash_offset;
                                }
                            }
                            else
                            {
                                //_input.slash = false;
                                lowerBodyState = LowerBodyState.AIRSLASH_DASH;
                                upperBodyState = UpperBodyState.AIRSLASH_DASH;
                                event_stepbegin = event_stepped = false;
                                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.AirSlash]._animID[0], 0.0f, 0);
                                _animator.speed = 0.0f;
                                stepremain = Sword.motionProperty[lowerBodyState].DashLength;
                                slash_reserved = false;
                                Sword.emitting = true;
                                lockonState = LockonState.SEEKING;
                                if (target_chest != null)
                                {
                                    Vector3 target_dir = target_chest.transform.position - transform.position;

                                    _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                                    float rotation = _targetRotation;

                                    // rotate to face input direction relative to camera position
                                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                                }
                            }
                        }
                    }

                    if (_input.subfire)
                    {
                        //_input.subfire = false;

                        if (lowerBodyState == LowerBodyState.AIR || lowerBodyState == LowerBodyState.DASH || lowerBodyState == LowerBodyState.AIRROTATE)
                        {
                            lowerBodyState = LowerBodyState.AIRSUBFIRE;
                        }
                        else
                        {
                            lowerBodyState = LowerBodyState.SUBFIRE;
                        }

                        upperBodyState = UpperBodyState.SUBFIRE;
                        event_subfired = false;
                        fire_done = false;
                        _animator.CrossFadeInFixedTime(_animIDSubFire, 0.25f, 0);
                        _animator.speed = 1.0f;

                        animator.Play("SubFire", 2, 0.0f);

                        lockonState = LockonState.SEEKING;
                    }

                    _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.04f);
                    _chestaimwait = Mathf.Max(0.0f, _chestaimwait - 0.04f);
                    _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f);

                    chest_no_aiming = true;
                }
                break;
            case UpperBodyState.KNOCKBACK:
            case UpperBodyState.DOWN:
            case UpperBodyState.GETUP:
                _chestaimwait = 0.0f;
                _headaimwait = 0.0f;
                _rarmaimwait = 0.0f;
                _barmlayerwait = 0.0f;
                lockonState = LockonState.FREE;
                break;
            case UpperBodyState.LowerSlash:
                _chestaimwait = 0.0f;
                _headaimwait = 1.0f;
                _rarmaimwait = 0.0f;
                _barmlayerwait = 0.0f;
                lockonState = LockonState.FREE;
                break;
            case UpperBodyState.GROUNDSLASH_DASH:
            case UpperBodyState.AIRSLASH_DASH:
            case UpperBodyState.DASHSLASH_DASH:
            case UpperBodyState.QUICKSLASH_DASH:
            case UpperBodyState.GroundSlash:
            case UpperBodyState.AirSlash:
            case UpperBodyState.QuickSlash:
            case UpperBodyState.DashSlash:
                _chestaimwait = 0.0f;
                _headaimwait = 0.0f;
                _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.08f);
                _barmlayerwait = Mathf.Max(0.0f, _barmlayerwait - 0.08f);
                break;
            default:
                _chestaimwait = 0.0f;
                _headaimwait = 0.0f;
                _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.08f);
                _barmlayerwait = Mathf.Max(0.0f, _barmlayerwait - 0.08f);
                lockonState = LockonState.FREE;
                break;
        }

        headmultiAimConstraint.weight = _headaimwait;

        //headmultiAimConstraint.weight = 1.0f;

        Quaternion target_rot_head;
        Quaternion target_rot_chest;
        Quaternion target_rot_rhand;


        if (target_chest != null)
        {

            Quaternion q_aim_global = Quaternion.LookRotation(aiming_hint.transform.position - target_chest.transform.position, new Vector3(0.0f, 1.0f, 0.0f));

            overrideTransform.data.position = shoulder_hint.transform.position;
            overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;

            target_rot_head = Quaternion.LookRotation(target_head.transform.position - Head.transform.position, new Vector3(0.0f, 1.0f, 0.0f));
            target_rot_chest = Quaternion.LookRotation(target_chest.transform.position - Chest.transform.position, new Vector3(0.0f, 1.0f, 0.0f));
            target_rot_rhand = Quaternion.LookRotation(target_chest.transform.position - RHand.transform.position, new Vector3(0.0f, 1.0f, 0.0f));
        }
        else
        {
            Quaternion q_aim_global = Quaternion.LookRotation(-aiming_hint.transform.forward, new Vector3(0.0f, 1.0f, 0.0f));

            overrideTransform.data.position = shoulder_hint.transform.position;
            overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;

            target_rot_head = Head.transform.rotation;
            target_rot_chest = Chest.transform.rotation;
            target_rot_rhand = Chest.transform.rotation;
        }

        //Quaternion thisframe_rot_head

        if (head_no_aiming)
        {
            AimTargetRotation_Head = target_rot_head;
        }
        else
        {
            AimTargetRotation_Head = Quaternion.RotateTowards(AimTargetRotation_Head, target_rot_head, 1.0f);
            //= target_rot_head;
            //= Head.transform.rotation;
        }

        AimHelper_Head.transform.position = Head.transform.position + AimTargetRotation_Head * Vector3.forward * 3;


        if (chest_no_aiming)
        {
            AimTargetRotation_Chest = target_rot_chest;
        }
        else
        {
            AimTargetRotation_Chest = Quaternion.RotateTowards(AimTargetRotation_Chest, target_rot_chest, 1.0f);
            //= target_rot_head;
            //= Head.transform.rotation;
        }


        Vector3 chestAim_Dir = AimTargetRotation_Chest * Vector3.forward * 3;

        if (!chest_pitch_aim)
            chestAim_Dir.y = 0.0f;

        AimHelper_Chest.transform.position = Chest.transform.position + chestAim_Dir;

        chestmultiAimConstraint.weight = _chestaimwait;

        AimHelper_RHand.transform.position = RHand.transform.position + target_rot_rhand * Vector3.forward * 3;
        rhandmultiAimConstraint.weight = rhandaimwait;

        overrideTransform.weight = _rarmaimwait;

        if (dualwielding)
            animator.SetLayerWeight(2, _barmlayerwait);
        else
            animator.SetLayerWeight(1, _rarmaimwait);

        Sword.dir = transform.forward;
    }

    //return angle in range -180 to 180
    float origin = 0.0f;
    private void LowerBodyMove()
    {
        float targetSpeed = 0.0f;
        bool boosting = false;
        switch (lowerBodyState)
        {
            case LowerBodyState.STAND:
            case LowerBodyState.WALK:
            case LowerBodyState.AIR:
            case LowerBodyState.DASH:
                {

                    if (lowerBodyState != LowerBodyState.AIR || _input.jump) //自由落下以外
                    {
                        // set target speed based on move speed, sprint speed and if sprint is pressed
                        if (lowerBodyState == LowerBodyState.AIR)
                            targetSpeed = AirMoveSpeed;
                        else if (lowerBodyState == LowerBodyState.DASH)
                            targetSpeed = SprintSpeed;
                        else
                            targetSpeed = MoveSpeed;

                        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

                        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                        // if there is no input, set the target speed to 0
                        if (_input.move == Vector2.zero && lowerBodyState != LowerBodyState.DASH)
                        {
                            targetSpeed = 0.0f;

                            _animationBlend = Mathf.Max(_animationBlend - 0.015f, 0.0f);

                            if (_animationBlend < 0.01f) _animationBlend = 0f;
                        }
                        else
                        {
                            _animationBlend = Mathf.Min(_animationBlend + 0.015f, 1.0f);
                        }


                        // a reference to the players current horizontal velocity
                        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                        float speedOffset = 0.1f;
                        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

                        // accelerate or decelerate to target speed
                        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                            currentHorizontalSpeed > targetSpeed + speedOffset)
                        {
                            // creates curved result rather than a linear one giving a more organic speed change
                            // note T in Lerp is clamped, so we don't need to clamp our speed
                            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                                Time.deltaTime * SpeedChangeRate);

                            // round speed to 3 decimal places
                            _speed = Mathf.Round(_speed * 1000f) / 1000f;
                        }
                        else
                        {
                            _speed = targetSpeed;
                        }



                        // normalise input direction
                        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

                        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                        // if there is a move input rotate player when the player is moving
                        if (_input.move != Vector2.zero)
                        {
                            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                              cameraRotation.eulerAngles.y;
                            //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                            //    RotationSmoothTime);

                            float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, lowerBodyState == LowerBodyState.DASH ? DashRotateSpeed : RotateSpeed);

                            // rotate to face input direction relative to camera position
                            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                        }
                    }
                    else //自由落下
                    {
                        // a reference to the players current horizontal velocity
                        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
                        float speedOffset = 0.1f;

                        targetSpeed = AirMoveSpeed;

                        // accelerate or decelerate to target speed
                        if (
                            //currentHorizontalSpeed < targetSpeed - speedOffset ||
                            currentHorizontalSpeed > targetSpeed + speedOffset
                            )
                        {
                            // creates curved result rather than a linear one giving a more organic speed change
                            // note T in Lerp is clamped, so we don't need to clamp our speed
                            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                                Time.deltaTime * SpeedChangeRate);

                            // round speed to 3 decimal places
                            _speed = Mathf.Round(_speed * 1000f) / 1000f;
                        }
                    }


                    switch (lowerBodyState)
                    {
                        case LowerBodyState.STAND:
                            if (_input.move != Vector2.zero)
                            {
                                lowerBodyState = LowerBodyState.WALK;
                                _animator.CrossFadeInFixedTime(_animIDWalk, 0.5f, 0);
                            }

                            break;
                        case LowerBodyState.WALK:
                            if (_input.move == Vector2.zero)
                            {
                                TransitLowerBodyState(LowerBodyState.STAND);
                            }
                            break;
                    }

                    if (lowerBodyState == LowerBodyState.STAND || lowerBodyState == LowerBodyState.WALK)
                    {



                        if (_input.jump)
                        {
                            event_jumped = false;
                            lowerBodyState = LowerBodyState.JUMP;
                            _animator.CrossFadeInFixedTime(_animIDJump, 0.5f, 0);
                        }
                        else if (_input.sprint && upperBodyState == UpperBodyState.STAND)
                        {
                            StartStep();




                        }

                        RegenBoost();
                    }

                    if (lowerBodyState == LowerBodyState.AIR)
                    {
                        if (_input.jump)
                        {
                            if (ConsumeBoost())
                            {
                                _verticalVelocity = Mathf.Min(_verticalVelocity + 1.8f, AscendingVelocity);
                                boosting = true;
                            }
                        }
                        else
                        {
                            if (_input.sprint && upperBodyState == UpperBodyState.STAND)
                            {
                                if (ConsumeBoost())
                                {

                                    // normalise input direction
                                    Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

                                    // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                                    // if there is a move input rotate player when the player is moving
                                    if (_input.move != Vector2.zero)
                                    {
                                        _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                                            cameraRotation.eulerAngles.y;
                                    }

                                    float degree = Mathf.DeltaAngle(transform.eulerAngles.y, _targetRotation);

                                    if (degree < RotateSpeed && degree > -RotateSpeed)
                                    {
                                        lowerBodyState = LowerBodyState.DASH;
                                        _animator.CrossFadeInFixedTime(_animIDDash, 0.25f, 0);
                                        event_dashed = false;
                                    }
                                    else
                                    {
                                        lowerBodyState = LowerBodyState.AIRROTATE;
                                    }

                                    boosting = true;
                                }
                            }
                        }
                        _animator.SetFloat(_animIDVerticalSpeed, _verticalVelocity);
                    }


                    if (lowerBodyState == LowerBodyState.DASH)
                    {
                        _verticalVelocity = 0.0f;

                        bool boost_remain = ConsumeBoost();

                        if ((!_input.sprint || !boost_remain) && event_dashed)
                        {
                            TransitLowerBodyState(LowerBodyState.AIR);
                        }
                        else
                        {
                            boosting = true;
                        }
                    }




                    JumpAndGravity();
                    GroundedCheck();
                }
                break;
            case LowerBodyState.FIRE:
            case LowerBodyState.AIRFIRE:
            case LowerBodyState.SUBFIRE:
            case LowerBodyState.AIRSUBFIRE:
            case LowerBodyState.HEAVYFIRE:
            case LowerBodyState.AIRHEAVYFIRE:
                {
                    targetSpeed = 0.0f;

                    // a reference to the players current horizontal velocity
                    float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                    float speedOffset = 0.1f;
                    float inputMagnitude = 1f;

                    
                    targetSpeed = 0.0f;

                    _animationBlend = Mathf.Max(_animationBlend - 0.015f, 0.0f);

                    if (_animationBlend < 0.01f) _animationBlend = 0f;

                    if (event_heavyfired)
                    {
                        if (lowerBodyState == LowerBodyState.AIRHEAVYFIRE || lowerBodyState == LowerBodyState.HEAVYFIRE)
                        {
                            Vector3 backBlastDir = -(rightWeapon.gameObject.transform.rotation * (Vector3.forward));

                            Vector3 backBlackDir_Horizontal = new Vector3(backBlastDir.x, 0.0f, backBlastDir.z);

                            currentHorizontalSpeed += 50.0f * backBlackDir_Horizontal.magnitude;

                            if (lowerBodyState == LowerBodyState.AIRHEAVYFIRE)
                            {
                                _verticalVelocity += 50.0f * backBlastDir.y;
                            }
                        }
                    }

                    float brakefactor = 1.0f;

                    if(lowerBodyState == LowerBodyState.AIRHEAVYFIRE)
                    {
                        brakefactor = 0.25f;
                    }
                    else if(lowerBodyState == LowerBodyState.HEAVYFIRE)
                    {
                        brakefactor = 0.5f;
                    }

                    // accelerate or decelerate to target speed
                    if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                        currentHorizontalSpeed > targetSpeed + speedOffset)
                    {
                        // creates curved result rather than a linear one giving a more organic speed change
                        // note T in Lerp is clamped, so we don't need to clamp our speed
                        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                            Time.deltaTime * SpeedChangeRate*brakefactor);

                        // round speed to 3 decimal places
                        _speed = Mathf.Round(_speed * 1000f) / 1000f;
                    }
                    else
                    {
                        _speed = targetSpeed;
                    }

                    if (target_chest != null)
                    {
                        Vector3 target_dir = target_chest.transform.position - transform.position;

                        _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                        float rotation;
                        
                        if(lowerBodyState == LowerBodyState.FIRE || lowerBodyState == LowerBodyState.AIRFIRE)
                            rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, RotateSpeed*2);
                        else
                            rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, RotateSpeed * 4);

                        // rotate to face input direction relative to camera position
                        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    }

                    if (lowerBodyState == LowerBodyState.AIRFIRE)
                        animator.SetFloat(_animIDVerticalSpeed, _verticalVelocity);

                    if (lowerBodyState == LowerBodyState.FIRE)
                    {
                        RegenBoost();
                    }

                    JumpAndGravity();
                    GroundedCheck();
                }
                break;
            case LowerBodyState.GROUND:
            case LowerBodyState.JUMP:
            case LowerBodyState.DOWN:
            case LowerBodyState.GETUP:
                {
                    targetSpeed = 0.0f;

                    _animationBlend = 0.0f;





                    switch (lowerBodyState)
                    {


                        case LowerBodyState.GROUND:
                            {
                                _speed = 0.0f;
                                if (event_grounded)
                                {
                                    TransitLowerBodyState(LowerBodyState.STAND);
                                }
                            }
                            break;

                        case LowerBodyState.JUMP:
                            {
                                _speed = 0.0f;
                                if (event_jumped)
                                {
                                    TransitLowerBodyState(LowerBodyState.AIR);
                                    _verticalVelocity = AscendingVelocity;

                                    _controller.Move(new Vector3(0.0f, 0.1f, 0.0f));
                                }
                            }
                            break;

                        case LowerBodyState.DOWN:
                            {
                                if (event_downed && Grounded)
                                {

                                    //_input.down = false;
                                    lowerBodyState = LowerBodyState.GETUP;
                                    upperBodyState = UpperBodyState.GETUP;
                                    _animator.Play(_animIDGetup, 0, 0);
                                    event_getup = false;
                                    origin = transform.position.y;
                                    _verticalVelocity = 0.0f;
                                }

                                JumpAndGravity();
                                GroundedCheck();
                            }
                            break;

                        case LowerBodyState.GETUP:
                            {
                                _speed = 0.0f;
                                if (event_getup)
                                {
                                    TransitLowerBodyState(LowerBodyState.STAND);
                                }
                                else
                                {
                                    AnimatorStateInfo animeStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                                    float prevheight = _controller.height;

                                    float newheight = _controller.height = min_controller_height + animeStateInfo.normalizedTime * (org_controller_height - min_controller_height);


                                    _verticalVelocity = (newheight - prevheight) / Time.deltaTime / 2;
                                }
                            }
                            break;
                    }

                    break;
                }
            case LowerBodyState.AIRROTATE:
                {
                    _verticalVelocity = 0.0f;
                    _speed = 0.0f;
                    // normalise input direction

                    // これを有効化すると、ダッシュ準備中に向きを修正できるようになるが、
                    // 代わりに修正し続けてのホバリングができるようになってしまう。
                    /*   Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

                    // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                    // if there is a move input rotate player when the player is moving
                    if (_input.move != Vector2.zero)
                    {
                        _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                          cameraRotation.eulerAngles.y;
                        //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        //    RotationSmoothTime);
                    }
                    else
                    {
                        _targetRotation = transform.eulerAngles.y;
                    }*/
                    float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, AirDashRotateSpeed);

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);


                    float degree = Mathf.DeltaAngle(transform.eulerAngles.y, _targetRotation);

                    if (degree < RotateSpeed && degree > -RotateSpeed)
                    {
                        lowerBodyState = LowerBodyState.DASH;
                        _animator.CrossFadeInFixedTime(_animIDDash, 0.25f, 0);
                        event_dashed = false;
                    }
                }
                break;
            case LowerBodyState.STEP:
                {


                    _speed = targetSpeed = /*event_stepbegin ? */SprintSpeed/* : 0.0f*/;

                    _animationBlend = 0.0f;

                    stepremain--;


                    if (event_stepped && (!_input.sprint || stepremain <= 0))
                    {
                        TransitLowerBodyState(LowerBodyState.GROUND);
                    }
                }
                break;
            case LowerBodyState.GROUNDSLASH_DASH:
            case LowerBodyState.AIRSLASH_DASH:
            case LowerBodyState.QUICKSLASH_DASH:
            case LowerBodyState.DASHSLASH_DASH:
                {
                    float rotatespeed;

                    /* if (lowerBodyState == LowerBodyState.DASHSLASH_DASH)
                         _speed = targetSpeed = SprintSpeed * 2.0f;
                     else if (lowerBodyState == LowerBodyState.GROUNDSLASH_DASH)
                         _speed = targetSpeed = SprintSpeed;
                     else
                         _speed = targetSpeed = SprintSpeed * 1.5f
                    */

                    _speed = targetSpeed = Sword.motionProperty[lowerBodyState].DashSpeed;

                    if (lowerBodyState == LowerBodyState.DASHSLASH_DASH)
                        rotatespeed = RotateSpeed / 2.0f;
                    else
                        rotatespeed = RotateSpeed;

                    _animationBlend = 0.0f;


                    if (lowerBodyState == LowerBodyState.AIRSLASH_DASH || lowerBodyState == LowerBodyState.DASHSLASH_DASH)
                    {
                        boosting = true;
                    }



                    if (target_chest != null)
                    {
                        if (lowerBodyState == LowerBodyState.DASHSLASH_DASH)
                        {

                            Vector3 targetOffset = dashslash_offset;

                            Vector3 targetPos = target_chest.transform.position + targetOffset.normalized * (Sword.SlashDistance * transform.lossyScale.x * 0.9f);

                            Vector3 targetDirection = (targetPos - Chest.transform.position).normalized;

                            _targetRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

                            float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, RotateSpeed);

                            // rotate to face input direction relative to camera position
                            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                        }
                        else
                        {
                            Vector3 target_dir = target_chest.transform.position - transform.position;

                            _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                            float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, rotatespeed);

                            // rotate to face input direction relative to camera position
                            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                        }
                    }

                    bool slash = false;
                    bool lowerslash = false;

                    if (target_chest == null)
                    {
                        slash = true;
                    }
                    else
                    {
                        if (lowerBodyState == LowerBodyState.DASHSLASH_DASH)
                        {
                            if ((target_chest.transform.position - Chest.transform.position).magnitude < Sword.SlashDistance * transform.lossyScale.x)
                            {
                                slash = true;
                            }
                        }
                        else
                        {
                            if ((target_chest.transform.position - Chest.transform.position).magnitude < Sword.SlashDistance * transform.lossyScale.x)
                            {
                                slash = true;
                            }
                        }

                        //if(target_chest.transform.position.y < Chest.transform.position.y - 0.00852969*Chest.transform.lossyScale.y)
                        if (Target_Robot.lowerBodyState == LowerBodyState.DOWN || Target_Robot.lowerBodyState == LowerBodyState.GETUP)
                        {
                            lowerslash = true;
                        }

                        if (target_chest.transform.lossyScale.y <= Chest.transform.lossyScale.y * 0.501
                            && target_chest.transform.position.y < Chest.transform.position.y)
                        {
                            lowerslash = true;
                        }
                    }

                    stepremain--;

                    if (slash || stepremain <= 0)
                    {

                        if (lowerBodyState == LowerBodyState.GROUNDSLASH_DASH)
                        {
                            if (lowerslash)
                            {
                                lowerBodyState = LowerBodyState.LowerSlash;
                                upperBodyState = UpperBodyState.LowerSlash;
                                _animator.speed = 1.0f;
                                event_slash = false;
                                slash_reserved = false;
                                Sword.slashing = false;
                                slash_count = 0;
                                Sword.damage = 100;
                                Sword.strong = true;
                                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.LowerSlash]._animID[slash_count], 0.0f, 0);
                            }
                            else
                            {
                                lowerBodyState = LowerBodyState.GroundSlash;
                                upperBodyState = UpperBodyState.GroundSlash;
                                _animator.speed = 1.0f;
                                event_slash = false;
                                slash_reserved = false;
                                Sword.slashing = false;
                                slash_count = 0;
                                Sword.damage = 100;
                                Sword.strong = true;

                                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.GroundSlash]._animID[slash_count], 0.0f, 0);
                            }
                        }
                        else if (lowerBodyState == LowerBodyState.QUICKSLASH_DASH)
                        {
                            if (lowerslash)
                            {
                                lowerBodyState = LowerBodyState.LowerSlash;
                                upperBodyState = UpperBodyState.LowerSlash;
                                _animator.speed = 1.0f;
                                event_slash = false;
                                slash_reserved = false;
                                Sword.slashing = false;
                                slash_count = 0;
                                Sword.damage = 100;
                                Sword.strong = true;
                                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.LowerSlash]._animID[slash_count], 0.0f, 0);
                            }
                            else
                            {
                                lowerBodyState = LowerBodyState.QuickSlash;
                                upperBodyState = UpperBodyState.QuickSlash;
                                _animator.speed = 1.0f;
                                event_slash = false;
                                slash_reserved = false;
                                Sword.slashing = false;
                                slash_count = 0;
                                Sword.damage = 100;
                                Sword.strong = true;
                                _verticalVelocity = 0.0f;
                                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.QuickSlash]._animID[slash_count], 0.0f, 0);
                            }
                        }
                        else if (lowerBodyState == LowerBodyState.DASHSLASH_DASH)
                        {
                            lowerBodyState = LowerBodyState.DashSlash;
                            upperBodyState = UpperBodyState.DashSlash;
                            _animator.speed = 1.0f;
                            event_slash = false;
                            slash_reserved = false;
                            Sword.slashing = false;
                            slash_count = 0;
                            Sword.damage = 100;
                            Sword.strong = true;
                            _verticalVelocity = 0.0f;
                            _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.DashSlash]._animID[slash_count], 0.0f, 0);
                        }
                        else
                        {
                            lowerBodyState = LowerBodyState.AirSlash;
                            upperBodyState = UpperBodyState.AirSlash;
                            _animator.speed = 1.0f;
                            event_slash = false;
                            slash_reserved = false;
                            Sword.slashing = false;
                            slash_count = 0;
                            Sword.damage = 100;
                            Sword.strong = true;
                            _verticalVelocity = 0.0f;
                            _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.AirSlash]._animID[slash_count], 0.0f, 0);
                        }
                    }


                }
                break;
            case LowerBodyState.KNOCKBACK:
                {
                    // a reference to the players current horizontal velocity
                    float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
                    float speedOffset = 0.1f;

                    targetSpeed = 0.0f;

                    // accelerate or decelerate to target speed
                    if (
                        //currentHorizontalSpeed < targetSpeed - speedOffset ||
                        currentHorizontalSpeed > targetSpeed + speedOffset
                        )
                    {
                        // creates curved result rather than a linear one giving a more organic speed change
                        // note T in Lerp is clamped, so we don't need to clamp our speed
                        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                            Time.deltaTime * SpeedChangeRate);

                        // round speed to 3 decimal places
                        _speed = Mathf.Round(_speed * 1000f) / 1000f;
                    }

                    if (event_knockbacked)
                    {
                        TransitLowerBodyState(LowerBodyState.STAND);

                    }

                    JumpAndGravity();
                    GroundedCheck();
                }
                break;
            case LowerBodyState.GroundSlash:
            case LowerBodyState.AirSlash:
            case LowerBodyState.LowerSlash:
            case LowerBodyState.QuickSlash:
            case LowerBodyState.DashSlash:
                {

                    _animationBlend = 0.0f;

                    _speed = 0.0f;



                    if (target_chest != null)
                    {
                        if (lowerBodyState == LowerBodyState.DashSlash)
                        {

                            Vector3 targetOffset = dashslash_offset;

                            Vector3 targetPos = target_chest.transform.position + targetOffset.normalized * (Sword.SlashDistance * transform.lossyScale.x * 0.9f);

                            Vector3 targetDirection = (targetPos - Chest.transform.position).normalized;

                            //_targetRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

                            //float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, RotateSpeed);

                            // rotate to face input direction relative to camera position
                            //transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);*/

                            //if ((target_chest.transform.position - Chest.transform.position).magnitude > Sword.SlashDistance * transform.lossyScale.x)
                            {
                                _speed = targetSpeed = /*event_stepbegin ? */SprintSpeed * 2/* : 0.0f*/;
                            }
                        }
                        else
                        {
                            Vector3 target_dir = target_chest.transform.position - transform.position;

                            _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                            float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, RotateSpeed);

                            // rotate to face input direction relative to camera position
                            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                            if ((target_chest.transform.position - Chest.transform.position).magnitude > Sword.SlashDistance * transform.lossyScale.x)
                            {
                                _speed = targetSpeed = /*event_stepbegin ? */MoveSpeed/* : 0.0f*/;
                            }
                        }
                    }
                    else
                    {
                        if (lowerBodyState == LowerBodyState.DashSlash)
                        {
                            _speed = targetSpeed = /*event_stepbegin ? */SprintSpeed * 2/* : 0.0f*/;
                        }
                    }

                    //targetSpeed = 0.0f;
                    //_speed = targetSpeed = /*event_stepbegin ? */MoveSpeed/* : 0.0f*/;


                    if (lowerBodyState == LowerBodyState.DashSlash)
                        boosting = true;

                    if(_input.slash)
                    {
                        slash_reserved = true;
                    }


                    if (lowerBodyState == LowerBodyState.AirSlash && event_slash)
                    {
                        slash_count++;
                        if (slash_count == Sword.slashMotionInfo[LowerBodyState.AirSlash].num || !slash_reserved)
                        {
                            Sword.emitting = false;

                            TransitLowerBodyState(LowerBodyState.AIR);
                        }
                        else
                        {

                            //_input.slash = false;

                            lowerBodyState = LowerBodyState.AirSlash;
                            upperBodyState = UpperBodyState.AirSlash;
                            event_slash = false;
                            slash_reserved = false;
                            Sword.slashing = false;
                            _verticalVelocity = 0.0f;
                            Sword.damage = 100;
                            Sword.strong = false;

                            _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.AirSlash]._animID[slash_count], 0.0f, 0);
                        }
                    }
                    else if (lowerBodyState == LowerBodyState.GroundSlash && event_slash)
                    {
                        slash_count++;
                        if (slash_count == Sword.slashMotionInfo[LowerBodyState.GroundSlash].num || !slash_reserved)
                        {
                            Sword.emitting = false;
                            TransitLowerBodyState(LowerBodyState.STAND);
                        }
                        else
                        {

                            //_input.slash = false;

                            lowerBodyState = LowerBodyState.GroundSlash;
                            upperBodyState = UpperBodyState.GroundSlash;
                            event_slash = false;
                            slash_reserved = false;
                            Sword.slashing = false;

                            if (slash_count == Sword.slashMotionInfo[LowerBodyState.GroundSlash].num - 1)
                            {
                                Sword.damage = 200;

                            }
                            else
                            {
                                Sword.damage = 100;

                            }

                            if (slash_count == 1)
                            {

                                Sword.strong = true;

                            }
                            else
                            {

                                Sword.strong = false;
                            }

                            _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.GroundSlash]._animID[slash_count], 0.0f, 0);
                        }
                    }
                    else if (lowerBodyState == LowerBodyState.LowerSlash && event_slash)
                    {
                        slash_count++;
                        if (slash_count == Sword.slashMotionInfo[LowerBodyState.LowerSlash].num || !slash_reserved)
                        {
                            Sword.emitting = false;
                            TransitLowerBodyState(LowerBodyState.STAND);
                        }
                        else
                        {

                            //_input.slash = false;

                            lowerBodyState = LowerBodyState.LowerSlash;
                            upperBodyState = UpperBodyState.LowerSlash;
                            event_slash = false;
                            slash_reserved = false;
                            Sword.slashing = false;

                            if (slash_count == Sword.slashMotionInfo[LowerBodyState.GroundSlash].num - 1)
                            {
                                Sword.damage = 200;

                            }
                            else
                            {
                                Sword.damage = 100;

                            }

                            if (slash_count == 1)
                            {

                                Sword.strong = true;

                            }
                            else
                            {

                                Sword.strong = false;
                            }

                            _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.LowerSlash]._animID[slash_count], 0.0f, 0);
                        }
                    }
                    else if (lowerBodyState == LowerBodyState.QuickSlash && event_slash)
                    {
                        slash_count++;
                        if (slash_count == Sword.slashMotionInfo[LowerBodyState.QuickSlash].num || !slash_reserved)
                        {
                            Sword.emitting = false;
                            TransitLowerBodyState(LowerBodyState.STAND);
                        }
                        else
                        {

                            //_input.slash = false;

                            lowerBodyState = LowerBodyState.QuickSlash;
                            upperBodyState = UpperBodyState.QuickSlash;
                            event_slash = false;
                            slash_reserved = false;
                            Sword.slashing = false;

                            _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.QuickSlash]._animID[slash_count], 0.0f, 0);
                        }
                    }
                    if (lowerBodyState == LowerBodyState.DashSlash && event_slash)
                    {
                        slash_count++;
                        if (slash_count == Sword.slashMotionInfo[LowerBodyState.DashSlash].num || !slash_reserved)
                        {
                            Sword.emitting = false;
                            TransitLowerBodyState(LowerBodyState.AIR);
                        }
                        else
                        {

                            //_input.slash = false;

                            lowerBodyState = LowerBodyState.DashSlash;
                            upperBodyState = UpperBodyState.DashSlash;
                            event_slash = false;
                            slash_reserved = false;
                            Sword.slashing = false;

                            _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.DashSlash]._animID[slash_count], 0.0f, 0);
                        }


                    }


                    JumpAndGravity();
                    GroundedCheck();
                    break;

            
               }
        }


        if (lowerBodyState == LowerBodyState.STEP)
        {
            Vector3 targetDirection;

            float stepangle = 0.0f;

            switch (stepDirection)
            {
                case StepDirection.LEFT:
                    stepangle = -90.0f;
                    break;
                case StepDirection.RIGHT:
                    stepangle = 90.0f;
                    break;
                case StepDirection.BACKWARD:
                    stepangle = -180.0f;
                    break;
                case StepDirection.FORWARD:
                    stepangle = 0.0f;
                    break;
            }

            targetDirection = Quaternion.Euler(0.0f, transform.eulerAngles.y + stepangle, 0.0f) * Vector3.forward;


            transform.rotation = Quaternion.Euler(0.0f, Mathf.MoveTowardsAngle(transform.eulerAngles.y, steptargetrotation, 1.0f), 0.0f);

            // move the player
            MoveAccordingTerrain(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else if (lowerBodyState == LowerBodyState.KNOCKBACK || lowerBodyState == LowerBodyState.DOWN)
        {
            Vector3 targetDirection;


            targetDirection = knockbackdir;


            //transform.rotation = Quaternion.Euler(0.0f, Mathf.MoveTowardsAngle(transform.eulerAngles.y, steptargetrotation, 1.0f), 0.0f);

            // move the player
            MoveAccordingTerrain(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else if (lowerBodyState == LowerBodyState.AIRSLASH_DASH)
        {
            Vector3 targetDirection;


            if (target_chest != null)
            {
                Vector3 targetOffset = (Chest.transform.position - target_chest.transform.position);

                targetOffset.y = 0.0f;

                Vector3 targetPos = target_chest.transform.position + targetOffset.normalized * (Sword.SlashDistance * transform.lossyScale.x * 0.9f);

                targetDirection = (targetPos - Chest.transform.position).normalized;

                _verticalVelocity = targetDirection.y * _speed;

                targetDirection.y = 0.0f;

                targetDirection = transform.rotation * Vector3.forward * targetDirection.magnitude;
            }
            else
            {
                targetDirection = (transform.rotation * Vector3.forward).normalized;
            }




            // move the player
            MoveAccordingTerrain(targetDirection * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else if (lowerBodyState == LowerBodyState.DASHSLASH_DASH)
        {
            Vector3 targetDirection;


            if (target_chest != null)
            {
                Vector3 targetOffset = dashslash_offset;

                Vector3 targetPos = target_chest.transform.position + targetOffset.normalized * (Sword.SlashDistance * transform.lossyScale.x * 0.9f);

                targetDirection = (targetPos - Chest.transform.position).normalized;

                targetDirection.y = Math.Min(Math.Max(targetDirection.y, -0.25f), 0.25f);

                targetDirection = targetDirection.normalized;

                _verticalVelocity = targetDirection.y * _speed;

                targetDirection.y = 0.0f;

                targetDirection = transform.rotation * Vector3.forward * targetDirection.magnitude;
            }
            else
            {
                targetDirection = (transform.rotation * Vector3.forward).normalized;
            }




            // move the player
            MoveAccordingTerrain(targetDirection * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else if(lowerBodyState == LowerBodyState.AIRHEAVYFIRE || lowerBodyState == LowerBodyState.HEAVYFIRE)
        {
            Vector3 targetDirection = transform.rotation * Vector3.back;

            // move the player
            MoveAccordingTerrain(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else
        {
            Vector3 targetDirection = transform.rotation * Vector3.forward;

            // move the player
            MoveAccordingTerrain(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }


        foreach (var thruster in thrusters)
        {
            thruster.emitting = boosting;
        }
    }


    // ここで扱わないもの
    // KNOCKBACKは複雑すぎるのでDoDamageにべた書き
    private void TransitLowerBodyState(LowerBodyState newState)
    {
        if (lowerBodyState == LowerBodyState.DOWN && newState != LowerBodyState.DOWN)
            _controller.height = org_controller_height;

        if (
            (lowerBodyState == LowerBodyState.GroundSlash && newState != LowerBodyState.GroundSlash)
            || (lowerBodyState == LowerBodyState.LowerSlash && newState != LowerBodyState.LowerSlash)
            || (lowerBodyState == LowerBodyState.QuickSlash && newState != LowerBodyState.QuickSlash)
             || (lowerBodyState == LowerBodyState.DashSlash && newState != LowerBodyState.DashSlash)
            )
        {
            Sword.emitting = false;
        }

        if (lowerBodyState == LowerBodyState.AirSlash && newState != LowerBodyState.AirSlash)
        {
            Sword.emitting = false;
        }

        switch (newState)
        {
            case LowerBodyState.AIR:

                if (lowerBodyState == LowerBodyState.DASH)
                    _animator.CrossFadeInFixedTime(_animIDAir, 0.25f, 0);
                else
                    _animator.CrossFadeInFixedTime(_animIDAir, 0.5f, 0);

                if (lowerBodyState == LowerBodyState.AirSlash)
                {
                    upperBodyState = UpperBodyState.STAND;
                }
                if (lowerBodyState == LowerBodyState.DashSlash)
                {
                    upperBodyState = UpperBodyState.STAND;
                }

                Grounded = false;
                break;
            case LowerBodyState.DOWN:
                upperBodyState = UpperBodyState.DOWN;

                _animator.Play(_animIDDown, 0, 0);
                event_downed = false;
                //_input.down = false;
                _controller.height = min_controller_height;
                break;
            case LowerBodyState.GROUND:

                if (lowerBodyState == LowerBodyState.STEP)
                {
                    event_grounded = false;
                    _animator.CrossFadeInFixedTime(_animIDGround, 0.25f, 0, 0.15f);
                }
                else
                {
                    _animator.Play(_animIDGround, 0, 0);
                    event_grounded = false;
                }

                if(lowerBodyState == LowerBodyState.AIRSUBFIRE 
                    || lowerBodyState == LowerBodyState.AIRHEAVYFIRE
                    || lowerBodyState == LowerBodyState.SUBFIRE
                    || lowerBodyState == LowerBodyState.HEAVYFIRE) // 地形が動いたり、押し出された落ちたりしたらこっちもありえるかも
                {
                    //upperBodyState = UpperBodyState.STAND;
                }

                break;
            case LowerBodyState.STAND:
                switch (lowerBodyState)
                {
                    case LowerBodyState.FIRE:
                        break;
                    case LowerBodyState.WALK:
                        _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                        break;
                    case LowerBodyState.GROUND:
                        _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                        break;
                    case LowerBodyState.GETUP:
                        upperBodyState = UpperBodyState.STAND;
                        _animator.Play(_animIDStand, 0, 0);
                        _controller.height = org_controller_height;
                        _verticalVelocity = 0.0f;
                        break;
                    case LowerBodyState.KNOCKBACK:
                    case LowerBodyState.GroundSlash:
                    case LowerBodyState.LowerSlash:
                    case LowerBodyState.QuickSlash:

                        upperBodyState = UpperBodyState.STAND;
                        _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                        break;
                  }
                break;
        }

        lowerBodyState = newState;
    }

    private void StartStep()
    {
        event_stepped = false;
        event_stepbegin = false;
        lowerBodyState = LowerBodyState.STEP;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        float steptargetdegree = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                        cameraRotation.eulerAngles.y;




        float stepmotiondegree = Mathf.Repeat(steptargetdegree - transform.eulerAngles.y + 180.0f, 360.0f) - 180.0f;



        if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
            stepDirection = StepDirection.RIGHT;
        else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
            stepDirection = StepDirection.BACKWARD;
        else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
            stepDirection = StepDirection.LEFT;
        else
            stepDirection = StepDirection.FORWARD;



        switch (stepDirection)
        {
            case StepDirection.LEFT:
                _animator.CrossFadeInFixedTime(_animIDStep_Left, 0.25f, 0);
                steptargetrotation = steptargetdegree + 90.0f;
                break;
            case StepDirection.BACKWARD:
                _animator.CrossFadeInFixedTime(_animIDStep_Back, 0.25f, 0);
                steptargetrotation = steptargetdegree + 180.0f;
                break;
            case StepDirection.RIGHT:
                _animator.CrossFadeInFixedTime(_animIDStep_Right, 0.25f, 0);
                steptargetrotation = steptargetdegree - 90.0f;
                break;
            default:
                //case StepDirection.FORWARD:
                _animator.CrossFadeInFixedTime(_animIDStep_Front, 0.25f, 0);
                steptargetrotation = steptargetdegree;
                break;
        }

        stepremain = StepLimit;
    }
    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

        }
        else
        {

        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        //if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity = Mathf.Max(_verticalVelocity + Gravity * Time.deltaTime, -TerminalVelocity);
        }
    }

    private void OnGrounded()
    {
        event_grounded = true;
    }
    private void OnJumped()
    {
        event_jumped = true;
    }

    private void OnStepped()
    {
        event_stepped = true;
    }

    private void OnStepBegin()
    {
        event_stepbegin = true;
    }
    private void OnDashed()
    {
        event_dashed = true;
    }
    private void OnKnockbacked()
    {
        event_knockbacked = true;
    }
    private void OnGetup()
    {
        event_getup = true;
    }

    private void OnDowned()
    {
        event_downed = true;
    }


    private void OnSlashEnd()
    {
        event_slash = true;
    }

    private void OnSlashBegin()
    {
        Sword.slashing = true;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        if (lowerBodyState == LowerBodyState.DOWN)
            Gizmos.DrawSphere(
          new Vector3(transform.position.x, transform.position.y + 3.4f, transform.position.z),
          GroundedRadius);
        else
            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }

    public Vector3 GetCenter()
    {
        return transform.TransformPoint(new Vector3(0, 3.805078f, 0));
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    void MoveAccordingTerrain(Vector3 velocity)
    {
        if (Grounded)
        {
            if (velocity.y <= 0.0f)
            {
                if (Vector3.Angle(Vector3.up, hitNormal) < _controller.slopeLimit)
                {

                    Vector3 newVelocity = velocity;

                    newVelocity.y = 0.0f;

                    Quaternion q = Quaternion.FromToRotation(Vector3.up, hitNormal);

                    newVelocity = q * newVelocity;
                    newVelocity.y += velocity.y;

                    if (newVelocity.y < 0.0f)
                        _controller.Move(newVelocity);
                    else
                        _controller.Move(velocity);
                }
                else
                    _controller.Move(velocity);
            }
            else
            {
                _controller.Move(velocity);
            }
        }
        else
        {
            _controller.Move(velocity);
        }
    }

    void OnSubFireAnim()
    {
        event_subfired = true;
    }

    void OnHeavyFire()
    {
        event_heavyfired = true;
    }
}
