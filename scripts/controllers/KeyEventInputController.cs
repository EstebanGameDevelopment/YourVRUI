using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourVRUI
{

	/******************************************
	 * 
	 * KeyEventInputController
	 * 
	 * Class used to process the system game input's
	 * 
	 * @author Esteban Gallardo
	 */
	public class KeyEventInputController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string ACTION_BUTTON_DOWN = "ACTION_BUTTON";
		public const string ACTION_BUTTON_UP = "ACTION_BUTTON_UP";
		public const string ACTION_SECONDARY_BUTTON_DOWN = "ACTION_SECONDARY_BUTTON";
		public const string ACTION_SECONDARY_BUTTON_UP = "ACTION_SECONDARY_BUTTON_UP";
		public const string ACTION_INVENTORY_VERTICAL = "ACTION_INVENTORY_VERTICAL";
		public const string ACTION_INVENTORY_HORIZONTAL = "ACTION_INVENTORY_HORIZONTAL";
		public const string ACTION_CANCEL_BUTTON = "ACTION_CANCEL_BUTTON";
		public const string ACTION_SET_ANCHOR_POSITION = "ACTION_SET_ANCHOR_POSITION";
		public const string ACTION_BACK_BUTTON = "ACTION_BACK_BUTTON";

		public const string ACTION_KEY_UP = "ACTION_KEY_UP";
		public const string ACTION_KEY_DOWN = "ACTION_KEY_DOWN";
		public const string ACTION_KEY_LEFT = "ACTION_KEY_LEFT";
		public const string ACTION_KEY_RIGHT = "ACTION_KEY_RIGHT";

		public const string ACTION_KEY_UP_RELEASED = "ACTION_KEY_UP_RELEASED";
		public const string ACTION_KEY_DOWN_RELEASED = "ACTION_KEY_DOWN_RELEASED";
		public const string ACTION_KEY_LEFT_RELEASED = "ACTION_KEY_LEFT_RELEASED";
		public const string ACTION_KEY_RIGHT_RELEASED = "ACTION_KEY_RIGHT_RELEASED";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		private const int AXIS_KEY_NONE = -1;
		private const int AXIS_KEY_DOWN_EVENT = 0;
		private const int AXIS_KEY_UP_EVENT = 1;
		private const int AXIS_KEY_DOWN_STILL_PRESSED_CODE = 2;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static KeyEventInputController _instance;

		public static KeyEventInputController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(KeyEventInputController)) as KeyEventInputController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "KeyEventInputController";
						_instance = container.AddComponent(typeof(KeyEventInputController)) as KeyEventInputController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private int m_currentDirection = -1;
		private bool m_enableActionOnMouseDown = true;
		private float m_timeAcumInventory = 0;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool EnableActionOnMouseDown
		{
			get { return m_enableActionOnMouseDown; }
			set { m_enableActionOnMouseDown = value; }
		}

		// -------------------------------------------
		/* 
		 * Initialization
		 */
		public void Initialization()
		{
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public void Destroy()
		{
			DestroyObject(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * GetControllerKeyCode
		 */
		private int GetControllerKeyCode(int _directionCheck)
		{
			int eventType = AXIS_KEY_NONE;

			switch (_directionCheck)
			{
				case UtilitiesYourVRUI.DIRECTION_LEFT:
					if (Input.GetAxis("Horizontal") < 0)
					{
						if (m_currentDirection == -1)
						{
							eventType = AXIS_KEY_DOWN_EVENT;
							m_currentDirection = UtilitiesYourVRUI.DIRECTION_LEFT;
						}
					}
					break;

				case UtilitiesYourVRUI.DIRECTION_RIGHT:
					if (Input.GetAxis("Horizontal") > 0)
					{
						if (m_currentDirection == -1)
						{
							eventType = AXIS_KEY_DOWN_EVENT;
							m_currentDirection = UtilitiesYourVRUI.DIRECTION_RIGHT;
						}
					}
					break;

				case UtilitiesYourVRUI.DIRECTION_UP:
					if (Input.GetAxis("Vertical") > 0)
					{
						if (m_currentDirection == -1)
						{
							eventType = AXIS_KEY_DOWN_EVENT;
							m_currentDirection = UtilitiesYourVRUI.DIRECTION_UP;
						}
					}
					break;

				case UtilitiesYourVRUI.DIRECTION_DOWN:
					if (Input.GetAxis("Vertical") < 0)
					{
						if (m_currentDirection == -1)
						{
							eventType = AXIS_KEY_DOWN_EVENT;
							m_currentDirection = UtilitiesYourVRUI.DIRECTION_DOWN;
						}
					}
					break;

				case UtilitiesYourVRUI.DIRECTION_NONE:
					if ((Input.GetAxis("Horizontal") == 0) && (Input.GetAxis("Vertical") == 0) && (m_currentDirection != -1))
					{
						eventType = AXIS_KEY_UP_EVENT;
						m_currentDirection = -1;
					}
					break;
			}

			return eventType;
		}

		// -------------------------------------------
		/* 
		 * KeyInputManagment
		 */
		private void KeyInputManagment()
		{
			/*
			// PRINT THE CURRENT KEY PRESSED
			if (Input.anyKeyDown)
			{
				foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
				{
					if (Input.GetKeyDown(kcode))
						Debug.Log("KeyCode down: " + kcode);
				}
			}
			*/

			// ACTION_SECONDARY_BUTTON
			if (Input.GetKeyDown(KeyCode.LeftShift) || (Input.GetKeyDown(KeyCode.Joystick1Button2)))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_SECONDARY_BUTTON_DOWN);
			}

			bool hasEntered = false;
			if (m_enableActionOnMouseDown)
			{
				// DAYDREAM CONTROLLER				
				if (YourVRUIScreenController.Instance.IsDayDreamActivated)
				{
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
					if (GvrControllerInput.TouchDown)
					{
						hasEntered = true;
						ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_BUTTON_DOWN);
					}
				}
#endif
				if (!hasEntered)
				{
					bool fire1Triggered = false;
					if (YourVRUIScreenController.Instance.ScreensTemporal.Count == 0)
					{
						if (Input.GetButtonDown("Fire1"))
						{
							fire1Triggered = true;
						}
					}

					if (Input.GetKeyDown(KeyCode.LeftControl) 
						|| Input.GetKeyDown(KeyCode.JoystickButton0)
						|| fire1Triggered)
					{
						ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_BUTTON_DOWN);
					}
				}
			}
			else
			{
				// DAYDREAM CONTROLLER
				if (YourVRUIScreenController.Instance.IsDayDreamActivated)
				{
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
					if (GvrControllerInput.TouchUp)
					{
						hasEntered = true;
						ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_BUTTON_DOWN);
					}
					if (GvrControllerInput.TouchDown)
					{
						hasEntered = true;
						ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_SET_ANCHOR_POSITION);
					}
#endif
				}

				if (!hasEntered)
				{
					bool fire1Triggered = false;
					if (YourVRUIScreenController.Instance.ScreensTemporal.Count == 0)
					{
						if (Input.GetButtonDown("Fire1"))
						{
							fire1Triggered = true;
						}
					}

					bool fire1Released = false;
					if (YourVRUIScreenController.Instance.ScreensTemporal.Count == 0)
					{
						if (Input.GetButtonUp("Fire1"))
						{
							fire1Released = true;
						}
					}

					// ACTION BUTTON
					if (Input.GetKeyDown(KeyCode.LeftControl) 
						|| Input.GetKeyDown(KeyCode.JoystickButton0)
						|| fire1Triggered)
					{
						ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_SET_ANCHOR_POSITION);
					}

					// ACTION BUTTON
					if (Input.GetKeyUp(KeyCode.LeftControl) 
						|| Input.GetKeyUp(KeyCode.JoystickButton0)
						|| fire1Released)
					{
						ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_BUTTON_DOWN);
					}
				}
			}

			// INVENTORY BUTTON
			if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Joystick1Button6))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_INVENTORY_VERTICAL);
			}
			// INVENTORY BUTTON
			if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Joystick1Button7))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_INVENTORY_HORIZONTAL);
			}

			// DAYDREAM CONTROLLER
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
			if (YourVRUIScreenController.Instance.IsDayDreamActivated)
			{
				if (GvrController.AppButtonDown)
				{
					ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_CANCEL_BUTTON);
				}
				if (GvrController.AppButton)
				{
					m_timeAcumInventory += Time.deltaTime;
					if (m_timeAcumInventory > 1.5f)
					{
						m_timeAcumInventory = 0;
						ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_INVENTORY_VERTICAL);
					}
				}
				if (GvrController.AppButtonUp)
				{
					m_timeAcumInventory = 0;
				}
			}
#endif

			// CANCEL BUTTON
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_CANCEL_BUTTON);
			}
			// BACK BUTTON
			if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Joystick1Button1))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_BACK_BUTTON);
			}

			// ARROWS KEYPAD
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				m_currentDirection = UtilitiesYourVRUI.DIRECTION_LEFT;
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_KEY_LEFT);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				m_currentDirection = UtilitiesYourVRUI.DIRECTION_RIGHT;
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_KEY_RIGHT);
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				m_currentDirection = UtilitiesYourVRUI.DIRECTION_DOWN;
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_KEY_UP);
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				m_currentDirection = UtilitiesYourVRUI.DIRECTION_UP;
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_KEY_DOWN);
			}


			// ARROW KEYS UP
			if (Input.GetKeyUp(KeyCode.LeftArrow))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_KEY_LEFT_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.RightArrow))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_KEY_RIGHT_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.UpArrow))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_KEY_UP_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.DownArrow))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_KEY_DOWN_RELEASED);
			}

			// ACTION BUTTON UP
			if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.JoystickButton0))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_BUTTON_UP);
			}

			// ACTION_SECONDARY_BUTTON UP
			if (Input.GetKeyUp(KeyCode.RightShift) || (Input.GetKeyDown(KeyCode.Joystick1Button8)))
			{
				ScreenVREventController.Instance.DispatchScreenVREvent(ACTION_SECONDARY_BUTTON_UP);
			}
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		void Update()
		{
			KeyInputManagment();
		}
	}

}