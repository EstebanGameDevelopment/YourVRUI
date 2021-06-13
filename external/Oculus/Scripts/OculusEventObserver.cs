using UnityEngine;
using System.Collections.Generic;

namespace YourVRUI
{

	public delegate void OculusEventHandler(string _nameEvent, params object[] _list);

	/******************************************
	 * 
	 * OculusEventController
	 * 
	 * Observer class that triggers Oculus events
	 * 
	 * @author Esteban Gallardo
	 */
	public class OculusEventObserver : MonoBehaviour
	{
		public event OculusEventHandler OculusEvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static OculusEventObserver _instance;

		public static OculusEventObserver Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(OculusEventObserver)) as OculusEventObserver;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<OculusEventData> m_listEvents = new List<OculusEventData>();

		// -------------------------------------------
		/* 
		 * Destroy
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
			if (Instance != null)
			{
				Destroy(_instance.gameObject);
				_instance = null;
			}
		}

        // -------------------------------------------
        /* 
		 * Will dispatch a Oculus event
		 */
        public void DispatchOculusEvent(string _nameEvent, params object[] _list)
		{
            if (_instance == null) return;

			if (OculusEvent != null) OculusEvent(_nameEvent, _list);
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a delayed Oculus event
		 */
		public void DelayOculusEvent(string _nameEvent, float _time, params object[] _list)
		{
			if (_instance == null) return;

			m_listEvents.Add(new OculusEventData(_nameEvent, _time, _list));
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a delayed Oculus event
		 */
		public void ClearOculusEvents(string _nameEvent = "")
		{
			if (_nameEvent.Length == 0)
			{
				for (int i = 0; i < m_listEvents.Count; i++)
				{
					m_listEvents[i].Time = -1000;
				}
			}
			else
			{
				for (int i = 0; i < m_listEvents.Count; i++)
				{
					OculusEventData eventData = m_listEvents[i];
					if (eventData.NameEvent == _nameEvent)
					{
						eventData.Time = -1000;
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Will process the queue of delayed events 
		 */
		void Update()
		{
			if (_instance == null) return;

			// DELAYED EVENTS
			for (int i = 0; i < m_listEvents.Count; i++)
			{
				OculusEventData eventData = m_listEvents[i];
				if (eventData.Time == -1000)
				{
					eventData.Destroy();
					m_listEvents.RemoveAt(i);
					break;
				}
				else
				{
					eventData.Time -= Time.deltaTime;
					if (eventData.Time <= 0)
					{
						if (OculusEvent != null) OculusEvent(eventData.NameEvent, eventData.ListParameters);
						eventData.Destroy();
						m_listEvents.RemoveAt(i);
						break;
					}
				}
			}
		}

	}
}
