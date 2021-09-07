using UnityEngine;
#if ENABLE_HTCVIVE
using Wave.Essence;
using Wave.Native;
#if ENABLE_YOURVRUI
using YourVRUI;
#endif
#endif

namespace YourCommonTools
{
    /******************************************
	 * 
	 * HTCHandController
	 * 
	 * @author Esteban Gallardo
	 */
    public class HTCHandController : MonoBehaviour
    {
        public const string EVENT_HTCHANDCONTROLLER_UPDATE_LASER = "EVENT_HTCHANDCONTROLLER_UPDATE_LASER";
        public const string EVENT_HTCHANDCONTROLLER_LINK_WITH_NETWORK_GAMEHAND = "EVENT_HTCHANDCONTROLLER_LINK_WITH_NETWORK_GAMEHAND";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static HTCHandController instance;

        public static HTCHandController Instance
        {
            get
            {
                if (!instance)
                {
                    instance = GameObject.FindObjectOfType(typeof(HTCHandController)) as HTCHandController;
                }
                return instance;
            }
        }


        public GameObject ControlledObject;
        public GameObject HTCCamera;
        public GameObject HTCLeftController;
        public GameObject HTCRightController;
        public GameObject LaserPointer;
        public GameObject LaserPointerLeft;
        public GameObject LaserPointerRight;

#if ENABLE_HTCVIVE
        private int m_handTypeSelected = 0;
        private GameObject m_currentController;
        private GameObject m_linkedWithHandRight;
        private GameObject m_linkedWithHandLeft;

        public GameObject CurrentController
        {
            get { return m_currentController; }
        }
        public int HandTypeSelected
        {
            get { return m_handTypeSelected; }
        }
        public WVR_DeviceType CurrentHandDevice
        {
            get
            {
                if (m_handTypeSelected == 1)
                    return WVR_DeviceType.WVR_DeviceType_Controller_Right;
                else
                    return WVR_DeviceType.WVR_DeviceType_Controller_Left;
            }
        }

        // -------------------------------------------
        /* 
		 * Start
		 */
        private void Start()
        {
            m_handTypeSelected = -1;
            SetLaserToRightHand();

            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        void OnDestroy()
        {
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
        }

        // -------------------------------------------
        /* 
		 * SetLaserToLeftHand
		 */
        private void SetLaserToLeftHand()
        {
            if (m_handTypeSelected != 0)
            {
                m_handTypeSelected = 0;
                if (LaserPointerLeft != null)
                {
                    if (LaserPointerRight != null) LaserPointerRight.SetActive(false);
                    if (LaserPointerLeft != null) LaserPointerLeft.SetActive(true);
                    SetMainLaserPoint(LaserPointerLeft);
                }
                m_currentController = HTCLeftController;
            }
        }

        // -------------------------------------------
        /* 
		 * SetLaserToRightHand
		 */
        private void SetLaserToRightHand()
        {
            if (m_handTypeSelected != 1)
            {
                m_handTypeSelected = 1;
                if (LaserPointerRight != null)
                {
                    if (LaserPointerRight != null) LaserPointerRight.SetActive(true);
                    if (LaserPointerLeft != null) LaserPointerLeft.SetActive(false);
                    SetMainLaserPoint(LaserPointerRight);
                }
                m_currentController = HTCRightController;
            }
        }

        // -------------------------------------------
        /* 
		 * SetMainLaserPoint
		 */
        private void SetMainLaserPoint(GameObject _laserPointer)
        {
            LaserPointer = _laserPointer;
#if ENABLE_YOURVRUI
            YourVRUIScreenController.Instance.LaserPointer = _laserPointer;
#endif
            UIEventController.Instance.DispatchUIEvent(EVENT_HTCHANDCONTROLLER_UPDATE_LASER, LaserPointer, (m_handTypeSelected == 1));
        }

        // -------------------------------------------
        /* 
		 * OnBasicSystemEvent
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_HTCHANDCONTROLLER_LINK_WITH_NETWORK_GAMEHAND)
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
		 * Update
		 */
        void Update()
        {
            if (WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_Trigger))
            {
                SetLaserToLeftHand();
            }
            else if (WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Right, WVR_InputId.WVR_InputId_Alias1_Trigger))
            {
                SetLaserToRightHand();
            }

            if (ControlledObject != null)
            {
                ControlledObject.transform.position = m_currentController.transform.position;
                ControlledObject.transform.rotation = m_currentController.transform.rotation;
            }

            if ((m_linkedWithHandRight != null) && (HTCRightController != null))
            {
                m_linkedWithHandRight.transform.position = HTCRightController.transform.position;
                m_linkedWithHandRight.transform.forward = HTCRightController.transform.forward;
            }
            if ((m_linkedWithHandLeft != null) && (HTCLeftController != null))
            {
                m_linkedWithHandLeft.transform.position = HTCLeftController.transform.position;
                m_linkedWithHandLeft.transform.forward = HTCLeftController.transform.forward;
            }
        }
#endif

    }
}

