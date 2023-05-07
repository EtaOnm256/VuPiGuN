using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

using UnityEngine.Animations.Rigging;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

using StarterAssets;

[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class RobotController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float AirMoveSpeed = 1.0f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

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

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;
    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    public MultiAimConstraint headmultiAimConstraint;

    //public MultiAimConstraint rarmmultiAimConstraint;
    public OverrideTransform overrideTransform;
    public GameObject aimingBase;
    public GameObject shoulder_hint;

    public GameObject target;

    private float _headaimwait = 0.0f;

    private float _rarmaimwait = 0.0f;

    public Animator animator;

    //public bool firing = false;

    bool event_grounded = false;
    bool event_jumped = false;
    bool event_stepped = false;
    bool event_stepbegin = false;
    public enum UpperBodyState
    {
        STAND,
        FIRE
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
        STEP
    }

    public enum StepDirection
    {
        FORWARD,LEFT,BACKWARD,RIGHT
    }

    StepDirection stepDirection;

    public UpperBodyState upperBodyState = RobotController.UpperBodyState.STAND;
    public LowerBodyState lowerBodyState = RobotController.LowerBodyState.STAND;

    float steptargetrotation;
    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }


    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }        
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

       
        UpperBodyMove();
        LowerBodyMove();
  
    }

    private void LateUpdate()
    {
        CameraRotation();
        CinemachineCameraTarget.transform.position = transform.position + CinemachineCameraTarget.transform.rotation * offset;
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
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        switch(lowerBodyState)
        {
            case LowerBodyState.AIR:
            case LowerBodyState.AIRFIRE:
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
                    lowerBodyState = LowerBodyState.AIR;
                    _animator.CrossFadeInFixedTime(_animIDAir, 0.5f, 0);
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
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

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

        float angle = Vector3.Angle(target.transform.position - transform.position, transform.forward);

        if (angle > 60)
        {
            _headaimwait = Mathf.Max(0.0f, _headaimwait - 0.05f);
        }
        else
            _headaimwait = Mathf.Min(1.0f, _headaimwait + 0.05f);

        headmultiAimConstraint.weight = _headaimwait;

        switch(upperBodyState)
        {
            case UpperBodyState.FIRE:
                {
                    _rarmaimwait = Mathf.Min(1.0f, _rarmaimwait + 0.02f);

                    if (animator.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1)
                    {
                        upperBodyState = UpperBodyState.STAND;

                        if (lowerBodyState == LowerBodyState.AIRFIRE)
                        {
                            lowerBodyState = LowerBodyState.AIR;
                            _animator.CrossFadeInFixedTime(_animIDAir, 0.5f, 0);
                            Grounded = false;
                        }
                        else if (lowerBodyState == LowerBodyState.FIRE)
                            lowerBodyState = LowerBodyState.STAND;
                    }
                }
                break;
            case UpperBodyState.STAND:
                {
                    if (_input.fire)
                    {
                        upperBodyState = UpperBodyState.FIRE;
                        _input.fire = false;

                        animator.Play("Armature|Fire", 1, 0.0f);

                        float angle_aim = Vector3.Angle(target.transform.position - shoulder_hint.transform.position, transform.forward);

                        if (angle > 60)
                        {
                            if (lowerBodyState == LowerBodyState.AIR)
                            {
                                lowerBodyState = LowerBodyState.AIRFIRE;
                            }
                            else
                            {
                                lowerBodyState = LowerBodyState.FIRE;
                                _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                            }
                        }
       
                    }

                    _rarmaimwait = Mathf.Max(0.0f, _rarmaimwait - 0.02f);
                }
                break;
        }



        Quaternion q_base_global = Quaternion.Inverse(shoulder_hint.transform.rotation);

        Quaternion q_aim_global = Quaternion.LookRotation(shoulder_hint.transform.position - target.transform.position, new Vector3(0.0f, 1.0f, 0.0f));

        Quaternion q_rotation_global = q_base_global * q_aim_global;

        Quaternion q_base = Quaternion.Inverse(aimingBase.transform.rotation);

        Quaternion q_final = q_base * q_rotation_global * aimingBase.transform.rotation;

        //overrideTransform.data.rotation = q_final.eulerAngles;
        overrideTransform.data.position = shoulder_hint.transform.position;
        overrideTransform.data.rotation = (q_aim_global * Quaternion.Euler(-90.0f, 0.0f, 0.0f)).eulerAngles;

        overrideTransform.weight = _rarmaimwait;

        animator.SetLayerWeight(1, _rarmaimwait);
    }

    //return angle in range -180 to 180
   

    private void LowerBodyMove()
    {
        float targetSpeed = 0.0f;

        switch (lowerBodyState)
        {
            case LowerBodyState.STAND:
            case LowerBodyState.WALK:
            case LowerBodyState.AIR:
                {

                    if (lowerBodyState != LowerBodyState.AIR || _input.jump)
                    {
                        // set target speed based on move speed, sprint speed and if sprint is pressed
                        if (lowerBodyState == LowerBodyState.AIR)
                            targetSpeed = AirMoveSpeed;
                        else
                            targetSpeed = MoveSpeed;

                        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

                        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                        // if there is no input, set the target speed to 0
                        if (_input.move == Vector2.zero)
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
                            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                                RotationSmoothTime);

                            // rotate to face input direction relative to camera position
                            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                        }
                    }

                    // update animator if using character
                    //if (_hasAnimator)
                    {
                        //_animator.SetFloat(_animIDSpeed, _animationBlend);
                        //_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);

                        switch(lowerBodyState)
                        {
                            case LowerBodyState.STAND:
                                if (_input.move != Vector2.zero)
                                {
                                    lowerBodyState = LowerBodyState.WALK;
                                    _animator.CrossFadeInFixedTime(_animIDWalk, 0.5f,0);
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

                        if(lowerBodyState == LowerBodyState.STAND || lowerBodyState == LowerBodyState.WALK)
                        {
                            if (_input.jump)
                            {
                                event_jumped = false;
                                lowerBodyState = LowerBodyState.JUMP;
                                _animator.CrossFadeInFixedTime(_animIDJump, 0.5f, 0);
                            }
                            else if( _input.sprint && upperBodyState == UpperBodyState.STAND)
                            {
                                event_stepped = false;
                                event_stepbegin = false;
                                lowerBodyState = LowerBodyState.STEP;

                                // normalise input direction
                                Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

                                float steptargetdegree = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                             _mainCamera.transform.eulerAngles.y;

           
                                

                                float stepmotiondegree = Mathf.Repeat(steptargetdegree- transform.eulerAngles.y+180.0f, 360.0f)-180.0f;

     

                                if (stepmotiondegree >= 45.0f && stepmotiondegree < 135.0f)
                                    stepDirection = StepDirection.RIGHT;
                                else if (stepmotiondegree >= 135.0f || stepmotiondegree < -135.0f)
                                    stepDirection = StepDirection.BACKWARD;
                                else if (stepmotiondegree >= -135.0f && stepmotiondegree < -45.0f)
                                    stepDirection = StepDirection.LEFT;
                                else 
                                    stepDirection = StepDirection.FORWARD;

                     

                                switch(stepDirection)
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

                 
                            }
                        }

                        if(lowerBodyState == LowerBodyState.AIR)
                        {
                            if (_input.jump)
                            {
                                _verticalVelocity = Mathf.Min(_verticalVelocity+0.4f, AscendingVelocity);


                            }
                            else
                            {
                                
                            }
                            _animator.SetFloat(_animIDVerticalSpeed, _verticalVelocity);
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


                    Vector3 target_dir = target.transform.position - transform.position;

                    _targetRotation = Mathf.Atan2(target_dir.x, target_dir.z) * Mathf.Rad2Deg;

                   float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                    if (lowerBodyState == LowerBodyState.AIRFIRE)
                        animator.SetFloat(_animIDVerticalSpeed, _verticalVelocity);

                    JumpAndGravity();
                    GroundedCheck();
                }
                break;
            case LowerBodyState.GROUND:
            case LowerBodyState.JUMP:
                {
                    targetSpeed = 0.0f;

                    _animationBlend = 0.0f;

                    /*// a reference to the players current horizontal velocity
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
                    }*/


                    _speed = 0.0f;





                    if (lowerBodyState == LowerBodyState.GROUND)
                    {
                        if (event_grounded)
                        {
                            lowerBodyState = LowerBodyState.STAND;
                            _animator.CrossFadeInFixedTime(_animIDStand, 0.5f, 0);
                        }
                    }

                    if (lowerBodyState == LowerBodyState.JUMP)
                    {
                        if (event_jumped)
                        {
                            lowerBodyState = LowerBodyState.AIR;
                            _animator.CrossFadeInFixedTime(_animIDAir, 0.5f, 0);
                            Grounded = false;
                            _verticalVelocity = AscendingVelocity;

                            _controller.Move(new Vector3(0.0f, 0.1f, 0.0f));
                        }
                    }
                }
                break;
            case LowerBodyState.STEP:
                {


                    _speed = targetSpeed = event_stepbegin ? SprintSpeed : 0.0f;

                    _animationBlend = 0.0f;


               
                   
                    if (event_stepped && !_input.sprint)
                    {
                        lowerBodyState = LowerBodyState.GROUND;

                        event_grounded = false;

                        _animator.CrossFadeInFixedTime(_animIDGround, 0.25f, 0,0.15f);
                    }
                }
                break;
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

            float degree_delta = (Mathf.Repeat(steptargetrotation - transform.eulerAngles.y + 180.0f, 360.0f) - 180.0f);

            if(degree_delta != 0.0f)
                Debug.Log(degree_delta);

            if(degree_delta < 1.0f && degree_delta > -1.0f)
                transform.rotation = Quaternion.Euler(0.0f, steptargetrotation, 0.0f);
            else if (degree_delta > 0.0f)
                transform.rotation = Quaternion.Euler(0.0f, transform.eulerAngles.y + 1.0f, 0.0f);
            else
                transform.rotation = Quaternion.Euler(0.0f, transform.eulerAngles.y - 1.0f, 0.0f);

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else
        {
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

     
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
                var index = Random.Range(0, FootstepAudioClips.Length);
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
