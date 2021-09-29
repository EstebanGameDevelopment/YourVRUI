using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YourCommonTools;

namespace YourVRUI
{
	/******************************************
	 * 
	 * InteractionController
	 * 
	 * Class that allows to add to an object information about the screen it should display
	 * 
	 * @author Esteban Gallardo
	 */
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	public class InteractionController : MonoBehaviour
	{
        public static bool StaticEnableInteraction = true;

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------
		public const string EVENT_INTERACTIONCONTROLLER_SCREEN_CREATED = "EVENT_INTERACTIONCONTROLLER_SCREEN_CREATED";
		public const string EVENT_INTERACTIONCONTROLLER_SCREEN_DESTROYED = "EVENT_INTERACTIONCONTROLLER_SCREEN_DESTROYED";

		public const string EVENT_INTERACTIONCONTROLLER_COLLIDED_WITH_PLAYER = "EVENT_INTERACTIONCONTROLLER_COLLIDED_WITH_PLAYER";

        public const string EVENT_INTERACTIONCONTROLLER_ENABLE_INTERACTION = "EVENT_INTERACTIONCONTROLLER_ENABLE_INTERACTION";

        public const string EVENT_INTERACTIONCONTROLLER_UPDATE_SETTINGS = "EVENT_INTERACTIONCONTROLLER_UPDATE_SETTINGS";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------
        public const float DETECTION_DISTANCE_DEFAULT = 2;
		public const float DISTANCE_SCREEN_BY_DEFAULT = 2.5f;

		// ----------------------------------------------
		// CUSTOMIZABLE PROPERTY MEMBERS
		// ----------------------------------------------
		[Tooltip("Distance from the player to consider the display of the screen")]
		public float DetectionDistance = DETECTION_DISTANCE_DEFAULT;

		[Tooltip("Screen prefab you want to display for this object")]
		public GameObject screenPrefab;
		[Tooltip("Action you want to apply to the previous screens displayed")]
		public UIScreenTypePreviousAction PreviousScreenAction;
		[Tooltip("Will override the global settings and it will use the local setting to display the screen")]
		public bool OverrideGlobalSettings = false;
		[Tooltip("It's a screen that should be placed in the world or it should be placed in the traditional screen way")]
		public bool IsWorldObject = true;
		[Tooltip("The displayed screen should be linked to the object or linked to the player. If it's linked to the object then the position of the object is considered to place the screen. If it's not linked to the object then the screen is just displayed in front of the camera")]
		public bool ScreenLinkedToObject = true;
		[Tooltip("If this property is true then the screen will be placed just in the position of the object")]
		public bool ScreenInCenterObject = false;
		[Tooltip("This property allows to force an screen to the user and not disappear it until the player has actively closed it")]
		public bool ForceScreen = false;
		[Tooltip("If activated will display the screen in the usual way")]
		public bool ForceOrthographic = false;
		[Tooltip("If activated the displayed screen will appear aligned to the camera instead of aligned to the object")]
		public bool AlignedToCamera = false;
		[Tooltip("It will use the collision point in order to display the screen")]
		public bool UseCollisionPoint = false;
		[Tooltip("If the screen is not linked to the object, but to the player this is the distance it will take to display the screen")]
		public float DistanceScreenDefault = DISTANCE_SCREEN_BY_DEFAULT;
		[Tooltip("This property is used to realign the screen in front of the camera when the user is not looking at it")]
		public bool Refocus = false;
		[Tooltip("If activated it will display the screen as soon as the player is detected. If this property is disable then the screen will be displayed when the user press the action button")]
		public bool TriggerMessageOnDetection = true;
		[Tooltip("If activated the screen will be destroyed when the player is far away from the object")]
		public bool DestroyMessageOnDistance = true;
		[Tooltip("This is the scale that the screen will have")]
		public float ScaleScreen = -1f;
		[Tooltip("This property will allow to block the creation of new screen while the screen it's still working.")]
		public bool BlockOtherScreens = true;
		[Tooltip("With this property you can apply or not the shader that allows to draw the screen in front of everything else")]
		public bool IgnoreZOrderScreen = true;
		[Tooltip("With this property all the interactive elements of the screen will have a graphic that will be highlighted when the gaze/daydream/arrow keys are interacted")]
		public bool HighlightSelector = true;
		[Tooltip("With this property enabled the actions with the elements are on MOUSE_DOWN, if disable are on MOUSE_UP. This is important when there are scrolling list in the screen in order to be able to scroll the list without selecting the component")]
		public bool EnableActionOnMouseDown = true;

		[Tooltip("(DEBUG PROPERTY) It's used to display a text in the HUD. It's used for the testing of this system, you can change it to your own project")]
		public string InfoDetectionMessage = "";
		[Tooltip("(DEBUG PROPERTY) It's used to send a collection of information to the screen. It's used for the testing of this system, you can change it to your own project")]
		public PageInformation[] InformationPages;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private bool m_screenIsDisplayed = false;
        private bool m_enableInteraction = true;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool ScreenIsDisplayed
		{
			get { return m_screenIsDisplayed; }
		}

		public string NameCharacter
		{
			get { return this.gameObject.name; }
		}

		// -------------------------------------------
		/* 
		 * Start
		 */
		void Start()
		{
			UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public void Destroy()
		{
			UIEventController.Instance.UIEvent -= OnBasicEvent;
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (this != null)
			{
                if (_nameEvent == EVENT_INTERACTIONCONTROLLER_ENABLE_INTERACTION)
                {
                    m_enableInteraction = (bool)_list[0];
                }
				if (_nameEvent == EVENT_INTERACTIONCONTROLLER_SCREEN_CREATED)
				{
					if (this.gameObject == (GameObject)_list[0])
					{
						m_screenIsDisplayed = true;
					}
				}
				if (_nameEvent == EVENT_INTERACTIONCONTROLLER_SCREEN_DESTROYED)
				{
					if (this.gameObject == (GameObject)_list[0])
					{
						m_screenIsDisplayed = false;
					}
				}
                if (_nameEvent == EVENT_INTERACTIONCONTROLLER_UPDATE_SETTINGS)
                {
                    OverrideGlobalSettings = (bool)_list[0];
                    IsWorldObject = (bool)_list[1];
                }
			}
		}

		// -------------------------------------------
		/* 
		 * Get the list of pages of information
		 */
		public List<PageInformation> GetListPagesInformation()
		{
			List<PageInformation> pages = new List<PageInformation>();
			if ((InformationPages.Length > 0))
			{
				for (int i = 0; i < InformationPages.Length; i++)
				{
					pages.Add(InformationPages[i].Clone());
				}
			}
			return pages;
		}

        // -------------------------------------------
        /* 
		 * DispatchCustomScreen
		 */
        public void DispatchCustomScreen(GameObject _screenPrefab, List<PageInformation> _pages, float _scaleScreen, bool _bypass = false)
        {
            if (!StaticEnableInteraction) return;

            if (!_bypass)
            {
                if (!m_enableInteraction) return;
            }

            KeysEventInputController.Instance.EnableActionOnMouseDown = EnableActionOnMouseDown;
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
													OverrideGlobalSettings,
													 GameObject.FindObjectOfType<PlayerRaycasterController>().gameObject,
													this.gameObject,
													_screenPrefab,
													PreviousScreenAction,
													DetectionDistance,
													IsWorldObject,
													ScreenLinkedToObject,
													ScreenInCenterObject,
													ForceScreen,
													ForceOrthographic,
													AlignedToCamera,
													UseCollisionPoint,
													DistanceScreenDefault,
													Refocus,
													DestroyMessageOnDistance,
													_scaleScreen,
													BlockOtherScreens,
													null,
													true, // Temporal Screen
													IgnoreZOrderScreen,
													HighlightSelector,
													_pages,
													-1f,
													0,
													this.gameObject
												);
        }

		// -------------------------------------------
		/* 
		 * Dispatch screen when collision detection
		 */
		public void DispatchScreen(GameObject _player, string[] _ignoreLayers, bool _force)
		{
            if (!StaticEnableInteraction) return;

            if (!m_enableInteraction) return;

            UIEventController.Instance.DispatchUIEvent(EVENT_INTERACTIONCONTROLLER_COLLIDED_WITH_PLAYER, this);
			if (TriggerMessageOnDetection || _force)
			{
				KeysEventInputController.Instance.EnableActionOnMouseDown = EnableActionOnMouseDown;
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
													OverrideGlobalSettings,
													_player,
													this.gameObject,
													screenPrefab,
													PreviousScreenAction,
													DetectionDistance,
													IsWorldObject,
													ScreenLinkedToObject,
													ScreenInCenterObject,
													ForceScreen,
													ForceOrthographic,
													AlignedToCamera,
													UseCollisionPoint,
													DistanceScreenDefault,
													Refocus,
													DestroyMessageOnDistance,
													ScaleScreen,
													BlockOtherScreens,
													_ignoreLayers,
													true, // Temporal Screen
													IgnoreZOrderScreen,
													HighlightSelector,
													GetListPagesInformation(),
													-1f,
													0,
													this.gameObject
													);
			}
		}

		// -------------------------------------------
		/* 
		 * Triggered when there is a collision enter with (isTrigger == false)
		 */
		public void OnCollisionEnter(Collision _collision)
		{
            if (!m_enableInteraction) return;

            if (YourVRUIScreenController.Instance.EnableCollisionDetection)
			{
				if (YourVRUIScreenController.Instance.DebugMode)
				{
					Debug.Log("OnCollisionEnter(++)::GAMEOBJECT[" + this.gameObject.name + "]::COLLIDED OBJECT TAG=" + _collision.gameObject.tag);
				}
				if (_collision != null)
				{
					if (_collision.gameObject != null)
					{
						if (_collision.gameObject.tag == YourVRUIScreenController.Instance.TagPlayerDetectionCollision)
						{
							DispatchScreen(GameObject.FindObjectOfType<PlayerRaycasterController>().gameObject, null, false);
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Triggered when there is a collision exit with (isTrigger == false)
		 */
		public void OnCollisionExit(Collision _collision)
		{
            if (!m_enableInteraction) return;

            if (YourVRUIScreenController.Instance.EnableCollisionDetection)
			{
				if (YourVRUIScreenController.Instance.DebugMode)
				{
					Debug.Log("OnCollisionExit(--)::GAMEOBJECT[" + this.gameObject.name + "]::COLLIDED OBJECT TAG=" + _collision.gameObject.tag);
				}

				if (_collision != null)
				{
					if (_collision.gameObject != null)
					{
						if (_collision.gameObject.tag == YourVRUIScreenController.Instance.TagPlayerDetectionCollision)
						{
							UIEventController.Instance.DispatchUIEvent(BaseVRScreenView.EVENT_SCREEN_DESTROYED_VIEW, this.gameObject);
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Triggered when there is a collision enter with (isTrigger == true)
		 */
		public void OnTriggerEnter(Collider _collider)
		{
            if (!m_enableInteraction) return;

            if (YourVRUIScreenController.Instance.EnableCollisionDetection)
			{
				if (YourVRUIScreenController.Instance.DebugMode)
				{
					Debug.Log("OnTriggerEnter(++)::GAMEOBJECT[" + this.gameObject.name + "]::COLLIDED OBJECT TAG=" + _collider.gameObject.tag);
				}
				if (_collider != null)
				{
					if (_collider.gameObject != null)
					{
						if (_collider.gameObject.tag == YourVRUIScreenController.Instance.TagPlayerDetectionCollision)
						{
							DispatchScreen(GameObject.FindObjectOfType<PlayerRaycasterController>().gameObject, null, false);
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Triggered when there is a collision exit with (isTrigger == true)
		 */
		public void OnTriggerExit(Collider _collider)
		{
            if (!m_enableInteraction) return;

            if (YourVRUIScreenController.Instance.EnableCollisionDetection)
			{
				if (YourVRUIScreenController.Instance.DebugMode)
				{
					Debug.Log("OnTriggerExit(--)::GAMEOBJECT[" + this.gameObject.name + "]::COLLIDED OBJECT TAG=" + _collider.gameObject.tag);
				}

				if (_collider != null)
				{
					if (_collider.gameObject != null)
					{
						if (_collider.gameObject.tag == YourVRUIScreenController.Instance.TagPlayerDetectionCollision)
						{
							UIEventController.Instance.DispatchUIEvent(BaseVRScreenView.EVENT_SCREEN_DESTROYED_VIEW, this.gameObject);
						}
					}
				}
			}
		}
	}
}