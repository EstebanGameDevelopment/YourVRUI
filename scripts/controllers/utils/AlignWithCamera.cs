using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourVRUI
{

	/******************************************
	 * 
	 * AlignWithCamera
	 * 
	 * (DEBUG CODE) Used to force the daydream
	 * GvrLaserPointer gameobject to have the
	 * same position and forward vector than the camera.
	 * 
	 * @author Esteban Gallardo
	 */
	public class AlignWithCamera : MonoBehaviour
	{
		// -------------------------------------------
		/* 
		 * Will update the GameObject with the position and orientation
		 * of the camera
		 */
		void Update()
		{
			this.gameObject.transform.position = YourVRUIScreenController.Instance.GameCamera.transform.position;
			this.gameObject.transform.forward = YourVRUIScreenController.Instance.GameCamera.transform.forward;
		}
	}
}