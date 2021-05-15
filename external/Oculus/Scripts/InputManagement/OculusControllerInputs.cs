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
    /// <summary>
    /// Listens all the Oculus Controller inputs
    /// </summary>
    public class OculusControllerInputs : MonoBehaviour
    {
#if ENABLE_OCULUS
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_ACTIVATION = "EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_ACTIVATION";
        public const string EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DEACTIVATION = "EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DEACTIVATION";
        public const string EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DOWN = "EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DOWN";
        public const string EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_UP = "EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_UP";

        public const string EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_TOUCH = "EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_TOUCH";
        public const string EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UNTOUCH = "EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UNTOUCH";
        public const string EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_DOWN = "EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_DOWN";
        public const string EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UP = "EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UP";

        public const string EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_TOUCH = "EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_TOUCH";
        public const string EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_UNTOUCH = "EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_UNTOUCH";
        public const string EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_DOWN = "EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_DOWN";
        public const string EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_UP = "EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_UP";

        public const string EVENT_OCULUSINPUTCONTROLLER_ONE_DOWN = "EVENT_OCULUSINPUTCONTROLLER_ONE_DOWN";
        public const string EVENT_OCULUSINPUTCONTROLLER_ONE_UP = "EVENT_OCULUSINPUTCONTROLLER_ONE_UP";
        public const string EVENT_OCULUSINPUTCONTROLLER_TWO_DOWN = "EVENT_OCULUSINPUTCONTROLLER_TWO_DOWN";
        public const string EVENT_OCULUSINPUTCONTROLLER_TWO_UP = "EVENT_OCULUSINPUTCONTROLLER_TWO_UP";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static OculusControllerInputs _instance;

        public static OculusControllerInputs Instance
        {
            get
            {
                _instance = GameObject.FindObjectOfType(typeof(OculusControllerInputs)) as OculusControllerInputs;
                if (!_instance)
                {
                    GameObject container = new GameObject();
                    // DontDestroyOnLoad(container);
                    container.name = "OculusControllerInputs";
                    _instance = container.AddComponent(typeof(OculusControllerInputs)) as OculusControllerInputs;
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private bool m_joystickPressedRight = false;
        private bool m_joystickPressedLeft = false;
        private GameObject m_handLeftController;
        private GameObject m_handRightController;

        private LineRenderer m_raycastLineLeft;
        private LineRenderer m_raycastLineRight;

        private bool m_indexTriggerLeftPressed;
        private bool m_indexTriggerRightPressed;

        private bool m_handTriggerLeftPressed;
        private bool m_handTriggerRightPressed;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public LineRenderer RaycastLineLeft
        {
            get { return m_raycastLineLeft; }
        }
        public LineRenderer RaycastLineRight
        {
            get { return m_raycastLineRight; }
        }
        public GameObject HandLeftController
        {
            get { return m_handLeftController; }
        }
        public GameObject HandRightController
        {
            get { return m_handRightController; }
        }

        // -------------------------------------------
        /* 
		 * Initialize
		 */
        public void Initialize(GameObject _handLeftController, GameObject _handRightController)
        {
            m_handLeftController = _handLeftController;
            m_handRightController = _handRightController;

            m_raycastLineLeft = m_handLeftController.GetComponentInChildren<LineRenderer>();
            m_raycastLineRight = m_handRightController.GetComponentInChildren<LineRenderer>();
            m_raycastLineLeft.gameObject.SetActive(false);
            m_raycastLineRight.gameObject.SetActive(false);

            if (!OculusHandsManager.InstanceManager.EnableVisualRays)
            {
                if (m_raycastLineLeft != null) m_raycastLineLeft.enabled = false;
                if (m_raycastLineRight != null) m_raycastLineRight.enabled = false;
            }
        }

        // -------------------------------------------
        /* 
		 * Deactivate
		 */
        public void Deactivate()
        {
            m_joystickPressedRight = false;
            m_joystickPressedLeft = false;
            m_indexTriggerLeftPressed = false;
            m_indexTriggerRightPressed = false;

            m_handLeftController.SetActive(false);
            m_handRightController.SetActive(false);

            m_raycastLineLeft.gameObject.SetActive(false);
            m_raycastLineRight.gameObject.SetActive(false);
        }

        // -------------------------------------------
        /* 
		 * Activate
		 */
        public void Activate()
        {
            if (m_raycastLineLeft != null) YourVRUIScreenController.Instance.LaserLeftPointer = m_raycastLineLeft.gameObject;
            if (m_raycastLineRight != null) YourVRUIScreenController.Instance.LaserRightPointer = m_raycastLineRight.gameObject;

            m_handLeftController.SetActive(true);
            m_handRightController.SetActive(true);

            m_raycastLineLeft.gameObject.SetActive(false);
            m_raycastLineRight.gameObject.SetActive(false);
        }

        // -------------------------------------------
        /* 
		 * IsRaycastController
		 */
        public bool IsRaycastController()
        {
            if ((m_raycastLineLeft == null) || (m_raycastLineRight == null))
            {
                return true;
            }
            else
            {
                return ((YourVRUIScreenController.Instance.LaserLeftPointer == m_raycastLineLeft.gameObject)
                        || (YourVRUIScreenController.Instance.LaserRightPointer == m_raycastLineRight.gameObject));
            }
        }

        // -------------------------------------------
        /* 
         * GetIndexTriggerInputs
         */
        private void GetIndexTriggerInputs()
        {
            // TRIGGER RIGHT
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_TOUCH, HAND.right, m_handRightController.transform, m_raycastLineRight);
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UNTOUCH, HAND.right, m_handRightController.transform, m_raycastLineRight);

            float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
            if (!m_indexTriggerRightPressed)
            {
                if (triggerValue > 0.4f)
                {
                    m_indexTriggerRightPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_DOWN, false, HAND.right, m_raycastLineRight.gameObject, true);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_DOWN, HAND.right, m_handRightController.transform, m_raycastLineRight);
                }
            }
            else
            {
                if (triggerValue < 0.2f)
                {
                    m_indexTriggerRightPressed = false;
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_UP, false, HAND.right, m_raycastLineRight.gameObject, true);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UP, HAND.right, m_handRightController.transform, m_raycastLineRight);
                }
            }

            // TRIGGER LEFT
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_TOUCH, HAND.left, m_handLeftController.transform, m_raycastLineLeft);
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UNTOUCH, HAND.left, m_handLeftController.transform, m_raycastLineLeft);

            triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
            if (!m_indexTriggerLeftPressed)
            {
                if (triggerValue > 0.4f)
                {
                    m_indexTriggerLeftPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_DOWN, false, HAND.left, m_raycastLineLeft.gameObject, true);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_DOWN, HAND.left, m_handLeftController.transform, m_raycastLineLeft);
                }
            }
            else
            {
                if (triggerValue < 0.2f)
                {
                    m_indexTriggerLeftPressed = false;
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_UP, false, HAND.left, m_raycastLineLeft.gameObject, true);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UP, HAND.left, m_handLeftController.transform, m_raycastLineLeft);
                }
            }
        }

        // -------------------------------------------
        /* 
         * GetHandTriggerInputs
         */
        private void GetHandTriggerInputs()
        {
            // HAND RIGHT
            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_TOUCH, HAND.right, m_handRightController.transform);
            if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_UNTOUCH, HAND.right, m_handRightController.transform);

            float handValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);
            if (!m_handTriggerRightPressed)
            {
                if (handValue > 0.4f)
                {
                    m_handTriggerRightPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_DOWN, false, HAND.right, m_handRightController.gameObject, true, false);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_DOWN, HAND.right, m_handRightController.transform);
                }
            }
            else
            {
                if (handValue < 0.2f)
                {
                    m_handTriggerRightPressed = false;
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_UP, false, HAND.right, m_handRightController.gameObject, true, false);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_UP, HAND.right, m_handRightController.transform);
                }
            }

            // TRIGGER LEFT
            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_TOUCH, HAND.left, m_handLeftController.transform);
            if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_UNTOUCH, HAND.left, m_handLeftController.transform);

            handValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);
            if (!m_handTriggerLeftPressed)
            {
                if (handValue > 0.4f)
                {
                    m_handTriggerLeftPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_DOWN, false, HAND.left, m_raycastLineLeft.gameObject, true, false);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_DOWN, HAND.left, m_handLeftController.transform);
                }
            }
            else
            {
                if (handValue < 0.2f)
                {
                    m_handTriggerLeftPressed = false;
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_UP, false, HAND.left, m_raycastLineLeft.gameObject, true, false);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_HAND_TRIGGER_UP, HAND.left, m_handLeftController.transform);
                }
            }
        }

        private bool m_oneButtonLeftPressed = false;
        private bool m_twoButtonLeftPressed = false;

        private bool m_oneButtonRightPressed = false;
        private bool m_twoButtonRightPressed = false;

        // -------------------------------------------
        /* 
         * GetButtonsInputs
         */
        private void GetButtonsInputs()
        {
            if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch))
            {
                if (!m_oneButtonLeftPressed)
                {
                    m_oneButtonLeftPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_ONE_DOWN, HAND.left, m_raycastLineLeft.gameObject);
                }
            }
            else
            {
                if (m_oneButtonLeftPressed)
                {
                    m_oneButtonLeftPressed = false;
                }
            }
            if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch))
            {
                if (!m_twoButtonLeftPressed)
                {
                    m_twoButtonLeftPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_TWO_DOWN, HAND.left, m_raycastLineLeft.gameObject);
                }
            }
            else
            {
                if (m_twoButtonLeftPressed)
                {
                    m_twoButtonLeftPressed = false;
                }
            }
            if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                if (!m_oneButtonRightPressed)
                {
                    m_oneButtonRightPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_ONE_DOWN, HAND.right, m_raycastLineRight.gameObject);
                }
            }
            else
            {
                if (m_oneButtonRightPressed)
                {
                    m_oneButtonRightPressed = false;
                }
            }
            if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            {
                if (!m_twoButtonRightPressed)
                {
                    m_twoButtonRightPressed = true;
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_TWO_DOWN, HAND.right, m_raycastLineRight.gameObject);
                }
            }
            else
            {
                if (m_twoButtonRightPressed)
                {
                    m_twoButtonRightPressed = false;
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_TWO_UP, HAND.right, m_raycastLineRight.gameObject);
                }
            }
        }

        // -------------------------------------------
        /* 
         * GetInputThumbsticks
         */
        public Vector2 GetInputThumbsticks(bool _checkVectorOnly = false)
        {
            Vector2 vectorThumbstick = Vector2.zero;
            Vector2 vectorLeftThumbstick = Vector2.zero;
            Vector2 vectorRightThumbstick = Vector2.zero;

            // LEFT CONTROLLER
            if (!_checkVectorOnly)
            {
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
                {
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_SET_UP_LASER_POINTER_INITIALIZE, false, HAND.left, m_raycastLineLeft.gameObject);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DOWN, HAND.left, m_handLeftController.transform);
                }
                if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_UP, HAND.left, m_handLeftController.transform);

                // RIGHT CONTROLLER
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))
                {
                    OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_SET_UP_LASER_POINTER_INITIALIZE, false, HAND.right, m_raycastLineRight.gameObject);
                    OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DOWN, HAND.right, m_handRightController.transform);
                }
                if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch)) OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_UP, HAND.right, m_handRightController.transform);
            }

            // LEFT CONTROLLER
            if (_checkVectorOnly)
            {
                if (m_joystickPressedLeft)
                {
                    vectorLeftThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
                }
            }
            else
            {
                if (!m_joystickPressedLeft)
                {
                    // PRESSED
                    vectorThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
                    if (vectorThumbstick.magnitude > 0.5)
                    {
                        m_joystickPressedLeft = true;
                        vectorLeftThumbstick = Utilities.Clone(vectorThumbstick);
                        OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_SET_UP_LASER_POINTER_INITIALIZE, false, HAND.left, m_raycastLineLeft.gameObject);
                        OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_ACTIVATION, HAND.left, m_handLeftController.transform, vectorThumbstick);
                    }
                }
                else
                {
                    // RELEASED
                    vectorThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
                    if (vectorThumbstick.magnitude < 0.1)
                    {
                        m_joystickPressedLeft = false;
                        vectorLeftThumbstick = Utilities.Clone(vectorThumbstick);
                        OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DEACTIVATION, HAND.left, m_handLeftController.transform, vectorThumbstick);
                    }
                }
            }

            // RIGHT CONTROLLER
            if (_checkVectorOnly)
            {
                if (m_joystickPressedRight)
                {
                    vectorRightThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
                }
            }
            else
            {
                if (!m_joystickPressedRight)
                {
                    // PRESSED
                    vectorThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
                    if (vectorThumbstick.magnitude > 0.5)
                    {
                        m_joystickPressedRight = true;
                        vectorRightThumbstick = Utilities.Clone(vectorThumbstick);
                        OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_SET_UP_LASER_POINTER_INITIALIZE, false, HAND.right, m_raycastLineRight.gameObject);
                        OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_ACTIVATION, HAND.right, m_handRightController.transform, vectorThumbstick);
                    }
                }
                else
                {
                    // RELEASE
                    vectorThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
                    if (vectorThumbstick.magnitude < 0.1)
                    {
                        m_joystickPressedRight = false;
                        vectorRightThumbstick = Utilities.Clone(vectorThumbstick);
                        OculusEventObserver.Instance.DispatchOculusEvent(EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DEACTIVATION, HAND.right, m_handLeftController.transform, vectorThumbstick);
                    }
                }
            }

            if (m_joystickPressedRight)
            {
                return vectorRightThumbstick;
            }
            else
            {
                if (m_joystickPressedLeft)
                {
                    return vectorLeftThumbstick;
                }
            }
            return Vector2.zero;
        }

        // -------------------------------------------
        /* 
		 * Logic
		 */
        public void Logic()
        {
            if ((m_handLeftController == null) || (m_handRightController == null))
            {
                throw new Exception("The controllers should be specified for OculusControllerInputs");
            }
            else
            {
                GetInputThumbsticks();

                GetIndexTriggerInputs();

                GetHandTriggerInputs();

                GetButtonsInputs();
            }
        }
#endif
    }
}