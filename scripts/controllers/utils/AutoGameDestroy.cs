using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourVRUI
{

	/******************************************
	 * 
	 * AutoGameDestroy
	 * 
	 * Destroys the object attached after 5 seconds
	 * 
	 * @author Esteban Gallardo
	 */
	public class AutoGameDestroy : MonoBehaviour
	{
        public float TimeToDestroy = 3;

		// -------------------------------------------
		/* 
		 * Start
		 */
		void Start()
		{
			Destroy(gameObject, TimeToDestroy);
		}
	}
}