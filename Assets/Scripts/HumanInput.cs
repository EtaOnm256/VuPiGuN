using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class HumanInput : InputBase
{
    bool continue_sprint = false;

    InputAction forwardStep;

    [SerializeField] InputActionAsset inputActions;
     
    private void Awake()
    {
        //forwardStep = inputActions.FindAction("ForwardStep",true);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput(context.ReadValue<Vector2>());

        if(move == Vector2.zero)
        {
            if (!continue_sprint)
                sprint_once = false;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (cursorInputForLook)
        {
            LookInput(context.ReadValue<Vector2>());
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        JumpInput(context.ReadValue<float>() > 0.5f);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        SprintInput(context.ReadValue<float>() > 0.5f);
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        FireInput(context.ReadValue<float>() > 0.5f);
    }

    public void OnDown(InputAction.CallbackContext context)
    {
        DownInput(context.ReadValue<float>() > 0.5f);
    }

    public void OnSlash(InputAction.CallbackContext context)
    {
        SlashInput(context.ReadValue<float>() > 0.5f);
    }
    public void OnSlashRingMenu(InputAction.CallbackContext context)
    {
        //SlashInput(context.ReadValue<float>() > 0.5f);

        if (context.ReadValue<float>() < 0.5f)
        {
            ringMenuDir = RobotController.RingMenuDir.Center;
            slash_forcedispatch = false;
        }
        else
        {
            if (move == Vector2.zero)
                slash_forcedispatch = true;

            else if (move.x > 0.0f)
            {
                ringMenuDir = RobotController.RingMenuDir.Right;
                slash_forcedispatch = true;
            }
            else if (move.x < 0.0f)
            {
                ringMenuDir = RobotController.RingMenuDir.Left;
                slash_forcedispatch = true;
            }
            else if (move.y > 0.0f)
            {
                ringMenuDir = RobotController.RingMenuDir.Up;
                slash_forcedispatch = true;
            }
            else if (move.y < 0.0f)
            {
                ringMenuDir = RobotController.RingMenuDir.Down;
                slash_forcedispatch = true;
            }
        }
    }

    public void OnSubFire(InputAction.CallbackContext context)
    {
        SubFireInput(context.ReadValue<float>() > 0.5f);
    }

    public void OnSubFireOrRingMenu(InputAction.CallbackContext context)
    {
        //SubFireInput(context.ReadValue<float>() > 0.5f);

        if (context.ReadValue<float>() < 0.5f)
        {
            ringMenuDir = RobotController.RingMenuDir.Center;
            fire_forcedispatch = false;
            subfire = false;
        }
        else
        {
            if (move == Vector2.zero)
                subfire = true;

            else if(move.x > 0.0f)
            {
                ringMenuDir = RobotController.RingMenuDir.Right;
                fire_forcedispatch = true;
            }
            else if( move.x < 0.0f)
            {
                ringMenuDir = RobotController.RingMenuDir.Left;
                fire_forcedispatch = true;
            }
            else if (move.y > 0.0f)
            {
                ringMenuDir = RobotController.RingMenuDir.Up;
                fire_forcedispatch = true;
            }
            else if (move.y < 0.0f)
            {
                ringMenuDir = RobotController.RingMenuDir.Down;
                fire_forcedispatch = true;
            }
        }
            
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        MenuInput(context.ReadValue<float>() > 0.5f);
    }
    public bool menu; // CPUがメニュー開くことはないので
    public void OnLockSwitch(InputAction.CallbackContext context)
    {
        LockSwitchInput(context.ReadValue<float>() > 0.5f);
    }
    public bool command; // CPUがメニュー開くことはないので
    public void OnCommand(InputAction.CallbackContext context)
    {
        CommandInput(context.ReadValue<float>() > 0.5f);
    }

    public void OnForwardStep(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            sprint_once = true;
            continue_sprint = true;
        }
        else if (context.performed)
        {
            continue_sprint = false;

            if (move == Vector2.zero)
                sprint_once = false;
        }
    }
    public void OnBackwardStep(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            sprint_once = true;
            continue_sprint = true;
        }
        else if (context.performed)
        {
            continue_sprint = false;

            if (move == Vector2.zero)
                sprint_once = false;
        }
    }

    public void OnLeftStep(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            sprint_once = true;
            continue_sprint = true;
        }
        else if (context.performed)
        {
            continue_sprint = false;

            if (move == Vector2.zero)
                sprint_once = false;
        }
    }
    public void OnRightStep(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            sprint_once = true;
            continue_sprint = true;
        }
        else if (context.performed)
        {
            continue_sprint = false;

            if (move == Vector2.zero)
                sprint_once = false;
        }
    }
#endif


    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    public void FireInput(bool newFireState)
    {
        fire = newFireState;
    }

    public void DownInput(bool newDownState)
    {
        down = newDownState;
    }

    public void SlashInput(bool newSlashState)
    {
        slash = newSlashState;
    }

    public void SubFireInput(bool newSlashState)
    {
        subfire = newSlashState;
    }

    public void MenuInput(bool newSlashState)
    {
        menu = newSlashState;
    }
    public void CommandInput(bool newSlashState)
    {
        command = newSlashState;
    }

    public void ForwardStepInput()
    {

    }

    public void BackwardStepInput()
    {

    }

    public void LeftStepInput()
    {

    }

    public void RightStepInput()
    {
    }

    public void LockSwitchInput(bool newSlashState)
    {
        lockswitch = newSlashState;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        //	SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}

