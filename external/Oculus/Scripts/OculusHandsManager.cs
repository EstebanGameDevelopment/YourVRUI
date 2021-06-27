#if ENABLE_OCULUS
using OculusSampleFramework;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourCommonTools;
using YourVRUI;

namespace YourVRUI
{
    /******************************************
     * 
     * OculusHandsManager
     * 
     * Observer class that manages the 2 modes of operation:
     * 
     * - Working with Hand Tracking
     * - Working with Oculus Controllers
     * 
     * @author Esteban Gallardo
     */
    public class OculusHandsManager :
#if ENABLE_OCULUS
        HandsManager
#else
        YourVRUI.HandsManager
#endif
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING = "EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING";
        public const string EVENT_OCULUSHANDMANAGER_SET_UP_LASER_POINTER_INITIALIZE = "EVENT_OCULUSHANDMANAGER_SET_UP_LASER_POINTER_INITIALIZE";
        public const string EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_DOWN = "EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_DOWN";
        public const string EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_UP = "EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_UP";
        public const string EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_DOWN = "EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_DOWN";
        public const string EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_UP = "EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_UP";
        public const string EVENT_OCULUSHANDMANAGER_ROTATION_CAMERA_APPLIED = "EVENT_OCULUSHANDMANAGER_ROTATION_CAMERA_APPLIED";

        public const string EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_DOWN = "EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_DOWN";
        public const string EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_UP = "EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_UP";
        public const string EVENT_OCULUSHANDMANAGER_HAND_BUTTON_DOWN = "EVENT_OCULUSHANDMANAGER_HAND_BUTTON_DOWN";
        public const string EVENT_OCULUSHANDMANAGER_HAND_BUTTON_UP = "EVENT_OCULUSHANDMANAGER_HAND_BUTTON_UP";

        public const string EVENT_OCULUSHANDMANAGER_LINK_WITH_NETWORK_GAMEHAND = "EVENT_OCULUSHANDMANAGER_LINK_WITH_NETWORK_GAMEHAND";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static OculusHandsManager _instanceManager;

        public static OculusHandsManager InstanceManager
        {
            get
            {
                if (!_instanceManager)
                {
                    _instanceManager = GameObject.FindObjectOfType(typeof(OculusHandsManager)) as OculusHandsManager;
                }
                return _instanceManager;
            }
        }

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public Transform CameraRig;
        public GameObject HandLeftController;
        public GameObject HandRightController;
        public bool EnableVisualRays = true;
        public bool EnableDebugFingers = true;
        public float InteractionFingerSize = 0.04f;

#if ENABLE_OCULUS
        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        protected bool m_handsBeingTracked = false;
        protected List<GameObject> m_fingersInteractionHand = new List<GameObject>();
        protected List<GameObject> m_fingersInteractionController = new List<GameObject>();

        protected HAND m_currentHandWithLaser = HAND.none;

        protected bool m_isBeingActionPressed = false;
        protected bool m_isBeingHandPressed = false;

        protected GameObject m_linkedWithHandRight = null;
        protected GameObject m_linkedWithHandLeft = null;

        // ----------------------------------------------
        // MEMBER FUNCTIONS
        // ----------------------------------------------	

        // -------------------------------------------
        /* 
         * Start
         */
        private void Start()
        {
            OculusControllerInputs.Instance.Initialize(HandLeftController, HandRightController);
            OculusEventObserver.Instance.OculusEvent += new OculusEventHandler(OnOculusEvent);
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
            OculusEventObserver.Instance.DelayOculusEvent(EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING, 0.01f, false);
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        private void OnDestroy()
        {
            if (InstanceManager !=  null)
            {
                if (OculusEventObserver.Instance != null) OculusEventObserver.Instance.OculusEvent -= OnOculusEvent;
                BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;

                GameObject.Destroy(_instanceManager.gameObject);
                _instanceManager = null;
            }
        }

        // -------------------------------------------
        /* 
		 * RefreshSphereInteractionRadius
		 */
        private void RefreshSphereInteractionRadius()
        {
            if (!m_handsBeingTracked)
            {
                foreach (GameObject item in m_fingersInteractionController)
                {
                    item.GetComponent<FingerInteractionRadius>().SetActive(true);
                    item.GetComponent<FingerInteractionRadius>().SetDebugMode(EnableDebugFingers);
                    item.GetComponent<FingerInteractionRadius>().SetRadius(InteractionFingerSize);
                }
                foreach (GameObject item in m_fingersInteractionHand)
                {
                    item.GetComponent<FingerInteractionRadius>().SetActive(false);
                }
            }
            else
            {
                foreach (GameObject item in m_fingersInteractionHand)
                {
                    item.GetComponent<FingerInteractionRadius>().SetActive(true);
                    item.GetComponent<FingerInteractionRadius>().SetDebugMode(EnableDebugFingers);
                    item.GetComponent<FingerInteractionRadius>().SetRadius(InteractionFingerSize);
                }
                foreach (GameObject item in m_fingersInteractionController)
                {
                    item.GetComponent<FingerInteractionRadius>().SetActive(false);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * HandsBeingTracked
		 */
        private void HandsBeingTracked()
        {
            bool handsTracked = false;

            if (LeftHand != null)
            {
                if (LeftHand.IsTracked)
                {
                    handsTracked = true;
                }
            }

            if (RightHand != null)
            {
                if (RightHand.IsTracked)
                {
                    handsTracked = true;
                }
            }

            if (handsTracked != m_handsBeingTracked)
            {
                bool previousTracking = m_handsBeingTracked;
                m_handsBeingTracked = handsTracked;
                if (!previousTracking && handsTracked)
                {
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING, true, CameraRig);
                }
                else
                {
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING, false);
                }                
                RefreshSphereInteractionRadius();
            }
        }

        // -------------------------------------------
        /* 
		 * ActivateTrackingHands
		 */
        private void ActivateTrackingHands()
        {
            PinchInteractionTool[] pinchTools = GameObject.FindObjectsOfType<PinchInteractionTool>();
            foreach (PinchInteractionTool item in pinchTools)
            {
                if (item.IsRightHandedTool)
                {
                    if (item.GetLineRender != null)
                    {
                        YourVRUIScreenController.Instance.LaserRightPointer = item.GetLineRender.gameObject;
                    }
                }
                else
                {
                    if (item.GetLineRender != null)
                    {
                        YourVRUIScreenController.Instance.LaserLeftPointer = item.GetLineRender.gameObject;
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * DeActivateTrackingHands
		 */
        private void DeActivateTrackingHands()
        {
            PinchInteractionTool[] pinchTools = GameObject.FindObjectsOfType<PinchInteractionTool>();
            foreach (PinchInteractionTool item in pinchTools)
            {
                if (item.IsRightHandedTool)
                {
                    if (item.GetLineRender != null)
                    {
                        item.GetLineRender.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (item.GetLineRender != null)
                    {
                        item.GetLineRender.gameObject.SetActive(false);
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * AssignLaserByButtonPressed
		 */
        protected virtual void AssignLaserByButtonPressed(HAND _handSelected, GameObject _myLaserPointer)
        {
            if (m_currentHandWithLaser != _handSelected)
            {
                m_currentHandWithLaser = _handSelected;
                YourVRUIScreenController.Instance.LaserPointer = _myLaserPointer.gameObject;
                if (m_currentHandWithLaser == HAND.right)
                {
                    YourVRUIScreenController.Instance.LaserLeftPointer.SetActive(false);
                    YourVRUIScreenController.Instance.LaserRightPointer.SetActive(true);
                }
                else
                {
                    YourVRUIScreenController.Instance.LaserLeftPointer.SetActive(true);
                    YourVRUIScreenController.Instance.LaserRightPointer.SetActive(false);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * HandleOculusDefaultAction
		 */
        protected virtual void HandleOculusDefaultAction(bool _isHandTracking, string _nameEvent, HAND _handSelected, GameObject _myLaserPointer, bool _triggerActionButtonDown, bool _eventDownConfirmed, bool _eventUpConfirmed)
        {
            AssignLaserByButtonPressed(_handSelected, _myLaserPointer);

            if (_triggerActionButtonDown)
            {
                if (_eventDownConfirmed)
                {
                    m_isBeingActionPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_DOWN, _isHandTracking, _handSelected);
                }
                else
                {
                    if (_eventUpConfirmed)
                    {
                        m_isBeingActionPressed = false;
                        OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_UP, _isHandTracking, _handSelected);
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * HandleOculusDefaultHand
		 */
        protected virtual void HandleOculusDefaultHand(bool _isHandTracking, string _nameEvent, HAND _handSelected, GameObject _myLaserPointer, bool _triggerActionButtonDown, bool _eventDownConfirmed, bool _eventUpConfirmed, bool _isOriginInHandTracking)
        {
            AssignLaserByButtonPressed(_handSelected, _myLaserPointer);

            if (_triggerActionButtonDown)
            {
                if (_eventDownConfirmed)
                {
                    m_isBeingHandPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSHANDMANAGER_HAND_BUTTON_DOWN, _isHandTracking, _handSelected, _isOriginInHandTracking);
                }
                else
                {
                    if (_eventUpConfirmed)
                    {
                        m_isBeingHandPressed = false;
                        OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSHANDMANAGER_HAND_BUTTON_UP, _isHandTracking, _handSelected, _isOriginInHandTracking);
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * SwitchControlsHandToControllers
		 */
        protected virtual void SwitchControlsHandToControllers(bool _handTrackingState)
        {
#if DISABLE_HAND_TRACKING
            DeActivateTrackingHands();
            OculusControllerInputs.Instance.Activate();
#else
            if (_handTrackingState)
            {
                ActivateTrackingHands();
                OculusControllerInputs.Instance.Deactivate();
            }
            else
            {
                DeActivateTrackingHands();
                OculusControllerInputs.Instance.Activate();
            }
#endif
            RefreshSphereInteractionRadius();
            OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSHANDMANAGER_SET_UP_LASER_POINTER_INITIALIZE, _handTrackingState, HAND.right, YourVRUIScreenController.Instance.LaserRightPointer, false);
        }

        // -------------------------------------------
        /* 
		 * OnBasicSystemEvent
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_OCULUSHANDMANAGER_LINK_WITH_NETWORK_GAMEHAND)
            {
                bool isHandRigth = (bool)_list[0];
                if (isHandRigth)
                {
                    m_linkedWithHandRight = (GameObject)_list[1];
                }
                else
                {
                    m_linkedWithHandLeft = (GameObject)_list[1];
                }
            }
        }

        // -------------------------------------------
        /* 
		 * OnOculusEvent
		 */
        private void OnOculusEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING)
            {
                bool handTrackingState = (bool)_list[0];
                // Debug.LogError("EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING::handTrackingState=" + handTrackingState);
                m_currentHandWithLaser = HAND.none;
                SwitchControlsHandToControllers(handTrackingState);
            }
            if (_nameEvent == FingerInteractionRadius.EVENT_SPHEREINTERACTIONRADIUS_INITED)
            {
                GameObject targetFinger = (GameObject)_list[0];
                if ((SystemTools.FindGameObjectInChilds(HandLeftController, targetFinger))
                    || (SystemTools.FindGameObjectInChilds(HandRightController, targetFinger)))
                {   
                    if (m_fingersInteractionController.Count < 2)
                    {
                        m_fingersInteractionController.Add(targetFinger);
                    }                    
                }
                else
                {
                    if (m_fingersInteractionHand.Count < 2)
                    {
                        m_fingersInteractionHand.Add(targetFinger);
                    }
                }
                RefreshSphereInteractionRadius();
            }
            if (_nameEvent == EVENT_OCULUSHANDMANAGER_SET_UP_LASER_POINTER_INITIALIZE)
            {
                bool isHandTracking = (bool)_list[0];
                HAND handSelected = (HAND)_list[1];
                GameObject myLaserPointer = (GameObject)_list[2];
                AssignLaserByButtonPressed(handSelected, myLaserPointer);
            }
            if ((_nameEvent == EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_DOWN) || (_nameEvent == EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_UP))
            {
                bool isHandTracking = (bool)_list[0];
                HAND handSelected = (HAND)_list[1];
                GameObject myLaserPointer = (GameObject)_list[2];
                bool triggerActionButtonDown = (bool)_list[3];
                bool onEventDownConfirmed = (_nameEvent == EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_DOWN);
                bool onEventUpConfirmed = (_nameEvent == EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_UP);
                HandleOculusDefaultAction(isHandTracking, _nameEvent, handSelected, myLaserPointer, triggerActionButtonDown, onEventDownConfirmed, onEventUpConfirmed);
            }
            if ((_nameEvent == EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_DOWN) || (_nameEvent == EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_UP))
            {
                bool isHandTracking = (bool)_list[0];
                HAND handSelected = (HAND)_list[1];
                GameObject myLaserPointer = (GameObject)_list[2];
                bool triggerHandButtonDown = (bool)_list[3];
                bool isOriginInHandTracking = (bool)_list[4];
                bool onHandEventDownConfirmed = (_nameEvent == EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_DOWN);
                bool onHandEventUpConfirmed = (_nameEvent == EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_UP);
                HandleOculusDefaultHand(isHandTracking, _nameEvent, handSelected, myLaserPointer, triggerHandButtonDown, onHandEventDownConfirmed, onHandEventUpConfirmed, isOriginInHandTracking);
            }
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        public void Update()
        {
            HandsBeingTracked();

            if (m_handsBeingTracked)
            {
                if (OculusControllerInputs.Instance.IsRaycastController())
                {
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING, true, CameraRig);
                }
            }
            else
            {
                OculusControllerInputs.Instance.Logic();
            }

            if (m_linkedWithHandRight != null)
            {
                if (YourVRUIScreenController.Instance.LaserRightPointer != null)
                {
                    m_linkedWithHandRight.transform.position = YourVRUIScreenController.Instance.LaserRightPointer.transform.position;
                    m_linkedWithHandRight.transform.forward = YourVRUIScreenController.Instance.LaserRightPointer.transform.forward;
                }
            }
            if (m_linkedWithHandLeft != null)
            {
                if (YourVRUIScreenController.Instance.LaserLeftPointer != null)
                {
                    m_linkedWithHandLeft.transform.position = YourVRUIScreenController.Instance.LaserLeftPointer.transform.position;
                    m_linkedWithHandLeft.transform.forward = YourVRUIScreenController.Instance.LaserLeftPointer.transform.forward;
                }
            }
        }
#endif
    }
}
