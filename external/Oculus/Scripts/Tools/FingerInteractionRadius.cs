using UnityEngine;
using System.Collections.Generic;
using System;

namespace YourVRUI
{

	/******************************************
	 * 
	 * FingerInteractionRadius
	 * 
	 * Component of the interaction sphere radius
	 * 
	 * @author Esteban Gallardo
	 */
	public class FingerInteractionRadius : MonoBehaviour
	{
		public const string EVENT_SPHEREINTERACTIONRADIUS_INITED = "EVENT_SPHEREINTERACTIONRADIUS_INITED";

		public HAND Hand;

		private CapsuleCollider m_sphereCollider;

		private void Start()
		{
			m_sphereCollider = this.gameObject.GetComponent<CapsuleCollider>();
			if (m_sphereCollider == null)
			{
				throw new Exception("This component pattern should have the component SphereCollider to work[" + this.name + "]");
			}
			else
			{
				OculusEventObserver.Instance.DelayOculusEvent(EVENT_SPHEREINTERACTIONRADIUS_INITED, 0.1f, this.gameObject);
				SetActive(false);
			}
		}

		public void SetActive(bool _active)
        {
			this.gameObject.SetActive(_active);
        }

		public void SetDebugMode(bool _enableDebug)
		{
			if (m_sphereCollider != null)
			{
				if (m_sphereCollider.gameObject.activeSelf)
				{
					m_sphereCollider.gameObject.GetComponent<MeshRenderer>().enabled = _enableDebug;
				}

			}
		}

		public void SetRadius(float _radius)
		{
			if (m_sphereCollider != null)
			{
				if (m_sphereCollider.gameObject.activeSelf)
				{
					m_sphereCollider.gameObject.transform.localScale = new Vector3(_radius, _radius, _radius);
					m_sphereCollider.enabled = true;
				}
			}
		}
	}
}
