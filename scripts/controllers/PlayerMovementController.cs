using UnityEngine;
using System.Collections;
using System;
using YourCommonTools;

namespace YourVRUI
{
	/******************************************
	 * 
	 * PlayerMovementController
	 * 
	 * (DEBUG CODE) It's only used for debug purposes. Use your own system to move the player 
	 * and add him "PlayerRaycasterController" script with the tag "Player"
	 * 
	 * @author Esteban Gallardo
	 */
	[RequireComponent(typeof(PlayerRaycasterController))]
	public class PlayerMovementController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_PLAYERMOVEMENT_GO_TO_POSITION = "EVENT_PLAYERMOVEMENT_GO_TO_POSITION";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public float MoveSpeed = 2;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private CharacterController m_characterController;
		private bool m_isMoving = false;

		// -------------------------------------------
		/* 
		 * Start
		 */
		void Start()
		{
			Initialize();
		}

		// -------------------------------------------
		/* 
		 * Initialize
		 */
		public void Initialize()
		{
			Debug.LogError("WARNING: In order to make work the right stick to control the view of your Xbox controller you should create the input int \"Edit->Project Settings->Input\". See the image in the link: http://www.yourvrexperience.com/ProjectSettingsInputRightStick.png (PLEASE, REMOVE THIS LOG MESSAGE WHEN YOU HAVE CREATED THEM)");

			UIEventController.Instance.UIEvent += new UIEventHandler(OnScreenVREvent);
			BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);

			// CHARACTER CONTROLLER
			m_characterController = this.gameObject.GetComponent<CharacterController>();
			if (this.gameObject.GetComponent<Rigidbody>()!=null)
			{
				this.gameObject.GetComponent<Rigidbody>().useGravity = false;
				this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
			}
			this.gameObject.tag = YourVRUIScreenController.Instance.TagPlayerDetectionCollision;			
		}

        // -------------------------------------------
        /* 
		 * Destroy all references
		 */
        void OnDestroy()
        {
            Destroy();
        }

        // -------------------------------------------
        /* 
		 * Destroy all references
		 */
        public void Destroy()
		{
			UIEventController.Instance.UIEvent -= OnScreenVREvent;
			BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		private void OnScreenVREvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_PLAYERMOVEMENT_GO_TO_POSITION)
			{
				m_isMoving = true;
				Vector3 targetPosition = Utilities.ClonePoint((Vector3)_list[0]);
				targetPosition.y = this.gameObject.transform.position.y;
				InterpolatorController.Instance.Interpolate(this.gameObject, targetPosition, 1);				
			}
			if (_nameEvent == KeysEventInputController.ACTION_INVENTORY_VERTICAL)
			{
				if (GameObject.FindObjectOfType<ScreenItemInventoryView>() != null)
				{
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, GameObject.FindObjectOfType<ScreenItemInventoryView>().gameObject);
				}
				else
				{
					KeysEventInputController.Instance.EnableActionOnMouseDown = false;
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
																true,
																this.gameObject,
																null,  // GameObject collided
																"SCREEN_INVENTORY_VERTICAL",            // interactedObject.screenName,
                                                                UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, // interactedObject.PreviousScreenAction,
																-1f, // interactedObject.DetectionDistance,
																true, // interactedObject.IsWorldObject,
																false, // interactedObject.ScreenLinkedToObject,
																false, // interactedObject.ScreenInCenterObject
																true, // interactedObject.ForceScreen,
																false, // interactedObject.ForceOrthographic,
																true, // interactedObject.AlignedToCamera,
																false, // interactedObject.UseCollisionPoint,
																2.5f, // interactedObject.DistanceScreenDefault,
																true, // interactedObject.Refocus,
																false, // interactedObject.DestroyMessageOnDistance,
																-1f, // interactedObject.ScaleScreen,
																true, // interactedObject.BlockOtherScreens,
																Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
																true, // Temporal Screen
																true, // interactedObject.IgnoreZOrderScreen,
																true, // HighlightSelector
																null // interactedObject.GetListPagesInformation()
																);
				}
			}
			if (_nameEvent == KeysEventInputController.ACTION_INVENTORY_HORIZONTAL)
			{
				if (GameObject.FindObjectOfType<ScreenItemInventoryView>() != null)
				{
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, GameObject.FindObjectOfType<ScreenItemInventoryView>().gameObject);
				}
				else
				{
					KeysEventInputController.Instance.EnableActionOnMouseDown = false;
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
																true,
																this.gameObject,
																null,  // GameObject collided
																"SCREEN_INVENTORY_HORIZONTAL", // interactedObject.screenName,
                                                                UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, // interactedObject.PreviousScreenAction,
																-1f, // interactedObject.DetectionDistance,
																true, // interactedObject.IsWorldObject,
																false, // interactedObject.ScreenLinkedToObject,
																false, // interactedObject.ScreenInCenterObject
																true, // interactedObject.ForceScreen,
																false, // interactedObject.ForceOrthographic,
																true, // interactedObject.AlignedToCamera,
																false, // interactedObject.UseCollisionPoint,
																2.5f, // interactedObject.DistanceScreenDefault,
																true, // interactedObject.Refocus,
																false, // interactedObject.DestroyMessageOnDistance,
																-1f, // interactedObject.ScaleScreen,
																true, // interactedObject.BlockOtherScreens,
																Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
																true, // Temporal Screen
																true, // interactedObject.IgnoreZOrderScreen,
																true, // HighlightSelector
																null // interactedObject.GetListPagesInformation()
																);
				}
			}
		}

		// (DEBUG CODE) Used for debug purposes
		private enum RotationAxes { None = 0, MouseXAndY = 1, MouseX = 2, MouseY = 3, Controller = 4 }
		private RotationAxes m_axes = RotationAxes.MouseXAndY;
		private float m_sensitivityX = 7F;
		private float m_sensitivityY = 7F;

		private float m_minimumY = -60F;
		private float m_maximumY = 60F;

		private float m_rotationY = 0F;

		// -------------------------------------------
		/* 
		 * We will apply the movement to the camera		
		 */
		private void MoveCameraWithMouse()
		{
			if (YourVRUIScreenController.Instance.BlockMouseMovement) return;

			m_axes = RotationAxes.None;

			// USE ARROW KEYS TO MOVE
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			if (m_characterController != null)
			{
				Vector3 forward = Input.GetAxis("Vertical") * transform.TransformDirection(YourVRUIScreenController.Instance.GameCamera.transform.forward) * MoveSpeed;
				m_characterController.Move(forward * Time.deltaTime);
				m_characterController.SimpleMove(Physics.gravity);
			}
#endif

			if ((Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0))
			{
				m_axes = RotationAxes.MouseXAndY;
			}

			// USE MOUSE TO ROTATE VIEW
			if ((m_axes != RotationAxes.Controller) && (m_axes != RotationAxes.None))
			{
				if (m_axes == RotationAxes.MouseXAndY)
				{
					float rotationX = YourVRUIScreenController.Instance.GameCamera.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * m_sensitivityX;

					m_rotationY += Input.GetAxis("Mouse Y") * m_sensitivityY;
					m_rotationY = Mathf.Clamp(m_rotationY, m_minimumY, m_maximumY);

					YourVRUIScreenController.Instance.GameCamera.transform.localEulerAngles = new Vector3(-m_rotationY, rotationX, 0);
				}
				else if (m_axes == RotationAxes.MouseX)
				{
					YourVRUIScreenController.Instance.GameCamera.transform.Rotate(0, Input.GetAxis("Mouse X") * m_sensitivityX, 0);
				}
				else
				{
					m_rotationY += Input.GetAxis("Mouse Y") * m_sensitivityY;
					m_rotationY = Mathf.Clamp(m_rotationY, m_minimumY, m_maximumY);

					YourVRUIScreenController.Instance.GameCamera.transform.localEulerAngles = new Vector3(-m_rotationY, transform.localEulerAngles.y, 0);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * On Basic System Events
		 */
		private void OnBasicSystemEvent(string _nameEvent, object[] _list)
		{
			if (_nameEvent == InterpolateData.EVENT_INTERPOLATE_COMPLETED)
			{
				if (this.gameObject == (GameObject)_list[0])
				{
					m_isMoving = false;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Apply the movement
		 */
		void Update()
		{
            if (YourVRUIScreenController.Instance == null) return;

            if (m_isMoving) return;

			// USE ARROW KEYS TO MOVE
			if (m_characterController != null)
			{
				Vector3 forward = Input.GetAxis("Vertical") * transform.TransformDirection(YourVRUIScreenController.Instance.GameCamera.transform.forward) * MoveSpeed;
				m_characterController.Move(forward * Time.deltaTime);
				m_characterController.SimpleMove(Physics.gravity);
			}

            // MOVE CAMERA
            if (YourVRUIScreenController.Instance.EnableMoveCamera)
			{
				MoveCameraWithMouse();
			}

			// USE DAYDREAM CONTROLLER INPUT TO MOVE
			if (YourVRUIScreenController.Instance.IsDayDreamActivated)
			{
				#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR) && !ENABLE_OCULUS
                /*
                if (ControllerInputDevice.GetButton(GvrControllerButton.TouchPadButton))
				{
                    Vector2 touchPos = GvrController.TouchPos;
					float directionMove = -(touchPos.y - 0.5f);
					directionMove = directionMove * MoveSpeed * 2 * Time.deltaTime;
					Vector3 directionVectorForward = Utilities.ClonePoint(YourVRUIScreenController.Instance.GameCamera.transform.forward.normalized);
					directionVectorForward.y = 0;
					this.gameObject.transform.position += (directionVectorForward * directionMove);
				}
                */
				#endif
			}
		}

	}
}