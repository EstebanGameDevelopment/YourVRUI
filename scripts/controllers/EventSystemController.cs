using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using YourCommonTools;

namespace YourVRUI
{

	/******************************************
	 * 
	 * EventSystemController
	 * 
	 * Class that allows to switch between gaze and standard input
	 * 
	 * @author Esteban Gallardo
	 */
	public class EventSystemController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_ACTIVATION_INPUT_STANDALONE = "EVENT_ACTIVATION_INPUT_STANDALONE";
        public const string EVENT_EVENTSYSTEMCONTROLLER_RAYCASTING_SYSTEM = "EVENT_EVENTSYSTEMCONTROLLER_RAYCASTING_SYSTEM";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static EventSystemController _instance;

		public static EventSystemController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(EventSystemController)) as EventSystemController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private StandaloneInputModule m_standAloneInputModule;
#if !ENABLE_OCULUS && !ENABLE_HTCVIVE && !ENABLE_PICONEO && UNITY_HAS_GOOGLEVR && ENABLE_PARTY_2018
		private GvrPointerInputModule m_gazeInputModule;
#endif

		private bool m_hasBeenInitialized = false;
        private bool m_hasBeenDestroyed = false;

        // -------------------------------------------
        /* 
		 * Getting the reference of the input controllers
		 */
        void Start()
		{
			Initialitzation();
		}

		// -------------------------------------------
		/* 
		 * Getting the reference of the input controllers
		 */
		public void Initialitzation()
		{
            if (m_hasBeenInitialized) return;
            m_hasBeenInitialized = true;

            m_standAloneInputModule = this.gameObject.GetComponent<StandaloneInputModule>();
			if (m_standAloneInputModule == null)
			{
				this.gameObject.AddComponent<StandaloneInputModule>();
				m_standAloneInputModule = this.gameObject.GetComponent<StandaloneInputModule>();
			}
#if !ENABLE_OCULUS && !ENABLE_HTCVIVE && !ENABLE_PICONEO && UNITY_HAS_GOOGLEVR && ENABLE_PARTY_2018
			m_gazeInputModule = this.gameObject.GetComponent<GvrPointerInputModule>();
			if (m_gazeInputModule == null)
			{
				Debug.LogError("WARNNING: The project can work in a non-VR related project, but it's meant to run mainly for VR projects");
			}
#endif

			UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);
		}

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        void OnDestroy()
        {
            Destroy();
        }

        // -------------------------------------------
        /* 
		 * Destroy
		 */
		public void Destroy()
		{
            if (m_hasBeenDestroyed) return;
            m_hasBeenDestroyed = true;

            UIEventController.Instance.UIEvent -= OnBasicEvent;
		}

		// -------------------------------------------
		/* 
		 * Process incoming events
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_ACTIVATION_INPUT_STANDALONE)
			{
				bool activation = (bool)_list[0];
				if (m_standAloneInputModule != null) m_standAloneInputModule.enabled = activation;
#if ENABLE_WORLDSENSE && ENABLE_PARTY_2018
                if (m_gazeInputModule != null) m_gazeInputModule.enabled = false;
#elif !ENABLE_OCULUS && !ENABLE_HTCVIVE && !ENABLE_PICONEO && UNITY_HAS_GOOGLEVR && ENABLE_PARTY_2018
				if (m_gazeInputModule != null) m_gazeInputModule.enabled = !activation;
#endif
			}
			if (_nameEvent == EVENT_EVENTSYSTEMCONTROLLER_RAYCASTING_SYSTEM)
            {
                this.gameObject.SetActive((bool)_list[0]);
            }
        }
	}
}