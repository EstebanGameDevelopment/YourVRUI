using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourVRUI
{

	public delegate void ScreenVREventHandler(string _nameEvent, params object[] _list);

	/******************************************
	 * 
	 * ScreenVREventController
	 * 
	 * Class used to dispatch events through all the system
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenVREventController : MonoBehaviour
	{
		public event ScreenVREventHandler ScreenVREvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static ScreenVREventController _instance;

		public static ScreenVREventController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(ScreenVREventController)) as ScreenVREventController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "ScreenVREventController";
						_instance = container.AddComponent(typeof(ScreenVREventController)) as ScreenVREventController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<TimedVREventData> m_listEvents = new List<TimedVREventData>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private ScreenVREventController()
		{
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * Will dispatch an event
		 */
		public void DispatchScreenVREvent(string _nameEvent, params object[] _list)
		{
			if (ScreenVREvent != null) ScreenVREvent(_nameEvent, _list);
		}

		// -------------------------------------------
		/* 
		 * Will add a new delayed event to the queue
		 */
		public void DelayScreenVREvent(string _nameEvent, float _time, params object[] _list)
		{
			m_listEvents.Add(new TimedVREventData(_nameEvent, _time, _list));
		}

		// -------------------------------------------
		/* 
		 * Clone a delayed event
		 */
		public void DelayScreenVREvent(TimedVREventData _timeEvent)
		{
			m_listEvents.Add(new TimedVREventData(_timeEvent.NameEvent, _timeEvent.Time, _timeEvent.List));
		}

		// -------------------------------------------
		/* 
		 * Will process the queue of delayed events 
		 */
		void Update()
		{
			// DELAYED EVENTS
			for (int i = 0; i < m_listEvents.Count; i++)
			{
				TimedVREventData eventData = m_listEvents[i];
				eventData.Time -= Time.deltaTime;
				if (eventData.Time <= 0)
				{
					ScreenVREvent(eventData.NameEvent, eventData.List);
					eventData.Destroy();
					m_listEvents.RemoveAt(i);
					break;
				}
			}
		}
	}

}