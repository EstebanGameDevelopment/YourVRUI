#if ENABLE_OCULUS
using OculusSampleFramework;
using System;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRUI
{
	/// <summary>
	/// Spawns all interactable tools that are specified for a scene.
	/// </summary>
	public class InteractableOculusHandsCreator : MonoBehaviour
	{
		public Transform CameraRig;
		[SerializeField] private Transform[] LeftHandTools = null;
		[SerializeField] private Transform[] RightHandTools = null;

#if ENABLE_OCULUS
		private bool m_initedLeft = false;
		private bool m_initedRight = false;
		private bool m_initedGeneral = false;

		private List<Transform> m_toolInstances = new List<Transform>();

		private void Start()
		{
			if (!m_initedGeneral)
            {
				m_initedGeneral = true;
				OculusEventObserver.Instance.OculusEvent += new OculusEventHandler(OnOculusEvent);
			}

			if (LeftHandTools != null && LeftHandTools.Length > 0 && !m_initedLeft)
			{
				m_initedLeft = true;
				StartCoroutine(AttachToolsToHands(LeftHandTools, false));
			}

			if (RightHandTools != null && RightHandTools.Length > 0 && !m_initedRight)
			{
				m_initedRight = true;
				StartCoroutine(AttachToolsToHands(RightHandTools, true));
			}
		}

		void OnDestroy()
        {
			OculusEventObserver.Instance.OculusEvent -= OnOculusEvent;
		}

        private IEnumerator AttachToolsToHands(Transform[] toolObjects, bool isRightHand)
		{
			HandsManager handsManagerObj = null;
			while ((handsManagerObj = HandsManager.Instance) == null || !handsManagerObj.IsInitialized())
			{
				yield return null;
			}

			// create set of tools per hand to be safe
			HashSet<Transform> toolObjectSet = new HashSet<Transform>();
			foreach (Transform toolTransform in toolObjects)
			{
				toolObjectSet.Add(toolTransform.transform);
			}

			foreach (Transform toolObject in toolObjectSet)
			{
				OVRSkeleton handSkeletonToAttachTo =
				  isRightHand ? handsManagerObj.RightHandSkeleton : handsManagerObj.LeftHandSkeleton;
				while (handSkeletonToAttachTo == null || handSkeletonToAttachTo.Bones == null)
				{
					yield return null;
				}

				AttachToolToHandTransform(toolObject, isRightHand);
			}
		}

		private void OnOculusEvent(string _nameEvent, object[] _list)
		{
			if (_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_ROTATION_CAMERA_APPLIED)
            {
				Vector3 rotationApplied = (Vector3)_list[0];
				foreach (Transform tool in m_toolInstances)
                {
					tool.Rotate(rotationApplied);
				}
			}
		}

		private void AttachToolToHandTransform(Transform tool, bool isRightHanded)
		{
			var newTool = Instantiate(tool).transform;
			newTool.SetParent(CameraRig, false);
			newTool.localPosition = Vector3.zero;
			PinchInteractionTool toolComp = newTool.GetComponent<PinchInteractionTool>();
			toolComp.IsRightHandedTool = isRightHanded;
			// Initialize only AFTER settings have been applied!
			toolComp.Initialize();
			newTool.GetComponentInChildren<FingerInteractionRadius>().Hand = (isRightHanded ? HAND.right : HAND.left);
			m_toolInstances.Add(newTool);
			// Debug.LogError("HANDS FULLY INITIALIZED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
		}
#endif
	}
}