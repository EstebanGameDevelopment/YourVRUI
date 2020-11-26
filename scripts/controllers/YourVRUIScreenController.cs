using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YourCommonTools;
using YourNetworkingTools;

namespace YourVRUI
{
    public enum CONFIGURATIONS_YOURVRUI
    {
        NONE = 0,
        CONFIGURATION_VR_RAYCAST = 1,
        CONFIGURATION_VR_RAYCAST_FORCE_SCREEN_ALIGNED_TO_CAMERA = 2,
        CONFIGURATION_VR_COLLISION_FORCE_SCREEN_ALIGNED_TO_CAMERA = 3,
        CONFIGURATION_COMPUTER_RAYCAST_WORLD_SCREENS = 4,
        CONFIGURATION_COMPUTER_RAYCAST_NORMAL_SCREENS = 5,
        CONFIGURATION_COMPUTER_COLLISION_WORLD_SCREENS = 6,
        CONFIGURATION_COMPUTER_COLLISION_NORMAL_SCREENS = 7
    }

    /******************************************
	 * 
	 * YourVRUIScreenController
	 * 
	 * Main class responsible of creating the screens in the VR/AR world
	 * 
	 * @author Esteban Gallardo
	 */
    public class YourVRUIScreenController : MonoBehaviour
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT = "EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT";
        public const string EVENT_SCREENMANAGER_DEBUG_LOG = "EVENT_SCREENMANAGER_DEBUG_LOG";

        public const string UI_TRIGGERER = "UI_TRIGGERER";
        public const string DEFAULT_YOURVUI_CONFIGURATION = "DEFAULT_YOURVUI_CONFIGURATION";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static YourVRUIScreenController _instance;

        public static YourVRUIScreenController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(YourVRUIScreenController)) as YourVRUIScreenController;
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        [Tooltip("Basic instructions file, this plugin is more about to enjoy a readable code that follow some long text instructions, if you get REALLY stuck somewhere along the way get in touch with me in the email address: esteban@yourvrexperience.com")]
        public TextAsset ReadMeFile;
        [Tooltip("It allows the debug messages(CHANGE IT TO FALSE WHEN YOU CREATE THE APK)")]
        public bool DebugMode = false;
        [Tooltip("It allows the debug projectiles to test the collision (it's useful when you use system's like Leap Motion where your fingers will collide with the colliders' UI element) (CHANGE IT TO FALSE WHEN YOU CREATE THE APK)")]
        public bool DebugThrowProjectile = false;
        [Tooltip("(Optional) You can choose between several pre-defined configuration, by default is NONE and it will use the parameters")]
        public CONFIGURATIONS_YOURVRUI DefaultConfiguration = CONFIGURATIONS_YOURVRUI.NONE;
        [Tooltip("(Optional) Place the camera you want to use, by default it will take the Camera.main")]
        public Camera MainCamera;
        [Tooltip("This shader will be applied on the UI elements and it allows to draw over everything else so the screen is not hidden by another object")]
        public Material MaterialDrawOnTop;
        [Tooltip("This shader RED will be applied on the UI elements and it allows to draw over everything else so the screen is not hidden by another object")]
        public Material MaterialRedDrawOnTop;
        [Tooltip("Default size that the screens can get")]
        public float GlobalScaleScreens = 2;

        [Tooltip("It will allow to operate in desktop mode, it will remove all the elements related to Google VR")]
        public bool EnableDesktopMode = false;
        [Tooltip("It will enable to move the camera with the mouse, only in desktop mode")]
        public bool EnableMoveCamera = false;
        [Tooltip("It will enable the default GVR Emulator")]
        public bool EnableGVREmulator = true;
        [Tooltip("It's the tag used when there is a collision with colliders to activate the screens")]
        public string TagPlayerDetectionCollision = "Player";

        [Tooltip("Enable the display of the screen with the collision triggering")]
        public bool EnableCollisionDetection = false;
        [Tooltip("Enable the display of the screen with the raycasting sight detection")]
        public bool EnableRaycastDetection = true;

        [Tooltip("Force all the screens to be in normal windows mode (Can be overriden by each individual screen)")]
        public bool ForceScreensInWindow = false;
        [Tooltip("Force all the screens to be aligned with the camera when they are displayed (Can be overriden by each individual screen)")]
        public bool ForceScreensAlignedToPlayer = false;
        [Tooltip("Force all the screens to be centered in the object when they ared displayed (Can be overriden by each individual screen)")]
        public bool ForceScreensInCenterObject = false;
        [Tooltip("Force all the screens to be not to be linked to the object when they ared displayed (Can be overriden by each individual screen)")]
        public bool ForceNotLinkedToObject = false;
        [Tooltip("Force all the screens to not use the highlight system to highlight the UI elements (Can be overriden by each individual screen)")]
        public bool ForceNotHighlightElements = false;
        [Tooltip("Force all the screens to appear at a certain distance from the camera (Can be overriden by each individual screen)")]
        public float ForceScreensDistanceObject = -1;
        [Tooltip("Force all the screens not to use the shader that allows the screen to ignore the ordering, so you can have the situation that some object can be between the screen and the camera (Can be overriden by each individual screen)")]
        public bool ForceUseZDepth = false;

        [Tooltip("Enable the daydream controller or the Gaze controller")]
        public bool EnableDaydreamController = false;

        [Tooltip("(Optional) You can call the screen by it's name, just place here the screen prefabs and use their name to call them. The other way is the one I explained in the video tutorial")]
        public GameObject[] ScreensPrefabs;
        [Tooltip("It's the graphic used to highlight the element. Remember that is automatically added unless you say the opposite")]
        public Sprite SelectorGraphic;

        [Tooltip("This sphere is for DEBUG purposes and it informs about the collision position of the raycasting")]
        public GameObject DotReferencePrefab;
        [Tooltip("This sphere is for DEBUG purposes and it's used to collide with the UI elements")]
        public GameObject DotTriggererPrefab;
        [Tooltip("This sphere is for DEBUG purposes and it's used to show the position where the player can teleport in Cardboard mode")]
        public GameObject DotPointerMovementPrefab;
        [Tooltip("Used for debug, get rid of it when you understand the whole thing")]
        public GUISkin SkinYourVRUI;

        [Tooltip("The collection of layers to apply the raycasting, using the Default layer if there are none")]
        public string[] LayersToRaycast;

        [Tooltip("Enable the cheat to set the resolution to adapt the UI screens to VR")]
        public bool EnableSetResolutionUIVR = false;
        public int ResolutionUIVRWidth = 350;
        public int ResolutionUIVRHeight = 720;

        [Tooltip("A virtual keyboard where to introduce text")]
        public GameObject VRKeyboard;

        [Tooltip("Maximum amount of characters allowed for the virtual keyboard to enter")]
        public int VRKeyboardInputMaxCharacters = 30;

        [Tooltip("Allow the items of a list be highlighted")]
        public bool DisplayHighlightedItemInList = true;

        [Tooltip("Enable the projection of a dot over a VR screen")]
        public bool EnableProjectionDot = true;
        public GameObject DotProjectionUI;

        [Tooltip("The default initial alpha of the screens")]
        public float DefaultInitialAlpha = 1;
        public float DefaultFinalAlpha = 1;
        public float DefaultTimeoutToAlpha = 0.5f;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private Camera m_camera;
        private List<GameObject> m_screensForever = new List<GameObject>();
        private List<GameObject> m_screensTemporal = new List<GameObject>();
        private bool m_enableScreens = true;
        private bool m_blockMouseMovement = false;
        private bool m_enableDebugTestingCode = true;
        private bool m_keysEnabled = false;
        private GameObject m_laserPointer = null;

        private GameObject m_screenToDestroy = null;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool IsDayDreamActivated
        {
            get
            {
                if (m_laserPointer != null)
                {
                    return m_laserPointer.activeSelf;
                }
                else
                {
                    return false;
                }
            }
        }
        public GameObject LaserPointer
        {
            get { return m_laserPointer; }
        }
        public float FinalScaleScreens
        {
            get { return GlobalScaleScreens / 1000; }
        }

        public bool BlockMouseMovement
        {
            get { return m_blockMouseMovement; }
            set { m_blockMouseMovement = value; }
        }

        public Camera GameCamera
        {
            get { return m_camera; }
            set { m_camera = value; }
        }

        public bool EnableDebugTestingCode
        {
            get { return m_enableDebugTestingCode; }
            set { m_enableDebugTestingCode = value; }
        }
        public bool KeysEnabled
        {
            get { return m_keysEnabled; }
            set { m_keysEnabled = value; }
        }
        public bool EnableScreens
        {
            get { return m_enableScreens; }
            set { m_enableScreens = value; }
        }
        public List<GameObject> ScreensTemporal
        {
            get { return m_screensTemporal; }
        }

        // -------------------------------------------
        /* 
		 * Initialitzation listener
		 */
        void Start()
        {
            if (DebugMode)
            {
                Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
            }

            int defaultConfiguration = PlayerPrefs.GetInt(DEFAULT_YOURVUI_CONFIGURATION, -1);
            if (defaultConfiguration != -1)
            {
                DefaultConfiguration = (CONFIGURATIONS_YOURVRUI)defaultConfiguration;
            }

            InitializePredefinedConfiguration();

            EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
            if (eventSystem != null)
            {
                if (eventSystem.gameObject.GetComponent<EventSystemController>() == null)
                {
                    eventSystem.gameObject.AddComponent<EventSystemController>();
                    eventSystem.gameObject.GetComponent<EventSystemController>().Initialitzation();
                }
            }

#if ENABLE_WORLDSENSE
            EnableDaydreamController = true;
            TagPlayerDetectionCollision = "MainCamera";
#else
            EnableDaydreamController = false;
#endif

#if !ENABLE_OCULUS && !ENABLE_HTCVIVE && UNITY_HAS_GOOGLEVR
            if (EnableDesktopMode)
            {
                GvrEditorEmulator gvrViewer = GameObject.FindObjectOfType<GvrEditorEmulator>();
                if (gvrViewer != null)
                {
                    GameObject.Destroy(gvrViewer.gameObject);
                    gvrViewer = null;
                }
                if (eventSystem != null)
                {
                    if (eventSystem.GetComponent<GvrPointerInputModule>() != null)
                    {
                        eventSystem.GetComponent<GvrPointerInputModule>().enabled = false;
                    }
                }
                GvrControllerInput gvrController = GameObject.FindObjectOfType<GvrControllerInput>();
                if (gvrController != null)
                {
                    GameObject.Destroy(gvrController.gameObject);
                    gvrController = null;
                }
            }
            GvrTrackedController controllerVisualManager = GameObject.FindObjectOfType<GvrTrackedController>();
            if (controllerVisualManager != null)
            {
                controllerVisualManager.gameObject.SetActive(EnableDaydreamController);
            }
            if (EnableDaydreamController)
            {
                InitDaydreamController();
            }
#else
            InitDaydreamController();
#endif

            if (MainCamera != null)
            {
                m_camera = MainCamera;
            }
            else
            {
                m_camera = Camera.main;
            }

#if !ENABLE_OCULUS && !ENABLE_HTCVIVE
#if UNITY_EDITOR && UNITY_HAS_GOOGLEVR
            if (!EnableGVREmulator)
            {
                GvrEditorEmulator gvrViewerEmulator = GameObject.FindObjectOfType<GvrEditorEmulator>();
                if (gvrViewerEmulator != null)
                {
                    GameObject.Destroy(gvrViewerEmulator.gameObject);
                    gvrViewerEmulator = null;
                }
            }
#endif
#endif

            KeysEventInputController.Instance.Initialization();
            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

            // ESSENTIAL PLAYER RAYCASTING
            if (GameObject.FindObjectOfType<PlayerRaycasterController>() != null)
            {
                GameObject.FindObjectOfType<PlayerRaycasterController>().Initialize();
            }
            else
            {
                Debug.LogError("YourVRUIScreenController::Start::PlayerRaycasterController NOT FOUND IN THE SYSTEM");
            }

#if !ENABLE_OCULUS && !ENABLE_HTCVIVE
#if UNITY_EDITOR && !ENABLE_WORLDSENSE && UNITY_HAS_GOOGLEVR
            EnableMoveCamera = true;
            if (GameObject.FindObjectOfType<GvrControllerInput>() != null) GameObject.FindObjectOfType<GvrControllerInput>().gameObject.SetActive(false);
            if (GameObject.FindObjectOfType<GvrEditorEmulator>() != null) GameObject.FindObjectOfType<GvrEditorEmulator>().gameObject.SetActive(false);
#else
            EnableMoveCamera = false;
#endif
#else
            EnableMoveCamera = false;
#endif
        }

#if ENABLE_OCULUS
        // -------------------------------------------
        /* 
		 * IsRightie
		 */
        private bool IsRightie()
        {
#if ENABLE_QUEST
            return true;
#else
            OVRPlugin.Handedness handedness = OVRPlugin.GetDominantHand();
            if (handedness == OVRPlugin.Handedness.RightHanded)
            {
                return true;
            }
            else
            {
                return false;
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * IsRightHandController
		 */
        public bool IsRightHandController(OVRControllerHelper _overHelper)
        {
            if (_overHelper.m_controller == OVRInput.Controller.RTouch)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
#endif

        // -------------------------------------------
        /* 
		 * Initialitzation of the daydream controller
		 */
        private void InitDaydreamController()
        {
#if ENABLE_OCULUS
            if (m_laserPointer == null)
            {
                bool lookForLaser = true;
                if (GameObject.FindObjectsOfType<HandRayToolView>() != null)
                {
                    HandRayToolView[] handRays = GameObject.FindObjectsOfType<HandRayToolView>();
                    for (int j = 0; j < handRays.Length; j++)
                    {
                        if (handRays[j].EnableState)
                        {
                            m_laserPointer = handRays[j].gameObject;
                            lookForLaser = false;
                        }
                    }
                }
                if (lookForLaser)
                {
                    if (m_laserPointer == null)
                    {
                        if (GameObject.FindObjectOfType<OVRControllerHelper>() != null)
                        {
                            OVRControllerHelper[] ovrTrackedRemotes = GameObject.FindObjectsOfType<OVRControllerHelper>();
                            bool isRightHand = KeysEventInputController.Instance.IsRightHanded();
                            GameObject ovrTrackedRemote = null;
                            for (int z = 0; z < ovrTrackedRemotes.Length; z++)
                            {
                                if ((IsRightHandController(ovrTrackedRemotes[z])) && isRightHand)
                                {
                                    ovrTrackedRemote = ovrTrackedRemotes[z].gameObject;
                                }
                                if ((!IsRightHandController(ovrTrackedRemotes[z])) && !isRightHand)
                                {
                                    ovrTrackedRemote = ovrTrackedRemotes[z].gameObject;
                                }                            
                            }
                            if (ovrTrackedRemote == null)
                            {
                                ovrTrackedRemote = GameObject.FindObjectOfType<OVRControllerHelper>().gameObject;
                            }
                            if (ovrTrackedRemote.GetComponentInChildren<LineRenderer>() != null)
                            {
                                m_laserPointer = ovrTrackedRemote.GetComponentInChildren<LineRenderer>().gameObject;
                                if (m_laserPointer.activeSelf)
                                {
                                    // WILL FORCE THE LASER POINTER WHEN RUNNING IN EDITOR
                                    if (DebugMode)
                                    {
                                        m_laserPointer.AddComponent<AlignWithCamera>();
                                    }
                                }
                            }
                        }
                    }
                }
            }
#elif ENABLE_HTCVIVE
            if (m_laserPointer == null)
            {
                HTCHandController deviceController = GameObject.FindObjectOfType<HTCHandController>();
                if (deviceController != null)
                {
                    if (deviceController.gameObject.GetComponentInChildren<LineRenderer>() != null)
                    {
                        m_laserPointer = deviceController.gameObject.GetComponentInChildren<LineRenderer>().gameObject;
                    }
                }
            }
#else
            if (EnableDaydreamController)
            {
                if (m_laserPointer == null)
                {
#if UNITY_HAS_GOOGLEVR
                    if (GameObject.FindObjectOfType<GvrLaserPointer>() != null)
                    {
                        m_laserPointer = GameObject.FindObjectOfType<GvrLaserPointer>().gameObject;
                        if (m_laserPointer.activeSelf)
                        {
                            // WILL FORCE THE LASER POINTER WHEN RUNNING IN EDITOR
                            if (DebugMode)
                            {
                                m_laserPointer.AddComponent<AlignWithCamera>();
                            }
                        }
                    }
#endif
                    if (GameObject.FindObjectOfType<CustomLaser>() != null)
                    {
                        m_laserPointer = GameObject.FindObjectOfType<CustomLaser>().gameObject;
                    }
                }
            }
#endif
            }

        // -------------------------------------------
        /* 
         * Will use one of the predefined configurations
         */
        public void InitializePredefinedConfiguration()
        {
            switch (DefaultConfiguration)
            {
                case CONFIGURATIONS_YOURVRUI.CONFIGURATION_VR_RAYCAST:
                    EnableDesktopMode = false;
                    EnableMoveCamera = true;
                    EnableCollisionDetection = false;
                    EnableRaycastDetection = true;
                    ForceScreensInWindow = false;
                    ForceScreensAlignedToPlayer = false;
                    ForceScreensInCenterObject = false;
                    ForceNotLinkedToObject = false;
                    ForceNotHighlightElements = false;
                    ForceScreensDistanceObject = -1;
                    ForceUseZDepth = false;
                    break;
                case CONFIGURATIONS_YOURVRUI.CONFIGURATION_VR_RAYCAST_FORCE_SCREEN_ALIGNED_TO_CAMERA:
                    EnableDesktopMode = false;
                    EnableMoveCamera = true;
                    EnableCollisionDetection = false;
                    EnableRaycastDetection = true;
                    ForceScreensInWindow = false;
                    ForceScreensAlignedToPlayer = true;
                    ForceScreensInCenterObject = true;
                    ForceNotLinkedToObject = false;
                    ForceNotHighlightElements = false;
                    ForceScreensDistanceObject = -1;
                    ForceUseZDepth = false;
                    break;
                case CONFIGURATIONS_YOURVRUI.CONFIGURATION_VR_COLLISION_FORCE_SCREEN_ALIGNED_TO_CAMERA:
                    EnableDesktopMode = false;
                    EnableMoveCamera = true;
                    EnableCollisionDetection = true;
                    EnableRaycastDetection = false;
                    ForceScreensInWindow = false;
                    ForceScreensAlignedToPlayer = true;
                    ForceScreensInCenterObject = false;
                    ForceNotLinkedToObject = true;
                    ForceNotHighlightElements = false;
                    ForceScreensDistanceObject = 1.5f;
                    ForceUseZDepth = false;
                    break;
                case CONFIGURATIONS_YOURVRUI.CONFIGURATION_COMPUTER_RAYCAST_WORLD_SCREENS:
                    EnableDesktopMode = true;
                    EnableMoveCamera = true;
                    EnableCollisionDetection = false;
                    EnableRaycastDetection = true;
                    ForceScreensInWindow = false;
                    ForceScreensAlignedToPlayer = false;
                    ForceScreensInCenterObject = false;
                    ForceNotLinkedToObject = false;
                    ForceNotHighlightElements = false;
                    ForceScreensDistanceObject = -1;
                    ForceUseZDepth = false;
                    EnableDaydreamController = false;
                    break;
                case CONFIGURATIONS_YOURVRUI.CONFIGURATION_COMPUTER_RAYCAST_NORMAL_SCREENS:
                    EnableDesktopMode = true;
#if UNITY_EDITOR
                    EnableMoveCamera = true;
#else
                    EnableMoveCamera = false;
#endif
                    EnableCollisionDetection = false;
                    EnableRaycastDetection = true;
                    ForceScreensInWindow = true;
                    ForceScreensAlignedToPlayer = false;
                    ForceScreensInCenterObject = false;
                    ForceNotLinkedToObject = false;
                    ForceNotHighlightElements = true;
                    ForceScreensDistanceObject = -1;
                    ForceUseZDepth = false;
                    EnableDaydreamController = false;
                    break;
                case CONFIGURATIONS_YOURVRUI.CONFIGURATION_COMPUTER_COLLISION_WORLD_SCREENS:
                    EnableDesktopMode = true;
                    EnableMoveCamera = true;
                    EnableCollisionDetection = true;
                    EnableRaycastDetection = false;
                    ForceScreensInWindow = false;
                    ForceScreensAlignedToPlayer = true;
                    ForceScreensInCenterObject = false;
                    ForceNotLinkedToObject = true;
                    ForceNotHighlightElements = false;
                    ForceScreensDistanceObject = 1.5f;
                    ForceUseZDepth = false;
                    EnableDaydreamController = false;
                    break;
                case CONFIGURATIONS_YOURVRUI.CONFIGURATION_COMPUTER_COLLISION_NORMAL_SCREENS:
                    EnableDesktopMode = true;
                    EnableMoveCamera = true;
                    EnableCollisionDetection = true;
                    EnableRaycastDetection = false;
                    ForceScreensInWindow = true;
                    ForceScreensAlignedToPlayer = false;
                    ForceScreensInCenterObject = false;
                    ForceNotLinkedToObject = false;
                    ForceNotHighlightElements = false;
                    ForceScreensDistanceObject = -1;
                    ForceUseZDepth = false;
                    EnableDaydreamController = false;
                    break;
            }
        }

        // -------------------------------------------
        /* 
		 * Release all resources
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
            if (_instance == null) return;

            UIEventController.Instance.UIEvent -= OnUIEvent;

            DestroyForeverScreens();
            DestroyScreens();

            Destroy(_instance);
            _instance = null;
        }

        // -------------------------------------------
        /* 
		 * Will create the HUD
		 */
        public void CreateHUD(string _nameScreen, float _distance, bool _ignoreZOrder = true)
        {
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
                                            false, // Override
                                            this.gameObject,
                                            null,  // GameObject collided
                                            _nameScreen,            // interactedObject.screenName,
                                            UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, // interactedObject.PreviousScreenAction,
                                            -1f, // interactedObject.DetectionDistance,
                                            !ForceScreensInWindow, // interactedObject.IsWorldObject,
                                            false, // interactedObject.ScreenLinkedToObject,
                                            false, // interactedObject.ScreenInCenterObject
                                            true, // interactedObject.ForceScreen,
                                            true, // interactedObject.ForceOrthographic,
                                            true, // interactedObject.AlignedToCamera,
                                            false, // interactedObject.UseCollisionPoint,
                                            _distance, // interactedObject.DistanceScreenDefault,
                                            false, // interactedObject.Refocus,
                                            false, // interactedObject.DestroyMessageOnDistance,
                                            1f, // interactedObject.ScaleScreen,
                                            false, // interactedObject.BlockOtherScreens,
                                            Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
                                            false, // Temporal Screen
                                            _ignoreZOrder, // interactedObject.IgnoreZOrderScreen,
                                            true, // HighlightSelector                                        
                                            null, // interactedObject.GetListPagesInformation()
                                            -1f // DELAY TO DESTROY
                                            );
        }

        // -------------------------------------------
        /* 
		 * Will create the HUD
		 */
        public void CreateHUD(GameObject _prefabScreen, float _distance, bool _ignoreZOrder = true, bool _isInWorld = true, float _scaleScreen = 1)
        {
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
                                            false, // Override
                                            this.gameObject,
                                            null,  // GameObject collided
                                            _prefabScreen,            // interactedObject.screenName,
                                            UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, // interactedObject.PreviousScreenAction,
                                            -1f, // interactedObject.DetectionDistance,
                                            _isInWorld, // interactedObject.IsWorldObject,
                                            false, // interactedObject.ScreenLinkedToObject,
                                            false, // interactedObject.ScreenInCenterObject
                                            true, // interactedObject.ForceScreen,
                                            true, // interactedObject.ForceOrthographic,
                                            true, // interactedObject.AlignedToCamera,
                                            false, // interactedObject.UseCollisionPoint,
                                            _distance, // interactedObject.DistanceScreenDefault,
                                            false, // interactedObject.Refocus,
                                            false, // interactedObject.DestroyMessageOnDistance,
                                            _scaleScreen, // interactedObject.ScaleScreen,
                                            false, // interactedObject.BlockOtherScreens,
                                            Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
                                            false, // Temporal Screen
                                            _ignoreZOrder, // interactedObject.IgnoreZOrderScreen,
                                            true, // HighlightSelector                                        
                                            null, // interactedObject.GetListPagesInformation()
                                            -1f // DELAY TO DESTROY
                                            );
        }

        // -------------------------------------------
        /* 
		 * Will create the HUD
		 */
        public void CreateDelayHUD(string _nameScreen, float _distance, bool _ignoreZOrder = true, float _delay = 1f)
        {
            UIEventController.Instance.DelayUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN, _delay,
                                            false, // Override
                                            this.gameObject,
                                            null,  // GameObject collided
                                            _nameScreen,            // interactedObject.screenName,
                                            UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, // interactedObject.PreviousScreenAction,
                                            -1f, // interactedObject.DetectionDistance,
                                            !ForceScreensInWindow, // interactedObject.IsWorldObject,
                                            false, // interactedObject.ScreenLinkedToObject,
                                            false, // interactedObject.ScreenInCenterObject
                                            true, // interactedObject.ForceScreen,
                                            true, // interactedObject.ForceOrthographic,
                                            true, // interactedObject.AlignedToCamera,
                                            false, // interactedObject.UseCollisionPoint,
                                            _distance, // interactedObject.DistanceScreenDefault,
                                            false, // interactedObject.Refocus,
                                            false, // interactedObject.DestroyMessageOnDistance,
                                            1f, // interactedObject.ScaleScreen,
                                            false, // interactedObject.BlockOtherScreens,
                                            Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
                                            false, // Temporal Screen
                                            _ignoreZOrder, // interactedObject.IgnoreZOrderScreen,
                                            true, // HighlightSelector                                        
                                            null, // interactedObject.GetListPagesInformation()
                                            -1f // DELAY TO DESTROY
                                            );
        }

        // -------------------------------------------
        /* 
		 * Will create a screen linked to the camera
		 */
        public void CreateScreenLinkedToCamera(string _nameScreen, object _pages, float _distance, float _delayToDestroy, bool _forceOrthographic = false, float _scaleScreen = -1f, UIScreenTypePreviousAction _typePreviousAction = UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, bool _ignoreZOrder = true, bool _isTemporal = true, int _layer = 0, bool _refocus = true, bool _alignedToCamera = true)
        {
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
                                                           true,
                                                           this.gameObject,
                                                           null,  // GameObject collided
                                                           _nameScreen,            // interactedObject.screenName,
                                                           _typePreviousAction, // interactedObject.PreviousScreenAction,
                                                           -1f, // interactedObject.DetectionDistance,
                                                           !ForceScreensInWindow, // interactedObject.IsWorldObject,
                                                           false, // interactedObject.ScreenLinkedToObject,
                                                           false, // interactedObject.ScreenInCenterObject,
                                                           true, // interactedObject.ForceScreen,
                                                           _forceOrthographic, // interactedObject.ForceOrthographic,
                                                           _alignedToCamera, // interactedObject.AlignedToCamera,
                                                           false, // interactedObject.UseCollisionPoint,
                                                           _distance, // interactedObject.DistanceScreenDefault,
                                                           _refocus, // interactedObject.Refocus,
                                                           false, // interactedObject.DestroyMessageOnDistance,
                                                           _scaleScreen, // interactedObject.ScaleScreen,
                                                           false, // interactedObject.BlockOtherScreens,
                                                           Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
                                                           _isTemporal, // Temporal Screen
                                                           _ignoreZOrder, // interactedObject.IgnoreZOrderScreen,
                                                           true, // HighlightSelector                                                       
                                                           _pages,
                                                           _delayToDestroy,
                                                           _layer
                                                           );
        }

        // -------------------------------------------
        /* 
		 * Will create a screen linked to the camera
		 */
        public void DelayScreenLinkedToCamera(string _nameScreen, object _pages, float _distance, float _delayToDestroy, bool _forceOrthographic = false, float _scaleScreen = -1f, UIScreenTypePreviousAction _typePreviousAction = UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, bool _ignoreZOrder = true, float _delay = 0.5f, int _layer = 0, bool _refocus = true, bool _alignedToCamera = true)
        {
            UIEventController.Instance.DelayUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN, _delay,
                                                           true,
                                                           this.gameObject,
                                                           null,  // GameObject collided
                                                           _nameScreen,            // interactedObject.screenName,
                                                           _typePreviousAction, // interactedObject.PreviousScreenAction,
                                                           -1f, // interactedObject.DetectionDistance,
                                                           !ForceScreensInWindow, // interactedObject.IsWorldObject,
                                                           false, // interactedObject.ScreenLinkedToObject,
                                                           false, // interactedObject.ScreenInCenterObject,
                                                           true, // interactedObject.ForceScreen,
                                                           _forceOrthographic, // interactedObject.ForceOrthographic,
                                                           _alignedToCamera, // interactedObject.AlignedToCamera,
                                                           false, // interactedObject.UseCollisionPoint,
                                                           _distance, // interactedObject.DistanceScreenDefault,
                                                           _refocus, // interactedObject.Refocus,
                                                           false, // interactedObject.DestroyMessageOnDistance,
                                                           _scaleScreen, // interactedObject.ScaleScreen,
                                                           false, // interactedObject.BlockOtherScreens,
                                                           Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
                                                           true, // Temporal Screen
                                                           _ignoreZOrder, // interactedObject.IgnoreZOrderScreen,
                                                           true, // HighlightSelector                                                       
                                                           _pages,
                                                           _delayToDestroy,
                                                           _layer
                                                           );
        }

        // -------------------------------------------
        /* 
		 * Will create a screen linked to the camera
		 */
        public void CreateScreenLinkedToCamera(GameObject _prefabScreen, object _pages, float _distance, float _delayToDestroy, bool _forceOrthographic = false, float _scaleScreen = -1f, UIScreenTypePreviousAction _typePreviousAction = UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, bool _isTemporal = true, int _layer = 0)
        {
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
                                                           true,
                                                           this.gameObject,
                                                           null,  // GameObject collided
                                                           _prefabScreen,            // interactedObject.screenName,
                                                           _typePreviousAction, // interactedObject.PreviousScreenAction,
                                                           -1f, // interactedObject.DetectionDistance,
                                                           !ForceScreensInWindow, // interactedObject.IsWorldObject,
                                                           false, // interactedObject.ScreenLinkedToObject,
                                                           false, // interactedObject.ScreenInCenterObject,
                                                           true, // interactedObject.ForceScreen,
                                                           _forceOrthographic, // interactedObject.ForceOrthographic,
                                                           true, // interactedObject.AlignedToCamera,
                                                           false, // interactedObject.UseCollisionPoint,
                                                           _distance, // interactedObject.DistanceScreenDefault,
                                                           true, // interactedObject.Refocus,
                                                           false, // interactedObject.DestroyMessageOnDistance,
                                                           _scaleScreen, // interactedObject.ScaleScreen,
                                                           false, // interactedObject.BlockOtherScreens,
                                                           Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
                                                           _isTemporal, // Temporal Screen
                                                           true, // interactedObject.IgnoreZOrderScreen,
                                                           true, // HighlightSelector                                                       
                                                           _pages,
                                                           _delayToDestroy,
                                                           _layer
                                                           );
        }

        // -------------------------------------------
        /* 
		 * Will create a screen linked to the camera
		 */
        public void CreateScreenLinkedToCamera(GameObject _prefabScreen, object _pages, float _distance, float _delayToDestroy, bool _forceOrthographic = false, float _scaleScreen = -1f, UIScreenTypePreviousAction _typePreviousAction = UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, int _layer = 0)
        {
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
                                                           true,
                                                           this.gameObject,
                                                           null,  // GameObject collided
                                                           _prefabScreen,            // interactedObject.screenName,
                                                           _typePreviousAction, // interactedObject.PreviousScreenAction,
                                                           -1f, // interactedObject.DetectionDistance,
                                                           !ForceScreensInWindow, // interactedObject.IsWorldObject,
                                                           false, // interactedObject.ScreenLinkedToObject,
                                                           false, // interactedObject.ScreenInCenterObject,
                                                           true, // interactedObject.ForceScreen,
                                                           _forceOrthographic, // interactedObject.ForceOrthographic,
                                                           true, // interactedObject.AlignedToCamera,
                                                           false, // interactedObject.UseCollisionPoint,
                                                           _distance, // interactedObject.DistanceScreenDefault,
                                                           true, // interactedObject.Refocus,
                                                           false, // interactedObject.DestroyMessageOnDistance,
                                                           _scaleScreen, // interactedObject.ScaleScreen,
                                                           false, // interactedObject.BlockOtherScreens,
                                                           Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
                                                           true, // Temporal Screen
                                                           true, // interactedObject.IgnoreZOrderScreen,
                                                           true, // HighlightSelector                                                       
                                                           _pages,
                                                           _delayToDestroy,
                                                           _layer
                                                           );
        }

        // -------------------------------------------
        /* 
		 * Will create a screen linked to the camera
		 */
        public void CreateScreenLinkedToObject(GameObject _prefabScreen, GameObject _target, object _pages, float _distance, float _delayToDestroy, bool _forceOrthographic = false, float _scaleScreen = -1f, UIScreenTypePreviousAction _typePreviousAction = UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, int _layer = 0, bool _isTemporal = true)
        {
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN,
                                                           true,
                                                           this.gameObject,
                                                           null,  // GameObject collided
                                                           _prefabScreen,            // interactedObject.screenName,
                                                           _typePreviousAction, // interactedObject.PreviousScreenAction,
                                                           -1f, // interactedObject.DetectionDistance,
                                                           true, // interactedObject.IsWorldObject,
                                                           true, // interactedObject.ScreenLinkedToObject,
                                                           true, // interactedObject.ScreenInCenterObject,
                                                           false, // interactedObject.ForceScreen,
                                                           false, // interactedObject.ForceOrthographic,
                                                           false, // interactedObject.AlignedToCamera,
                                                           false, // interactedObject.UseCollisionPoint,
                                                           _distance, // interactedObject.DistanceScreenDefault,
                                                           true, // interactedObject.Refocus,
                                                           false, // interactedObject.DestroyMessageOnDistance,
                                                           _scaleScreen, // interactedObject.ScaleScreen,
                                                           false, // interactedObject.BlockOtherScreens,
                                                           Utilities.IgnoreLayersForDebug, // IgnoreLayers, 
                                                           _isTemporal, // Temporal Screen
                                                           true, // interactedObject.IgnoreZOrderScreen,
                                                           true, // HighlightSelector
                                                           _pages,
                                                           _delayToDestroy,
                                                           _layer,
                                                           _target
                                                           );
        }

        // -------------------------------------------
        /* 
		 * Get the bounds of the object
		 */
        private Vector2 GetMaxMinBounds(GameObject _gameObject)
        {
            float maxSize = 0;
            if (_gameObject.GetComponent<Collider>() != null)
            {
                maxSize = _gameObject.GetComponent<Collider>().bounds.size.x;
                if (maxSize < _gameObject.GetComponent<Collider>().bounds.size.z)
                {
                    maxSize = _gameObject.GetComponent<Collider>().bounds.size.z;
                }
            }

            float minSize = 0;
            if (_gameObject.GetComponent<Collider>() != null)
            {
                minSize = _gameObject.GetComponent<Collider>().bounds.size.x;
                if (maxSize > _gameObject.GetComponent<Collider>().bounds.size.z)
                {
                    minSize = _gameObject.GetComponent<Collider>().bounds.size.z;
                }
            }

            return new Vector2(minSize, maxSize);
        }

        // -------------------------------------------
        /* 
		 * Destroy all the screens in memory
		 */
        private void CreateQuietDotReference(Vector3 _position, bool _force)
        {
            if (DebugMode || _force)
            {
                Instantiate(DotReferencePrefab, _position, new Quaternion());
            }
        }

        // -------------------------------------------
        /* 
		 * Destroy all the screens in memory
		 */
        private void CreateMovingDotReference(Vector3 _origin, Vector3 _forward, float _speed)
        {
            GameObject dot = (GameObject)Instantiate(DotTriggererPrefab, _origin, new Quaternion());
            dot.AddComponent<AutoProjectile>();
            dot.GetComponent<AutoProjectile>().Initialize(_origin, _forward, _speed);
        }

        // -------------------------------------------
        /* 
		 * Destroy all the screens in memory
		 */
        public void DestroyScreens()
        {
            // DESTROY TEMPORAL
            List<GameObject> listNotToDestroy = new List<GameObject>();
            for (int i = 0; i < m_screensTemporal.Count; i++)
            {
                bool isNotToDestroy = false;
                if (m_screensTemporal[i] != null)
                {
                    if (m_screensTemporal[i].GetComponent<IBasicView>() != null)
                    {
                        if (m_screensTemporal[i].GetComponent<IBasicView>().MustBeDestroyed)
                        {
                            m_screensTemporal[i].GetComponent<IBasicView>().Destroy();
                        }
                        else
                        {
                            listNotToDestroy.Add(m_screensTemporal[i]);
                            isNotToDestroy = true;
                        }
                    }
                    if (!isNotToDestroy)
                    {
                        if (i < m_screensTemporal.Count)
                        {
                            if (m_screensTemporal[i] != null)
                            {
                                GameObject.Destroy(m_screensTemporal[i]);
                                m_screensTemporal[i] = null;
                            }
                        }
                    }
                }
            }
            m_screensTemporal.Clear();
            if (listNotToDestroy.Count > 0)
            {
                for (int i = 0; i < listNotToDestroy.Count; i++)
                {
                    m_screensTemporal.Add(listNotToDestroy[i]);
                }
                listNotToDestroy.Clear();
            }
            KeysEventInputController.Instance.TemporalNumberScreensActive = m_screensTemporal.Count;
        }

        // -------------------------------------------
        /* 
		 * DestroyForeverScreens
		 */
        private void DestroyForeverScreens()
        {
            // DESTROY FOREVER
            List<GameObject> listNotToDestroy = new List<GameObject>();
            for (int i = 0; i < m_screensForever.Count; i++)
            {
                if (m_screensForever[i] != null)
                {
                    bool isNotToDestroy = false;
                    if (m_screensForever[i].GetComponent<IBasicView>() != null)
                    {
                        if (m_screensForever[i].GetComponent<IBasicView>().MustBeDestroyed)
                        {
                            m_screensForever[i].GetComponent<IBasicView>().Destroy();
                        }
                        else
                        {
                            listNotToDestroy.Add(m_screensForever[i]);
                            isNotToDestroy = true;
                        }
                    }
                    if (!isNotToDestroy)
                    {
                        if (i < m_screensForever.Count)
                        {
                            if (m_screensForever[i] != null)
                            {
                                GameObject.Destroy(m_screensForever[i]);
                                m_screensForever[i] = null;
                            }
                        }
                    }
                }
            }
            m_screensForever.Clear();
            if (listNotToDestroy.Count > 0)
            {
                for (int i = 0; i < listNotToDestroy.Count; i++)
                {
                    m_screensForever.Add(listNotToDestroy[i]);
                }
                listNotToDestroy.Clear();
            }
        }


        // -------------------------------------------
        /* 
		 * Destroy all the screens that are null
		 */
        public void DestroyNullScreens()
        {
            for (int i = 0; i < m_screensTemporal.Count; i++)
            {
                bool isNull = false;
                if (m_screensTemporal[i] != null)
                {
                    if (m_screensTemporal[i].GetComponent<IBasicView>() == null)
                    {
                        isNull = true;
                    }
                }
                else
                {
                    isNull = true;
                }
                if (isNull)
                {
                    m_screensTemporal.RemoveAt(i);
                    i--;
                    Debug.LogError("YourVRUIController::REMOVED NULL SCREEN IN m_screensTemporal");
                }
            }
            for (int i = 0; i < m_screensForever.Count; i++)
            {
                bool isNull = false;
                if (m_screensForever[i] != null)
                {
                    if (m_screensForever[i].GetComponent<IBasicView>() == null)
                    {
                        isNull = true;
                    }
                }
                else
                {
                    isNull = true;
                }
                if (isNull)
                {
                    m_screensForever.RemoveAt(i);
                    i--;
                    Debug.LogError("YourVRUIController::REMOVED NULL SCREEN IN m_screensForever");
                }
            }

            KeysEventInputController.Instance.TemporalNumberScreensActive = m_screensTemporal.Count;
        }

        private InputField m_inputFieldVR;
        private GameObject m_vrKeyboard;

        // -------------------------------------------
        /* 
		 * Manager of global events
		 */
        private void OnUIEvent(string _nameEvent, params object[] _list)
        {
            if (_nameEvent == ScreenController.EVENT_SCREENCONTROLLER_REMOVE_SCREEN_NAME)
            {
                ScreenController sc = GameObject.FindObjectOfType<ScreenController>();
                if (sc != null)
                {
                    sc.RemoveScreenName((string)_list[0]);
                }
            }
            if (_nameEvent == ButtonVRView.EVENT_BUTTONVR_REQUEST_LAYER_INFORMATION)
            {
                UIEventController.Instance.DispatchUIEvent(ButtonVRView.EVENT_BUTTONVR_RESPONSE_LAYER_INFORMATION, _list[0], MaximumCurrentLayerScreenActive());
            }
            if (_nameEvent == ScreenVRKeyboardView.EVENT_SCREENVRKEYBOARD_CONFIRM_INPUT)
            {
                m_inputFieldVR = null;
            }
            if (_nameEvent == ButtonVRView.EVENT_BUTTONVR_SELECTED_INPUTFIELD)
            {
                if ((GameObject.FindObjectOfType<ScreenVRKeyboardView>() == null) && (m_inputFieldVR == null))
                {
                    m_inputFieldVR = (InputField)_list[0];
                    Vector3 inputWorldPosition = m_inputFieldVR.transform.position;
                    CreateScreenLinkedToCamera(VRKeyboard, m_inputFieldVR, 2.5f, -1, false, -1f, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, true, ScreenController.TOTAL_LAYERS_SCREENS - 1);
                }
            }
            if (DebugThrowProjectile)
            {
                if (_nameEvent == KeysEventInputController.ACTION_SECONDARY_BUTTON_DOWN)
                {
                    CreateMovingDotReference(m_camera.transform.position, m_camera.transform.forward, 5);
                }
            }
            if (_nameEvent == BaseVRScreenView.EVENT_SCREEN_OPEN_VIEW)
            {
                m_enableScreens = !(bool)_list[1];
            }
            if (_nameEvent == BaseVRScreenView.EVENT_SCREEN_CLOSE_VIEW)
            {
                m_enableScreens = true;
                GameObject screen = (GameObject)_list[0];
                DestroyGameObjectSingleScreen(screen, false);
            }
            if (_nameEvent == InteractionController.EVENT_INTERACTIONCONTROLLER_SCREEN_DESTROYED)
            {
                m_blockMouseMovement = false;
            }
            if (_nameEvent == EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT)
            {
                m_keysEnabled = (bool)_list[0];
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN)
            {
                m_enableScreens = true;
                m_screenToDestroy = (GameObject)_list[0];
                Invoke("DestroySpecificScreen", 0.05f);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN_BY_NAME)
            {
                m_enableScreens = true;
                string screenName = (string)_list[0];
                GameObject screen = GameObject.Find(screenName);
                if (screen != null)
                {
                    DestroyGameObjectSingleScreen(screen, true);
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_ALL_SCREEN)
            {
                DestroyScreens();
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_FOREVER_SCREEN)
            {
                DestroyForeverScreens();
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_REPORT_DESTROYED)
            {
                DestroyNullScreens();
            }
            if (m_enableScreens)
            {
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_MOVE_TEMPORAL_TO_FOREVER)
                {
                    GameObject screenToMove = (GameObject)_list[0];
                    if (m_screensTemporal.Remove(screenToMove))
                    {
                        if (_list.Length > 1)
                        {
                            screenToMove.SetActive((bool)_list[1]);
                        }
                        else
                        {
                            screenToMove.SetActive(false);
                        }
                        m_screensForever.Add(screenToMove);
                    }
                }
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_MOVE_FOREVER_TO_TEMPORAL)
                {
                    GameObject screenToMove = (GameObject)_list[0];
                    if (m_screensForever.Remove(screenToMove))
                    {
                        screenToMove.SetActive(true);
                        m_screensTemporal.Add(screenToMove);
                    }
                }
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_VR_OPEN_GENERIC_SCREEN)
                {
                    bool overrideGlobalSetting = (bool)_list[0];
                    GameObject playerInteracted = (GameObject)_list[1];
                    GameObject originCharacter = (GameObject)_list[2];
                    string screenName = null;
                    GameObject currentPrefab = null;
                    if (_list[3] is string)
                    {
                        screenName = (string)_list[3];
                    }
                    else
                    {
                        currentPrefab = (GameObject)_list[3];
                        screenName = currentPrefab.name;
                    }
                    if (DebugMode)
                    {
                        Debug.Log("EVENT_SCREENMANAGER_OPEN_SCREEN::Creating the screen[" + screenName + "]");
                    }
                    UIScreenTypePreviousAction previousScreenAction = (UIScreenTypePreviousAction)_list[4];
                    float detectionDistance = (float)_list[5];
                    bool isWorldObject = (bool)_list[6];
                    if (ForceScreensInWindow)
                    {
                        isWorldObject = false;
                    }
                    bool screenLinkedToObject = (bool)_list[7];
                    if (ForceNotLinkedToObject && !overrideGlobalSetting)
                    {
                        screenLinkedToObject = false;
                    }
                    bool screenInCenterObject = (bool)_list[8];
                    if (ForceScreensInCenterObject && !overrideGlobalSetting)
                    {
                        screenInCenterObject = true;
                    }
                    bool forceScreen = (bool)_list[9];
                    bool forceOrthographic = (bool)_list[10];
                    bool alignedToCamera = (bool)_list[11];
                    if (ForceScreensAlignedToPlayer && !overrideGlobalSetting)
                    {
                        alignedToCamera = true;
                    }
                    bool useCollisionPoint = (bool)_list[12];
                    float distanceObj = (float)_list[13];
                    if ((ForceScreensDistanceObject > -1) && !overrideGlobalSetting)
                    {
                        distanceObj = ForceScreensDistanceObject;
                    }
                    bool refocus = (bool)_list[14];
                    bool destroyMessageOnDistance = (bool)_list[15];
                    float scaleScreen = (float)_list[16];
                    bool blockOtherScreens = (bool)_list[17];
                    string[] ignoreLayers = (string[])_list[18];
                    bool isTemporalScreen = (bool)_list[19];
                    bool screenZOrderIgnore = (bool)_list[20];
                    if (ForceUseZDepth)
                    {
                        screenZOrderIgnore = false;
                    }
                    bool highlightSelector = (bool)_list[21];
                    if (ForceNotHighlightElements)
                    {
                        highlightSelector = false;
                    }
                    float delayToDestroy = -1;
                    if (_list.Length > 23)
                    {
                        delayToDestroy = (float)_list[23];
                    }
                    int layerScreenDestroy = 0;
                    if (_list.Length > 24)
                    {
                        layerScreenDestroy = (int)_list[24];
                    }
                    GameObject targetObject = null;
                    if (_list.Length > 25)
                    {
                        if (_list[25] is GameObject)
                        {
                            targetObject = (GameObject)_list[25];
                        }
                    }

                    if (isTemporalScreen && EnableDesktopMode && !isWorldObject)
                    {
                        BlockMouseMovement = true;
                    }

                    // ACTION BEFORE INITIALIZATON OF THE NEXT SCREEN
                    switch (previousScreenAction)
                    {
                        case UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN:
                            if (m_screensTemporal.Count > 0)
                            {
                                m_screensTemporal[m_screensTemporal.Count - 1].SetActive(false);
                            }
                            break;

                        case UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN:
                            if (m_screensTemporal.Count > 0)
                            {
                                m_screensTemporal[m_screensTemporal.Count - 1].SetActive(false);
                            }
                            break;

                        case UIScreenTypePreviousAction.DESTROY_CURRENT_SCREEN:
                            if (m_screensTemporal.Count > 0)
                            {
                                GameObject sCurrentScreen = m_screensTemporal[m_screensTemporal.Count - 1];
                                if (sCurrentScreen.GetComponent<IBasicView>() != null)
                                {
                                    sCurrentScreen.GetComponent<IBasicView>().Destroy();
                                }
                                GameObject.Destroy(sCurrentScreen);
                                m_screensTemporal.RemoveAt(m_screensTemporal.Count - 1);
                            }
                            break;

                        case UIScreenTypePreviousAction.DESTROY_ALL_SCREENS:
                            DestroyScreens();
                            break;
                    }

                    // CREATION OF THE NEW SCREEN
                    bool applyCentered = EnableSetResolutionUIVR;
                    if (EnableSetResolutionUIVR)
                    {
                        if (GameObject.FindObjectOfType<CardboardLoaderVR>() != null)
                        {
                            if (!GameObject.FindObjectOfType<CardboardLoaderVR>().IsCardboardDevice())
                            {
                                if (currentPrefab != null)
                                {
                                    applyCentered = (currentPrefab.GetComponentInChildren<ScreenVRKeyboardView>() == null);
                                    if (applyCentered)
                                    {
                                        Screen.SetResolution(ResolutionUIVRWidth, ResolutionUIVRHeight, true);
                                    }
                                }
                                else
                                {
                                    Screen.SetResolution(ResolutionUIVRWidth, ResolutionUIVRHeight, true);
                                }
                            }
                        }
                    }
                    GameObject currentScreen = null;
                    if (currentPrefab != null)
                    {
                        currentScreen = CreateUIScreen(currentPrefab, overrideGlobalSetting, isWorldObject, screenLinkedToObject, screenInCenterObject, forceScreen, forceOrthographic, alignedToCamera, useCollisionPoint, distanceObj, refocus, ignoreLayers, scaleScreen, targetObject);
                        // ++ YOU SHOULD INITIALIZE HERE YOUR OWN SCREEN BEFORE INITIALIZING THE BASE SCREEN CLASS ++
                        if (currentScreen.GetComponent<IBasicView>() != null)
                        {
                            // Debug.LogError("CREATING NEW SCREEN["+ screenName + "]::_list[22]="+ _list[22]);
                            currentScreen.GetComponent<IBasicView>().Initialize(_list[22]);
                            currentScreen.GetComponent<IBasicView>().NameOfScreen = currentPrefab.name;
                            if (applyCentered) currentScreen.GetComponent<IBasicView>().ApplyCentered();
                            currentScreen.GetComponent<IBasicView>().SetLayer(layerScreenDestroy);
                        }
                        if (delayToDestroy > 0)
                        {
                            Destroy(currentScreen, delayToDestroy);
                        }
                    }
                    else
                    {
                        // ++ YOU SHOULD INITIALIZE HERE YOUR OWN SCREEN BEFORE INITIALIZING THE BASE SCREEN CLASS ++
                        for (int i = 0; i < ScreensPrefabs.Length; i++)
                        {
                            if (ScreensPrefabs[i] != null)
                            {
                                if (ScreensPrefabs[i].name == screenName)
                                {
                                    // Debug.LogError("CREATING NEW SCREEN["+ screenName + "]");
                                    currentScreen = CreateUIScreen(ScreensPrefabs[i], overrideGlobalSetting, isWorldObject, screenLinkedToObject, screenInCenterObject, forceScreen, forceOrthographic, alignedToCamera, useCollisionPoint, distanceObj, refocus, ignoreLayers, scaleScreen, targetObject);
                                    currentScreen.GetComponent<IBasicView>().Initialize(_list[22], originCharacter, blockOtherScreens);
                                    currentScreen.GetComponent<IBasicView>().NameOfScreen = screenName;
                                    if (applyCentered) currentScreen.GetComponent<IBasicView>().ApplyCentered();
                                    currentScreen.GetComponent<IBasicView>().SetLayer(layerScreenDestroy);
                                    currentScreen.gameObject.name = screenName;
                                    if (delayToDestroy > 0)
                                    {
                                        Destroy(currentScreen, delayToDestroy);
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    ScreenController sc = GameObject.FindObjectOfType<ScreenController>();
                    if (sc != null)
                    {
                        sc.AddScreenNameToStack(screenName, previousScreenAction);
                    }

                    // Debug.LogError("YourVRUIScreenController::OnUIEvent::currentScreen=" + currentScreen.name + "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

                    // NOW THE BASE SCREEN CLASS INITIALIZES HAD THE BEHAVIOUR SO YOU CAN FORGET ABOUT THE SCREEN IN VR
                    if (currentScreen.GetComponent<BaseVRScreenView>() == null)
                    {
                        currentScreen.AddComponent<BaseVRScreenView>();
                    }
                    currentScreen.GetComponent<BaseVRScreenView>().InitBaseScreen(originCharacter, blockOtherScreens, highlightSelector);

                    if (destroyMessageOnDistance)
                    {
                        currentScreen.GetComponent<BaseVRScreenView>().DestroyMessageOnDistance(playerInteracted, 1.2f * detectionDistance);
                    }
                    if (forceScreen && !screenLinkedToObject && !forceOrthographic)
                    {
                        currentScreen.GetComponent<BaseVRScreenView>().SetLinkToPlayer(Utilities.ClonePoint(m_camera.transform.forward), distanceObj, refocus);
                    }
                    if (screenZOrderIgnore)
                    {
                        Utilities.ApplyMaterialOnImages(currentScreen, MaterialDrawOnTop);
                    }

                    // ADDING THE SCREEN TO THE RIGHT LIST
                    if (isTemporalScreen)
                    {
                        m_screensTemporal.Add(currentScreen);
                    }
                    else
                    {
                        m_screensForever.Add(currentScreen);
                    }
                    KeysEventInputController.Instance.TemporalNumberScreensActive = m_screensTemporal.Count;
                }
            }
        }

        // -------------------------------------------
        /* 
		 * DestroySpecificScreen
		 */
        private void DestroySpecificScreen()
        {
            DestroyGameObjectSingleScreen(m_screenToDestroy, true);
            ActivationLastScreen(true);
        }

        // -------------------------------------------
        /* 
		 * Remove the screen from the list of screens
		 */
        private void DestroyGameObjectSingleScreen(GameObject _screen, bool _runDestroy)
        {
            if (_screen == null) return;

            for (int i = 0; i < m_screensTemporal.Count; i++)
            {
                GameObject screen = (GameObject)m_screensTemporal[i];
                if (_screen == screen)
                {
                    if (_runDestroy)
                    {
                        screen.GetComponent<IBasicView>().Destroy();
                    }
                    if (screen != null)
                    {
                        GameObject.Destroy(screen);
                        if (i < m_screensTemporal.Count)
                        {
                            m_screensTemporal.RemoveAt(i);
                            KeysEventInputController.Instance.TemporalNumberScreensActive = m_screensTemporal.Count;
                        }
                    }
                    return;
                }
            }

            for (int i = 0; i < m_screensForever.Count; i++)
            {
                GameObject screen = (GameObject)m_screensForever[i];
                if (_screen == screen)
                {
                    if (_runDestroy)
                    {
                        screen.GetComponent<IBasicView>().Destroy();
                    }
                    if (screen != null)
                    {
                        GameObject.Destroy(screen);
                        if (i < m_screensForever.Count)
                        {
                            m_screensForever.RemoveAt(i);
                        }
                    }
                    return;
                }
            }
        }


        // -------------------------------------------
        /* 
		 * Set the activation of the last screen
		 */
        public void ActivationLastScreen(bool _enable)
        {
            if (m_screensTemporal.Count > 0)
            {
                m_screensTemporal[m_screensTemporal.Count - 1].SetActive(_enable);
            }
        }

        // -------------------------------------------
        /* 
		 * Activation of all the screens
		 */
        public void ActivationScreens(bool _enable)
        {
            for (int i = 0; i < m_screensTemporal.Count; i++)
            {
                m_screensTemporal[i].SetActive(_enable);
            }
        }

        // -------------------------------------------
        /* 
		 * MaximumCurrentLayerScreenActive
		 */
        public int MaximumCurrentLayerScreenActive()
        {
            BaseVRScreenView[] baseVRScreens = GameObject.FindObjectsOfType<BaseVRScreenView>();
            int maxLayer = 0;
            for (int i = 0; i < baseVRScreens.Length; i++)
            {
                if (baseVRScreens[i].LayerScreen > maxLayer)
                {
                    maxLayer = baseVRScreens[i].LayerScreen;
                }
            }
            return maxLayer;
        }


        // -------------------------------------------
        /* 
            * Will create the screen with the collection of parameters
            * 
            * @param _screenPrefab This is the screen we want to create an instance
            * @param _overrideGlobalSetting We override the global setting with the local settings
            * @param _isWorldObject If it's not a world object then we display the screen as a normal screen
            * @param _screenLinkedToObject The screen is linked to the object (we consider object's position to place the screen on the world) or related to the player's camera position
            * @param _screenInCenterObject The screen should be created in the center of the object
            * @param _forceScreen We force the screen not to disappear until player has interact with it
            * @param _forceOrthographic We force the orthographic mode
            * @param _alignedToCamera If the screen should be aligned to the camera
            * @param _useCollisionPoint If we must used the collision point to place the screen
            * @param _distance The distance we should place the screen
            * @param _refocus If the screen should realign with the camera after it's not visible
            * @param _ignoreLayers The layers to ignore
            * @param _scaleScreen The scale of the screen
            */
        public GameObject CreateUIScreen(GameObject _screenPrefab,
                                    bool _overrideGlobalSetting,
                                    bool _isWorldObject,
                                    bool _screenLinkedToObject,
                                    bool _screenInCenterObject,
                                    bool _forceScreen,
                                    bool _forceOrthographic,
                                    bool _alignedToCamera,
                                    bool _useCollisionPoint,
                                    float _distance,
                                    bool _refocus,
                                    string[] _ignoreLayers,
                                    float _scaleScreen,
                                    GameObject _targetObject)
        {
            if (DebugMode) Debug.Log("++YourVRUIScreenController::CreateUIScreen::_screenPrefab=" + _screenPrefab.name);
            if (!_isWorldObject)
            {
                if (DebugMode)
                {
                    Debug.Log("++++NORMAL UI SCREEN");
                }
                UIEventController.Instance.DispatchUIEvent(EventSystemController.EVENT_ACTIVATION_INPUT_STANDALONE, true);
                return (GameObject)Instantiate(_screenPrefab);
            }
            else
            {
                if (_forceScreen && !_screenLinkedToObject)
                {
                    if (!_forceOrthographic)
                    {
                        if (DebugMode)
                        {
                            Debug.Log("++++FORCED WORLD SCREEN(WILL STAY THERE UNTIL PLAYER CONFIRMATION) AND IT'S NOT LINKED TO ANY OBJECT");
                        }
                        UIEventController.Instance.DispatchUIEvent(EventSystemController.EVENT_ACTIVATION_INPUT_STANDALONE, false);
                        GameObject instance = (GameObject)Instantiate(_screenPrefab, this.transform);
                        float distanceToPlayer = _distance;
                        Vector3 collisionPoint = m_camera.transform.position + (m_camera.transform.forward.normalized * distanceToPlayer);
                        instance.layer = LayerMask.NameToLayer("UI");
                        instance.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                        instance.GetComponent<Canvas>().worldCamera = m_camera;
                        instance.transform.position = collisionPoint;
                        if (_scaleScreen == -1)
                        {
                            instance.GetComponent<RectTransform>().localScale = new Vector3(FinalScaleScreens, FinalScaleScreens, FinalScaleScreens);
                        }
                        else
                        {
                            instance.GetComponent<RectTransform>().localScale = new Vector3(_scaleScreen / 1000, _scaleScreen / 1000, _scaleScreen / 1000);
                        }
                        instance.transform.forward = Utilities.ClonePoint(m_camera.transform.forward);
                        return instance;
                    }
                    else
                    {
                        if (DebugMode)
                        {
                            Debug.Log("++++FORCED UI SCREEN(WILL STAY THERE UNTIL PLAYER CONFIRMATION) AND USING NORMAL UI");
                        }
                        GameObject instance = Utilities.AddChild(m_camera.transform, _screenPrefab);
                        instance.layer = LayerMask.NameToLayer("UI");
                        instance.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                        instance.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
                        if (EnableDesktopMode)
                        {
                            instance.GetComponent<RectTransform>().localPosition = new Vector3(0, -0.01f, 0.7f);
                        }
                        else
                        {
                            instance.GetComponent<RectTransform>().localPosition = new Vector3(0, 0.05f, 0.6f);
                        }
                        if (_scaleScreen == -1)
                        {
                            instance.GetComponent<RectTransform>().localScale = new Vector3(FinalScaleScreens, FinalScaleScreens, FinalScaleScreens);
                        }
                        else
                        {
                            instance.GetComponent<RectTransform>().localScale = new Vector3(_scaleScreen / 1000, _scaleScreen / 1000, _scaleScreen / 1000);
                        }
                        Utilities.ApplyMaterialOnImages(instance, MaterialDrawOnTop);
                        return instance;
                    }
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(EventSystemController.EVENT_ACTIVATION_INPUT_STANDALONE, false);
                    GameObject instance = (GameObject)Instantiate(_screenPrefab, this.transform);
                    RaycastHit objectCollided;
                    Vector3 vectorNormalUI = Utilities.ClonePoint(m_camera.transform.forward);
                    Vector3 collisionPoint = m_camera.transform.position + (m_camera.transform.forward.normalized * _distance);
                    if (!_screenLinkedToObject)
                    {
                        if (DebugMode)
                        {
                            Debug.Log("++++WORLD SCREEN JUST IN FRONT OF THE CAMERA AND NOT LINKED TO ANY OBJECT");
                        }
                        collisionPoint = m_camera.transform.position + (_distance * m_camera.transform.forward.normalized);
                        vectorNormalUI = Utilities.ClonePoint(m_camera.transform.forward);
                        instance.transform.forward = vectorNormalUI;
                    }
                    else
                    {
                        if (_targetObject != null)
                        {
                            instance.transform.parent = _targetObject.transform;
                            collisionPoint = _targetObject.transform.position;
                        }
                        else
                        {
                            objectCollided = new RaycastHit();
                            if (LayersToRaycast.Length == 0)
                            {
                                objectCollided = Utilities.GetRaycastHitInfoByRay(m_camera.transform.position, m_camera.transform.forward, _ignoreLayers);
                            }
                            else
                            {
                                Utilities.GetRaycastHitInfoByRayWithMask(m_camera.transform.position, m_camera.transform.forward, ref objectCollided, LayersToRaycast);
                            }
                            if (objectCollided.collider != null)
                            {
                                GameObject finalObject = objectCollided.collider.gameObject;
                                vectorNormalUI = Utilities.ClonePoint(objectCollided.normal);
                                if (_screenInCenterObject)
                                {
                                    collisionPoint = new Vector3(finalObject.transform.position.x, finalObject.transform.position.y, finalObject.transform.position.z);
                                }
                                else
                                {
                                    if (_useCollisionPoint)
                                    {
                                        if (DebugMode)
                                        {
                                            Debug.Log("++++WE WILL USE THE COLLISION POINT TO PLACE THE WORLD SCREEN");
                                        }
                                        collisionPoint = Utilities.ClonePoint(objectCollided.point);
                                    }
                                    else
                                    {
                                        if (DebugMode)
                                        {
                                            Debug.Log("++++WE WILL USE THE BARYCENTER OF THE OBJECT COLLIDED AS POSITION OF THE WORLD SCREEN");
                                        }

                                        Vector3 posCenterIGamePlayer = new Vector3(finalObject.transform.position.x, m_camera.transform.position.y, finalObject.transform.position.z);

                                        // GET DISTANCE TO BOUNDARY
                                        Vector2 minMax = GetMaxMinBounds(finalObject);
                                        Vector3 posStartingObject = posCenterIGamePlayer + (vectorNormalUI.normalized * (minMax.y / 2));
                                        float distancePointToWall = 0;
                                        RaycastHit pointToWall = Utilities.GetRaycastHitInfoByRay(posStartingObject, -vectorNormalUI.normalized);
                                        if (pointToWall.collider != null)
                                        {
                                            distancePointToWall = Vector3.Distance(posCenterIGamePlayer, pointToWall.point);
                                            if ((distancePointToWall < (minMax.x / 2)) || (distancePointToWall > 1.1f * (minMax.y / 2)))
                                            {
                                                distancePointToWall = 1.1f * (minMax.y / 2);
                                            }
                                        }

                                        // UPDATE THE COLLISION POINT WITH THE POSITION WHERE THE UI WILL BE DISPLAYED
                                        collisionPoint = posCenterIGamePlayer + (vectorNormalUI.normalized * distancePointToWall);
                                    }
                                }
                                CreateQuietDotReference(collisionPoint, false);
                                if (_alignedToCamera)
                                {
                                    if (DebugMode)
                                    {
                                        Debug.Log("++++ALIGNED TO THE CAMERA");
                                    }
                                    vectorNormalUI = Utilities.ClonePoint(m_camera.transform.forward);
                                    instance.transform.forward = vectorNormalUI;
                                }
                                else
                                {
                                    if (DebugMode)
                                    {
                                        Debug.Log("++++ALIGNED TO THE OBJECT NORMAL COLLISION");
                                    }
                                    instance.transform.rotation = Quaternion.FromToRotation(Vector3.right, vectorNormalUI);
                                    instance.transform.Rotate(new Vector3(0, -90, 0));
                                    vectorNormalUI = Utilities.ClonePoint(instance.transform.forward);
                                }
                            }
                        }
                    }
                    instance.layer = LayerMask.NameToLayer("UI");
                    instance.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                    instance.GetComponent<Canvas>().worldCamera = m_camera;
                    instance.transform.position = collisionPoint;
                    if (_scaleScreen == -1)
                    {
                        instance.GetComponent<RectTransform>().localScale = new Vector3(FinalScaleScreens, FinalScaleScreens, FinalScaleScreens);
                    }
                    else
                    {
                        instance.GetComponent<RectTransform>().localScale = new Vector3(_scaleScreen / 1000, _scaleScreen / 1000, _scaleScreen / 1000);
                    }
                    instance.transform.forward = vectorNormalUI;
                    return instance;
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Check the laser pointer to activate daydream
		 */
        void OnGUI()
        {
            if (DebugMode)
            {
                if (m_keysEnabled)
                {
                    GUI.skin = SkinYourVRUI;
                    GUI.Box(new Rect(0, 0, 300, 40), "KEY INPUT ACTIVATED\n PRESS BACKSPACE OR BACK TO CANCEL");
                }
            }
        }
    }
}

