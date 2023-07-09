using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

using UnityEngine.Animations.Rigging;
using System;
using System.Collections.Generic;
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

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    public float TerminalVelocity = 53.0f;
    public float AscendingVelocity = 20.0f;

    public int HP = 500;
    public int MaxHP = 500;

    public int StepLimit = 30;

    public float SlashDistance = 7.0f;

    int stepremain = 0;

    public Vector3 offset;

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

    private int _animIDDown;
    private int _animIDGetup;

    private int _animIDGroundSlash;

    private Animator _animator;
    private CharacterController _controller;
    
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    public InputBase _input;

    public MultiAimConstraint headmultiAimConstraint;
    public MultiAimConstraint chestmultiAimConstraint;
    public RigBuilder rigBuilder;

    //public MultiAimConstraint rarmmultiAimConstraint;
    public OverrideTransform overrideTransform;
    public GameObject aimingBase;
    public GameObject shoulder_hint;

    public RobotController Target_Robot;
    public GameObject Head = null;
    public GameObject Chest = null;
    private GameObject target_chest;
    private GameObject target_head;

    public List<RobotController> lockingEnemy = new List<RobotController>();

    private float _headaimwait = 0.0f;

    private float _chestaimwait = 0.0f;

    private float _rarmaimwait = 0.0f;

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
    bool event_groundslash = false;

    public Vector3 center_offset;

    public enum UpperBodyState
    {
        STAND,
        FIRE,
        KNOCKBACK,
        DOWN,
        GETUP,
        GROUNDSLASH_DASH,
        GROUNDSLASH
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
        GROUNDSLASH
    }

    public enum StepDirection
    {
        FORWARD,LEFT,BACKWARD,RIGHT
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

    int boost
    {
        get { return _boost; }
        set {
                _boost = value;

            if (HUDCanvas != null)
                boostSlider.value = _boost;
            }
    }

    GameObject beam_prefab;

    public GameObject Rhand;

    public GameObject Gun;
    public BeamSaber Sword;

    public UIController_Overlay reticle_UICO;

    private bool spawn_completed = false; // スポーンしたフレームにDoDamage呼ばれると落ちるので

    public GameObject AimHelper_Head = null;
    public GameObject AimHelper_Chest = null;

    Quaternion AimTargetRotation_Head;
    Quaternion AimTargetRotation_Chest;

    public void DoDamage(Vector3 dir,int damage)
    {
        if (!spawn_completed)
            return;

        if (lowerBodyState != LowerBodyState.DOWN)
        {
            if (lowerBodyState == LowerBodyState.AIR || lowerBodyState == LowerBodyState.AIRFIRE || lowerBodyState == LowerBodyState.AIRROTATE || lowerBodyState==LowerBodyState.DASH)
            {
                TransitLowerBodyState(LowerBodyState.DOWN);
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

                if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
                    _animator.Play(_animIDKnockback_Right, 0, 0);
                else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
                    _animator.Play(_animIDKnockback_Back, 0, 0);
                else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
                    _animator.Play(_animIDKnockback_Left, 0, 0);
                else
                    _animator.Play(_animIDKnockback_Front, 0, 0);

                _controller.height = 7.0f;

                _verticalVelocity = 0.0f;

                _speed = SprintSpeed;
            }
        }

        HP = Math.Max(0, HP - damage);

        if(HP <= 0)
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

        if(HUDCanvas != null)
            reticle_UICO.targetTfm = Target_Robot.transform;
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
            reticle_UICO.targetTfm = null;
    }

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        //HUDCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
        if (HUDCanvas != null)
        {
            boostSlider = HUDCanvas.gameObject.transform.Find("BoostSlider").GetComponent<Slider>();

            Transform reticle = HUDCanvas.gameObject.transform.Find("Reticle");

            reticle_UICO = reticle.GetComponent<UIController_Overlay>();
        }

        beam_prefab = Resources.Load<GameObject>("Beam");
        //gun = Rhand.transform.Find("BeamRifle").gameObject;

        center_offset = transform.Find("Robot").localPosition;
    }

    private void Start()
    {
        if(CinemachineCameraTarget!=null)
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        //_input = GetComponent<StarterAssetsInputs>();

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        if(HUDCanvas != null)
            boostSlider.value = boostSlider.maxValue = boost = Boost_Max;

        AimTargetRotation_Head = Head.transform.rotation;
        AimTargetRotation_Chest = Chest.transform.rotation;

        if (Target_Robot != null)
        {
            TargetEnemy(Target_Robot);
        }

        HP = MaxHP;

        if(team == null)
        {
            worldManager.AssignToTeam(this);
        }

        Sword.emitting = false;

        spawn_completed = true;
    }

    private bool ConsumeBoost()
    {
        if(boost > 0)
        {
            boost--;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void RegenBoost()
    {
        boost = Math.Min(boost+1, Boost_Max);
    }

    public static float DistanceToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    private void Update()
    {
        if (!dead)
        {
            _hasAnimator = TryGetComponent(out _animator);

            float mindist = float.MaxValue;

            RobotController nearest_robot = null;

            foreach (var team in worldManager.teams)
            {
                if (team == this.team)
                    continue;

               

                foreach(var robot in team.robotControllers)
                {
                    float dist = DistanceToLine(Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0.0f)), robot.transform.TransformPoint(robot.center_offset));

                    if(dist < mindist)
                    {
                        mindist = dist;
                        nearest_robot = robot;
                    }
                }
            }

            if(Target_Robot != nearest_robot)
                TargetEnemy(nearest_robot);

            UpperBodyMove();
            LowerBodyMove();
        }
        else
        {
            UntargetEnemy();

            for (int i=0;i<lockingEnemy.Count;i++)
            {
                lockingEnemy[i].PurgeTarget(this);
            }

            worldManager.HandleRemoveUnit(this);

            GameObject.Instantiate(explode_prefab, transform.position, Quaternion.identity);

            GameObject.Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {


        if (CinemachineCameraTarget != null)
        {
            CameraRotation();
            CinemachineCameraTarget.transform.position = transform.position + CinemachineCameraTarget.transform.rotation * offset;
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

        _animIDDown = Animator.StringToHash("Down");
        _animIDGetup = Animator.StringToHash("Getup");

        _animIDGroundSlash = Animator.StringToHash("GroundSlash");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition;

        if(lowerBodyState == LowerBodyState.DOWN)
            spherePosition = new Vector3(transform.position.x, transform.position.y+3.4f,
                transform.position.z);
        else
            spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        switch(lowerBodyState)
        {
            case LowerBodyState.AIR:
            case LowerBodyState.AIRFIRE:
            case LowerBodyState.DASH:
            case LowerBodyState.AIRROTATE:

                if (Grounded)
                {
                    _animator.Play(_animIDGround,0,0);
                    lowerBodyState = LowerBodyState.GROUND;
                    event_grounded = false;
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

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            //float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            float deltaTimeMultiplier =1.0f;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void UpperBodyMove()
    {
        bool head_no_aiming = false;
        bool chest_no_aiming = false;
  

        switch(upperBodyState)
        {
            case UpperBodyState.FIRE:
                {
                    _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.05f);

                    _rarmaimwait = Mathf.Min(1.0f, _rarmaimwait + 0.02f);
                    _chestaimwait = Mathf.Min(1.0f, _chestaimwait + 0.02f);

                    if (animator.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1)
                    {
                        upperBodyState = UpperBodyState.STAND;

                        if (lowerBodyState == LowerBodyState.AIRFIRE)
                        {
                            TransitLowerBodyState(LowerBodyState.AIR);
                        }
                        else if (lowerBodyState == LowerBodyState.FIRE)
                            lowerBodyState = LowerBodyState.STAND;

                        GameObject beam_obj = GameObject.Instantiate(beam_prefab, Gun.transform.position,Gun.transform.rotation);

                        Beam beam = beam_obj.GetComponent<Beam>();

                        beam.direction = Gun.transform.forward;
                    }
                }
                break;
            case UpperBodyState.STAND:
                {
                    float angle = 180.0f;

                    if (target_chest)
                    {
                        angle = Vector3.Angle(target_chest.transform.position - transform.position, transform.forward);

                        if (angle > 60)
                        {
                            _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.05f);
                            head_no_aiming = true;
                        }
                        else
                            _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.05f);
                    }
                    else
                    {
                        _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.05f);
                        head_no_aiming = true;
                    }


                    if (_input.fire)
                    {
                        upperBodyState = UpperBodyState.FIRE;
                        _input.fire = false;

                        animator.Play("Armature|Fire", 1, 0.0f);

                                                    

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
                        }
       
                    }

                    if(_input.slash)
                    {
                        _input.slash = false;
                        lowerBodyState = LowerBodyState.GROUNDSLASH_DASH;
                        upperBodyState = UpperBodyState.GROUNDSLASH_DASH;
                        event_stepbegin = event_stepped = false;
                        _animator.CrossFadeInFixedTime(_animIDStep_Front, 0.0f, 0);
                        stepremain = StepLimit;
                        

                        if (target_chest != null)
                        {
                            Vector3 target_dir = target_chest.transform.position - transform.position;

                            _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                            float rotation = _targetRotation;

                            // rotate to face input direction relative to camera position
                            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                        }
                    }

                    _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.02f);
                    _chestaimwait = Mathf.Max(0.0f, _chestaimwait - 0.02f);
                    chest_no_aiming = true;
                }
                break;
            case UpperBodyState.KNOCKBACK:
            case UpperBodyState.DOWN:
            case UpperBodyState.GETUP:
            case UpperBodyState.GROUNDSLASH:
            case UpperBodyState.GROUNDSLASH_DASH:
                _rarmaimwait = 0.0f;
                _chestaimwait = 0.0f;
                _headaimwait = 0.0f;
                break;
        }

        headmultiAimConstraint.weight = _headaimwait;

        //headmultiAimConstraint.weight = 1.0f;

        Quaternion target_rot_head;
        Quaternion target_rot_chest;

        if (target_chest != null)
        {

            Quaternion q_base_global = Quaternion.Inverse(shoulder_hint.transform.rotation);

            Quaternion q_aim_global = Quaternion.LookRotation(shoulder_hint.transform.position - target_chest.transform.position, new Vector3(0.0f, 1.0f, 0.0f));

            Quaternion q_rotation_global = q_base_global * q_aim_global;

            Quaternion q_base = Quaternion.Inverse(aimingBase.transform.rotation);

            Quaternion q_final = q_base * q_rotation_global * aimingBase.transform.rotation;

            //overrideTransform.data.rotation = q_final.eulerAngles;
            overrideTransform.data.position = shoulder_hint.transform.position;
            overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;

            target_rot_head = Quaternion.LookRotation(target_head.transform.position - Head.transform.position, new Vector3(0.0f, 1.0f, 0.0f));
            target_rot_chest = Quaternion.LookRotation(target_chest.transform.position - Chest.transform.position, new Vector3(0.0f, 1.0f, 0.0f));
        }
        else
        {
            Quaternion q_base_global = Quaternion.Inverse(shoulder_hint.transform.rotation);

            Quaternion q_aim_global = Quaternion.LookRotation(-shoulder_hint.transform.forward, new Vector3(0.0f, 1.0f, 0.0f));

            Quaternion q_rotation_global = q_base_global * q_aim_global;

            Quaternion q_base = Quaternion.Inverse(aimingBase.transform.rotation);

            Quaternion q_final = q_base * q_rotation_global * aimingBase.transform.rotation;

            //overrideTransform.data.rotation = q_final.eulerAngles;
            overrideTransform.data.position = shoulder_hint.transform.position;
            overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;

            target_rot_head = Head.transform.rotation;
            target_rot_chest = Chest.transform.rotation;
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

        AimHelper_Head.transform.position = Head.transform.position + AimTargetRotation_Head * Vector3.forward*3;


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

        AimHelper_Chest.transform.position = Chest.transform.position + AimTargetRotation_Chest * Vector3.forward * 3;



        overrideTransform.weight = _rarmaimwait;

        animator.SetLayerWeight(1, _rarmaimwait);

        chestmultiAimConstraint.weight = _chestaimwait;

        Sword.dir = transform.forward;
    }

    //return angle in range -180 to 180
    float accum = 0.0f;
    float origin = 0.0f;
    private void LowerBodyMove()
    {
        float targetSpeed = 0.0f;

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
                                              _mainCamera.transform.eulerAngles.y;
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
                                lowerBodyState = LowerBodyState.STAND;
                                _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
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
                                _verticalVelocity = Mathf.Min(_verticalVelocity + 0.4f, AscendingVelocity);
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
                                                            _mainCamera.transform.eulerAngles.y;
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
                    }




                    JumpAndGravity();
                    GroundedCheck();
                }
                break;
            case LowerBodyState.FIRE:
            case LowerBodyState.AIRFIRE:
                {
                    targetSpeed = 0.0f;

                    // a reference to the players current horizontal velocity
                    float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                    float speedOffset = 0.1f;
                    float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;


                    targetSpeed = 0.0f;

                    _animationBlend = Mathf.Max(_animationBlend - 0.015f, 0.0f);

                    if (_animationBlend < 0.01f) _animationBlend = 0f;

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

                    if (target_chest != null)
                    {
                        Vector3 target_dir = target_chest.transform.position - transform.position;

                        _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                        float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, RotateSpeed);

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


                    _speed = 0.0f;


                    switch (lowerBodyState)
                    {


                        case LowerBodyState.GROUND:
                            {
                                if (event_grounded)
                                {
                                    lowerBodyState = LowerBodyState.STAND;
                                    _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                                }
                            }
                            break;

                        case LowerBodyState.JUMP:
                            {
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

                                    _input.down = false;
                                    lowerBodyState = LowerBodyState.GETUP;
                                    upperBodyState = UpperBodyState.GETUP;
                                    _animator.Play(_animIDGetup, 0, 0);
                                    event_getup = false;
                                    accum = 0.0f;
                                    origin = transform.position.y;
                                    _verticalVelocity = 0.0f;
                                }

                                JumpAndGravity();
                                GroundedCheck();
                            }
                            break;

                        case LowerBodyState.GETUP:
                            {


                                if (event_getup)
                                {
                                    lowerBodyState = LowerBodyState.STAND;
                                    upperBodyState = UpperBodyState.STAND;
                                    _animator.Play(_animIDStand, 0, 0);
                                    _controller.height = 7.0f;
                                    _verticalVelocity = 0.0f;

                                }
                                else
                                {
                                    AnimatorStateInfo animeStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                                    float prevheight = _controller.height;

                                    float newheight = _controller.height = 4.0f + animeStateInfo.normalizedTime * (7.0f - 4.0f);


                                    _verticalVelocity = (newheight - prevheight) / Time.deltaTime / 2.0f;

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
                                          _mainCamera.transform.eulerAngles.y;
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
                        lowerBodyState = LowerBodyState.GROUND;

                        event_grounded = false;

                        _animator.CrossFadeInFixedTime(_animIDGround, 0.25f, 0, 0.15f);
                    }
                }
                break;
            case LowerBodyState.GROUNDSLASH_DASH:
                {


                    _speed = targetSpeed = /*event_stepbegin ? */SprintSpeed/* : 0.0f*/;

                    _animationBlend = 0.0f;

                    bool slash = false;

                    if(target_chest == null)
                    {
                        slash = true;
                    }
                    else
                    {
                        if( (target_chest.transform.position-Chest.transform.position).magnitude < SlashDistance)
                        {
                            slash = true;
                        }
                    }

                    stepremain--;

                    if (slash || stepremain <= 0)
                    {
                        lowerBodyState = LowerBodyState.GROUNDSLASH;
                        upperBodyState = UpperBodyState.GROUNDSLASH;
                        event_groundslash = false;
                        Sword.emitting = true;
                        _animator.CrossFadeInFixedTime(_animIDGroundSlash, 0.0f, 0);
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
                        lowerBodyState = LowerBodyState.STAND;
                        upperBodyState = UpperBodyState.STAND;
                        _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);

                    }
                }
                break;
            case LowerBodyState.GROUNDSLASH:
                {
                    targetSpeed = 0.0f;

                    _animationBlend = 0.0f;


                    _speed = 0.0f;

                    if(event_groundslash)
                    {
                        Sword.emitting = false;
                        lowerBodyState = LowerBodyState.STAND;
                        upperBodyState = UpperBodyState.STAND;
                        _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                    }

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
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else if (lowerBodyState == LowerBodyState.KNOCKBACK)
        {
            Vector3 targetDirection;

            float stepangle = -180.0f;

          
            targetDirection = knockbackdir;

         
            //transform.rotation = Quaternion.Euler(0.0f, Mathf.MoveTowardsAngle(transform.eulerAngles.y, steptargetrotation, 1.0f), 0.0f);

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else
        {
            Vector3 targetDirection = transform.rotation * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

     
    }


    // KNOCKBACKは複雑すぎるのでDoDamageにべた書き
    private void TransitLowerBodyState(LowerBodyState newState)
    {
        if(lowerBodyState == LowerBodyState.DOWN && newState != LowerBodyState.DOWN)
            _controller.height = 7.0f;

        switch (newState)
        {
            case LowerBodyState.AIR:
                lowerBodyState = LowerBodyState.AIR;

                if (lowerBodyState == LowerBodyState.DASH)
                    _animator.CrossFadeInFixedTime(_animIDAir, 0.25f, 0);
                else
                    _animator.CrossFadeInFixedTime(_animIDAir, 0.5f, 0);
                
                Grounded = false;
                break;
            case LowerBodyState.DOWN:
                lowerBodyState = LowerBodyState.DOWN;
                upperBodyState = UpperBodyState.DOWN;

                _animator.Play(_animIDDown, 0, 0);
                event_downed = false;
                _input.down = false;
                _controller.height = 4;
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
                        _mainCamera.transform.eulerAngles.y;




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
            _verticalVelocity = Mathf.Max(_verticalVelocity+Gravity * Time.deltaTime, -TerminalVelocity);
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

    private void OnGroundSlash()
    {
        event_groundslash = true;
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

        if(lowerBodyState == LowerBodyState.DOWN)
            Gizmos.DrawSphere(
          new Vector3(transform.position.x, transform.position.y+3.4f, transform.position.z),
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
}
