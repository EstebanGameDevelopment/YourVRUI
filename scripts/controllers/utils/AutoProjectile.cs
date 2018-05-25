using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourVRUI
{

	/******************************************
	 * 
	 * AutoProjectile
	 * 
	 * Create a projectile throw in a vector direction
	 * 
	 * @author Esteban Gallardo
	 */
	public class AutoProjectile : MonoBehaviour
	{
		private Vector3 m_forward;
		private float m_speed;

		public void Initialize(Vector3 _origin, Vector3 _forward, float _speed)
		{
			if (this.gameObject.GetComponent<Collider>() != null)
			{
				this.gameObject.GetComponent<Collider>().isTrigger = true;
			}
			if (this.gameObject.GetComponent<Rigidbody>() != null)
			{
				this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
				this.gameObject.GetComponent<Rigidbody>().useGravity = false;
			}
			transform.position = _origin;
			m_forward = _forward.normalized;
			m_speed = _speed;
		}

		void Update()
		{
			transform.position += m_forward * m_speed * Time.deltaTime;
		}
	}
}