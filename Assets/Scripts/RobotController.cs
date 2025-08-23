#define ACCURATE_SEEK

using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

using UnityEngine.Animations.Rigging;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity​Engine.Rendering;
/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

using StarterAssets;
using System.Linq;



[RequireComponent(typeof(CharacterController))]
public class RobotController : Pausable
{





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

    //振りかぶり中の旋回。2段目以降のスカり防止のためなので、遅めに
    [SerializeField] float RotateSpeed_BeforeSlash = 3.0f;

    //ジャンプ格闘の上下方向は固定にしたいのでここで持つ。
    [SerializeField] float InitialVerticalSpeed_JumpSlash = 60.0f;

    //連ザ格闘の上下角度限界。
    [SerializeField] float seedSlash_PitchLimit = 30.0f;

    const float voidshift_limit_distance = 20.0f;
    const float voidshift_speed = 120.0f;


    public Vector3 cameraPosition;
    public Quaternion cameraRotation;

    public int _HP = 500;

    public int HP
    {
        get { return _HP; }
        set
        {
            if (value != _HP)
            {

                _HP = value;

                if (is_player)
                {
                    HPSlider.value = _HP;
                    HPText.text = $"{_HP}/{robotParameter.MaxHP}";
                }


            }
        }
    }




    int stepremain = 0;
    int nextdrive_free_boost = 0;

    public bool is_player;

    static public Vector3 offset = new Vector3(0.0f, 8.0f, -15.0f);

    public Vector3 slash_camera_offset;
    public bool slash_camera_offset_set = false;

    // cinemachine
    public float _cinemachineTargetYaw;
    public float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    public float _verticalVelocity;


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
    private int _animIDStand3;
    private int _animIDStand4;
    private int _animIDStand5;

    private int _animIDRollingFire_Left;
    private int _animIDRollingFire_Right;
    private int _animIDRollingFire2_Left;
    private int _animIDRollingFire2_Right;
    private int _animIDRollingFire3_Left;
    private int _animIDRollingFire3_Right;
    private int _animIDRollingFire4_Left;
    private int _animIDRollingFire4_Right;

    private int _animIDSnipeFire;
    private int _animIDSnipeFire2;
    private int _animIDSnipeFire3;
    private int _animIDSnipeFire4;

    private int _animIDSweep_Left;
    private int _animIDSweep_Right;

    int ChooseDualwieldStandMotion()
    {
        if (Sword != null && Sword.dualwielded)
            return _animIDStand4;
        if (carrying_weapon)
            return _animIDStand3;
        if (gatling)
            return _animIDStand5;
        else
            return _animIDStand2;
    }

    private int _animIDSubFire;

    private int _animIDHeavyFire;

    public int slash_count = 0;
    bool combo_reserved = false;

    enum ComboType
    {
        SLASH,
        SHOOT,
        ROLLINGSLASH,
        DASHSLASH
    }

    ComboType comboType;

    bool jumpslash_end_forward = false;

    /*   const int GroundSlash_Num = 3;
       private int[] _animIDGroundSlash;

       const int AirSlash_Num = 1;
       private int[] _animIDAirSlash = new int[AirSlash_Num];

       const int LowerSlash_Num = 1;
       private int[] _animIDLowerSlash = new int[LowerSlash_Num];*/





    Vector3 dashslash_offset;

    int hitslow_timer = 0;
    int hitstop_timer = 0;
    float intend_animator_speed = 1.0f;

    private Animator _animator;
    private CharacterController _controller;
    private Vector3 hitNormal;

    private float org_controller_height;
    private float min_controller_height;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    public InputBase _input = null; // プレイヤーの場合、WorldManagerから設定される
    bool prev_slash = false;
    bool prev_sprint = false;
    bool prev_fire = false;
    bool prev_lockswitch = false;

    bool _burst = false;

    bool burst
    {
        get { return _burst; }

        set
        {
            if (_burst != value)
            {
                _burst = value;

                if (is_player)
                {
                    ringMenu.SetActive(_burst);
                }
            }
        }
    }



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

    public GameObject[] chestWeapon_anchor;

    //public bool dualwielding
    //{
    //    get { return rightWeapon.heavy || rightWeapon.dualwielded; }
    //}

    public bool dualwielding
    {
        get
        {
            bool rightWeapon_heavy = rightWeapon == null ? false : rightWeapon.heavy;
            bool Sword_dualwielded = Sword == null ? false : Sword.dualwielded;

            return rightWeapon_heavy || robotParameter.dualwield_lightweapon || Sword_dualwielded || carrying_weapon || gatling;
        }
    }

    public bool carrying_weapon
    {
        get
        {
            if (rightWeapon != null)
                return rightWeapon.carrying;
            else
                return false;
        }
    }

    public bool gatling
    {
        get
        {
            if (rightWeapon != null)
                return rightWeapon.gatling;
            else
                return false;
        }
    }

    public bool step_boost = false;
    public enum LockonState
    {
        FREE,
        SEEKING,
        LOCKON
    }

    [SerializeField]
    List<Thruster> thrusters = new List<Thruster>();

    LockonState lockonState = LockonState.FREE;


#if ACCURATE_SEEK
    float initial_lockon_angle = 90.0f;
    int seek_time = 0;
    int seek_time_max = 20;
#endif
    public RobotController Target_Robot;

    public bool lockonmode = false;

    //private GameObject target_chest;
    private GameObject target_head;

    public List<RobotController> lockingEnemys = new List<RobotController>(); // 自分をロックオンしてる敵

    private float _headaimwait = 0.0f;

    private float _chestaimwait = 0.0f;

    private float _rarmaimwait = 0.0f;

    public Vector3 virtual_targeting_position_forBody;
    public Vector3 virtual_targeting_position_forUI;

    public bool fire_done = false;
    public int fire_followthrough = 0;
    public bool rollingfire_followthrough = false;
    public bool quickdraw_followthrough = false;

    private float _barmlayerwait = 0.0f;

    private int death_timer = 0;
    private int death_timer_max = 30;

    private bool nextdrive = false;

    RobotController finish_dealer;
    Vector3 finish_dir;

    float firing_multiplier = 1.0f;

    float lockon_multiplier
    {
        get
        {
            if (rightWeapon != null)
                return rightWeapon.lockon_multiplier;
            else
                return 1.0f;
        }
    }

    public Animator animator;

    public GameObject explode_prefab;

    public GameObject stompHitEffect_prefab;

    SkinnedMeshRenderer skinnedMeshRenderer;

    Material[] material_org;
    Material[] material_in_voidshift;

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
    bool event_fired = false;
    bool event_subfired = false;
    bool event_heavyfired = false;
    bool event_swing = false;
    bool event_acceptnextslash = false;

    bool backblast_processed = false;

    public enum UpperBodyState
    {
        STAND,
        FIRE,
        KNOCKBACK,
        DOWN,
        GETUP,
        SLASH,
        SLASH_DASH,
        SUBFIRE,
        HEAVYFIRE,
        JumpSlash,
        JumpSlash_Jump,
        JumpSlash_Ground,
        ROLLINGFIRE,
        ROLLINGHEAVYFIRE,
        SNIPEFIRE,
        SNIPEHEAVYFIRE,
        VOIDSHIFT,
        SWEEP
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
        STEPGROUND,
        DASH,
        AIRROTATE,
        KNOCKBACK,
        DOWN,
        GETUP,
        SLASH,
        SLASH_DASH,
        SUBFIRE,
        AIRSUBFIRE,
        HEAVYFIRE,
        AIRHEAVYFIRE,
        JumpSlash,
        JumpSlash_Jump,
        JumpSlash_Ground,
        ROLLINGFIRE,
        AIRROLLINGFIRE,
        ROLLINGHEAVYFIRE,
        AIRROLLINGHEAVYFIRE,
        SNIPEFIRE,
        AIRSNIPEFIRE,
        SNIPEHEAVYFIRE,
        AIRSNIPEHEAVYFIRE,
        GROUND_FIRE,
        GROUND_SUBFIRE,
        GROUND_HEAVYFIRE,
        VOIDSHIFT,
        SWEEP
    }

    public enum SubState_Slash
    {
        GroundSlash,
        AirSlash,
        LowerSlash,
        QuickSlash,
        DashSlash,
        AirSlashSeed,
        SlideSlashSeed,
        RollingSlash,
        JumpSlash, //SlashMotionInfoの定義用
        JumpSlash_Jump, //SlashMotionInfoの定義用
        JumpSlash_Ground, //SlashMotionInfoの定義用
    }

    public enum SubState_SlashType
    {
        GROUND,
        AIR
    }

    public Dictionary<SubState_Slash, SubState_SlashType> dicSubStateSlashType = new Dictionary<SubState_Slash, SubState_SlashType>
    {
        {SubState_Slash.GroundSlash,SubState_SlashType.GROUND},
        {SubState_Slash.AirSlash,SubState_SlashType.AIR},
        {SubState_Slash.LowerSlash,SubState_SlashType.GROUND},
        {SubState_Slash.QuickSlash,SubState_SlashType.GROUND},
        {SubState_Slash.DashSlash,SubState_SlashType.AIR},
        {SubState_Slash.AirSlashSeed,SubState_SlashType.AIR },
        {SubState_Slash.SlideSlashSeed,SubState_SlashType.AIR },
        {SubState_Slash.RollingSlash,SubState_SlashType.AIR }
    };

    public SubState_Slash subState_Slash;

    bool strongdown = false;

    public enum StepDirection
    {
        FORWARD, LEFT, BACKWARD, RIGHT,
        FORWARD_LEFT, FORWARD_RIGHT,
        BACKWARD_LEFT, BACKWARD_RIGHT,
    }

    public enum StepMotion
    {
        FORWARD, LEFT, BACKWARD, RIGHT
    }

    public StepDirection stepDirection; //AIから参照するので
    StepMotion stepMotion;

    public UpperBodyState upperBodyState = RobotController.UpperBodyState.STAND;
    public LowerBodyState lowerBodyState = RobotController.LowerBodyState.STAND;

    float steptargetrotation;

    public GameObject HUDCanvas;
    Slider boostSlider;
    Slider HPSlider;
    TMPro.TextMeshProUGUI HPText;
    Image alertImage_forward;
    Image alertImage_back;
    Image alertImage_left;
    Image alertImage_right;

    GameObject ringMenu;
    RectTransform ringMenu_Center_Outline_rectTfm;
    RectTransform ringMenu_Up_Outline_rectTfm;
    RectTransform ringMenu_Down_Outline_rectTfm;
    RectTransform ringMenu_Left_Outline_rectTfm;
    RectTransform ringMenu_Right_Outline_rectTfm;

    Image ringMenu_Center_LMB;
    Image ringMenu_Center_RMB;
    Image ringMenu_Up_LMB;
    Image ringMenu_Up_RMB;
    Image ringMenu_Down_LMB;
    Image ringMenu_Down_RMB;
    Image ringMenu_Left_LMB;
    Image ringMenu_Left_RMB;
    Image ringMenu_Right_LMB;
    Image ringMenu_Right_RMB;

    RectTransform ringMenu_Cursor_rectTfm;

    bool ringMenu_Center_LMB_available;
    bool ringMenu_Center_RMB_available;
    bool ringMenu_Up_LMB_available;
    bool ringMenu_Up_RMB_available;
    bool ringMenu_Down_LMB_available;
    bool ringMenu_Down_RMB_available;
    bool ringMenu_Left_LMB_available;
    bool ringMenu_Left_RMB_available;
    bool ringMenu_Right_LMB_available;
    bool ringMenu_Right_RMB_available;

    public enum RingMenuDir
    {
        Center, Left, Up, Right, Down
    }

    RingMenuDir ringMenuDir = RingMenuDir.Center;

    enum RingMenuState
    {
        Close, Shoot, Slash
    }

    RingMenuState ringMenuState = RingMenuState.Close;

    bool fire_dispatch;
    int fire_dispatch_triggerhold_max = 10;
    int fire_dispatch_triggerhold = 0;
    bool slash_dispatch;

    Vector2 ringMenuAccum = Vector2.zero;
    public GameObject damageText_prefab;
    public GameObject damageText_player_prefab;

    Vector3 knockbackdir;
    bool speed_overrideby_knockback = false;



    [SerializeField] int _boost;

    public bool dead = false;

    public int boost
    {
        get { return _boost; }
        set
        {
            _boost = value;

            if (is_player)
                boostSlider.value = _boost;
        }
    }

    int boost_regen_time = 0;


    public GameObject Rhand;

    public Weapon rightWeapon;

    public Weapon shoulderWeapon;

    public InfightWeapon Sword;

    public UIController_Overlay uIController_Overlay;

    private bool spawn_completed = false; // スポーンしたフレームにTakeDamage呼ばれると落ちるので

    public GameObject AimHelper_Head = null;
    public GameObject AimHelper_Chest = null;
    public GameObject AimHelper_RHand = null;

    Quaternion AimTargetRotation_Head;
    Quaternion AimTargetRotation_Chest;

    public enum KnockBackType
    {
        None,
        Normal,
        Down,
        Finish,
        KnockUp,
        Weak,
        Aerial
    }

    [Flags]
    public enum ItemFlag
    {
        NextDrive = 1 << 0,
        ExtremeSlide = 1 << 1,
        GroundBoost = 1 << 2,
        DropAssault = 1 << 3,
        QuickIgniter = 1 << 4,
        Hovercraft = 1 << 5,
        FlightUnit = 1 << 6,
        RollingShoot = 1 << 7,
        DashSlash = 1 << 8,
        ChainFire = 1 << 9,
        IaiSlash = 1 << 10,
        JumpSlash = 1 << 11,
        SnipeShoot = 1 << 12,
        QuickDraw = 1 << 13,
        RunningTakeOff = 1 << 14,
        CounterMeasure = 1 << 15,
        AeroMirage = 1 << 16,
        TrackingSystem = 1 << 17,
        MirageCloud = 1 << 18,
        InfightBoost = 1 << 19,
        MassIllusion = 1 << 20,
        SeedOfArts = 1 << 21,
        RollingSlash = 1 << 22,
        VoidShift = 1 << 23,
        HorizonSweep = 1 << 24
    }



    public AudioSource audioSource;
    public AudioSource audioSource_Boost;

    [SerializeField] AudioClip audioClip_Walk;
    [SerializeField] AudioClip audioClip_Ground;
    [SerializeField] AudioClip audioClip_Step;
    [SerializeField] AudioClip audioClip_Swing;

    public bool has_hitbox
    {
        get
        {
            return spawn_completed && !dead && lowerBodyState != LowerBodyState.VOIDSHIFT;
        }
    }

    public void TakeDamage(Vector3 pos, Vector3 dir, int damage, KnockBackType knockBackType, RobotController dealer)
    {
        /*if (!spawn_completed)
            return;

        if (dead)
            return;

        if (lowerBodyState == LowerBodyState.VOIDSHIFT)
            return;*/

        _input.OnTakeDamage(pos, dir, damage, knockBackType, dealer);

        HP = Math.Max(0, HP - damage);

        if (is_player || (dealer && dealer.is_player))
        {
            GameObject damageText_obj = GameObject.Instantiate(is_player ? damageText_player_prefab : damageText_prefab, HUDCanvas.transform);
            RectTransform rectTransform = damageText_obj.GetComponent<RectTransform>();
            DamageText damageText = damageText_obj.GetComponent<DamageText>();

            damageText.Position = pos;
            damageText.rectTransform = rectTransform;
            damageText.canvasTransform = HUDCanvas.GetComponent<RectTransform>();
            damageText.uiCamera = uIController_Overlay.uiCamera;
            damageText.from_player = (dealer && dealer.is_player);
            damageText.damage = damage;
            //damageText.worldManager = worldManager;
        }

        if (HP <= 0 && robotParameter.MaxHP > 0)
        {
            dead = true;
            death_timer = death_timer_max;
            finish_dealer = dealer;
            finish_dir = dir;
        }

        

        //将来的にはDoDamageを作ってそっちで処理
        if (dealer && dealer.is_player)
            WorldManager.current_instance.player_dealeddamage += damage;

        if (knockBackType != KnockBackType.None || dead)
        {
            //if (lowerBodyState != LowerBodyState.DOWN || !Grounded)
            {
                if ((!Grounded && knockBackType!=KnockBackType.Aerial) || knockBackType == KnockBackType.Down || knockBackType == KnockBackType.KnockUp)
                {


                    if (knockBackType == KnockBackType.KnockUp && lowerBodyState != LowerBodyState.DOWN)
                    {
                        knockbackdir = Vector3.zero;
                        knockbackdir.y = 1.0f;
                        _speed = 0.0f;
                        _verticalVelocity = robotParameter.KnockbackSpeed*1.5f;
                        strongdown = true;
                    }
                    else
                    {
                        knockbackdir = dir;
                        knockbackdir.y = 0.0f;


                        if (knockBackType == KnockBackType.Finish)
                        {
                            _speed = robotParameter.KnockbackSpeed;
                            _verticalVelocity = 0;
                            strongdown = true;
                        }
                        else
                        {
                            _speed = robotParameter.KnockbackSpeed / 2;
                            _verticalVelocity = 0.0f;
                            strongdown = false;
                        }
                    }

                    TransitLowerBodyState(LowerBodyState.DOWN);

                    intend_animator_speed = 1.0f;
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

                    if (dead)
                    {
                        if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
                            _animator.Play(_animIDKnockback_Strong_Right, 0, 0);
                        else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
                            _animator.Play(_animIDKnockback_Strong_Back, 0, 0);
                        else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
                            _animator.Play(_animIDKnockback_Strong_Left, 0, 0);
                        else
                            _animator.Play(_animIDKnockback_Strong_Front, 0, 0);

                        _speed = robotParameter.KnockbackSpeed * 4;

                        intend_animator_speed = 1.0f;
                    }
                    else if (knockBackType == KnockBackType.Finish)
                    {
                        if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
                            _animator.Play(_animIDKnockback_Strong_Right, 0, 0);
                        else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
                            _animator.Play(_animIDKnockback_Strong_Back, 0, 0);
                        else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
                            _animator.Play(_animIDKnockback_Strong_Left, 0, 0);
                        else
                            _animator.Play(_animIDKnockback_Strong_Front, 0, 0);

                        _speed = robotParameter.KnockbackSpeed * 4;

                        intend_animator_speed = 4.0f;

                    }
                    else if (knockBackType == KnockBackType.Weak)
                    {
                        if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
                            _animator.Play(_animIDKnockback_Strong_Right, 0, 0);
                        else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
                            _animator.Play(_animIDKnockback_Strong_Back, 0, 0);
                        else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
                            _animator.Play(_animIDKnockback_Strong_Left, 0, 0);
                        else
                            _animator.Play(_animIDKnockback_Strong_Front, 0, 0);

                        _speed = robotParameter.KnockbackSpeed;

                        intend_animator_speed = 4.0f;

                    }
                    else //if (knockBackType == KnockBackType.Normal)
                    {

                        if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
                            _animator.Play(_animIDKnockback_Right, 0, 0);
                        else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
                            _animator.Play(_animIDKnockback_Back, 0, 0);
                        else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
                            _animator.Play(_animIDKnockback_Left, 0, 0);
                        else
                            _animator.Play(_animIDKnockback_Front, 0, 0);

                        _speed = robotParameter.KnockbackSpeed * 2;

                        intend_animator_speed = 1.0f;
                    }

                    _controller.height = 7.0f;

                    _verticalVelocity = 0.0f;

                    speed_overrideby_knockback = true;
                }

                if (Sword != null)
                    Sword.emitting = false;

                // 射撃中だった場合に備えて
                if (dualwielding)
                {
                    _animator.CrossFadeInFixedTime(ChooseDualwieldStandMotion(), 0.5f, 2);
                }
            }

            if (rightWeapon != null)
                rightWeapon.OnKnockback();
            if (shoulderWeapon != null)
                shoulderWeapon.OnKnockback();

            StopMirage();

            transform.rotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
        }


    }

    private void TargetEnemy(RobotController robotController)
    {
        UntargetEnemy();

        Target_Robot = robotController;

        //target_chest = Target_Robot.Chest;
        target_head = Target_Robot.Head;

        //rigBuilder.Build();

        Target_Robot.lockingEnemys.Add(this);

        if (is_player)
            uIController_Overlay.target = Target_Robot;
    }

    public void UntargetEnemy()
    {
        if (Target_Robot != null)
        {
            Target_Robot.lockingEnemys.Remove(this);
            lockonmode = false;
        }
    }

    public void PurgeTarget(RobotController robotController)
    {
        Target_Robot = null;
        lockonmode = false;

        //target_chest = null;
        target_head = null;

        //rigBuilder.Build();

        if (is_player)
            uIController_Overlay.target = null;
    }

    private void Awake()
    {
        ArmWeapon();

        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        material_org = skinnedMeshRenderer.materials;

        material_in_voidshift = new Material[skinnedMeshRenderer.materials.Length];

        for (int j = 0; j < material_in_voidshift.Length; j++)
        {
            Material material = new Material(material_org[j]);

            material.SetFloat("_Surface", 1);

            material.SetOverrideTag("RenderType", "Transparent");

            material.renderQueue = (int)RenderQueue.Transparent;

            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);

            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

            material.SetInt("_ZWrite", 0);

            material.DisableKeyword("_ALPHATEST_ON");

            material.EnableKeyword("_ALPHABLEND_ON");

            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            var color = material.color;

            color.a = 0.1f;

            material.color = color;

            material_in_voidshift[j] = material;
        }

        WorldManager.current_instance.pausables.Add(this);


    }

    private void Start()
    {



        if (CinemachineCameraTarget != null)
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;
        }

        //is_player = GetComponent<HumanInput>() != null; 作成直後に判定して代入させるようにした

        //HUDCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
        if (is_player)
        {
            boostSlider = HUDCanvas.gameObject.transform.Find("BoostSlider").GetComponent<Slider>();

            GameObject RobotInfo = HUDCanvas.gameObject.transform.Find("RobotInfo").gameObject;

            HPSlider = RobotInfo.gameObject.transform.Find("HPSlider").GetComponent<Slider>();
            HPSlider.maxValue = robotParameter.MaxHP;
            HPSlider.minValue = 0;
            HPSlider.value = HP;

            HPText = RobotInfo.gameObject.transform.Find("HPText").GetComponent<TMPro.TextMeshProUGUI>();

            HPText.text = $"{HP}/{robotParameter.MaxHP}";

            alertImage_forward = HUDCanvas.gameObject.transform.Find("Alert_Forward").GetComponent<Image>();
            alertImage_back = HUDCanvas.gameObject.transform.Find("Alert_Back").GetComponent<Image>();
            alertImage_left = HUDCanvas.gameObject.transform.Find("Alert_Left").GetComponent<Image>();
            alertImage_right = HUDCanvas.gameObject.transform.Find("Alert_Right").GetComponent<Image>();

            uIController_Overlay.origin = this;

            if (rightWeapon != null)
                uIController_Overlay.AddWeapon(rightWeapon);

            if (shoulderWeapon != null)
                uIController_Overlay.AddWeapon(shoulderWeapon);

            ringMenu = HUDCanvas.gameObject.transform.Find("RingMenu").gameObject;
            ringMenu_Center_Outline_rectTfm = ringMenu.transform.Find("Center_Outline").gameObject.GetComponent<RectTransform>();
            ringMenu_Center_LMB = ringMenu.transform.Find("Center_LMB").gameObject.GetComponent<Image>();
            ringMenu_Center_RMB = ringMenu.transform.Find("Center_RMB").gameObject.GetComponent<Image>();
            ringMenu_Up_Outline_rectTfm = ringMenu.transform.Find("Up_Outline").gameObject.GetComponent<RectTransform>();
            ringMenu_Up_LMB = ringMenu.transform.Find("Up_LMB").gameObject.GetComponent<Image>();
            ringMenu_Up_RMB = ringMenu.transform.Find("Up_RMB").gameObject.GetComponent<Image>();
            ringMenu_Down_Outline_rectTfm = ringMenu.transform.Find("Down_Outline").gameObject.GetComponent<RectTransform>();
            ringMenu_Down_LMB = ringMenu.transform.Find("Down_LMB").gameObject.GetComponent<Image>();
            ringMenu_Down_RMB = ringMenu.transform.Find("Down_RMB").gameObject.GetComponent<Image>();
            ringMenu_Left_Outline_rectTfm = ringMenu.transform.Find("Left_Outline").gameObject.GetComponent<RectTransform>();
            ringMenu_Left_LMB = ringMenu.transform.Find("Left_LMB").gameObject.GetComponent<Image>();
            ringMenu_Left_RMB = ringMenu.transform.Find("Left_RMB").gameObject.GetComponent<Image>();
            ringMenu_Right_Outline_rectTfm = ringMenu.transform.Find("Right_Outline").gameObject.GetComponent<RectTransform>();
            ringMenu_Right_LMB = ringMenu.transform.Find("Right_LMB").gameObject.GetComponent<Image>();
            ringMenu_Right_RMB = ringMenu.transform.Find("Right_RMB").gameObject.GetComponent<Image>();

            ringMenu_Cursor_rectTfm = ringMenu.transform.Find("Cursor").GetComponent<RectTransform>();

            if (robotParameter.itemFlag.HasFlag(ItemFlag.RollingShoot))
            {
                ringMenu_Left_LMB.gameObject.SetActive(true);
                ringMenu_Right_LMB.gameObject.SetActive(true);
            }

            if (robotParameter.itemFlag.HasFlag(ItemFlag.DashSlash))
            {
                ringMenu_Up_RMB.gameObject.SetActive(true);
            }

            if (robotParameter.itemFlag.HasFlag(ItemFlag.JumpSlash))
            {
                ringMenu_Down_RMB.gameObject.SetActive(true);
            }

            if (robotParameter.itemFlag.HasFlag(ItemFlag.SnipeShoot))
            {
                ringMenu_Down_LMB.gameObject.SetActive(true);
            }

            if (robotParameter.itemFlag.HasFlag(ItemFlag.HorizonSweep))
            {
                ringMenu_Left_RMB.gameObject.SetActive(true);
                ringMenu_Right_RMB.gameObject.SetActive(true);
            }
        }

        if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
            robotParameter.Boost_Max = robotParameter.Boost_Max * 2;

        if (robotParameter.itemFlag.HasFlag(ItemFlag.FlightUnit))
        {
            robotParameter.AirMoveSpeed = robotParameter.AirMoveSpeed * 2;
            robotParameter.AscendingAccelerate = robotParameter.AscendingAccelerate * 1.5f;
        }

        if (shoulderWeapon != null)
        {
            if (robotParameter.itemFlag.HasFlag(ItemFlag.ChainFire))
                shoulderWeapon.reloadfactor = 1.5f;
            else
                shoulderWeapon.reloadfactor = 1.0f;
        }

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();

        _animator.SetFloat("JumpSpeed", robotParameter.JumpSpeed);

        org_controller_height = _controller.height;
        min_controller_height = _controller.radius * 2;

        //_input = GetComponent<StarterAssetsInputs>();

        AssignAnimationIDs();

        if (is_player)
            boostSlider.value = boostSlider.maxValue = boost = robotParameter.Boost_Max;

        AimTargetRotation_Head = Head.transform.rotation;
        AimTargetRotation_Chest = chest_hint.transform.rotation;

        virtual_targeted_position = GetCenter();

        if (Target_Robot != null)
        {
            TargetEnemy(Target_Robot);
        }

        HP = robotParameter.MaxHP;

        if (team == null)
        {
            WorldManager.current_instance.AssignToTeam(this);
        }

        if (Sword != null)
        {
            Sword.autovanish = dualwielding && !Sword.dualwielded;
            Sword.emitting = false;
        }



        if (dualwielding)
            animator.Play(ChooseDualwieldStandMotion(), 2);

        if (rightWeapon != null)
        {
            rightWeapon.owner = this;
            //rightWeapon.worldManager = worldManager;
        }

        if (Sword != null)
        {
            Sword.owner = this;
            // Sword.worldManager = worldManager;
        }
        if (shoulderWeapon != null)
        {
            shoulderWeapon.owner = this;
            // shoulderWeapon.worldManager = worldManager;
        }

        spawn_completed = true;

        // _input.worldManager = worldManager;

        if (_input == null)
            _input = GetComponent<InputBase>();

        _rarmaimwait = 0.0f;
        _chestaimwait = 0.0f;

        if (dualwielding)
            _barmlayerwait = 1.0f;
        else
            _barmlayerwait = 0.0f;

        UpperBodyMove();

        prev_slash = _input.slash;
        prev_sprint = _input.sprint;
        prev_fire = _input.fire;
        prev_lockswitch = _input.lockswitch;

    }

    private bool ConsumeBoost(int amount)
    {
        if(boost > 0)
        {
            boost -= amount;
            boost_regen_time = robotParameter.BoostRegenDelay;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void RegenBoost()
    {
        if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
            boost = Math.Min(boost + 24, robotParameter.Boost_Max);
        else
            boost = Math.Min(boost + 12, robotParameter.Boost_Max);
    }

    public static float DistanceToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    public void OnRobotAdded(RobotController robotController)
    {
        if (is_player)
            uIController_Overlay.AddRobot(robotController);
    }

    public void OnRobotRemoved(RobotController robotController)
    {
        if (is_player)
            uIController_Overlay.RemoveRobot(robotController);
    }

    //bool pausing = false;
    bool prev_down = false;
    bool paused = false;
    protected override void OnFixedUpdateForce()
    {
        if (is_player)
        {
            if (_input.down && !prev_down)
            {
                if (!WorldManager.current_instance.pausing)
                    WorldManager.current_instance.Pause();
                else
                    WorldManager.current_instance.Unpause();
            }

            prev_down = _input.down;
        }

        if (paused || hitstop_timer > 0)
        {
            animator.speed = 0.0f;
        }
        else if (hitslow_timer > 0)
        {
            animator.speed = 0.2f;
        }
        else
            animator.speed = intend_animator_speed;
    }

    static Color ringMenu_enableColor_LMB = new Color(0.0f, 0.5f, 1.0f, 1.0f);
    static Color ringMenu_disableColor_LMB = new Color(0.0f, 0.5f, 1.0f, 0.25f);

    static Color ringMenu_enableColor_RMB = new Color(1.0f, 0.5f, 0.75f, 1.0f);
    static Color ringMenu_disableColor_RMB = new Color(1.0f, 0.5f, 0.75f, 0.25f);

    static Color getRingMenuColor(bool enable, bool LMB)
    {
        if (LMB)
        {
            if (enable)
                return ringMenu_enableColor_LMB;
            else
                return ringMenu_disableColor_LMB;
        }
        else
        {
            if (enable)
                return ringMenu_enableColor_RMB;
            else
                return ringMenu_disableColor_RMB;
        }
    }

    protected override void OnFixedUpdate()
    {
        if (!dead)
        {
            if (WorldManager.current_instance.finished)
            {
                _input.jump = _input.fire = _input.slash = _input.sprint = false;
                _input.move = Vector2.zero;
            }

            if (hitstop_timer <= 0)
            {
                ringMenu_Center_LMB_available = false;
                ringMenu_Center_RMB_available = false;
                ringMenu_Up_LMB_available = false;
                ringMenu_Up_RMB_available = false;
                ringMenu_Down_LMB_available = false;
                ringMenu_Down_RMB_available = false;
                ringMenu_Left_LMB_available = false;
                ringMenu_Left_RMB_available = false;
                ringMenu_Right_LMB_available = false;
                ringMenu_Right_RMB_available = false;
                fire_dispatch = slash_dispatch = false;
            }

            

            _hasAnimator = TryGetComponent(out _animator);

            float mindist = float.MaxValue;
            float minangle = float.MaxValue;
            RobotController nearest_robot = null;

            foreach (var team in WorldManager.current_instance.teams)
            {
                if (team == this.team)
                    continue;

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
                    if (Target_Robot)
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

                }
                if (blocked)
                {
                    uIController_Overlay.distance = mindist;

                }
                else
                    uIController_Overlay.distance = 1000.0f;

            }

            if (nearest_robot == null)
            {
                UntargetEnemy();

                Target_Robot = null;
                lockonmode = false;
                //target_chest = null;
                target_head = null;

                if (is_player)
                    uIController_Overlay.target = null;
            }
            else
            {
                if (Target_Robot != nearest_robot)
                    TargetEnemy(nearest_robot);
            }


            if (rightWeapon != null)
                rightWeapon.Target_Robot = Target_Robot;
            if (shoulderWeapon != null)
                shoulderWeapon.Target_Robot = Target_Robot;


            if (is_player)
            {
                uIController_Overlay.lockonState = lockonState;
                uIController_Overlay.lockonmode = lockonmode;

                switch (ringMenuState)
                {
                    case RingMenuState.Close:
                        {
                            if (_input.fire)
                            {
                                ringMenu.SetActive(true);
                                ringMenuState = RingMenuState.Shoot;
                                ringMenuDir = RingMenuDir.Center;
                                ringMenuAccum = Vector2.zero;
                            }
                            else if (_input.slash)
                            {
                                ringMenu.SetActive(true);
                                ringMenuState = RingMenuState.Slash;
                                ringMenuDir = RingMenuDir.Center;
                                ringMenuAccum = Vector2.zero;
                            }

                        }
                        break;
                    case RingMenuState.Shoot:
                        {
                            if (!_input.fire)
                            {
                                fire_dispatch = true;
                                ringMenu.SetActive(false);
                                ringMenuState = RingMenuState.Close;
                            }
                        }
                        break;
                    case RingMenuState.Slash:
                        {
                            if (!_input.slash)
                            {
                                slash_dispatch = true;
                                ringMenu.SetActive(false);
                                ringMenuState = RingMenuState.Close;
                            }
                        }
                        break;
                }

                /*if(burst)
                {
                    if (_input.move.x > 0.0f)
                    {
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Right_Outline_rectTfm.anchoredPosition;
                    }
                    else if (_input.move.x < 0.0f)
                    {
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Left_Outline_rectTfm.anchoredPosition;
                    }
                    else if (_input.move.y < 0.0f)
                    {
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Down_Outline_rectTfm.anchoredPosition;
                    }
                    else if (_input.move.y > 0.0f)
                    {
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Up_Outline_rectTfm.anchoredPosition;
                    }
                    else
                    {
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Center_Outline_rectTfm.anchoredPosition;
                    }

                    ringMenu_Center_LMB.color = getRingMenuColor(ringMenu_Center_LMB_available, true);
                    ringMenu_Center_RMB.color = getRingMenuColor(ringMenu_Center_RMB_available, false);
                    ringMenu_Up_LMB.color = getRingMenuColor(ringMenu_Up_LMB_available, true);
                    ringMenu_Up_RMB.color = getRingMenuColor(ringMenu_Up_RMB_available, false);
                    ringMenu_Down_LMB.color = getRingMenuColor(ringMenu_Down_LMB_available, true);
                    ringMenu_Down_RMB.color = getRingMenuColor(ringMenu_Down_RMB_available, false);
                    ringMenu_Left_LMB.color = getRingMenuColor(ringMenu_Left_LMB_available, true);
                    ringMenu_Left_RMB.color = getRingMenuColor(ringMenu_Left_RMB_available, false);
                    ringMenu_Right_LMB.color = getRingMenuColor(ringMenu_Right_LMB_available, true);
                    ringMenu_Right_RMB.color = getRingMenuColor(ringMenu_Right_RMB_available, false);
                }*/

                {

                    // 視点変更時でも機体基準でのアラートにしたいのでここで再計算
                    Quaternion cameraRotation_tmp = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                        _cinemachineTargetYaw, 0.0f);

                    Vector3 cameraPosition_tmp = transform.position + cameraRotation_tmp * offset * transform.lossyScale.x;

                    Transform2 transform_tmp = new Transform2 { position = cameraPosition_tmp, rotation = cameraRotation_tmp, localScale = Vector3.one };


                    alertImage_right.enabled = false;
                    alertImage_left.enabled = false;
                    alertImage_forward.enabled = false;
                    alertImage_back.enabled = false;

                    foreach (var lockingEnemy in lockingEnemys)
                    {
                        if (lockingEnemy)
                        {
                            Vector3 rel = transform_tmp.InverseTransformPoint(lockingEnemy.GetCenter());

                            if (Mathf.Abs(rel.x) > Mathf.Abs(rel.z))
                            {
                                if (rel.x > 0.0f)
                                {
                                    alertImage_right.enabled = true;
                                }
                                else
                                    alertImage_left.enabled = true;
                            }
                            else
                            {
                                if (rel.z > 0.0f)
                                {
                                    alertImage_forward.enabled = true;
                                }
                                else
                                    alertImage_back.enabled = true;
                            }
                        }
                    }
                }
            }
            else
            {
                fire_dispatch = _input.fire;
                slash_dispatch = _input.slash;
                ringMenuDir = _input.ringMenuDir;
            }

            if (_input.fire_forcedispatch)
            {
                fire_dispatch = true;
                ringMenuDir = _input.ringMenuDir;
            }

            if (_input.slash_forcedispatch)
            {
                slash_dispatch = true;
                ringMenuDir = _input.ringMenuDir;
            }

            LowerBodyMove(); // HEAVYFIREの反動処理変えたから順番入れ替えても大丈夫かも
            UpperBodyMove();

            if (ringMenuState != RingMenuState.Close)
            {
                bool stay = false;

                switch (ringMenuDir)
                {
                    case RingMenuDir.Center:
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Center_Outline_rectTfm.anchoredPosition;
                        break;
                    case RingMenuDir.Left:
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Left_Outline_rectTfm.anchoredPosition;
                        break;
                    case RingMenuDir.Right:
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Right_Outline_rectTfm.anchoredPosition;
                        break;
                    case RingMenuDir.Up:
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Up_Outline_rectTfm.anchoredPosition;
                        break;
                    case RingMenuDir.Down:
                        ringMenu_Cursor_rectTfm.anchoredPosition = ringMenu_Down_Outline_rectTfm.anchoredPosition;
                        break;
                }

                if (ringMenuAccum.x > 1.0f && ((ringMenuState == RingMenuState.Shoot && ringMenu_Right_LMB_available)
                        || (ringMenuState == RingMenuState.Slash && ringMenu_Right_RMB_available)))
                {
                        ringMenuDir = RingMenuDir.Right;
                }
                else if (ringMenuAccum.x < -1.0f && ((ringMenuState == RingMenuState.Shoot && ringMenu_Left_LMB_available)
                        || (ringMenuState == RingMenuState.Slash && ringMenu_Left_RMB_available)))
                {
                        ringMenuDir = RingMenuDir.Left;
                }
                else if (ringMenuAccum.y > 1.0f && ((ringMenuState == RingMenuState.Shoot && ringMenu_Down_LMB_available)
                        || (ringMenuState == RingMenuState.Slash && ringMenu_Down_RMB_available)))
                {
                        ringMenuDir = RingMenuDir.Down;
                }
                else if (ringMenuAccum.y < -1.0f && ((ringMenuState == RingMenuState.Shoot && ringMenu_Up_LMB_available)
                        || (ringMenuState == RingMenuState.Slash && ringMenu_Up_RMB_available)))
                {
                        ringMenuDir = RingMenuDir.Up;
                }
                else
                    stay = true;

                if (!stay)
                {
                    ringMenuAccum = Vector2.zero;
                }
                else
                    ringMenuAccum += _input.look;

                ringMenu_Center_LMB.color = getRingMenuColor(ringMenu_Center_LMB_available, true);
                ringMenu_Center_RMB.color = getRingMenuColor(ringMenu_Center_RMB_available, false);
                ringMenu_Up_LMB.color = getRingMenuColor(ringMenu_Up_LMB_available, true);
                ringMenu_Up_RMB.color = getRingMenuColor(ringMenu_Up_RMB_available, false);
                ringMenu_Down_LMB.color = getRingMenuColor(ringMenu_Down_LMB_available, true);
                ringMenu_Down_RMB.color = getRingMenuColor(ringMenu_Down_RMB_available, false);
                ringMenu_Left_LMB.color = getRingMenuColor(ringMenu_Left_LMB_available, true);
                ringMenu_Left_RMB.color = getRingMenuColor(ringMenu_Left_RMB_available, false);
                ringMenu_Right_LMB.color = getRingMenuColor(ringMenu_Right_LMB_available, true);
                ringMenu_Right_RMB.color = getRingMenuColor(ringMenu_Right_RMB_available, false);
            }
        }
        else
        {
            if (death_timer == death_timer_max)
            {

                UntargetEnemy();

                for (int i = 0; i < lockingEnemys.Count; i++)
                {
                    lockingEnemys[i].PurgeTarget(this);
                }

                if (is_player)
                    uIController_Overlay.origin = null;

                if (rightWeapon != null)
                {
                    uIController_Overlay.RemoveWeapon(rightWeapon);
                    rightWeapon.Destroy_Called_By_Unit();

                }
                if (shoulderWeapon != null)
                {
                    uIController_Overlay.RemoveWeapon(shoulderWeapon);
                    shoulderWeapon.Destroy_Called_By_Unit();
                }

                WorldManager.current_instance.HandleRemoveUnit(this, finish_dealer, finish_dir);
            }
            else if (death_timer <= 0)
            {
                GameObject.Instantiate(explode_prefab, transform.position, Quaternion.identity);
                GameObject.Destroy(gameObject);
            }
            else
            {
                DeadBodyMove();
            }

            death_timer--;
        }

        prev_slash = _input.slash;
        prev_sprint = _input.sprint;

        if (!_input.sprint_once)
            sprint_once_consumed = false;

        prev_fire = _input.fire;
    }

    private void DeadBodyMove()
    {
        if (rightWeapon != null)
            rightWeapon.trigger = false;

        if (shoulderWeapon != null)
            shoulderWeapon.trigger = false;

        if (hitslow_timer <= 0 && hitstop_timer <= 0)
        {


            bool chest_pitch_aim = false;


            switch (upperBodyState)
            {
                case UpperBodyState.KNOCKBACK:
                case UpperBodyState.DOWN:
                case UpperBodyState.GETUP:
                    _chestaimwait = 0.0f;
                    _headaimwait = 0.0f;
                    _rarmaimwait = 0.0f;
                    _barmlayerwait = 0.0f;
                    lockonState = LockonState.FREE;
                    break;
            }


            headmultiAimConstraint.weight = _headaimwait;

            //headmultiAimConstraint.weight = 1.0f;

            Quaternion target_rot_head;
            Quaternion target_rot_chest;
            Quaternion target_rot_rhand;

            Quaternion q_aim_global = Quaternion.LookRotation(-aiming_hint.transform.forward, new Vector3(0.0f, 1.0f, 0.0f));

            overrideTransform.data.position = shoulder_hint.transform.position;
            overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;

            target_rot_head = Head.transform.rotation;
            target_rot_chest = chest_hint.transform.rotation;
            target_rot_rhand = chest_hint.transform.rotation;


            AimTargetRotation_Head = target_rot_head;
            AimHelper_Head.transform.position = Head.transform.position + AimTargetRotation_Head * Vector3.forward * 3;
            AimTargetRotation_Chest = target_rot_chest;

            Vector3 chestAim_Dir = AimTargetRotation_Chest * Vector3.forward * 3;

            if (!chest_pitch_aim)
                chestAim_Dir.y = 0.0f;

            AimHelper_Chest.transform.position = Chest.transform.position + chestAim_Dir;

            chestmultiAimConstraint.weight = _chestaimwait;

            AimHelper_RHand.transform.position = RHand.transform.position + target_rot_rhand * Vector3.forward * 3;
            rhandmultiAimConstraint.weight = aiming_factor;

            overrideTransform.weight = _rarmaimwait;

            animator.SetLayerWeight(2, _barmlayerwait);

            if (!dualwielding)
                animator.SetLayerWeight(1, _rarmaimwait);

            float targetSpeed = 0.0f;

            switch (lowerBodyState)
            {
                case LowerBodyState.DOWN:
                    {
                        _animationBlend = 0.0f;

                        switch (lowerBodyState)
                        {
                            case LowerBodyState.DOWN:
                                {
                                    JumpAndGravity();
                                    GroundedCheck();
                                }
                                break;
                        }

                        break;
                    }
                case LowerBodyState.KNOCKBACK:
                    {

                        // a reference to the players current horizontal velocity
                        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
                        float speedOffset = 0.1f;

                        targetSpeed = 0.0f;

                        if (speed_overrideby_knockback) // リセットは末尾で。（こことかでやる作りだと漏れたときのバグが怖い）
                        {

                        }
                        // accelerate or decelerate to target speed
                        else if (
                            //currentHorizontalSpeed < targetSpeed - speedOffset ||
                            currentHorizontalSpeed > targetSpeed + speedOffset
                            )
                        {
                            // creates curved result rather than a linear one giving a more organic speed change
                            // note T in Lerp is clamped, so we don't need to clamp our speed
                            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                                Time.deltaTime * robotParameter.SpeedChangeRate);

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
            }
        }

        bool hitslow_now = false;
        bool hitstop_now = false;

        if (hitslow_timer > 0)
        {
            hitslow_timer--;

            hitslow_now = true;
        }

        if (hitstop_timer > 0)
        {
            hitstop_timer--;

            hitstop_now = true;
        }

        if (!hitslow_now && !hitstop_now)
        {
            if (lowerBodyState == LowerBodyState.KNOCKBACK || lowerBodyState == LowerBodyState.DOWN)
            {

                Vector3 targetDirection;


                targetDirection = knockbackdir;


                //transform.rotation = Quaternion.Euler(0.0f, Mathf.MoveTowardsAngle(transform.eulerAngles.y, steptargetrotation, 1.0f), 0.0f);

                // move the player
                MoveAccordingTerrain(targetDirection.normalized * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
        }

        if (!hitslow_now && !hitstop_now)
            speed_overrideby_knockback = false;
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
        _animIDStand3 = Animator.StringToHash("Stand3");
        _animIDStand4 = Animator.StringToHash("Stand4");
        _animIDStand5 = Animator.StringToHash("Stand5");

        _animIDSubFire = Animator.StringToHash("SubFire");

        _animIDHeavyFire = Animator.StringToHash("HeavyFire");



        _animIDRollingFire_Left = Animator.StringToHash("RollingFire_Left");
        _animIDRollingFire_Right = Animator.StringToHash("RollingFire_Right");
        _animIDRollingFire2_Left = Animator.StringToHash("RollingFire2_Left");
        _animIDRollingFire2_Right = Animator.StringToHash("RollingFire2_Right");
        _animIDRollingFire3_Left = Animator.StringToHash("RollingFire3_Left");
        _animIDRollingFire3_Right = Animator.StringToHash("RollingFire3_Right");
        _animIDRollingFire4_Left = Animator.StringToHash("RollingFire4_Left");
        _animIDRollingFire4_Right = Animator.StringToHash("RollingFire4_Right");

        _animIDSnipeFire = Animator.StringToHash("SnipeFire");
        _animIDSnipeFire2 = Animator.StringToHash("SnipeFire2");
        _animIDSnipeFire3 = Animator.StringToHash("SnipeFire3");
        _animIDSnipeFire4 = Animator.StringToHash("SnipeFire4");

        _animIDSweep_Left = Animator.StringToHash("Sweep_Left");
        _animIDSweep_Right = Animator.StringToHash("Sweep_Right");
    }

    Collider[] stompHit = new Collider[16];

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
            case LowerBodyState.DASH:
            case LowerBodyState.AIRROTATE:
                if (Grounded)
                {
                    if (upperBodyState == UpperBodyState.FIRE)
                    {
                        if (fire_done)
                        {
                            fire_followthrough = 0;
                        }

                        if (robotParameter.itemFlag.HasFlag(ItemFlag.DropAssault))
                            TransitLowerBodyState(LowerBodyState.STAND);
                        else
                            TransitLowerBodyState(LowerBodyState.GROUND);
                    }
                    else
                        TransitLowerBodyState(LowerBodyState.GROUND);
                }
                break;
            case LowerBodyState.AIRFIRE:

                if (Grounded)
                {
                    if (robotParameter.itemFlag.HasFlag(ItemFlag.DropAssault))
                    {
                        TransitLowerBodyState(LowerBodyState.STAND);
                        if (fire_done)
                            fire_followthrough = 0;
                    }
                    else
                        TransitLowerBodyState(LowerBodyState.GROUND_FIRE);
                }
                break;
            case LowerBodyState.AIRSUBFIRE:
                if (Grounded)
                {
                    if (robotParameter.itemFlag.HasFlag(ItemFlag.DropAssault))
                    {
                        TransitLowerBodyState(LowerBodyState.STAND);
                        if (fire_done)
                            fire_followthrough = 0;
                    }
                    else
                        TransitLowerBodyState(LowerBodyState.GROUND_SUBFIRE);
                }
                break;
            case LowerBodyState.AIRHEAVYFIRE:
                if (Grounded)
                {
                    if (robotParameter.itemFlag.HasFlag(ItemFlag.DropAssault))
                    {
                        TransitLowerBodyState(LowerBodyState.STAND);
                        if (fire_done)
                            fire_followthrough = 0;
                    }
                    else
                        TransitLowerBodyState(LowerBodyState.GROUND_HEAVYFIRE);
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
            case LowerBodyState.JumpSlash:
                if (Sword.slashing)
                {
                    Vector3 point0 = transform.position + _controller.center;
                    Vector3 point1 = transform.position + _controller.center;

                    point0.y -= _controller.height / 2 + _controller.radius;

                    int numhit = Physics.OverlapCapsuleNonAlloc(point0, point1, _controller.radius, stompHit, 1 << 6);

                    for (int idx_hit = 0; idx_hit < numhit; idx_hit++)
                    {


                        if (Sword.hitHistory.Contains(stompHit[idx_hit].gameObject))
                            continue;

                        Sword.hitHistory[Sword.hitHistoryCount++] = stompHit[idx_hit].gameObject;



                        RobotController robotController = stompHit[idx_hit].gameObject.GetComponentInParent<RobotController>();


                        if (robotController != null)
                        {
                            if (robotController.team == team)
                                continue;

                            if (!robotController.has_hitbox)
                                continue;

                            if (Sword.hitHistoryRC.Contains(robotController))
                                continue;



                            Sword.hitHistoryRC[Sword.hitHistoryRCCount++] = robotController;

                            Vector3 diff = robotController.GetCenter() - GetCenter();

                            diff.y = 0.0f;

                            robotController.TakeDamage(point0 + Vector3.down * _controller.radius, diff.normalized, Sword.damage, KnockBackType.Finish, this);

                            GameObject.Instantiate(stompHitEffect_prefab, point0 + Vector3.down * _controller.radius, Quaternion.identity);
                        }


                    }
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

        if (!WorldManager.current_instance.finished && !WorldManager.current_instance.attention)
        {
            if (CinemachineCameraTarget != null)
            {
                CinemachineCameraTarget.transform.position = cameraPosition;
                CinemachineCameraTarget.transform.rotation = cameraRotation;
            }
        }

        if (Target_Robot != null)
        {
            if (aiming_factor < aiming_begin_aiming_factor_current || robotParameter.itemFlag.HasFlag(ItemFlag.TrackingSystem))
            {
                virtual_targeting_position_forUI = Target_Robot.GetTargetedPosition();

                //if (is_player)
                //    uIController_Overlay.aim_fixed = true;
            }
            else
            {
                Vector3 a = virtual_targeting_position_forUI - GetCenter();
                Vector3 b = Target_Robot.GetTargetedPosition() - GetCenter();

                if (Vector3.Angle(a, b) < aiming_angle_speed_current)
                {
                    //if (is_player)
                    //    uIController_Overlay.aim_fixed = true;

                    virtual_targeting_position_forUI = GetCenter() + b;
                }
                else
                {
                    //if (is_player)
                    //    uIController_Overlay.aim_fixed = false;

                    virtual_targeting_position_forUI = GetCenter() + Vector3.RotateTowards(a, b, aiming_angle_speed_current * Mathf.Deg2Rad, float.MaxValue);
                }
            }

        }
        else
        {
            if (is_player)
            {
                //uIController_Overlay.aiming = false;
                //uIController_Overlay.aim_fixed = false;
            }
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
        if (!(Target_Robot != null && lockonState != LockonState.FREE) && !WorldManager.current_instance.finished)
        {

            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition && ringMenuState == RingMenuState.Close)
            {
                //Don't multiply mouse input by Time.deltaTime;
                //float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                float deltaTimeMultiplier = 1.5f;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }
        }
    }

    static public Quaternion GetTargetQuaternionForView_FromTransform(RobotController target, Transform2 _transform)
    {
        return GetTargetQuaternionForView_FromTransform(target.GetCenter(), _transform);
    }

    static public Quaternion GetTargetQuaternionForView_FromTransform(Vector3 target_center, Transform2 _transform)
    {
        Quaternion qtarget = Quaternion.LookRotation(target_center - GetCenterFromTransform(_transform), Vector3.up);

        Vector3 vtarget = qtarget.eulerAngles;

        vtarget.x += 10.0f;

        return Quaternion.Euler(vtarget);
    }

    // AIから呼びされるので
    public Quaternion GetTargetQuaternionForView(RobotController target)
    {
        return GetTargetQuaternionForView_FromTransform(target, new Transform2(transform));
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
        int cam_norm0_infight1_seed2 = 0;

        if (Target_Robot != null &&
                        (
                        lowerBodyState == LowerBodyState.SLASH
       
                        || lowerBodyState == LowerBodyState.SLASH_DASH
                     
                        
                   //|| lowerBodyState == LowerBodyState.JumpSlash_Jump
                   //|| lowerBodyState == LowerBodyState.JumpSlash
                   ) && lockonState != LockonState.FREE
                   )
        {
            float dist_enemy = float.MaxValue;

            dist_enemy = (GetCenter() - Target_Robot.GetCenter()).magnitude;

            if (
                (subState_Slash != SubState_Slash.AirSlashSeed && subState_Slash != SubState_Slash.SlideSlashSeed)
              )
            {
                cam_norm0_infight1_seed2 = dist_enemy < 20.0f ? 1 : 0;
            }
            else if (lowerBodyState == LowerBodyState.SLASH && slash_count >= Sword.slashMotionInfo[subState_Slash].num - 1)
            {
                cam_norm0_infight1_seed2 = dist_enemy < 20.0f ? 2 : 0;
            }


        }

        if (cam_norm0_infight1_seed2!=0)
        {
            if (is_player)
                uIController_Overlay.infight = true;

            Vector3 off_hori = GetCenter() - Target_Robot.GetCenter();
            off_hori.y = 0.0f;

            if (cam_norm0_infight1_seed2 == 1)
            {
                if (Sword.dashslash_cutthrough && (lowerBodyState == LowerBodyState.SLASH || lowerBodyState == LowerBodyState.SLASH_DASH) && subState_Slash == SubState_Slash.DashSlash)
                {
                    if (!slash_camera_offset_set)
                    {
                        slash_camera_offset = Quaternion.AngleAxis(180.0f, Vector3.up) * (off_hori).normalized * 20.0f;
                    }
                }
                else
                    slash_camera_offset = Quaternion.AngleAxis(90.0f, Vector3.up) * (off_hori).normalized * 20.0f;
            }
            else
            {
                if(subState_Slash == SubState_Slash.AirSlashSeed)
                    slash_camera_offset = Quaternion.AngleAxis(90.0f, Vector3.up) *  off_hori.normalized * 15.0f+ Vector3.up*5.0f;
                else
                    slash_camera_offset = Quaternion.AngleAxis(-90.0f, Vector3.up) * off_hori.normalized * 15.0f + Vector3.up * 5.0f;
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
            if (is_player)
                uIController_Overlay.infight = false;

            slash_camera_offset_set = false;

            if (Target_Robot != null)
            {
                if (lockonState == LockonState.FREE)
                {
                    if (lockonmode)
                    {
                        StartSeeking();
                    }
                }

                if (lockonState == LockonState.SEEKING)
                {
#if !ACCURATE_SEEK
                    Quaternion cameraRotation_current = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                            _cinemachineTargetYaw, 0.0f);


                    float angle = Quaternion.Angle(cameraRotation_current, GetTargetQuaternionForView(Target_Robot));

                    if (angle < 1.0f * lockon_multiplier)
                    {
                        lockonState = LockonState.LOCKON;
                    }
                    else
                    {
                        Quaternion q = Quaternion.RotateTowards(cameraRotation_current, GetTargetQuaternionForView(Target_Robot), 1.0f * lockon_multiplier);

                        _cinemachineTargetYaw = q.eulerAngles.y;
                        _cinemachineTargetPitch = q.eulerAngles.x;

                        if (_cinemachineTargetPitch > 180.0f)
                            _cinemachineTargetPitch -= 360.0f;
                    }
#else
                    if (seek_time <= 0)
                    {
                        lockonState = LockonState.LOCKON;

                        Quaternion q = GetTargetQuaternionForView(Target_Robot);

                        _cinemachineTargetYaw = q.eulerAngles.y;
                        _cinemachineTargetPitch = q.eulerAngles.x;
                    }
                    else
                    {
                        float angle = Quaternion.Angle(GetTargetQuaternionForView(Target_Robot), cameraRotation);

                        Quaternion q = Quaternion.RotateTowards(cameraRotation, GetTargetQuaternionForView(Target_Robot), angle / seek_time);

                        _cinemachineTargetYaw = q.eulerAngles.y;
                        _cinemachineTargetPitch = q.eulerAngles.x;

                        seek_time--;
                    }

                    if (_cinemachineTargetPitch > 180.0f)
                        _cinemachineTargetPitch -= 360.0f;
#endif                        
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
                lockonmode = false;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, TopClamp, BottomClamp);


            // Cinemachine will follow this target
            cameraRotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);

            cameraPosition = transform.position + cameraRotation * offset * transform.lossyScale.x;


        }

        if (Target_Robot != null)
        {
            if (_input.lockswitch && !prev_lockswitch)
            {
                lockonmode = !lockonmode;
            }
        }

        prev_lockswitch = _input.lockswitch;
    }

    float aiming_begin_aiming_factor_current = 0.0f;
    float aiming_angle_speed_current = 0.0f;
    float aiming_factor = 0.0f;
    private void UpperBodyMove()
    {
        if (hitstop_timer > 0)
        {
            // トリガーホールド実装時に実装変えた方がいい…ように見えて、
            // 実際はフリックとの兼ね合いでホールドが難しいからずっとこのままでよさそう

            if (rightWeapon != null)
                rightWeapon.trigger = false;

            if (shoulderWeapon != null)
                shoulderWeapon.trigger = false;

            return;
        }

        bool head_no_aim_smooth = false;
        bool chest_no_aim_smooth = false;

        float rhandaimwait_thisframe = 0.0f;

        bool chest_pitch_aim = false;

        bool rightWeapon_trigger_thisframe = false;
        bool shoulderWeapon_trigger_thisframe = false;

        //float aiming_begin_aiming_factor_current = 0.0f;
        //float aiming_angle_speed_current = 0.0f;
        bool current_aiming = false;
        bool canhold_current = false;

        bool miragecloud_invalid = false;
        bool massillusion_invalid = false;

        switch (upperBodyState)
        {
            case UpperBodyState.FIRE:
            case UpperBodyState.ROLLINGFIRE:
            case UpperBodyState.SNIPEFIRE:
                {
                    miragecloud_invalid = true;

                    if (upperBodyState == UpperBodyState.FIRE)
                    {
                        _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.10f * firing_multiplier);
                        _rarmaimwait = Mathf.Min(1.0f, _rarmaimwait + 0.04f * firing_multiplier);
                        _chestaimwait = Mathf.Min(1.0f, _chestaimwait + 0.04f * firing_multiplier);

                        if (dualwielding)
                        {
                            aiming_factor = animator.GetCurrentAnimatorStateInfo(2).normalizedTime;
                            rhandaimwait_thisframe = Mathf.Clamp((aiming_factor - 0.70f) * 4, 0.0f, 1.0f);
                            _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f * firing_multiplier);
                        }
                        else
                        {
                            aiming_factor = animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
                            rhandaimwait_thisframe = Mathf.Clamp((aiming_factor - 0.70f) * 4, 0.0f, 1.0f);
                        }
                    }
                    else
                    {
                        _headaimwait = 0.0f;
                        _rarmaimwait = 0.0f;
                        _chestaimwait = 0.0f;
                        _barmlayerwait = 0.0f;
                        aiming_factor = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                        rhandaimwait_thisframe = Mathf.Clamp((aiming_factor - 0.75f) * 4.0f, 0.0f, 1.0f);
                    }



                    if (!fire_done)
                    {
                        bool shoot = false;

                        if (upperBodyState == UpperBodyState.FIRE)
                        {
                            if (dualwielding)
                            {
                                shoot = event_fired;
                            }
                            else
                            {
                                shoot = event_fired;
                            }
                        }
                        else
                        {
                            shoot = event_fired;
                        }

                        if (shoot)
                        {
                            fire_done = true;
#if !ACCURATE_SEEK
                            if (lockonState == LockonState.LOCKON)
#else
                            if (lockonState != LockonState.FREE)
#endif
                            {
                                rightWeapon.Target_Robot = Target_Robot;
                            }
                            else
                            {
                                rightWeapon.Target_Robot = null;

                                lockonState = LockonState.FREE;
                            }

                            rightWeapon.chargeshot = upperBodyState == UpperBodyState.SNIPEFIRE;
                            rightWeapon_trigger_thisframe = true;

                            if (upperBodyState != UpperBodyState.SNIPEFIRE)
                            {
                                fire_followthrough = rightWeapon.fire_followthrough;
                            }
                            else
                            {
                                fire_followthrough = 30;
                            }

                            if (upperBodyState == UpperBodyState.ROLLINGFIRE)
                            {
                                upperBodyState = UpperBodyState.FIRE;
                                if (lowerBodyState == LowerBodyState.ROLLINGFIRE)
                                    TransitLowerBodyState(LowerBodyState.STAND);
                                else
                                    TransitLowerBodyState(LowerBodyState.AIR);

                                rollingfire_followthrough = true;
                            }
                        }
                    }
                    else
                    {
                        fire_followthrough--;

                        if ( (fire_dispatch && ringMenuDir == RingMenuDir.Center) || rightWeapon.forceHold)
                            fire_dispatch_triggerhold = fire_dispatch_triggerhold_max;

                        if (rightWeapon.canHold && fire_dispatch_triggerhold > 0)
                        {
                            rightWeapon_trigger_thisframe = true;
                            fire_followthrough = rightWeapon.fire_followthrough;
                        }
                        else
                        {
                            //rightWeapon.trigger = false;
                        }

                        if (fire_dispatch_triggerhold > 0)
                            fire_dispatch_triggerhold--;

                        if (fire_followthrough <= 0)
                        {
                            upperBodyState = UpperBodyState.STAND;

                            if (lowerBodyState == LowerBodyState.AIRFIRE || lowerBodyState == LowerBodyState.AIRROLLINGFIRE)
                            {
                                TransitLowerBodyState(LowerBodyState.AIR);
                            }
                            else if (lowerBodyState == LowerBodyState.GROUND_FIRE)
                                TransitLowerBodyState(LowerBodyState.GROUND);
                            else if (lowerBodyState == LowerBodyState.FIRE || lowerBodyState == LowerBodyState.ROLLINGFIRE)
                            {
                                TransitLowerBodyState(LowerBodyState.STAND);
                            }
                            else if (lowerBodyState == LowerBodyState.SNIPEFIRE || lowerBodyState == LowerBodyState.AIRSNIPEFIRE)
                            {
                                if (Grounded)
                                    TransitLowerBodyState(LowerBodyState.STAND);
                                else
                                    TransitLowerBodyState(LowerBodyState.AIR);
                            }

                            if (dualwielding)
                            {
                                _animator.CrossFadeInFixedTime(ChooseDualwieldStandMotion(), 0.5f, 2);
                                intend_animator_speed = 1.0f;
                            }
                        }
                    }

                    if (upperBodyState == UpperBodyState.FIRE && !rollingfire_followthrough)
                        AcceptRollingShoot();

                    if (robotParameter.itemFlag.HasFlag(ItemFlag.ChainFire) && upperBodyState == UpperBodyState.FIRE)
                    {
                        AcceptSubFire();
                    }

                    if (upperBodyState != UpperBodyState.SNIPEFIRE)
                    {
                        AcceptSnipeShoot();
                        if (!AcceptDashSlash())
                            AcceptJumpSlash();
                    }

                    if (robotParameter.itemFlag.HasFlag(ItemFlag.IaiSlash) && upperBodyState == UpperBodyState.FIRE && !quickdraw_followthrough && !rollingfire_followthrough)
                    {
                        AcceptSlash();
                    }

                    aiming_begin_aiming_factor_current = rightWeapon.aiming_begin_aiming_factor;
                    aiming_angle_speed_current = rightWeapon.aiming_angle_speed;
                    current_aiming = true;
                    canhold_current = rightWeapon.canHold;
                }
                break;
            case UpperBodyState.SUBFIRE:
                {
                    miragecloud_invalid = true;

                    if (!fire_done)
                    {
                        if (event_subfired)
                        {
#if !ACCURATE_SEEK                        	
                            if (lockonState == LockonState.LOCKON)
#else                            
                            if (lockonState != LockonState.FREE)
#endif
                            {
                                shoulderWeapon.Target_Robot = Target_Robot;
                            }
                            else
                            {
                                shoulderWeapon.Target_Robot = null;
                                lockonState = LockonState.FREE;
                            }

                            shoulderWeapon_trigger_thisframe = true;
                            fire_done = true;
                            fire_followthrough = shoulderWeapon.fire_followthrough;
                        }
                    }
                    else
                    {
                        fire_followthrough--;

                        if (shoulderWeapon.canHold && _input.subfire)
                        {
                            shoulderWeapon_trigger_thisframe = _input.subfire;
                            fire_followthrough = shoulderWeapon.fire_followthrough;
                        }
                        else
                        {
                        }

                        if (fire_followthrough <= 0)
                        {

                            upperBodyState = UpperBodyState.STAND;
                            if (lowerBodyState == LowerBodyState.AIRSUBFIRE)
                            {
                                TransitLowerBodyState(LowerBodyState.AIR);
                            }
                            else if (lowerBodyState == LowerBodyState.GROUND_SUBFIRE)
                                TransitLowerBodyState(LowerBodyState.GROUND);
                            else
                                TransitLowerBodyState(LowerBodyState.STAND);

                        }
                    }

                    if (shoulderWeapon.allrange)
                    {
                        _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.04f);
                        _chestaimwait = Mathf.Max(0.0f, _chestaimwait - 0.04f);

                        //if (dualwielding)
                        _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f);
                        //else
                        //    _barmlayerwait = Mathf.Max(0.0f, _barmlayerwait - 0.08f);

                        chest_no_aim_smooth = true;



                        if (Target_Robot != null)
                        {
                            float angle;

                            angle = Vector3.Angle(Target_Robot.GetTargetedPosition() - GetCenter(), transform.forward);

                            if (angle > 60)
                            {
                                _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.1f);
                                head_no_aim_smooth = true;
                            }
                            else
                                _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.1f);
                        }
                        else
                        {
                            _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.1f);
                            head_no_aim_smooth = true;
                        }
                    }
                    else
                    {
                        _chestaimwait = Mathf.Min(1.0f, _chestaimwait + 0.08f);
                        chest_pitch_aim = true;
                        _headaimwait = 0.0f;
                        _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.16f);
                        _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f);
                    }

                    if (!AcceptDashSlash())
                        AcceptJumpSlash();

                    AcceptSnipeShoot();

                    chest_no_aim_smooth = true;

                    aiming_begin_aiming_factor_current = shoulderWeapon.aiming_begin_aiming_factor;
                    aiming_angle_speed_current = shoulderWeapon.aiming_angle_speed;
                    current_aiming = true;
                    canhold_current = shoulderWeapon.canHold;
                }
                break;
            case UpperBodyState.HEAVYFIRE:
            case UpperBodyState.ROLLINGHEAVYFIRE:
            case UpperBodyState.SNIPEHEAVYFIRE:
                {
                    miragecloud_invalid = true;

                    if (!fire_done)
                    {
                        if (event_heavyfired)
                        {
#if !ACCURATE_SEEK                        	
                            if (lockonState == LockonState.LOCKON)
#else
                            if (lockonState != LockonState.FREE)
#endif
                            {
                                rightWeapon.Target_Robot = Target_Robot;
                            }
                            else
                            {
                                rightWeapon.Target_Robot = null;
                                if (!lockonmode)
                                    lockonState = LockonState.FREE;
                            }
                            //rightWeapon.trigger = true;
                            rightWeapon_trigger_thisframe = true;
                            rightWeapon.chargeshot = upperBodyState == UpperBodyState.SNIPEHEAVYFIRE;
                            fire_done = true;
                            fire_followthrough = rightWeapon.fire_followthrough;
                        }



                        if (upperBodyState == UpperBodyState.ROLLINGHEAVYFIRE)
                        {
                            aiming_factor = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                            rhandaimwait_thisframe = Mathf.Clamp((aiming_factor - 0.25f) * 4.0f, 0.0f, 1.0f);
                        }
                        else if (upperBodyState == UpperBodyState.SNIPEHEAVYFIRE)
                        {
                            aiming_factor = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                            rhandaimwait_thisframe = Mathf.Clamp((aiming_factor - 0.33f) * 4.0f, 0.0f, 1.0f);
                        }
                        else
                        {
                            aiming_factor = animator.GetCurrentAnimatorStateInfo(2).normalizedTime;
                            rhandaimwait_thisframe = Mathf.Clamp((aiming_factor - 0.0f) * 4, 0.0f, 1.0f);
                        }
                    }
                    else
                    {
                        if (upperBodyState == UpperBodyState.ROLLINGHEAVYFIRE)
                        {
                            upperBodyState = UpperBodyState.HEAVYFIRE;

                            if (lowerBodyState == LowerBodyState.AIRROLLINGHEAVYFIRE)
                                TransitLowerBodyState(LowerBodyState.AIRHEAVYFIRE);
                            else
                                TransitLowerBodyState(LowerBodyState.HEAVYFIRE);

                            rollingfire_followthrough = true;
                        }

                        fire_followthrough--;

                        //if (rightWeapon.canHold)
                        // {
                        //    shoulderWeapon.trigger = _input.subfire;
                        //}
                        //else
                        {
                            //rightWeapon.trigger = false;
                        }

                        if (fire_followthrough <= 0)
                        {
                            upperBodyState = UpperBodyState.STAND;
                            if (lowerBodyState == LowerBodyState.AIRHEAVYFIRE)
                            {
                                TransitLowerBodyState(LowerBodyState.AIR);
                            }
                            else if (lowerBodyState == LowerBodyState.GROUND_HEAVYFIRE)
                                TransitLowerBodyState(LowerBodyState.GROUND);
                            else if (lowerBodyState == LowerBodyState.HEAVYFIRE)
                                TransitLowerBodyState(LowerBodyState.STAND);

                            if (lowerBodyState == LowerBodyState.SNIPEHEAVYFIRE || lowerBodyState == LowerBodyState.AIRSNIPEHEAVYFIRE)
                            {
                                if (Grounded)
                                    TransitLowerBodyState(LowerBodyState.STAND);
                                else
                                    TransitLowerBodyState(LowerBodyState.AIR);
                            }

                            if (dualwielding)
                            {
                                _animator.CrossFadeInFixedTime(ChooseDualwieldStandMotion(), 0.5f, 2);
                                intend_animator_speed = 1.0f;
                            }
                        }

                        aiming_factor = rhandaimwait_thisframe = 1.0f;

                    }

                    if (upperBodyState == UpperBodyState.HEAVYFIRE)
                    {
                        _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.10f);
                        _rarmaimwait = Mathf.Min(1.0f, _rarmaimwait + 0.04f);
                        _chestaimwait = Mathf.Min(1.0f, _chestaimwait + 0.04f);
                        _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f);
                    }
                    else if (upperBodyState == UpperBodyState.SNIPEHEAVYFIRE)
                    {
                        _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.10f);
                        _rarmaimwait = 0.0f;
                        _chestaimwait = 0.0f;
                        _barmlayerwait = 0.0f;
                    }
                    else
                    {
                        _headaimwait = 0.0f;
                        _rarmaimwait = 0.0f;
                        _chestaimwait = 0.0f;
                        _barmlayerwait = 0.0f;
                    }

                    chest_pitch_aim = true;

                    if (robotParameter.itemFlag.HasFlag(ItemFlag.ChainFire) && upperBodyState == UpperBodyState.HEAVYFIRE)
                    {
                        AcceptSubFire();
                    }

                    if (upperBodyState == UpperBodyState.HEAVYFIRE && !rollingfire_followthrough)
                        AcceptRollingShoot();

                    if (upperBodyState != UpperBodyState.SNIPEHEAVYFIRE)
                        AcceptSnipeShoot();

                    if (robotParameter.itemFlag.HasFlag(ItemFlag.IaiSlash) && upperBodyState == UpperBodyState.HEAVYFIRE && !quickdraw_followthrough && !rollingfire_followthrough)
                    {
                        AcceptSlash();
                    }

                    if (!AcceptDashSlash())
                        AcceptJumpSlash();

                    aiming_begin_aiming_factor_current = rightWeapon.aiming_begin_aiming_factor;
                    aiming_angle_speed_current = rightWeapon.aiming_angle_speed;
                    current_aiming = true;
                    canhold_current = rightWeapon.canHold;
                }
                break;
            case UpperBodyState.STAND:
                {
                    massillusion_invalid = true;

                    if (!lockonmode)
                        lockonState = LockonState.FREE;

                    float angle = 180.0f;

                    if (Target_Robot != null)
                    {
                        angle = Vector3.Angle(Target_Robot.GetTargetedPosition() - GetCenter(), transform.forward);

                        if (angle > 60)
                        {
                            _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.1f);
                            head_no_aim_smooth = true;
                        }
                        else
                            _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.1f);
                    }
                    else
                    {
                        _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.1f);
                        head_no_aim_smooth = true;
                    }

                    if (!AcceptRollingShoot())
                    {
                        if (!AcceptSnipeShoot())
                        {
                            AcceptMainFire(angle);
                        }

                    }

                    if (Sword != null)
                    {
                        if (AcceptDashSlash())
                        {

                        }
                        else if (AcceptJumpSlash())
                        {
                            
                        }
                        else if(AcceptSweep())
                        {

                        }
                        else
                            AcceptSlash();

                    }

                    AcceptSubFire();

                    _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.04f);
                    _chestaimwait = Mathf.Max(0.0f, _chestaimwait - 0.04f);

                    if (dualwielding)
                        _barmlayerwait = Mathf.Min(1.0f, _barmlayerwait + 0.08f);
                    else
                        _barmlayerwait = Mathf.Max(0.0f, _barmlayerwait - 0.08f);

                    chest_no_aim_smooth = true;
                }
                break;
            case UpperBodyState.KNOCKBACK:
            case UpperBodyState.DOWN:
            case UpperBodyState.GETUP:
                miragecloud_invalid = true;
                massillusion_invalid = true;
                _chestaimwait = 0.0f;
                _headaimwait = 0.0f;
                _rarmaimwait = 0.0f;
                _barmlayerwait = 0.0f;
                if (!lockonmode)
                    lockonState = LockonState.FREE;
                break;
            case UpperBodyState.SLASH:
            case UpperBodyState.SLASH_DASH:
            case UpperBodyState.VOIDSHIFT:

                miragecloud_invalid = true;

				if (subState_Slash == SubState_Slash.DashSlash)
                    AcceptJumpSlash();

                AcceptSnipeShoot();

                if (subState_Slash == SubState_Slash.LowerSlash)
                {
                    _chestaimwait = 0.0f;
                    _headaimwait = 1.0f;
                    _rarmaimwait = 0.0f;
                    _barmlayerwait = 0.0f;
                }
                else
                {
                    _chestaimwait = 0.0f;
                    _headaimwait = 0.0f;
                    _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.08f);
                    _barmlayerwait = Mathf.Max(0.0f, _barmlayerwait - 0.08f);
                }

                current_aiming = true;
                aiming_begin_aiming_factor_current = float.MaxValue;

                break;
            case UpperBodyState.JumpSlash_Jump:

                miragecloud_invalid = true;

                _chestaimwait = 0.0f;
                _headaimwait = 0.0f;
                _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.08f);
                _barmlayerwait = 0.0f;
                AcceptSnipeShoot();
                current_aiming = true;
                aiming_begin_aiming_factor_current = float.MaxValue;
                break;
            case UpperBodyState.JumpSlash:
            case UpperBodyState.JumpSlash_Ground:
            case UpperBodyState.SWEEP:
                miragecloud_invalid = true;

                _chestaimwait = 0.0f;
                _headaimwait = 0.0f;
                _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.08f);
                _barmlayerwait = 0.0f;
                AcceptSnipeShoot();
                current_aiming = true;
                aiming_begin_aiming_factor_current = float.MaxValue;
                break;
            default:
                _chestaimwait = 0.0f;
                _headaimwait = 0.0f;
                _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.08f);
                _barmlayerwait = Mathf.Max(0.0f, _barmlayerwait - 0.08f);
                if (!lockonmode)
                    lockonState = LockonState.FREE;
                break;
        }

        headmultiAimConstraint.weight = _headaimwait;

        //headmultiAimConstraint.weight = 1.0f;

        Quaternion target_rot_head;
        Quaternion target_rot_chest;
        Quaternion target_rot_rhand;

        if (Target_Robot != null)
        {
            // キャラの処理順で狙いが1フレームずれる（公平性はともかく、ロックオンカーソルがずれる）
            // 問題の対処のため、一旦LateFixedUpdate()との二重処理にする

            if(is_player)
                uIController_Overlay.aiming = current_aiming;

            if (!fire_done || canhold_current)
            {
                if (aiming_factor < aiming_begin_aiming_factor_current || robotParameter.itemFlag.HasFlag(ItemFlag.TrackingSystem))
                {
                    virtual_targeting_position_forBody = Target_Robot.GetTargetedPosition();

                    //if (is_player)
                    //    uIController_Overlay.aim_fixed = true;
                }
                else
                {
                    Vector3 a = virtual_targeting_position_forBody - GetCenter();
                    Vector3 b = Target_Robot.GetTargetedPosition() - GetCenter();

                    if (Vector3.Angle(a, b) < aiming_angle_speed_current)
                    {
                        //if (is_player)
                        //    uIController_Overlay.aim_fixed = true;

                        virtual_targeting_position_forBody = GetCenter() + b;
                    }
                    else
                    {
                        //if (is_player)
                        //    uIController_Overlay.aim_fixed = false;

                        virtual_targeting_position_forBody = GetCenter() + Vector3.RotateTowards(a, b, aiming_angle_speed_current * Mathf.Deg2Rad, float.MaxValue);
                    }
                }
            }

            target_rot_head = Process_Aiming_Head(true);
            target_rot_chest = Process_Aiming_Chest(true);
            target_rot_rhand = Process_Aiming_RHand(true);


        }
        else
        {
            if (is_player)
            {
                uIController_Overlay.aiming = false;
                //uIController_Overlay.aim_fixed = false;
            }

            target_rot_head = Process_Aiming_Head(false);
            target_rot_chest = Process_Aiming_Chest(false);
            target_rot_rhand = Process_Aiming_RHand(false);
        }

        //Quaternion thisframe_rot_head

        if (head_no_aim_smooth)
        {
            AimTargetRotation_Head = target_rot_head;
        }
        else
        {
            AimTargetRotation_Head = Quaternion.RotateTowards(AimTargetRotation_Head, target_rot_head, 4.0f);
            //= target_rot_head;
            //= Head.transform.rotation;
        }

        AimHelper_Head.transform.position = Head.transform.position + AimTargetRotation_Head * Vector3.forward * 3;


        if (chest_no_aim_smooth)
        {
            AimTargetRotation_Chest = target_rot_chest;
        }
        else
        {
            AimTargetRotation_Chest = Quaternion.RotateTowards(AimTargetRotation_Chest, target_rot_chest, 2.0f);
        }


        Vector3 chestAim_Dir = AimTargetRotation_Chest * Vector3.forward * 3;

        if (!chest_pitch_aim)
            chestAim_Dir.y = 0.0f;

        AimHelper_Chest.transform.position = Chest.transform.position + chestAim_Dir;

        chestmultiAimConstraint.weight = _chestaimwait;

        AimHelper_RHand.transform.position = RHand.transform.position + target_rot_rhand * Vector3.forward * 10.0f;

        rhandmultiAimConstraint.weight = rhandaimwait_thisframe;

        overrideTransform.weight = _rarmaimwait;


        animator.SetLayerWeight(2, _barmlayerwait);

        if (!dualwielding)
            animator.SetLayerWeight(1, _rarmaimwait);

        if (rightWeapon != null)
            rightWeapon.trigger = rightWeapon_trigger_thisframe;

        if (shoulderWeapon != null)
            shoulderWeapon.trigger = shoulderWeapon_trigger_thisframe;

        if (Sword != null)
            Sword.dir = transform.forward;

        bool passivemirage_thistime =
            (!miragecloud_invalid && robotParameter.itemFlag.HasFlag(ItemFlag.MirageCloud))
            || (!massillusion_invalid && robotParameter.itemFlag.HasFlag(ItemFlag.MassIllusion))
            ;
        
        if (passivemirage_thistime)
        {
            if (passiveMirage_interval <= 0)
            {
                StartMirage(15);
                passiveMirage_interval = 30;
            }
        }
        else
        {
            passiveMirage_interval = 30;
        }

        if (passiveMirage_interval > 0)
            passiveMirage_interval--;
        
    }

    Quaternion Process_Aiming_Head(bool aiming)
    {
        Quaternion result;

        if (aiming)
        {
            //result = Quaternion.LookRotation(Target_Robot.GetCenter() - GetCenter(), new Vector3(0.0f, 1.0f, 0.0f));
            result = Quaternion.LookRotation(Target_Robot.GetTargetedPosition() - GetCenter(), new Vector3(0.0f, 1.0f, 0.0f));
        }
        else
        {
            result = Head.transform.rotation;
        }

        return result;
    }

    Quaternion Process_Aiming_Chest(bool aiming)
    {
        Quaternion result;

        if (aiming)
        {

            if (rightWeapon == null || rightWeapon.trajectory == Weapon.Trajectory.Straight || (upperBodyState != UpperBodyState.HEAVYFIRE && upperBodyState != UpperBodyState.ROLLINGHEAVYFIRE))
            {
                result = Quaternion.LookRotation(Target_Robot.GetTargetedPosition() - GetCenter(), new Vector3(0.0f, 1.0f, 0.0f));

                if(gatling)
                {
                    result = result * Quaternion.Euler(0.0f, 20.0f, 0.0f);
                }
            }
            else
            {
                Vector3 relative = Target_Robot.GetTargetedPosition() - GetCenter();

                float h = relative.y;

                relative.y = 0.0f;

                float v = rightWeapon.projectile_speed;
                float g = rightWeapon.projectile_gravity;
                float L = relative.magnitude;

                //float rad = Mathf.Asin(L * g / (v * v))/2;

                float a = g;
                float b = -2 * v * v / L;
                float c = 2 * h * v * v / L / L + g;

                float rad = Mathf.Atan((-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a));

                relative.y = relative.magnitude * Mathf.Tan(rad);


                result = Quaternion.LookRotation(relative, new Vector3(0.0f, 1.0f, 0.0f));




            }
        }
        else
        {
            result = chest_hint.transform.rotation;
        }

        return result;
    }

    // 
    Quaternion Process_Aiming_RHand(bool aiming)
    {
        Quaternion result;

        if (aiming)
        {

            if (rightWeapon == null || rightWeapon.trajectory == Weapon.Trajectory.Straight || (upperBodyState != UpperBodyState.HEAVYFIRE && upperBodyState != UpperBodyState.ROLLINGHEAVYFIRE))
            {
                result = Quaternion.LookRotation(virtual_targeting_position_forBody - RHand.transform.position, new Vector3(0.0f, 1.0f, 0.0f));

                Quaternion q_aim_global = Quaternion.LookRotation(aiming_hint.transform.position - virtual_targeting_position_forBody, new Vector3(0.0f, 1.0f, 0.0f));
                overrideTransform.data.position = shoulder_hint.transform.position;
                overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;
            }
            else
            {
                Vector3 relative = virtual_targeting_position_forBody - RHand.transform.position;

                float h = relative.y;

                relative.y = 0.0f;

                float v = rightWeapon.projectile_speed;
                float g = rightWeapon.projectile_gravity;
                float L = relative.magnitude;

                //float rad = Mathf.Asin(L * g / (v * v))/2;

                float a = g;
                float b = -2 * v * v / L;
                float c = 2 * h * v * v / L / L + g;

                float rad = Mathf.Atan((-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a));

                relative.y = relative.magnitude * Mathf.Tan(rad);


                result = Quaternion.LookRotation(relative, new Vector3(0.0f, 1.0f, 0.0f));

                Quaternion q_aim_global = Quaternion.LookRotation(-relative, new Vector3(0.0f, 1.0f, 0.0f));
                overrideTransform.data.position = shoulder_hint.transform.position;
                overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;
            }
        }
        else
        {
            result = chest_hint.transform.rotation;

            Quaternion q_aim_global = Quaternion.LookRotation(-aiming_hint.transform.forward, new Vector3(0.0f, 1.0f, 0.0f));
            overrideTransform.data.position = shoulder_hint.transform.position;
            overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;
        }

        return result;
    }


    //return angle in range -180 to 180
    float origin = 0.0f;
    bool prev_boosting = false;

    int mirage_time = 0;
    [SerializeField] AfterimageSample.AfterimageRenderer afterimageRenderer;
    [SerializeField] AfterimageSample.EnqueueAfterimage enqueueAfterimage;

    [SerializeField] AfterimageSample.AfterimageRenderer afterimageRenderer_VoidShift;
    [SerializeField] AfterimageSample.EnqueueAfterimage enqueueAfterimage_VoidShift;

    int passiveMirage_interval = 0;

    private Vector3 virtual_targeted_position;

    private void LowerBodyMove()
    {
        float targetSpeed = 0.0f;
        bool boosting = false;
        bool ground_boost_now = false;
        bool regen_boost_now = false;

        if (hitstop_timer <= 0)
        {

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
                                targetSpeed = robotParameter.AirMoveSpeed;
                            else if (lowerBodyState == LowerBodyState.DASH)
                                targetSpeed = robotParameter.AirDashSpeed;
                            else
                                targetSpeed = robotParameter.MoveSpeed;

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
                                Time.deltaTime * robotParameter.SpeedChangeRate);

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
                                                  _cinemachineTargetYaw;
                                //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                                //    RotationSmoothTime);

                                float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, lowerBodyState == LowerBodyState.DASH ? robotParameter.DashRotateSpeed : robotParameter.RotateSpeed);

                                // rotate to face input direction relative to camera position
                                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                            }
                        }
                        else //自由落下
                        {
                            // a reference to the players current horizontal velocity
                            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
                            float speedOffset = 0.1f;

                            targetSpeed = robotParameter.AirMoveSpeed;

                            // accelerate or decelerate to target speed
                            if (
                                //currentHorizontalSpeed < targetSpeed - speedOffset ||
                                currentHorizontalSpeed > targetSpeed + speedOffset
                                )
                            {
                                // creates curved result rather than a linear one giving a more organic speed change
                                // note T in Lerp is clamped, so we don't need to clamp our speed
                                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                                    Time.deltaTime * robotParameter.SpeedChangeRate);

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
                            else
                            {
                                AcceptStep(false);

                            }

                            regen_boost_now = true;
                        }

                        if (lowerBodyState == LowerBodyState.AIR)
                        {
                            bool inputing_boost = false;

                            if (robotParameter.itemFlag.HasFlag(ItemFlag.RunningTakeOff) && _verticalVelocity > robotParameter.AscendingVelocity)
                            {
                                //重力で減速するのでそれを待つようにした
                                //_verticalVelocity -= robotParameter.AscendingAccelerate; 

                                boosting = true;
                            }
                            else
                            {
                                if (_input.jump)
                                {
                                    if (ConsumeBoost(4))
                                    {
                                        _verticalVelocity = Mathf.Min(_verticalVelocity + robotParameter.AscendingAccelerate, robotParameter.AscendingVelocity);
                                        boosting = true;
                                        inputing_boost = true;
                                    }
                                }
                            }

                            if (!inputing_boost)
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.FlightUnit))
                                    regen_boost_now = true;

                                AcceptDash(false);
                            }
                            _animator.SetFloat(_animIDVerticalSpeed, _verticalVelocity);
                        }
                        else if (lowerBodyState == LowerBodyState.DASH)
                        {
                            _verticalVelocity = 0.0f;

                            bool boost_remain;
                            bool force_boost = false;

                            if (nextdrive && upperBodyState == UpperBodyState.FIRE && (!fire_done || rightWeapon.canHold))
                            {
                                stepremain = 30;
                                force_boost = true;

                                if (nextdrive_free_boost > 0)
                                {
                                    boost_remain = true;
                                    nextdrive_free_boost--;
                                }
                                else
                                    boost_remain = ConsumeBoost(4);
                            }
                            else if (nextdrive && stepremain > 0)
                            {
                                stepremain--;
                                force_boost = true;

                                if (nextdrive_free_boost > 0)
                                {
                                    boost_remain = true;
                                    nextdrive_free_boost--;
                                }
                                else
                                    boost_remain = ConsumeBoost(4);
                            }
                            else
                                boost_remain = ConsumeBoost(4);
                       
                            if (
                                ((!_input.sprint && !_input.sprint_once && !force_boost)/*_input.move == Vector2.zero*/ || _input.jump || !boost_remain) && event_dashed
                                )
                            {
                                TransitLowerBodyState(LowerBodyState.AIR);
                            }
                            else
                            {
                                boosting = true;
                            }

                            if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive) && !prev_sprint)
                                AcceptDash(false);
                        }
                        
                        if(lowerBodyState != LowerBodyState.DASH)
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
                case LowerBodyState.ROLLINGFIRE:
                case LowerBodyState.AIRROLLINGFIRE:
                case LowerBodyState.ROLLINGHEAVYFIRE:
                case LowerBodyState.AIRROLLINGHEAVYFIRE:
                case LowerBodyState.SNIPEFIRE:
                case LowerBodyState.AIRSNIPEFIRE:
                case LowerBodyState.SNIPEHEAVYFIRE:
                case LowerBodyState.AIRSNIPEHEAVYFIRE:
                    {
                        // a reference to the players current horizontal velocity
                        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                        float speedOffset = 0.1f;
                        float inputMagnitude = 1f;


                        targetSpeed = 0.0f;

                        _animationBlend = Mathf.Max(_animationBlend - 0.015f, 0.0f);

                        if (_animationBlend < 0.01f) _animationBlend = 0f;

                        // 滑り撃ちのときは、LowerBodyMove()末尾の別個処理でやってる
                        if (!backblast_processed &&
                            !(robotParameter.itemFlag.HasFlag(ItemFlag.Hovercraft) && (lowerBodyState == LowerBodyState.HEAVYFIRE || lowerBodyState == LowerBodyState.ROLLINGHEAVYFIRE)))
                        {
                            if (lowerBodyState == LowerBodyState.AIRHEAVYFIRE || lowerBodyState == LowerBodyState.HEAVYFIRE
                                || lowerBodyState == LowerBodyState.AIRROLLINGHEAVYFIRE || lowerBodyState == LowerBodyState.ROLLINGHEAVYFIRE)
                            {
                                Vector3 backBlastDir = -(rightWeapon.gameObject.transform.rotation * (Vector3.forward));

                                Vector3 backBlackDir_Horizontal = new Vector3(backBlastDir.x, 0.0f, backBlastDir.z);

                                currentHorizontalSpeed += 50.0f * backBlackDir_Horizontal.magnitude;

                                if (lowerBodyState == LowerBodyState.AIRHEAVYFIRE)
                                {
                                    _verticalVelocity += 50.0f * backBlastDir.y;
                                }

                                backblast_processed = true;
                            }
                        }



                        if (lowerBodyState == LowerBodyState.ROLLINGFIRE || lowerBodyState == LowerBodyState.AIRROLLINGFIRE
                            || lowerBodyState == LowerBodyState.ROLLINGHEAVYFIRE || lowerBodyState == LowerBodyState.AIRROLLINGHEAVYFIRE
                            )
                        {
                            _verticalVelocity = 0.0f;
                            if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickDraw))
                                _speed = robotParameter.StepSpeed * 1.5f;
                            else
                                _speed = robotParameter.StepSpeed;
                        }
                        else
                        {
                            float brakefactor = 1.0f;
                            if (lowerBodyState == LowerBodyState.AIRHEAVYFIRE)
                            {
                                brakefactor = 0.25f;
                            }
                            else if (lowerBodyState == LowerBodyState.HEAVYFIRE)
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
                                    Time.deltaTime * robotParameter.SpeedChangeRate * brakefactor);

                                // round speed to 3 decimal places
                                _speed = Mathf.Round(_speed * 1000f) / 1000f;
                            }
                            else
                            {
                                _speed = targetSpeed;
                            }

                            if (robotParameter.itemFlag.HasFlag(ItemFlag.FlightUnit))
                            {
                                if (_input.jump)
                                {
                                    if (ConsumeBoost(4))
                                    {
                                        _verticalVelocity = Mathf.Min(_verticalVelocity + robotParameter.AscendingAccelerate, robotParameter.AscendingVelocity);
                                        boosting = true;
                                    }
                                }
                            }
                        }

                        if (Target_Robot != null)
                        {
                            Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                            _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                            float rotation;

                            if (!((lowerBodyState == LowerBodyState.SUBFIRE || lowerBodyState == LowerBodyState.AIRSUBFIRE) && shoulderWeapon.allrange))
                            {
                                if (lowerBodyState == LowerBodyState.ROLLINGFIRE || lowerBodyState == LowerBodyState.AIRROLLINGFIRE)
                                    rotation = _targetRotation;
                                else if (lowerBodyState == LowerBodyState.FIRE || lowerBodyState == LowerBodyState.AIRFIRE)
                                    rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, robotParameter.RotateSpeed * 2 * firing_multiplier);
                                else
                                    rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, robotParameter.RotateSpeed * 4);
                            }
                            else
                                rotation = transform.eulerAngles.y;

                            // rotate to face input direction relative to camera position
                            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                        }

                        if (lowerBodyState == LowerBodyState.AIRFIRE)
                            animator.SetFloat(_animIDVerticalSpeed, _verticalVelocity);

                        if (lowerBodyState == LowerBodyState.FIRE || lowerBodyState == LowerBodyState.SUBFIRE || lowerBodyState == LowerBodyState.HEAVYFIRE
                            || lowerBodyState == LowerBodyState.ROLLINGFIRE || lowerBodyState == LowerBodyState.ROLLINGHEAVYFIRE)
                        {
                            regen_boost_now = true;
                        }

                        if (lowerBodyState == LowerBodyState.AIRFIRE || lowerBodyState == LowerBodyState.AIRHEAVYFIRE || lowerBodyState == LowerBodyState.AIRSUBFIRE
                            || lowerBodyState == LowerBodyState.AIRROLLINGFIRE || lowerBodyState == LowerBodyState.AIRROLLINGHEAVYFIRE
                            || lowerBodyState == LowerBodyState.AIRSNIPEFIRE || lowerBodyState == LowerBodyState.AIRSNIPEHEAVYFIRE
                            )
                        {
                            if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
                                AcceptDash(true);

                            if (robotParameter.itemFlag.HasFlag(ItemFlag.FlightUnit))
                                regen_boost_now = true;
                        }
                        else
                        {
                            if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
                                AcceptStep(true);
                        }

                        if (lowerBodyState != LowerBodyState.AIRROLLINGFIRE && lowerBodyState != LowerBodyState.AIRROLLINGHEAVYFIRE)
                        {
                            JumpAndGravity();
                            GroundedCheck();
                        }
                    }
                    break;
                case LowerBodyState.GROUND:
                case LowerBodyState.GROUND_FIRE:
                case LowerBodyState.GROUND_SUBFIRE:
                case LowerBodyState.GROUND_HEAVYFIRE:
                case LowerBodyState.JUMP:
                case LowerBodyState.DOWN:
                case LowerBodyState.GETUP:
                case LowerBodyState.STEPGROUND:
                case LowerBodyState.JumpSlash_Jump:
                case LowerBodyState.JumpSlash_Ground:
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
                            case LowerBodyState.GROUND_FIRE:
                                {
                                    _speed = 0.0f;
                                    if (event_grounded)
                                    {
                                        TransitLowerBodyState(LowerBodyState.FIRE);
                                    }
                                }
                                break;
                            case LowerBodyState.GROUND_SUBFIRE:
                                {
                                    _speed = 0.0f;
                                    if (event_grounded)
                                    {
                                        TransitLowerBodyState(LowerBodyState.SUBFIRE);
                                    }
                                }
                                break;
                            case LowerBodyState.GROUND_HEAVYFIRE:
                                {
                                    backblast_processed = true; // 着地硬直中に撃って、終わった瞬間に反動が始まるのを防止
                                    _speed = 0.0f;
                                    if (event_grounded)
                                    {
                                        TransitLowerBodyState(LowerBodyState.HEAVYFIRE);
                                    }
                                }
                                break;

                            case LowerBodyState.STEPGROUND:
                            case LowerBodyState.JumpSlash_Ground:
                                {
                                    _speed = 0.0f;

                                    if (robotParameter.itemFlag.HasFlag(ItemFlag.RunningTakeOff) && lowerBodyState == LowerBodyState.STEPGROUND)
                                    {
                                        if (_input.jump && ConsumeBoost(80))
                                        {
                                            TransitLowerBodyState(LowerBodyState.AIR);
                                            _verticalVelocity = robotParameter.AscendingVelocity * 1.5f;

                                            _controller.Move(new Vector3(0.0f, 0.1f, 0.0f));

                                            StartMirage(10);
                                        }
                                    }

                                    if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
                                        AcceptStep(true);

                                    if (event_grounded)
                                    {
                                        if (lowerBodyState == LowerBodyState.JumpSlash_Ground)
                                        {
                                            Sword.emitting = false;
                                        }

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
                                        _verticalVelocity = robotParameter.AscendingVelocity;

                                        _controller.Move(new Vector3(0.0f, 0.1f, 0.0f));
                                    }
                                }
                                break;
                            case LowerBodyState.JumpSlash_Jump:
                                {
                                    _speed = 0.0f;
                                    if (event_jumped)
                                    {
                                        lowerBodyState = LowerBodyState.JumpSlash;
                                        upperBodyState = UpperBodyState.JumpSlash;
                                        intend_animator_speed = 1.0f;
                                        event_slash = false;
                                        event_acceptnextslash = false;
                                        combo_reserved = false;
                                        Sword.slashing = false;
                                        slash_count = 0;
                                        Sword.damage = Sword.slashMotionInfo[SubState_Slash.JumpSlash].damage[slash_count];
                                        Sword.knockBackType = KnockBackType.Finish;
                                        //stepremain = Sword.motionProperty[LowerBodyState.JumpSlash_Jump].DashLength;

                                        //_animator.CrossFadeInFixedTime(_animIDAir, 0.5f, 0);
                                        //_animator.CrossFadeInFixedTime(Sword.slashMotionInfo[LowerBodyState.JumpSlash]._animID[slash_count], 0.0f, 2);
                                        _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[SubState_Slash.JumpSlash]._animID[slash_count], 0.0f, 0);
                                        //_animator.SetLayerWeight(2, 1.0f);
                                        _animator.SetFloat("SlashSpeed", 0.0f);
                                        audioSource.PlayOneShot(audioClip_Swing);

                                        _verticalVelocity = InitialVerticalSpeed_JumpSlash;
                                        //_verticalVelocity = Sword.motionProperty[LowerBodyState.JumpSlash_Jump].DashSpeed;

                                        //if (robotParameter.itemFlag.HasFlag(ItemFlag.InfightBoost))
                                        //    _verticalVelocity *= 1.5f;

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

                                        if (strongdown)
                                            intend_animator_speed = 0.5f;
                                        else
                                            intend_animator_speed = 1.0f;

                                        event_getup = false;
                                        origin = transform.position.y;
                                        _verticalVelocity = 0.0f;
                                    }

                                    JumpAndGravity();
                                    GroundedCheck();
                                    regen_boost_now = true;
                                }
                                break;

                            case LowerBodyState.GETUP:
                                {
                                    _speed = 0.0f;
                                    if (event_getup)
                                    {
                                        TransitLowerBodyState(LowerBodyState.STAND);
                                        intend_animator_speed = 1.0f;
                                    }
                                    else
                                    {
                                        AnimatorStateInfo animeStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                                        float prevheight = _controller.height;

                                        float newheight = _controller.height = min_controller_height + animeStateInfo.normalizedTime * (org_controller_height - min_controller_height);


                                        _verticalVelocity = (newheight - prevheight) / Time.deltaTime / 2;
                                    }
                                    regen_boost_now = true;
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
                                              _cinemachineTargetYaw;
                            //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                            //    RotationSmoothTime);
                        }
                        else
                        {
                            _targetRotation = transform.eulerAngles.y;
                        }*/
                        float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive) ? 360.0f : robotParameter.AirDashRotateSpeed);

                        // rotate to face input direction relative to camera position
                        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);


                        float degree = Mathf.DeltaAngle(transform.eulerAngles.y, _targetRotation);

                        if (degree < robotParameter.RotateSpeed && degree > -robotParameter.RotateSpeed)
                        {
                            lowerBodyState = LowerBodyState.DASH;
                            _animator.CrossFadeInFixedTime(_animIDDash, 0.25f, 0);
                            event_dashed = false;

                            if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickIgniter))
                            {
                                _speed = robotParameter.AirDashSpeed * 2;
                            }
                            if (robotParameter.itemFlag.HasFlag(ItemFlag.AeroMirage))
                            {
                                StartMirage(10);
                            }
                        }
                    }
                    break;
                case LowerBodyState.STEP:
                    {
                        // a reference to the players current horizontal velocity
                        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                        float speedOffset = 0.1f;
                        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

                        targetSpeed = robotParameter.StepSpeed;

                        // accelerate or decelerate to target speed
                        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                            currentHorizontalSpeed > targetSpeed + speedOffset)
                        {
                            // creates curved result rather than a linear one giving a more organic speed change
                            // note T in Lerp is clamped, so we don't need to clamp our speed
                            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                                Time.deltaTime * robotParameter.SpeedChangeRate);

                            // round speed to 3 decimal places
                            _speed = Mathf.Round(_speed * 1000f) / 1000f;
                        }
                        else
                        {
                            _speed = targetSpeed;
                        }

                        _animationBlend = 0.0f;

                        stepremain--;

                        if (step_boost)
                            boosting = true;

                        if (event_stepped)
                        {


                            bool stop = false;

                            if (!_input.sprint && !_input.sprint_once/*_input.move == Vector2.zero*/)
                                stop = true;
                            else
                            {
                                if (stepremain <= 0)
                                {
                                    if (robotParameter.itemFlag.HasFlag(ItemFlag.GroundBoost) && ConsumeBoost(4))
                                    {
                                        ground_boost_now = true;
                                    }
                                    else
                                        stop = true;
                                }
                            }

                            if (stop)
                                TransitLowerBodyState(LowerBodyState.STEPGROUND);
                            else
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
                                    AcceptStep(true);

                                if (robotParameter.itemFlag.HasFlag(ItemFlag.GroundBoost))
                                {
                                    // normalise input direction
                                    Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

                                    if (inputDirection != Vector3.zero)
                                    {
                                        StepDirection stepDirection_old = stepDirection;

                                        stepDirection = determineStepDirection(inputDirection);

                                        if (stepDirection != stepDirection_old && ConsumeBoost(40))
                                        {
                                            float steptargetdegree = degreeFromStepDirection(stepDirection) + _cinemachineTargetYaw;
                                            float stepmotiondegree = Mathf.Repeat(steptargetdegree - transform.eulerAngles.y + 180.0f, 360.0f) - 180.0f;

                                            if (stepmotiondegree <= 45.0f && stepmotiondegree >= -45.0f)
                                                stepMotion = StepMotion.FORWARD;
                                            else if (stepmotiondegree > -135.0f && stepmotiondegree < -45.0f)
                                                stepMotion = StepMotion.LEFT;
                                            else if (stepmotiondegree < 135.0f && stepmotiondegree > 45.0f)
                                                stepMotion = StepMotion.RIGHT;
                                            else// if (stepmotiondegree >= 135.0f || stepmotiondegree <= -135.0f)
                                                stepMotion = StepMotion.BACKWARD;


                                            event_stepbegin = false;
                                            event_stepped = false;

                                            switch (stepMotion)
                                            {
                                                case StepMotion.LEFT:
                                                    _animator.CrossFadeInFixedTime(_animIDStep_Left, 0.25f, 0);
                                                    steptargetrotation = steptargetdegree + 90.0f;
                                                    break;
                                                case StepMotion.BACKWARD:
                                                    _animator.CrossFadeInFixedTime(_animIDStep_Back, 0.25f, 0);
                                                    steptargetrotation = steptargetdegree + 180.0f;
                                                    break;
                                                case StepMotion.RIGHT:
                                                    _animator.CrossFadeInFixedTime(_animIDStep_Right, 0.25f, 0);
                                                    steptargetrotation = steptargetdegree - 90.0f;
                                                    break;
                                                default:
                                                    //case StepMotion.FORWARD:
                                                    _animator.CrossFadeInFixedTime(_animIDStep_Front, 0.25f, 0);
                                                    steptargetrotation = steptargetdegree;
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case LowerBodyState.SLASH_DASH:
                case LowerBodyState.VOIDSHIFT:
                    {
                        float rotatespeed;

                        if (subState_Slash == SubState_Slash.DashSlash && !event_swing)
                        {
                            _speed = targetSpeed = 0.0f;
                        }
                        else
                        {
                            if (lowerBodyState == LowerBodyState.VOIDSHIFT)
                                _speed = targetSpeed = voidshift_speed;
                            else
                            {
                                _speed = targetSpeed = Sword.motionProperty[subState_Slash].DashSpeed;

                                if (robotParameter.itemFlag.HasFlag(ItemFlag.InfightBoost) && subState_Slash != SubState_Slash.DashSlash)
                                {
                                    _speed *= 1.5f;
                                    targetSpeed *= 1.5f;
                                }
                            }
                        }

                        if (subState_Slash == SubState_Slash.DashSlash && event_swing)
                        {
                            intend_animator_speed = 0.0f;
                        }

                        rotatespeed = Sword.motionProperty[subState_Slash].RotateSpeed;

                        if (robotParameter.itemFlag.HasFlag(ItemFlag.InfightBoost) && subState_Slash != SubState_Slash.DashSlash)
                        {
                            rotatespeed *= 1.5f;
                        }

                        _animationBlend = 0.0f;


                        if (dicSubStateSlashType[subState_Slash] == SubState_SlashType.AIR)
                        {
                            boosting = true;
                        }



                        if (Target_Robot != null)
                        {
                            if (Sword.dashslash_cutthrough && subState_Slash == SubState_Slash.DashSlash)
                            {

                                Vector3 targetOffset = dashslash_offset;

                                Vector3 targetPos = Target_Robot.GetTargetedPosition() + targetOffset.normalized * (Sword.motionProperty[subState_Slash].SlashDistance * transform.lossyScale.x * 0.75f);

                                Vector3 targetDirection = (targetPos - GetCenter()).normalized;

                                _targetRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

                                float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, rotatespeed);

                                // rotate to face input direction relative to camera position
                                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                            }
                            else if(subState_Slash == SubState_Slash.AirSlashSeed || subState_Slash == SubState_Slash.SlideSlashSeed)
                            {
                                Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                                _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                                float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, rotatespeed);

                                Quaternion look = Quaternion.LookRotation(target_dir, Vector3.up);

                                float rotation_pitch = Mathf.Clamp(Mathf.DeltaAngle(0.0f, look.eulerAngles.x), -seedSlash_PitchLimit, seedSlash_PitchLimit);

                                // rotate to face input direction relative to camera position
                                transform.rotation = Quaternion.Euler(rotation_pitch, rotation, 0.0f);
                            }
                            else
                            {
                                Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                                _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                                float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, rotatespeed);

                                // rotate to face input direction relative to camera position
                                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                            }
                        }

                        bool slash = false;
                        bool lowerslash = false;

                        if (Target_Robot == null)
                        {
                            if(subState_Slash == SubState_Slash.DashSlash)
                            {
                                if(event_swing)
                                    slash = true;
                            }
                            else
                                slash = true;
                        }
                        else
                        {
                            if(lowerBodyState == LowerBodyState.VOIDSHIFT && Vector3.Distance(Target_Robot.GetTargetedPosition(),GetCenter()) < voidshift_limit_distance)
                            {
                                lowerBodyState = LowerBodyState.SLASH_DASH;
                            }

                            Vector3 rel = Target_Robot.GetTargetedPosition() - GetCenter();

                            if (subState_Slash == SubState_Slash.AirSlashSeed || subState_Slash == SubState_Slash.SlideSlashSeed)
                            {
                                if (rel.magnitude < Sword.motionProperty[subState_Slash].SlashDistance * transform.lossyScale.x)
                                {
                                    slash = true;
                                }
                            }
                            else
                            {
                                float rely = rel.y;
                                rel.y = 0.0f;

                                if (rel.magnitude < Sword.motionProperty[subState_Slash].SlashDistance * transform.lossyScale.x && rely < 1.0f && rely > -3.0f)
                                {
                                    slash = true;
                                }
                            }

                          

                            //if(Target_Robot.GetTargetPosition().y < Chest.transform.position.y - 0.00852969*Chest.transform.lossyScale.y)
                            if (Target_Robot.lowerBodyState == LowerBodyState.DOWN || Target_Robot.lowerBodyState == LowerBodyState.GETUP)
                            {
                                lowerslash = true;
                            }

                            if (Target_Robot.transform.lossyScale.y <= transform.lossyScale.y * 0.501
                                && Target_Robot.GetTargetedPosition().y < GetCenter().y)
                            {
                                lowerslash = true;
                            }
                        }

                        if(lowerBodyState == LowerBodyState.SLASH_DASH)
                            stepremain--;

                        if ( (slash || stepremain <= 0) && (subState_Slash != SubState_Slash.DashSlash || event_swing))
                        {
                            if (subState_Slash == SubState_Slash.GroundSlash || subState_Slash == SubState_Slash.QuickSlash)
                            {
                                if (lowerslash)
                                    subState_Slash = SubState_Slash.LowerSlash;
                            }

                            lowerBodyState = LowerBodyState.SLASH;
                            upperBodyState = UpperBodyState.SLASH;
                            intend_animator_speed = 1.0f;
                            event_slash = false;
                            event_acceptnextslash = false;
                            combo_reserved = false;
                            if (subState_Slash != SubState_Slash.DashSlash)
                                Sword.slashing = false;
                            slash_count = 0;

                            Sword.damage = Sword.slashMotionInfo[subState_Slash].damage[slash_count];
 
                            if (subState_Slash == SubState_Slash.AirSlash)
                                Sword.knockBackType = KnockBackType.Normal;
                            else if(subState_Slash == SubState_Slash.DashSlash)
                                Sword.knockBackType = KnockBackType.KnockUp;
                            else if (subState_Slash == SubState_Slash.AirSlashSeed || subState_Slash == SubState_Slash.SlideSlashSeed)
                                Sword.knockBackType = KnockBackType.Aerial;
                            else
                                Sword.knockBackType = slash_count < Sword.slashMotionInfo[subState_Slash].num - 1 ? KnockBackType.Normal : KnockBackType.Finish;

                            if(subState_Slash != SubState_Slash.DashSlash)
                                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[subState_Slash]._animID[slash_count], 0.0f, 0);

                            audioSource.PlayOneShot(audioClip_Swing);
        
                        }

                        if (robotParameter.itemFlag.HasFlag(ItemFlag.SeedOfArts))
                        {
                            GroundedCheck();

                            if (!Grounded)
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
                                    AcceptDash(true);
                            }
                            else
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
                                    AcceptStep(true);
                            }
                        }
                        else
                        {
                            if (dicSubStateSlashType[subState_Slash] == SubState_SlashType.AIR)
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
                                    AcceptDash(true);
                            }
                            else
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
                                    AcceptStep(true);
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



                        if (speed_overrideby_knockback) // リセットはLowerBodyMoveの末尾。（こことかでやる作りだと漏れたときのバグが怖い）
                        {

                        }
                        // accelerate or decelerate to target speed
                        else if (
                            //currentHorizontalSpeed < targetSpeed - speedOffset ||
                            currentHorizontalSpeed > targetSpeed + speedOffset
                            )
                        {
                            // creates curved result rather than a linear one giving a more organic speed change
                            // note T in Lerp is clamped, so we don't need to clamp our speed
                            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                                Time.deltaTime * robotParameter.SpeedChangeRate);

                            // round speed to 3 decimal places
                            _speed = Mathf.Round(_speed * 1000f) / 1000f;
                        }

                        if (event_knockbacked)
                        {
                            if(Grounded)
                                TransitLowerBodyState(LowerBodyState.STAND);
                            else
                                TransitLowerBodyState(LowerBodyState.AIR);

                        }



                        JumpAndGravity();
                        GroundedCheck();
                        regen_boost_now = true;
                    }
                    break;
                case LowerBodyState.SLASH:
                    {


                        _animationBlend = 0.0f;

                        _speed = 0.0f;
                    

                        if (Target_Robot != null)
                        {
                            if (Sword.dashslash_cutthrough && subState_Slash == SubState_Slash.DashSlash)
                            {

                                Vector3 targetOffset = dashslash_offset;

                                Vector3 targetPos = Target_Robot.GetTargetedPosition() + targetOffset.normalized * (Sword.motionProperty[SubState_Slash.DashSlash].SlashDistance * transform.lossyScale.x * 0.9f);

                                Vector3 targetDirection = (targetPos - GetCenter()).normalized;

                                //_targetRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

                                //float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, rotatespeed);

                                // rotate to face input direction relative to camera position
                                //transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);*/

                                //if ((Target_Robot.GetTargetPosition() - GetCenter()).magnitude > Sword.SlashDistance * transform.lossyScale.x)
                                {
                                    _speed = targetSpeed = Sword.motionProperty[SubState_Slash.DashSlash].DashSpeed;
                                }
                            }
                            else
                            {
                                if (!Sword.slashing || Sword.motionProperty[subState_Slash].AlwaysForward)
                                {
                                    if (subState_Slash == SubState_Slash.AirSlashSeed || subState_Slash == SubState_Slash.SlideSlashSeed)
                                    {
                                        Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                                        _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                                        float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, RotateSpeed_BeforeSlash);

                                        Quaternion look = Quaternion.LookRotation(target_dir, Vector3.up);

                                        float rotation_pitch = Mathf.Clamp(Mathf.DeltaAngle(0.0f, look.eulerAngles.x), -seedSlash_PitchLimit, seedSlash_PitchLimit);

                                        // rotate to face input direction relative to camera position
                                        transform.rotation = Quaternion.Euler(rotation_pitch, rotation, 0.0f);
                                    }
                                    else
                                    {
                                        Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                                        _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                                        float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, RotateSpeed_BeforeSlash);

                                        // rotate to face input direction relative to camera position
                                        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                                    }




                                    if ((Target_Robot.GetTargetedPosition() - GetCenter()).magnitude > Sword.motionProperty[subState_Slash].SlashDistance * transform.lossyScale.x)
                                    {
                                        //if (lowerBodyState == LowerBodyState.DashSlash)// !Sword.dashslash_cutthroughのとき
                                        {
                                            _speed = targetSpeed = robotParameter.InfightCorrectSpeed;
                                        }
                                        /*else
                                        {
                                            _speed = targetSpeed = robotParameter.MoveSpeed;
                                        }*/
                                    }
                                    //else if ((Target_Robot.GetTargetPosition() - GetCenter()).magnitude < Sword.motionProperty[motionProperty_key].SlashDistance_Min * transform.lossyScale.x)
                                    //{
                                    //_speed = targetSpeed = /*event_stepbegin ? */-robotParameter.SprintSpeed/* : 0.0f*/;
                                    //}
                                }
                            }
                        }
                        else
                        {
                            if (subState_Slash ==SubState_Slash.DashSlash)
                            {
                                _speed = targetSpeed = Sword.motionProperty[SubState_Slash.DashSlash].DashSpeed;
                            }
                        }

                        //targetSpeed = 0.0f;
                        //_speed = targetSpeed = /*event_stepbegin ? */robotParameter.MoveSpeed/* : 0.0f*/;


                        if (subState_Slash == SubState_Slash.DashSlash && Sword.dashslash_cutthrough)
                            boosting = true;

                        bool air_to_ground_chain = subState_Slash == SubState_Slash.AirSlash && Grounded;

                        //if (Sword.hitHistoryRCCount == 0)
                        {
                            if ( ((slash_dispatch && ringMenuDir == RingMenuDir.Center) || Sword.motionProperty[subState_Slash].ForceProceed)
                                && (slash_count < Sword.slashMotionInfo[subState_Slash].num - 1 || air_to_ground_chain))
                            {
                                combo_reserved = true;

                                if (robotParameter.itemFlag.HasFlag(ItemFlag.RollingSlash) &&  _input.move.y < 0.0f && subState_Slash != SubState_Slash.RollingSlash)
                                {
                                    comboType = ComboType.ROLLINGSLASH;
                                }
                                else if (robotParameter.itemFlag.HasFlag(ItemFlag.DashSlash) && _input.move.y > 0.0f && subState_Slash != SubState_Slash.RollingSlash)
                                {
                                    comboType = ComboType.DASHSLASH;
                                }
                                else
                                    comboType = ComboType.SLASH;
                            }

                            if (fire_dispatch && ringMenuDir == RingMenuDir.Center && robotParameter.itemFlag.HasFlag(ItemFlag.QuickDraw))
                            {
                                combo_reserved = true;
                                comboType = ComboType.SHOOT;
                            }
                        }

                    

                        bool combo_accepted = false;

                        if(combo_reserved)
                        {
                            combo_accepted = comboType != ComboType.SHOOT || Sword.hitHistoryRCCount > 0;
                        }

                        if (combo_accepted && (event_acceptnextslash || comboType == ComboType.SHOOT))
                            event_slash = true;

                        if (event_slash)
                        {
                            slash_count++;
                            if (!combo_accepted)
                            {
                                Sword.emitting = false;

                                if(dicSubStateSlashType[subState_Slash] == SubState_SlashType.AIR)
                                    TransitLowerBodyState(LowerBodyState.AIR);
                                else
                                    TransitLowerBodyState(LowerBodyState.STAND);
                            }
                            else
                            {
                                if (comboType == ComboType.SLASH || comboType == ComboType.ROLLINGSLASH || comboType == ComboType.DASHSLASH)
                                {
                                    if (air_to_ground_chain)
                                    {
                                        bool lowerslash = false;

                                        if (Target_Robot == null)
                                        {

                                        }
                                        else
                                        {
                                            if (Target_Robot.lowerBodyState == LowerBodyState.DOWN || Target_Robot.lowerBodyState == LowerBodyState.GETUP)
                                            {
                                                lowerslash = true;
                                            }

                                            if (Target_Robot.transform.lossyScale.y <= transform.lossyScale.y * 0.501
                                                && Target_Robot.GetTargetedPosition().y < GetCenter().y)
                                            {
                                                lowerslash = true;
                                            }
                                        }

                                        if (lowerslash)
                                        {
                                            subState_Slash = SubState_Slash.LowerSlash;
                                        }
                                        else
                                        {
                                            subState_Slash = SubState_Slash.QuickSlash;
                                        }
                                        slash_count = 0;
                                    }

                                    if (comboType == ComboType.ROLLINGSLASH && Sword.hitHistoryRCCount > 0)
                                    {
                                        subState_Slash = SubState_Slash.RollingSlash;
                                        slash_count = 0;
                                    }

                                    if (comboType == ComboType.DASHSLASH && Sword.hitHistoryRCCount > 0)
                                    {
                                        DoDashSlash(true);
                                    }
                                    else
                                    {
                                        event_slash = false;
                                        event_acceptnextslash = false;
                                        combo_reserved = false;
                                        Sword.slashing = false;
                                        _verticalVelocity = 0.0f;
          
                                        Sword.damage = Sword.slashMotionInfo[subState_Slash].damage[slash_count];

                                        if (subState_Slash == SubState_Slash.AirSlashSeed || subState_Slash == SubState_Slash.SlideSlashSeed)
                                            Sword.knockBackType = slash_count < Sword.slashMotionInfo[subState_Slash].num - 1 ? KnockBackType.Aerial : KnockBackType.Finish;
                                        else if (subState_Slash == SubState_Slash.DashSlash)
                                            Sword.knockBackType = KnockBackType.KnockUp;
                                        else if (subState_Slash == SubState_Slash.RollingSlash && robotParameter.itemFlag.HasFlag(ItemFlag.SeedOfArts))
                                            Sword.knockBackType = KnockBackType.Aerial;
                                        else
                                            Sword.knockBackType = slash_count < Sword.slashMotionInfo[subState_Slash].num - 1 ? KnockBackType.Normal : KnockBackType.Finish;

                                        _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[subState_Slash]._animID[slash_count], 0.0f, 0);
                                        audioSource.PlayOneShot(audioClip_Swing);
                                    }
                                }
                                else
                                {

                                    if (dicSubStateSlashType[subState_Slash] == SubState_SlashType.AIR)
                                        TransitLowerBodyState(LowerBodyState.AIR);
                                    else
                                        TransitLowerBodyState(LowerBodyState.STAND);

                                    DoMainFire(0.0f, true);

                                    Sword.emitting = false;
                                }
                            }
                        }


                        JumpAndGravity();
                        GroundedCheck();

                        if (robotParameter.itemFlag.HasFlag(ItemFlag.SeedOfArts))
                        {
                            if (!Grounded)
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
                                    AcceptDash(true);
                            }
                            else
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
                                    AcceptStep(true);
                            }
                        }
                        else
                        {
                            if (dicSubStateSlashType[subState_Slash] == SubState_SlashType.AIR)
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
                                    AcceptDash(true);
                            }
                            else
                            {
                                if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
                                    AcceptStep(true);
                            }
                        }

                        break;


                    }
                case LowerBodyState.JumpSlash:
                    {
                        //_speed = targetSpeed = 0.0f;

                        float dist_xz = float.MinValue;
                        //float dist_y = float.MaxValue;

                        if (Target_Robot != null)
                        {
                            Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                            _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                            float rotatespeed = Sword.motionProperty[SubState_Slash.JumpSlash_Jump].RotateSpeed;


                            float rotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, _targetRotation, rotatespeed);

                            // rotate to face input direction relative to camera position
                            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                            Vector3 sub_xz = (Target_Robot.GetTargetedPosition() - GetCenter());
                            //dist_y = sub_xz.y;

                            sub_xz.y = 0.0f;

                            dist_xz = sub_xz.magnitude;
                        }




                        if (dist_xz > Sword.motionProperty[SubState_Slash.JumpSlash_Jump].SlashDistance * transform.lossyScale.x)
                        {

                        }
                        else
                        {
                            jumpslash_end_forward = true;
                            _speed = targetSpeed = 0.0f;
                        }

                        if (!jumpslash_end_forward)
                        {
                            _speed = targetSpeed = Sword.motionProperty[SubState_Slash.JumpSlash_Jump].DashSpeed;
                        }
                        else
                            _speed = targetSpeed = Mathf.Max(_speed - robotParameter.SpeedChangeRate, 0.0f);


                        /*   if (dist_xz > -dist_y + Sword.motionProperty[LowerBodyState.JumpSlash_Jump].SlashDistance * transform.lossyScale.x)
                           {

                           }
                           else
                           {
                               //if (stepremain < 10)
                               //    stepremain = 0;


                           }*/

                        //if (robotParameter.itemFlag.HasFlag(ItemFlag.InfightBoost))
                        //{
                        //    _verticalVelocity = Mathf.Max(_verticalVelocity + robotParameter.Gravity * 1.5f * Time.deltaTime * 4, -Sword.motionProperty[LowerBodyState.JumpSlash_Jump].DashSpeed * 1.5f);
                        //}
                        //else
                        //{
                            _verticalVelocity = Mathf.Max(_verticalVelocity + robotParameter.Gravity * Time.deltaTime * 6, -InitialVerticalSpeed_JumpSlash/*float.MinValue*/);
                        //}

                        if (_verticalVelocity < 0.0f)
                        {
                            _animator.SetFloat("SlashSpeed", 0.5f);
                            //jumpslash_end_forward = true;
                            boosting = false;
                        }
                        else
                            boosting = true;



                        /* if (stepremain > 0)
                         {
                             stepremain--;
                             //_verticalVelocity = Sword.motionProperty[LowerBodyState.JumpSlash_Jump].DashSpeed;

                             //_verticalVelocity = robotParameter.AscendingVelocity * 2;
                         }
                         else*/
                        {



                            // _verticalVelocity = Mathf.Max(_verticalVelocity- robotParameter.SpeedChangeRate, -Sword.motionProperty[LowerBodyState.JumpSlash_Jump].DashSpeed);



                            if (event_slash/* && _verticalVelocity == -Sword.motionProperty[LowerBodyState.JumpSlash_Jump].DashSpeed*/)
                            {
                                
                                if (Grounded)
                                {
                                    Sword.slashing = false;
                                    TransitLowerBodyState(LowerBodyState.JumpSlash_Ground);
                                    upperBodyState = UpperBodyState.JumpSlash_Ground;
                                }
                            }
                            GroundedCheck();
                        }


                        //_animator.SetFloat(_animIDVerticalSpeed, _verticalVelocity);

                    }
                    break;
                case LowerBodyState.SWEEP:

                    _speed = targetSpeed = _verticalVelocity = 0.0f;

                    if (event_slash)
                    {
                        if(Grounded)
                            TransitLowerBodyState(LowerBodyState.STAND);
                        else
                            TransitLowerBodyState(LowerBodyState.AIR);
                    }
    
                    if (Grounded)
                    {
                        if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
                            AcceptStep(true);
                    }
                    else
                    {
                        if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
                            AcceptDash(true);
                    }

                    GroundedCheck();
                    break;
            }
        }

        if (lowerBodyState == LowerBodyState.VOIDSHIFT)
        {
            skinnedMeshRenderer.materials = material_in_voidshift;
            enqueueAfterimage_VoidShift.enabled = true;
        }
        else
        {
            skinnedMeshRenderer.materials = material_org;
            enqueueAfterimage_VoidShift.enabled = false;
            afterimageRenderer_VoidShift.Clear();
        }

        bool hitslow_now = false;
        if (hitslow_timer > 0)
        {
            hitslow_timer--;


            hitslow_now = true;
        }

        bool hitstop_now = false;
        if (hitstop_timer > 0)
        {
            hitstop_timer--;

            hitstop_now = true;
        }

        if (!hitslow_now && !hitstop_now)
        {

            if (lowerBodyState == LowerBodyState.STEP || lowerBodyState == LowerBodyState.ROLLINGFIRE || lowerBodyState == LowerBodyState.AIRROLLINGFIRE
            || lowerBodyState == LowerBodyState.ROLLINGHEAVYFIRE || lowerBodyState == LowerBodyState.AIRROLLINGHEAVYFIRE)
            {
                Vector3 targetDirection;

                float stepangle = 0.0f;

                float steptargetdegree;

                switch (stepDirection)
                {
                    case StepDirection.LEFT:
                        steptargetdegree = _cinemachineTargetYaw - 90.0f;
                        break;
                    case StepDirection.RIGHT:
                        steptargetdegree = _cinemachineTargetYaw + 90.0f;
                        break;
                    case StepDirection.BACKWARD:
                        steptargetdegree = _cinemachineTargetYaw + 180.0f;
                        break;
                    case StepDirection.FORWARD:
                        steptargetdegree = _cinemachineTargetYaw;
                        break;
                    case StepDirection.FORWARD_LEFT:
                        steptargetdegree = _cinemachineTargetYaw - 45.0f;
                        break;
                    case StepDirection.FORWARD_RIGHT:
                        steptargetdegree = _cinemachineTargetYaw + 45.0f;
                        break;
                    case StepDirection.BACKWARD_LEFT:
                        steptargetdegree = _cinemachineTargetYaw - 135.0f;
                        break;
                    //case StepDirection.BACKWARD_RIGHT:
                    default:
                        steptargetdegree = _cinemachineTargetYaw + 135.0f;
                        break;
                }

                switch (stepMotion)
                {
                    case StepMotion.LEFT:
                        stepangle = -90.0f;
                        steptargetrotation = steptargetdegree + 90.0f;
                        break;
                    case StepMotion.RIGHT:
                        stepangle = 90.0f;
                        steptargetrotation = steptargetdegree - 90.0f;
                        break;
                    case StepMotion.BACKWARD:
                        stepangle = -180.0f;
                        steptargetrotation = steptargetdegree + 180.0f;
                        break;
                    case StepMotion.FORWARD:
                        stepangle = 0.0f;
                        steptargetrotation = steptargetdegree;
                        break;
                }

                targetDirection = Quaternion.Euler(0.0f, transform.eulerAngles.y + stepangle, 0.0f) * Vector3.forward;

                transform.rotation = Quaternion.Euler(0.0f, Mathf.MoveTowardsAngle(transform.eulerAngles.y, steptargetrotation, ground_boost_now ? robotParameter.DashRotateSpeed : robotParameter.RotateSpeed), 0.0f);

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
            else if ( (lowerBodyState == LowerBodyState.SLASH_DASH || lowerBodyState == LowerBodyState.VOIDSHIFT) && (subState_Slash != SubState_Slash.DashSlash && dicSubStateSlashType[subState_Slash] == SubState_SlashType.AIR))
            {
                Vector3 targetDirection;


                if (Target_Robot != null)
                {
 
                    Vector3 targetOffset = (GetCenter() - Target_Robot.GetTargetedPosition());

                    

                    if (subState_Slash == SubState_Slash.AirSlashSeed || subState_Slash == SubState_Slash.SlideSlashSeed)
                    {
                        //Vector3 targetOffset_Horiz = targetOffset;
                        //targetOffset_Horiz.y = 0.0f;

                        //float rotation_pitch = Mathf.Asin(Vector3.Cross(targetOffset, targetOffset_Horiz).magnitude / targetOffset.magnitude / targetOffset_Horiz.magnitude) * Mathf.Rad2Deg;

                        Quaternion look = Quaternion.LookRotation(targetOffset, Vector3.up);

                        float rotation_pitch = Mathf.Clamp(Mathf.DeltaAngle(0.0f,look.eulerAngles.x), -seedSlash_PitchLimit, seedSlash_PitchLimit);

                        targetOffset = Quaternion.Euler(rotation_pitch, look.eulerAngles.y, look.eulerAngles.z) *Vector3.forward;
                    }
                    else
                        targetOffset.y = 0.0f;

                    Vector3 targetPos = Target_Robot.GetTargetedPosition() + targetOffset.normalized * (Sword.motionProperty[subState_Slash].SlashDistance * transform.lossyScale.x * 0.9f);

                    targetDirection = (targetPos - GetCenter()).normalized;

                    if (subState_Slash == SubState_Slash.DashSlash)
                        targetDirection.y = Mathf.Min(Mathf.Max(targetDirection.y, Mathf.Sin(-15.0f * Mathf.Deg2Rad)), Mathf.Sin(15.0f * Mathf.Deg2Rad));

                    if(subState_Slash == SubState_Slash.SlideSlashSeed && lowerBodyState != LowerBodyState.VOIDSHIFT)
                    {
                        Quaternion targetQ = Quaternion.LookRotation(targetDirection);
                        if (stepDirection == StepDirection.LEFT)
                        {
                            targetDirection = targetQ*Quaternion.Euler(0.0f, -30.0f, 0.0f)*Vector3.forward;
                        }
                        else
                            targetDirection = targetQ * Quaternion.Euler(0.0f, 30.0f, 0.0f) * Vector3.forward;
                    }

                    _verticalVelocity = targetDirection.y * _speed;

                    targetDirection.y = 0.0f;

                }
                else
                {
                    targetDirection = (transform.rotation * Vector3.forward).normalized;
                }




                // move the player
                MoveAccordingTerrain(targetDirection * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
            else if ( (lowerBodyState == LowerBodyState.SLASH_DASH || lowerBodyState == LowerBodyState.VOIDSHIFT) && subState_Slash == SubState_Slash.DashSlash) // Sword.dashslash_cutthrough有効時のDASHSLASH_DASH
            {
                Vector3 targetDirection;


                if (Target_Robot != null)
                {
                    Vector3 targetOffset = dashslash_offset;

                    Vector3 targetPos = Target_Robot.GetTargetedPosition() + targetOffset.normalized * (Sword.motionProperty[subState_Slash].SlashDistance * transform.lossyScale.x * 0.9f);

                    targetDirection = (targetPos - GetCenter()).normalized;

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
            else if (robotParameter.itemFlag.HasFlag(ItemFlag.Hovercraft) &&
                (lowerBodyState == LowerBodyState.STEPGROUND || lowerBodyState == LowerBodyState.SUBFIRE || lowerBodyState == LowerBodyState.FIRE
                || lowerBodyState == LowerBodyState.HEAVYFIRE))
            {

                float speedOffset = 0.1f;

                targetSpeed = 0.0f;

                //float brakefactor = 0.25f;
                float brakefactor = 0.10f;


                // a reference to the players current horizontal velocity
                float currentHorizontalSpeed_X = _controller.velocity.x;


                // accelerate or decelerate to target speed
                if (currentHorizontalSpeed_X < targetSpeed - speedOffset ||
                    currentHorizontalSpeed_X > targetSpeed + speedOffset)
                {
                    // creates curved result rather than a linear one giving a more organic speed change
                    // note T in Lerp is clamped, so we don't need to clamp our speed
                    currentHorizontalSpeed_X = Mathf.Lerp(currentHorizontalSpeed_X, 0.0f, Time.deltaTime * robotParameter.SpeedChangeRate * brakefactor);

                    // round speed to 3 decimal places
                    currentHorizontalSpeed_X = Mathf.Round(currentHorizontalSpeed_X * 1000f) / 1000f;
                }
                else
                {
                    currentHorizontalSpeed_X = targetSpeed;
                }

                // a reference to the players current horizontal velocity
                float currentHorizontalSpeed_Z = _controller.velocity.z;


                // accelerate or decelerate to target speed
                if (currentHorizontalSpeed_Z < targetSpeed - speedOffset ||
                    currentHorizontalSpeed_Z > targetSpeed + speedOffset)
                {
                    // creates curved result rather than a linear one giving a more organic speed change
                    // note T in Lerp is clamped, so we don't need to clamp our speed
                    currentHorizontalSpeed_Z = Mathf.Lerp(currentHorizontalSpeed_Z, 0.0f, Time.deltaTime * robotParameter.SpeedChangeRate * brakefactor);

                    // round speed to 3 decimal places
                    currentHorizontalSpeed_Z = Mathf.Round(currentHorizontalSpeed_Z * 1000f) / 1000f;
                }
                else
                {
                    currentHorizontalSpeed_Z = targetSpeed;
                }

                if (lowerBodyState == LowerBodyState.HEAVYFIRE || lowerBodyState == LowerBodyState.ROLLINGHEAVYFIRE)
                {
                    if (!backblast_processed)
                    {
                        Vector3 backBlastDir = -(rightWeapon.gameObject.transform.rotation * (Vector3.forward));

                        Vector3 backBlackDir_Horizontal = new Vector3(backBlastDir.x, 0.0f, backBlastDir.z);

                        currentHorizontalSpeed_X += 30.0f * backBlackDir_Horizontal.x;
                        currentHorizontalSpeed_Z += 30.0f * backBlackDir_Horizontal.z;

                        backblast_processed = true;
                    }
                }

                // move the player
                MoveAccordingTerrain(new Vector3(currentHorizontalSpeed_X, 0.0f, currentHorizontalSpeed_Z) * Time.deltaTime +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
            else
            {
                if (lowerBodyState == LowerBodyState.AIRHEAVYFIRE || lowerBodyState == LowerBodyState.HEAVYFIRE)
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
            }

            if(regen_boost_now)
            {
                if (boost_regen_time <= 0)
                {
                    RegenBoost();
                }
                else
                    boost_regen_time--;
            }
            else
            {
                boost_regen_time = robotParameter.BoostRegenDelay;
            }
        }

        /*if(burst)
        {
            if (!_input.sprint)
                burst = false;
        }
        else
        {
            if(_input.sprint && _input.move == Vector2.zero)
                burst = true;
        }*/

        foreach (var thruster in thrusters)
        {
            thruster.emitting = boosting;
        }

        if (boosting)
        {
            if (!prev_boosting)
            {
                audioSource_Boost.Play();
            }
        }
        else
        {
            if (prev_boosting)
            {
                audioSource_Boost.Stop();
            }
        }

        prev_boosting = boosting;

        if (mirage_time > 0)
        {
            mirage_time--;
        }
        else
        {
            StopMirage();
        }

        if (!hitslow_now && !hitstop_now)
            speed_overrideby_knockback = false;
    }


    // ここで扱わないもの
    // KNOCKBACKへの移行は複雑すぎるのでTakeDamageにべた書き
    private void TransitLowerBodyState(LowerBodyState newState)
    {
        if (lowerBodyState == LowerBodyState.DOWN && newState != LowerBodyState.DOWN)
            _controller.height = org_controller_height;

        if (
            (lowerBodyState == LowerBodyState.SLASH && newState != LowerBodyState.SLASH)
            || (lowerBodyState == LowerBodyState.SWEEP && newState != LowerBodyState.SWEEP)
            )
        {
            Sword.emitting = false;
        }

        if (lowerBodyState == LowerBodyState.KNOCKBACK)
        {
            intend_animator_speed = 1.0f;
        }

        if (newState != LowerBodyState.DASH)
        {
            nextdrive = false;
        }
            
        if(
            (lowerBodyState == LowerBodyState.SLASH || lowerBodyState == LowerBodyState.SLASH_DASH)
            && (newState != LowerBodyState.SLASH && newState != LowerBodyState.SLASH_DASH)
            && (subState_Slash == SubState_Slash.AirSlashSeed || subState_Slash == SubState_Slash.SlideSlashSeed)
            )
        {
            transform.rotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
        }

        switch (newState)
        {
            case LowerBodyState.AIR:

                if (lowerBodyState == LowerBodyState.DASH)
                    _animator.CrossFadeInFixedTime(_animIDAir, 0.25f, 0);
                else
                    _animator.CrossFadeInFixedTime(_animIDAir, 0.5f, 0);

                if (lowerBodyState == LowerBodyState.SLASH || lowerBodyState == LowerBodyState.SLASH_DASH || lowerBodyState == LowerBodyState.KNOCKBACK
                    || lowerBodyState == LowerBodyState.SWEEP)
                {
                    upperBodyState = UpperBodyState.STAND;
                }
             

                Grounded = false;
                break;
            case LowerBodyState.DOWN:
                if (lowerBodyState != LowerBodyState.DOWN)
                {
                    upperBodyState = UpperBodyState.DOWN;
                    _animator.Play(_animIDDown, 0, 0);
                    event_downed = false;
                    _controller.height = min_controller_height;
                }
                //_input.down = false;
                break;
            case LowerBodyState.GROUND:
            case LowerBodyState.GROUND_FIRE:
            case LowerBodyState.GROUND_SUBFIRE:
            case LowerBodyState.GROUND_HEAVYFIRE:
            case LowerBodyState.STEPGROUND:


                if (lowerBodyState == LowerBodyState.STEP)
                {
                    event_grounded = false;
                    animator.SetFloat("GroundSpeed", robotParameter.StepGroundSpeed);
                    _animator.CrossFadeInFixedTime(_animIDGround, 0.25f, 0, 0.15f);
                    audioSource.PlayOneShot(audioClip_Ground);
                }
                else
                {
                    if (newState == LowerBodyState.GROUND &&
                         (lowerBodyState == LowerBodyState.GROUND_FIRE || lowerBodyState == LowerBodyState.GROUND_SUBFIRE || lowerBodyState == LowerBodyState.GROUND_HEAVYFIRE)
                        )
                    {

                    }
                    else
                    {
                        animator.SetFloat("GroundSpeed", robotParameter.GroundSpeed);
                        _animator.Play(_animIDGround, 0, 0);
                        event_grounded = false;
                        audioSource.PlayOneShot(audioClip_Ground);
                    }
                }

                break;
            case LowerBodyState.JumpSlash_Ground:
                _animator.Play(Sword.slashMotionInfo[SubState_Slash.JumpSlash_Ground]._animID[0], 0, 0);
                event_grounded = false;
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
                    case LowerBodyState.GROUND_FIRE:
                    case LowerBodyState.GROUND_SUBFIRE:
                    case LowerBodyState.GROUND_HEAVYFIRE:
                    case LowerBodyState.STEPGROUND:
                        _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                        break;
                    case LowerBodyState.GETUP:
                        upperBodyState = UpperBodyState.STAND;
                        _animator.Play(_animIDStand, 0, 0);
                        _controller.height = org_controller_height;
                        _verticalVelocity = 0.0f;
                        break;
                    case LowerBodyState.KNOCKBACK:
                    case LowerBodyState.SLASH:
                    case LowerBodyState.JumpSlash_Ground:
                    case LowerBodyState.SWEEP:
                        upperBodyState = UpperBodyState.STAND;
                        _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);

                        if (lowerBodyState == LowerBodyState.JumpSlash_Ground)
                        {
                            _animator.CrossFadeInFixedTime(ChooseDualwieldStandMotion(), 0.5f, 2);
                        }

                        break;
                }
                break;
        }

        lowerBodyState = newState;
    }

    //AIから使う
    static public StepDirection determineStepDirection(Vector3 inputDirection)
    {
        float stepdirectiondegree = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;

        /* if (stepdirectiondegree >= 135.0f || stepdirectiondegree <= -135.0f)
             return StepDirection.BACKWARD;
         else if (stepdirectiondegree <= 45.0f && stepdirectiondegree >= -45.0f)
             return StepDirection.FORWARD;
         else if (stepdirectiondegree > 45.0f && stepdirectiondegree < 135.0f)
             return StepDirection.RIGHT;
         else// if (stepdirectiondegree > -135.0f && stepdirectiondegree < -45.0f)
             return StepDirection.LEFT;*/

        if (stepdirectiondegree >= 157.5f || stepdirectiondegree <= -157.5f)
            return StepDirection.BACKWARD;
        else if (stepdirectiondegree > -157.5f && stepdirectiondegree < -112.5f)
            return StepDirection.BACKWARD_LEFT;
        else if (stepdirectiondegree >= -112.5f && stepdirectiondegree <= -67.5f)
            return StepDirection.LEFT;
        else if (stepdirectiondegree > -67.5f && stepdirectiondegree < -22.5f)
            return StepDirection.FORWARD_LEFT;
        else if (stepdirectiondegree >= -22.5f && stepdirectiondegree <= 22.5f)
            return StepDirection.FORWARD;
        else if (stepdirectiondegree < 67.5f && stepdirectiondegree > 22.5f)
            return StepDirection.FORWARD_RIGHT;
        else if (stepdirectiondegree <= 112.5f && stepdirectiondegree >= 67.5f)
            return StepDirection.RIGHT;
        else //if (stepdirectiondegree < 157.5f && stepdirectiondegree > 112.5f)
            return StepDirection.BACKWARD_RIGHT;
    }

    float degreeFromStepDirection(StepDirection stepDirection)
    {
        switch (stepDirection)
        {
            case StepDirection.LEFT:
                return -90.0f;
            case StepDirection.BACKWARD:
                return 180.0f;
            case StepDirection.RIGHT:
                return 90.0f;
            case StepDirection.FORWARD:
                return 0.0f;
            case StepDirection.FORWARD_LEFT:
                return -45.0f;
            case StepDirection.BACKWARD_LEFT:
                return -135.0f;
            case StepDirection.FORWARD_RIGHT:
                return 45.0f;
            //case StepDirection.BACKWARD_RIGHT:
            default:
                return 135.0f;
        }
    }

    private void StartStep()
    {
        event_stepped = false;
        event_stepbegin = false;
        lowerBodyState = LowerBodyState.STEP;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        stepDirection = determineStepDirection(inputDirection);

        float steptargetdegree = degreeFromStepDirection(stepDirection) + _cinemachineTargetYaw;
        float stepmotiondegree = Mathf.Repeat(steptargetdegree - transform.eulerAngles.y + 180.0f, 360.0f) - 180.0f;



        if (stepmotiondegree <= 45.0f && stepmotiondegree >= -45.0f)
            stepMotion = StepMotion.FORWARD;
        else if (stepmotiondegree > -135.0f && stepmotiondegree < -45.0f)
            stepMotion = StepMotion.LEFT;
        else if (stepmotiondegree < 135.0f && stepmotiondegree > 45.0f)
            stepMotion = StepMotion.RIGHT;
        else// if (stepmotiondegree >= 135.0f || stepmotiondegree <= -135.0f)
            stepMotion = StepMotion.BACKWARD;



        switch (stepMotion)
        {
            case StepMotion.LEFT:
                _animator.CrossFadeInFixedTime(_animIDStep_Left, 0.25f, 0);
                steptargetrotation = steptargetdegree + 90.0f;
                break;
            case StepMotion.BACKWARD:
                _animator.CrossFadeInFixedTime(_animIDStep_Back, 0.25f, 0);
                steptargetrotation = steptargetdegree + 180.0f;
                break;
            case StepMotion.RIGHT:
                _animator.CrossFadeInFixedTime(_animIDStep_Right, 0.25f, 0);
                steptargetrotation = steptargetdegree - 90.0f;
                break;
            default:
                //case StepMotion.FORWARD:
                _animator.CrossFadeInFixedTime(_animIDStep_Front, 0.25f, 0);
                steptargetrotation = steptargetdegree;
                break;
        }

        stepremain = robotParameter.StepLimit;

        intend_animator_speed = 1.0f;

        if (Sword != null)
            Sword.emitting = false;

        // 射撃中だった場合に備えて
        if (dualwielding)
        {
            _animator.CrossFadeInFixedTime(ChooseDualwieldStandMotion(), 0.5f, 2);
        }

        StartMirage(10);
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
            _verticalVelocity = Mathf.Max(_verticalVelocity + robotParameter.Gravity * Time.deltaTime, -robotParameter.TerminalVelocity);
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
        audioSource.PlayOneShot(audioClip_Step);
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

    private void OnDropFromAir()
    {
        _verticalVelocity = -robotParameter.TerminalVelocity;
    }

    private void OnSwingBegin()
    {
        event_swing = true;
    }
    private void OnAcceptNextSlash()
    {
        event_acceptnextslash = true;
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

    private void OnFootStep(AnimationEvent animationEvent)
    {
        audioSource.PlayOneShot(audioClip_Walk);
    }




    public class Transform2
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        public Vector3 TransformPoint(Vector3 p)
        {
            Vector3 scaled = p;

            scaled.Scale(localScale);

            return rotation * scaled + position;
        }

        public Vector3 InverseTransformPoint(Vector3 p)
        {
            Vector3 sub = p - position;

            Vector3 rot = Quaternion.Inverse(rotation) * sub;

            return new Vector3(rot.x / localScale.x, rot.y / localScale.y, rot.z / localScale.z);
        }

        public Transform2(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;
        }

        public Transform2() { }
    }

    static public Vector3 GetCenterFromTransform(Transform2 _transform)
    {
        return _transform.TransformPoint(new Vector3(0, 3.805078f, 0));
    }

    public Vector3 GetCenter()
    {
        return GetCenterFromTransform(new Transform2(transform));
    }

    public Vector3 GetTargetedPosition()
    {
        return virtual_targeted_position;
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

    void OnAnimFire()
    {
        event_fired = true;
    }

    void OnHeavyFire()
    {
        event_heavyfired = true;
        backblast_processed = false;
    }

    void DoMainFire(float angle, bool quick)
    {
        if (rightWeapon.heavy)
        {
            animator.Play("HeavyFire", 2, 0.0f); //2重にしてるのは、モーション中に着地した場合に備えて（レイヤー0だけだとバグる）

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
            _animator.CrossFadeInFixedTime(_animIDHeavyFire, 0.25f, 0);
            intend_animator_speed = 1.0f;
            backblast_processed = true;

        }
        else
        {
            upperBodyState = UpperBodyState.FIRE;


            if (dualwielding)
                animator.Play(gatling ? "Fire5" : (carrying_weapon ? "Fire3" : "Fire2"), 2, 0.0f);
            else
            {
                if(rightWeapon.wrist_equipped)
                    animator.Play("Fire4", 1, 0.0f);
                else
                    animator.Play("Fire", 1, 0.0f);
            }



            if (angle > rightWeapon.firing_angle)
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
                intend_animator_speed = 1.0f;
            }

            event_fired = false;
        }


        if (quick)
            firing_multiplier = 3.0f;
        else
        {
            if (rightWeapon != null)
                firing_multiplier = rightWeapon.firing_multiplier;
            else
                firing_multiplier = 1.0f;

            if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickDraw))
            {
                firing_multiplier *= 1.5f;
            }
        }

        animator.SetFloat("FiringSpeed", firing_multiplier);
        StartSeeking(firing_multiplier);
        fire_done = false;
        rollingfire_followthrough = false;
        quickdraw_followthrough = quick;
        rightWeapon.ResetCycle();
        fire_dispatch_triggerhold = fire_dispatch_triggerhold_max;
    }

    bool AcceptMainFire(float angle)
    {
        if (rightWeapon != null)
        {
            if (fire_dispatch && ringMenuDir == RingMenuDir.Center)
            {
                DoMainFire(angle, false);
                return true;
            }
        }
        return false;
    }
    void AcceptSubFire()
    {
        if (shoulderWeapon != null)
        {
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
                shoulderWeapon.ResetCycle();
                _animator.CrossFadeInFixedTime(_animIDSubFire, 0.25f, 0);
                intend_animator_speed = 1.0f;

                animator.Play("SubFire", 2, 0.0f);

                StartSeeking();
            }
        }
    }
    void AcceptDash(bool canceling)
    {
        if (upperBodyState != UpperBodyState.STAND)
        {
            if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive) && !prev_sprint)
            {
                canceling = true;
            }
            else
                return;
        }

        if ((_input.sprint || (_input.sprint_once && !sprint_once_consumed)) && (!canceling || ConsumeBoost(80)) )
        {
            if (ConsumeBoost(4))
            {
                nextdrive = canceling;

                if (nextdrive)
                {
                    stepremain = 30;
                    nextdrive_free_boost = 30;
                }
                else
                {
                    stepremain = 0;
                    nextdrive_free_boost = 0;
                }

                sprint_once_consumed = true;

                if (robotParameter.itemFlag.HasFlag(ItemFlag.NextDrive))
                {
                    upperBodyState = UpperBodyState.STAND;
                    fire_followthrough = 0;
                }

                // normalise input direction
                Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

                // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // if there is a move input rotate player when the player is moving
                if (_input.move != Vector2.zero)
                {
                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                        _cinemachineTargetYaw;
                }

                float degree = Mathf.DeltaAngle(transform.eulerAngles.y, _targetRotation);

                if (degree < robotParameter.RotateSpeed && degree > -robotParameter.RotateSpeed)
                {
                    lowerBodyState = LowerBodyState.DASH;
                    _animator.CrossFadeInFixedTime(_animIDDash, 0.25f, 0);
                    event_dashed = false;

                    if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickIgniter))
                    {
                        _speed = robotParameter.AirDashSpeed * 2;
                    }
                    if (robotParameter.itemFlag.HasFlag(ItemFlag.AeroMirage))
                    {
                        StartMirage(10);
                    }
                }
                else
                {
                    lowerBodyState = LowerBodyState.AIRROTATE;
                }

                intend_animator_speed = 1.0f;

                if (Sword != null)
                    Sword.emitting = false;

                // 射撃中だった場合に備えて
                if (dualwielding)
                {
                    _animator.CrossFadeInFixedTime(ChooseDualwieldStandMotion(), 0.5f, 2);
                }
            }
        }
    }

    bool sprint_once_consumed = false;
    void AcceptStep(bool canceling)
    {
        if (upperBodyState != UpperBodyState.STAND)
        {
            if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide) && !prev_sprint)
            {
                canceling = true;
            }
            else
                return;
        }

        if (
            (_input.sprint || (_input.sprint_once && !sprint_once_consumed)) &&  (!canceling || ConsumeBoost(80))
            ) 
        {
            sprint_once_consumed = true;

            if (robotParameter.itemFlag.HasFlag(ItemFlag.ExtremeSlide))
            {
                upperBodyState = UpperBodyState.STAND;
                fire_followthrough = 0;
            }

            StartStep();

            if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickIgniter))
            {
                _speed = robotParameter.StepSpeed * 2;
            }
            else
                _speed = robotParameter.StepSpeed;
        }
    }

    void DoDashSlash(bool combo)
    {
        lowerBodyState = LowerBodyState.SLASH_DASH;
        upperBodyState = UpperBodyState.SLASH_DASH;
        subState_Slash = SubState_Slash.DashSlash;
        event_stepbegin = event_stepped = false;
        _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[subState_Slash]._animID[0], 0.0f, 0);

        if(combo)
            intend_animator_speed = 1.0f;
        else
            intend_animator_speed = 0.5f;

        stepremain = Sword.motionProperty[subState_Slash].DashDuration;
        combo_reserved = false;
        Sword.slashing = false;
        Sword.emitting = true;
        Sword.sweep = false;
        event_swing = false;

        // DashSlashに遷移する前にslashingがtrueになることがあるので
        Sword.damage = Sword.slashMotionInfo[SubState_Slash.DashSlash].damage[0];
        Sword.knockBackType = KnockBackType.KnockUp;

        StartSeeking();
        if (Target_Robot != null)
        {
            dashslash_offset = (GetCenter() - Target_Robot.GetTargetedPosition());

            dashslash_offset.y = 0.0f;

            dashslash_offset = Quaternion.AngleAxis(45, new Vector3(0.0f, 1.0f, 0.0f)) * dashslash_offset;

            Vector3 targetPos = Target_Robot.GetTargetedPosition() + dashslash_offset.normalized * (Sword.motionProperty[SubState_Slash.DashSlash].SlashDistance * transform.lossyScale.x * 0.75f);

            Vector3 target_dir = targetPos - GetCenter();

            _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

            float rotation = _targetRotation;

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
    }

    bool AcceptDashSlash()
    {
        if (Sword != null && robotParameter.itemFlag.HasFlag(ItemFlag.DashSlash) && Sword.can_dash_slash)
        {
            ringMenu_Up_RMB_available = true;

            if (slash_dispatch && ringMenuDir == RingMenuDir.Up && ConsumeBoost(80))
            {
                DoDashSlash(false);

                return true;
            }
        }

        return false;
    }

    bool AcceptJumpSlash()
    {
        if (Sword != null && robotParameter.itemFlag.HasFlag(ItemFlag.JumpSlash) && Sword.can_jump_slash)
        {
            ringMenu_Down_RMB_available = true;

            if (slash_dispatch && ringMenuDir == RingMenuDir.Down && ConsumeBoost(80))
            {
                if (Target_Robot != null)
                {
                    Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                    _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                    float rotation = _targetRotation;

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                event_jumped = false;
                lowerBodyState = LowerBodyState.JumpSlash_Jump;
                upperBodyState = UpperBodyState.JumpSlash_Jump;
                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[SubState_Slash.JumpSlash_Jump]._animID[0], 0.0f, 0);
                combo_reserved = false;
                jumpslash_end_forward = false;
                Sword.emitting = true;
                Sword.sweep = false;
                StartSeeking();

                intend_animator_speed = 0.75f;
                return true;
            }
        }

        return false;
    }

    bool AcceptSweep()
    {
        if (Sword != null && robotParameter.itemFlag.HasFlag(ItemFlag.HorizonSweep))
        {
            ringMenu_Left_RMB_available = true;
            ringMenu_Right_RMB_available = true;

            if (slash_dispatch && (ringMenuDir == RingMenuDir.Left || ringMenuDir == RingMenuDir.Right))
            {
                if (Target_Robot != null)
                {
                    Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                    _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                    float rotation = _targetRotation;
  
                    Quaternion look = Quaternion.LookRotation(target_dir, Vector3.up);

                    float rotation_pitch = Mathf.Clamp(Mathf.DeltaAngle(0.0f, look.eulerAngles.x), -seedSlash_PitchLimit, seedSlash_PitchLimit);

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(rotation_pitch, rotation, 0.0f);
                }

                event_slash = false;
                lowerBodyState = LowerBodyState.SWEEP;
                upperBodyState = UpperBodyState.SWEEP;

                if (ringMenuDir == RingMenuDir.Left)
                {
                    _animator.CrossFadeInFixedTime(_animIDSweep_Left, 0.0f, 0);
                }
                else
                {
                    _animator.CrossFadeInFixedTime(_animIDSweep_Right, 0.0f, 0);
                }

                Sword.emitting = true;
                Sword.sweep = true;
                Sword.damage = 100;

                if(robotParameter.itemFlag.HasFlag(ItemFlag.SeedOfArts))
                    Sword.knockBackType = KnockBackType.Aerial;
                else
                    Sword.knockBackType = KnockBackType.Normal;
                
                StartSeeking();

                intend_animator_speed = 1.0f;
                return true;
            }
        }

        return false;
    }

    bool AcceptRollingShoot()
    {
        bool start = false;

        if (rightWeapon != null)
        {
            if (robotParameter.itemFlag.HasFlag(ItemFlag.RollingShoot))
            {
                ringMenu_Left_LMB_available = true;
                ringMenu_Right_LMB_available = true;

                if (fire_dispatch && (ringMenuDir == RingMenuDir.Left || ringMenuDir == RingMenuDir.Right) && ConsumeBoost(40))
                {
                    if (rightWeapon.heavy)
                    {
                        if (!Grounded)
                        {
                            lowerBodyState = LowerBodyState.AIRROLLINGHEAVYFIRE;
                        }
                        else
                        {
                            lowerBodyState = LowerBodyState.ROLLINGHEAVYFIRE;
                        }

                        upperBodyState = UpperBodyState.ROLLINGHEAVYFIRE;

                        if (ringMenuDir == RingMenuDir.Left)
                        {
                            stepMotion = StepMotion.LEFT;

                            animator.Play(_animIDRollingFire3_Left, 0, 0.0f);
                        }
                        else
                        {
                            stepMotion = StepMotion.RIGHT;

                            animator.Play(_animIDRollingFire3_Right, 0, 0.0f);
                        }

                        //animator.Play("HeavyFire", 2, 0.0f);
                        intend_animator_speed = 1.0f;

                        if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickDraw))
                            animator.SetFloat("FiringSpeed", 1.5f);
                        else
                            animator.SetFloat("FiringSpeed", 1.0f);

                        StartSeeking();
                        event_heavyfired = false;
                        rightWeapon.ResetCycle();
                        fire_dispatch_triggerhold = fire_dispatch_triggerhold_max;
                        fire_done = false;
                        backblast_processed = true;
                        start = true;
                    }
                    else
                    {

                        if (!Grounded)
                        {
                            lowerBodyState = LowerBodyState.AIRROLLINGFIRE;
                        }
                        else
                        {
                            lowerBodyState = LowerBodyState.ROLLINGFIRE;
                        }

                        upperBodyState = UpperBodyState.ROLLINGFIRE;

                        if (ringMenuDir == RingMenuDir.Left)
                        {
                            stepMotion = StepMotion.LEFT;

                            if (carrying_weapon)
                            {
                                animator.Play(_animIDRollingFire2_Left, 0, 0.0f);
                                animator.Play("Fire3", 2, 0.0f);
                            }
                            else if (gatling)
                            {
                                animator.Play(_animIDRollingFire4_Left, 0, 0.0f);
                                animator.Play("Fire5", 2, 0.0f);
                            }
                            else
                                animator.Play(_animIDRollingFire_Left, 0, 0.0f);
                        }
                        else
                        {
                            stepMotion = StepMotion.RIGHT;

                            if (carrying_weapon)
                            {
                                animator.Play(_animIDRollingFire2_Right, 0, 0.0f);
                                animator.Play("Fire3", 2, 0.0f);
                            }
                            else if (gatling)
                            {
                                animator.Play(_animIDRollingFire4_Right, 0, 0.0f);
                                animator.Play("Fire5", 2, 0.0f);
                            }
                            else
                                animator.Play(_animIDRollingFire_Right, 0, 0.0f);
                        }

                        //_animator.CrossFadeInFixedTime(_animIDRollingFire_Left, 0.25f, 0);
                        intend_animator_speed = 1.0f;

                        if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickDraw))
                            animator.SetFloat("FiringSpeed", 1.5f);
                        else
                            animator.SetFloat("FiringSpeed", 1.0f);

                        StartSeeking();
                        event_fired = false;
                        fire_done = false;
                        rightWeapon.ResetCycle();
                        fire_dispatch_triggerhold = fire_dispatch_triggerhold_max;

                        start = true;
                    }
                }
            }
        }

        return start;
    }

    bool AcceptSnipeShoot()
    {
        bool start = false;

        if (rightWeapon != null)
        {
            if (robotParameter.itemFlag.HasFlag(ItemFlag.SnipeShoot))
            {
                ringMenu_Down_LMB_available = true;

                if (fire_dispatch && (ringMenuDir == RingMenuDir.Down))
                {
                    if (rightWeapon.heavy)
                    {
                        if (!Grounded)
                        {
                            lowerBodyState = LowerBodyState.AIRSNIPEHEAVYFIRE;
                        }
                        else
                        {
                            lowerBodyState = LowerBodyState.SNIPEHEAVYFIRE;
                        }

                        upperBodyState = UpperBodyState.SNIPEHEAVYFIRE;

                        animator.Play(_animIDSnipeFire3, 0, 0.0f);

                        //animator.Play("HeavyFire", 2, 0.0f);
                        intend_animator_speed = 1.0f;

                        if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickDraw))
                            animator.SetFloat("FiringSpeed", 1.5f);
                        else
                            animator.SetFloat("FiringSpeed", 1.0f);

                        StartSeeking();
                        event_heavyfired = false;
                        rightWeapon.ResetCycle();
                        fire_dispatch_triggerhold = fire_dispatch_triggerhold_max;
                        fire_done = false;
                        backblast_processed = true;
                        start = true;
                    }
                    else
                    {

                        if (!Grounded)
                        {
                            lowerBodyState = LowerBodyState.AIRSNIPEFIRE;
                        }
                        else
                        {
                            lowerBodyState = LowerBodyState.SNIPEFIRE;
                        }

                        upperBodyState = UpperBodyState.SNIPEFIRE;

                        if (carrying_weapon)
                        {
                            animator.Play(_animIDSnipeFire2, 0, 0.0f);
                            animator.Play("Fire3", 2, 0.0f);
                        }
                        else if (gatling)
                        {
                            animator.Play(_animIDSnipeFire4, 0, 0.0f);
                            animator.Play("Fire5", 2, 0.0f);
                        }
                        else
                            animator.Play(_animIDSnipeFire, 0, 0.0f);

                        //_animator.CrossFadeInFixedTime(_animIDRollingFire_Left, 0.25f, 0);
                        intend_animator_speed = 1.0f;

                        if (robotParameter.itemFlag.HasFlag(ItemFlag.QuickDraw))
                            animator.SetFloat("FiringSpeed", 1.5f);
                        else
                            animator.SetFloat("FiringSpeed", 1.0f);

                        StartSeeking();
                        event_fired = false;
                        fire_done = false;
                        rightWeapon.ResetCycle();
                        fire_dispatch_triggerhold = fire_dispatch_triggerhold_max;
                        start = true;
                    }

                    if (Sword != null)
                        Sword.emitting = false;
                }
            }
        }

        return start;
    }

    void AcceptSlash()
    {
        if (slash_dispatch && ringMenuDir == RingMenuDir.Center)
        {
            bool aerial_slash = false;
            float dist = 0.0f;
            if (Target_Robot != null)
            {
                if (robotParameter.itemFlag.HasFlag(ItemFlag.IaiSlash) && !Target_Robot.Grounded && !(Target_Robot.lowerBodyState == LowerBodyState.DOWN || Target_Robot.lowerBodyState == LowerBodyState.GETUP))
                    aerial_slash = true;

                dist = Vector3.Distance(Target_Robot.GetCenter(), GetCenter());
            }

            bool voidshift = false;

            if (robotParameter.itemFlag.HasFlag(ItemFlag.VoidShift) && dist >= voidshift_limit_distance)
            {
                voidshift = true;
            }

            if (robotParameter.itemFlag.HasFlag(ItemFlag.SeedOfArts))
            {
                if (voidshift)
                {
                    lowerBodyState = LowerBodyState.VOIDSHIFT;
                    upperBodyState = UpperBodyState.VOIDSHIFT;
                }
                else
                {
                    lowerBodyState = LowerBodyState.SLASH_DASH;
                    upperBodyState = UpperBodyState.SLASH_DASH;
                }

                if (_input.move.x == 0.0f)
                    subState_Slash = SubState_Slash.AirSlashSeed;
                else
                {
                    subState_Slash = SubState_Slash.SlideSlashSeed;

                    if (_input.move.x > 0.0f)
                        stepDirection = StepDirection.RIGHT;
                    else
                        stepDirection = StepDirection.LEFT;
                }

                event_stepbegin = event_stepped = false;
                _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[subState_Slash]._animID[0], 0.0f, 0);
                intend_animator_speed = 0.0f;
                stepremain = Sword.motionProperty[subState_Slash].DashDuration;
                combo_reserved = false;
                Sword.emitting = true;
                Sword.sweep = false;
                StartSeeking();
                if (Target_Robot != null)
                {
                    Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                    _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                    float rotation = _targetRotation;

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
                Grounded = false;
            }
            else
            {
                if (Grounded && !aerial_slash && !voidshift)
                {

                    if (Target_Robot != null)
                    {
                        Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                        _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                        float rotation = _targetRotation;

                        // rotate to face input direction relative to camera position
                        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    }

                    if (lowerBodyState == LowerBodyState.STAND || lowerBodyState == LowerBodyState.WALK)
                    {
                        if (voidshift)
                        {
                            lowerBodyState = LowerBodyState.VOIDSHIFT;
                            upperBodyState = UpperBodyState.VOIDSHIFT;
                        }
                        else
                        {
                            lowerBodyState = LowerBodyState.SLASH_DASH;
                            upperBodyState = UpperBodyState.SLASH_DASH;
                        }
                        subState_Slash = SubState_Slash.GroundSlash;
                        event_stepbegin = event_stepped = false;
                        _animator.CrossFadeInFixedTime(_animIDStep_Front, 0.0f, 0);
                        stepremain = Sword.motionProperty[subState_Slash].DashDuration;
                        combo_reserved = false;
                        Sword.emitting = true;
                        Sword.sweep = false;
                        StartSeeking();
                    }
                    else
                    {
                        if (voidshift)
                        {
                            lowerBodyState = LowerBodyState.VOIDSHIFT;
                            upperBodyState = UpperBodyState.VOIDSHIFT;
                        }
                        else
                        {
                            lowerBodyState = LowerBodyState.SLASH_DASH;
                            upperBodyState = UpperBodyState.SLASH_DASH;
                        }
                        subState_Slash = SubState_Slash.QuickSlash;
                        event_stepbegin = event_stepped = false;
                        _animator.CrossFadeInFixedTime(_animIDStep_Front, 0.0f, 0);
                        stepremain = Sword.motionProperty[subState_Slash].DashDuration;
                        combo_reserved = false;
                        Sword.emitting = true;
                        Sword.sweep = false;
                        StartSeeking();
                    }
                    intend_animator_speed = 1.0f;
                }
                else
                {
                    if (voidshift)
                    {
                        lowerBodyState = LowerBodyState.VOIDSHIFT;
                        upperBodyState = UpperBodyState.VOIDSHIFT;
                    }
                    else
                    {
                        lowerBodyState = LowerBodyState.SLASH_DASH;
                        upperBodyState = UpperBodyState.SLASH_DASH;
                    }
                    subState_Slash = SubState_Slash.AirSlash;
                    event_stepbegin = event_stepped = false;
                    _animator.CrossFadeInFixedTime(Sword.slashMotionInfo[subState_Slash]._animID[0], 0.0f, 0);
                    intend_animator_speed = 0.0f;
                    stepremain = Sword.motionProperty[subState_Slash].DashDuration;
                    combo_reserved = false;
                    Sword.emitting = true;
                    Sword.sweep = false;
                    StartSeeking();
                    if (Target_Robot != null)
                    {
                        Vector3 target_dir = Target_Robot.GetTargetedPosition() - GetCenter();

                        _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                        float rotation = _targetRotation;

                        // rotate to face input direction relative to camera position
                        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    }
                    Grounded = false;
                }
            }
        }
    }

    public override void OnPause()
    {
        paused = true;
    }

    public override void OnUnpause()
    {
        paused = false;
    }

    private void OnDestroy()
    {
        WorldManager.current_instance.pausables.Remove(this);
    }
    public void DoHitStop(int time)
    {
        if (hitstop_timer == 0)
        {
        }

        hitstop_timer = time;
    }
    public void DoHitSlow(int time)
    {
        if (hitslow_timer == 0)
        {
        }

        hitslow_timer = time;
    }
    [System.Serializable]
    public class RobotParameter
    {
        public GameObject rweapon_prefab = null;
        public GameObject lweapon_prefab = null;
        public GameObject subweapon_prefab = null;
        public bool dualwield_lightweapon = false;
        public ItemFlag itemFlag = 0;
        //public ItemFlag itemFlag = ItemFlag.NextDrive | ItemFlag.ExtremeSlide | ItemFlag.GroundBoost | ItemFlag.VerticalVernier | ItemFlag.QuickIgniter;

        public int Cost = 100;
        public int MaxHP = 500;
        public int Boost_Max = 200;
        public int BoostRegenDelay = 15;

        public float MoveSpeed = 2.0f;
        public float StepSpeed = 5.335f;
        public float AirDashSpeed = 5.335f;
        public int StepLimit = 30;
        public float GroundSpeed = 1.0f;
        public float StepGroundSpeed = 1.0f;
        public float JumpSpeed = 1.0f;

        public float RotateSpeed = 0.2f;
        public float DashRotateSpeed = 0.05f;
        public float AirDashRotateSpeed = 0.05f;
        public float AirMoveSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;
        public float Gravity = -15.0f;
        public float TerminalVelocity = 53.0f;
        public float AscendingVelocity = 20.0f;
        public float AscendingAccelerate = 1.8f;
        public float KnockbackSpeed = 30.0f;
        public float InfightCorrectSpeed = 30.0f;
    }

    public RobotParameter robotParameter;

    void ArmWeapon()
    {
        if (robotParameter.rweapon_prefab != null)
        {
            GameObject playerrweapon_r = GameObject.Instantiate(robotParameter.rweapon_prefab);

            rightWeapon = playerrweapon_r.GetComponent<Weapon>();

            playerrweapon_r.transform.parent = RHand.transform;
            playerrweapon_r.transform.localScale = new Vector3(1, 1, 1);

            if (rightWeapon.wrist_equipped)
            {
                playerrweapon_r.transform.localPosition = new Vector3(-0.00892f, -0.00069f, 0.00044f);
                playerrweapon_r.transform.localEulerAngles = new Vector3(-90, 0, -90);
            }
            else
            {
                playerrweapon_r.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
                playerrweapon_r.transform.localEulerAngles = new Vector3(-90, 0, 180);
            }

            rightWeapon = playerrweapon_r.GetComponent<Weapon>();
        }

        if (robotParameter.lweapon_prefab != null)
        {
            GameObject playerlweapon_l = GameObject.Instantiate(robotParameter.lweapon_prefab);

            playerlweapon_l.transform.parent = LHand.transform;
            playerlweapon_l.transform.localPosition = new Vector3(0.0004f, 0.0072f, 0.004f);
            playerlweapon_l.transform.localEulerAngles = new Vector3(-90, 0, 180);
            playerlweapon_l.transform.localScale = new Vector3(1, 1, 1);

            if (playerlweapon_l.GetComponent<InfightWeapon>().paired)
            {
                GameObject playerlweapon_r = GameObject.Instantiate(robotParameter.lweapon_prefab);

                playerlweapon_r.transform.parent = RHand.transform;
                playerlweapon_r.transform.localPosition = new Vector3(-0.0004f, 0.0072f, 0.004f);
                playerlweapon_r.transform.localEulerAngles = new Vector3(-90, 0, 180);
                playerlweapon_r.transform.localScale = new Vector3(1, 1, 1);

                playerlweapon_l.GetComponent<InfightWeapon>().this_is_slave = false;
                playerlweapon_l.GetComponent<InfightWeapon>().another = playerlweapon_r.GetComponent<InfightWeapon>();

                playerlweapon_r.GetComponent<InfightWeapon>().this_is_slave = true;
                playerlweapon_r.GetComponent<InfightWeapon>().another = playerlweapon_l.GetComponent<InfightWeapon>();
            }
    
            Sword = playerlweapon_l.GetComponent<InfightWeapon>();
        }

        if (robotParameter.subweapon_prefab != null)
        {
            GameObject playersubweapon_l = GameObject.Instantiate(robotParameter.subweapon_prefab);

            playersubweapon_l.transform.parent = chestWeapon_anchor[0].transform;
            playersubweapon_l.transform.localPosition = Vector3.zero;
            playersubweapon_l.transform.localEulerAngles = Vector3.zero;
            playersubweapon_l.transform.localScale = Vector3.one;

            if (playersubweapon_l.GetComponent<Weapon>().chest_paired)
            {
                GameObject playersubweapon_r = GameObject.Instantiate(robotParameter.subweapon_prefab);

                playersubweapon_r.transform.parent = chestWeapon_anchor[1].transform;
                playersubweapon_r.transform.localPosition = Vector3.zero;
                playersubweapon_r.transform.localEulerAngles = Vector3.zero;
                playersubweapon_r.transform.localScale = Vector3.one;

                playersubweapon_r.GetComponent<Weapon>().this_is_slave = true;

                playersubweapon_l.GetComponent<Weapon>().this_is_slave = false;
                playersubweapon_l.GetComponent<Weapon>().another = playersubweapon_r.GetComponent<Weapon>();

                
            }
  
            shoulderWeapon = playersubweapon_l.GetComponent<Weapon>();

        }
    }

    void StartSeeking(float multiplier = 1.0f)
    {
        if (lockonState != LockonState.LOCKON && Target_Robot != null)
        {
            lockonState = LockonState.SEEKING;
#if ACCURATE_SEEK        
            initial_lockon_angle = Quaternion.Angle(cameraRotation, GetTargetQuaternionForView(Target_Robot));
            seek_time = (int)(seek_time_max / multiplier);
#endif        
        }
    }

    void StartMirage(int baseduration)
    {
        if (robotParameter.itemFlag.HasFlag(ItemFlag.CounterMeasure))
            mirage_time = baseduration * 2;
        else
            mirage_time = baseduration;

        //if (team.affected_by_sensorarray > 0)
        //    mirage_time /= 2;

        enqueueAfterimage.enabled = true;
        afterimageRenderer.Clear();
        virtual_targeted_position = GetCenter();
    }

    void StopMirage()
    {
        mirage_time = 0;
        enqueueAfterimage.enabled = false;
        afterimageRenderer.Clear();
        virtual_targeted_position = GetCenter();
    }
}
