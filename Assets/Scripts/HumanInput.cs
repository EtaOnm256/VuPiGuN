using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


	public class HumanInput : InputBase
	{
	

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnFire(InputValue value)
		{
			FireInput(value.isPressed);
		}

		public void OnDown(InputValue value)
		{
			DownInput(value.isPressed);
		}

		public void OnSlash(InputValue value)
		{
			SlashInput(value.isPressed);
		}

		public void OnSubFire(InputValue value)
		{
			SubFireInput(value.isPressed);
		}

		public void OnMenu(InputValue value)
		{
			MenuInput(value.isPressed);
		}
		public bool menu; // CPUÇ™ÉÅÉjÉÖÅ[äJÇ≠Ç±Ç∆ÇÕÇ»Ç¢ÇÃÇ≈
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

	private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
