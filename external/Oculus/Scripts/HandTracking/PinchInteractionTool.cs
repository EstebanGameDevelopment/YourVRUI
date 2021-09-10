#if ENABLE_OCULUS
using OculusSampleFramework;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace YourVRUI
{
	/// <summary>
	/// Ray tool used for far-field interactions.
	/// </summary>
	public class PinchInteractionTool :
#if ENABLE_OCULUS
        InteractableTool
#else
        MonoBehaviour
#endif
    {
        // EVENTS
        public const string EVENT_PINCHINTERACTIONTOOL_PINCH_PRESSED = "EVENT_PINCHINTERACTIONTOOL_PINCH_PRESSED";
        public const string EVENT_PINCHINTERACTIONTOOL_PINCH_RELEASED = "EVENT_PINCHINTERACTIONTOOL_PINCH_RELEASED";

        // CONSTANTS
        private const int NUM_VELOCITY_FRAMES = 10;
        private const float TIME_FOR_HAND_TRIGGER = 2;

        // MEMBERS
        public GameObject _rayToolViewGO = null;
        public GameObject _fingerTipPokeToolViewGO = null;
        public int _fingerToFollowGO = 1;

#if ENABLE_OCULUS
        // MEMBERS
        [SerializeField] public HandRayToolView _rayToolView = null;

        [SerializeField] private FingerTipPokeToolView _fingerTipPokeToolView = null;
        [SerializeField] private OVRPlugin.HandFinger _fingerToFollow = OVRPlugin.HandFinger.Index;


        public override InteractableToolTags ToolTags
		{
			get
			{
				return InteractableToolTags.Ray;
			}
		}

		private PinchStateCustom _pinchStateCustom = new PinchStateCustom();
		private Interactable _focusedInteractable;

        private Vector3[] _velocityFrames;
        private int _currVelocityFrame = 0;
        private bool _sampledMaxFramesAlready;
        private Vector3 _position;
        private bool _previousStatePinch = false;

        private BoneCapsuleTriggerLogic[] _boneCapsuleTriggerLogic;

        private float _lastScale = 1.0f;
        private bool _isInitialized = false;
        private OVRBoneCapsule _capsuleToTrack;

        private float m_timeAcumDetectStablePinch = 0;
        private bool m_pressedStablePinch = false;

        private Vector3 m_rotationAcumulated = Vector3.zero;

        public override ToolInputState ToolInputState
		{
			get
			{
				if (_pinchStateCustom.PinchDownOnFocusedObject)
				{
					return ToolInputState.PrimaryInputDown;
				}
				if (_pinchStateCustom.PinchSteadyOnFocusedObject)
				{
					return ToolInputState.PrimaryInputDownStay;
				}
				if (_pinchStateCustom.PinchUpAndDownOnFocusedObject)
				{
					return ToolInputState.PrimaryInputUp;
				}

				return ToolInputState.Inactive;
			}
		}

        public override bool IsFarFieldTool
        {
            get { return false; }
        }

        public override bool EnableState
        {
            get
            {
                return _fingerTipPokeToolView.gameObject.activeSelf;
            }
            set
            {
                _fingerTipPokeToolView.gameObject.SetActive(value);
            }
        }

        public Transform GetLineRender
        {
            get { return _rayToolView.ReferenceRay; }
        }

        private void AssignInitialMembers()
        {
            _rayToolView = _rayToolViewGO.GetComponent<HandRayToolView>();
            _fingerTipPokeToolView = _fingerTipPokeToolViewGO.GetComponent<FingerTipPokeToolView>();
            _fingerToFollow = (OVRPlugin.HandFinger)_fingerToFollowGO;
        }

        public override void Initialize()
		{
            AssignInitialMembers();

            Assert.IsNotNull(_fingerTipPokeToolView);
            _fingerTipPokeToolView.InteractableTool = this;
            if (_rayToolView != null) _rayToolView.InteractableTool = this;

            _velocityFrames = new Vector3[NUM_VELOCITY_FRAMES];
            Array.Clear(_velocityFrames, 0, NUM_VELOCITY_FRAMES);

            StartCoroutine(AttachTriggerLogic());
        }

        private void Start()
        {
            AssignInitialMembers();

            OculusEventObserver.Instance.OculusEvent += new OculusEventHandler(OnOculusEvent);
        }

        private void OnDestroy()
        {
            if (OculusEventObserver.Instance != null) OculusEventObserver.Instance.OculusEvent -= OnOculusEvent;
        }

        private void OnOculusEvent(string _nameEvent, object[] _list)
        {
            if ((_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_STATE_HANDTRACKING)
                || (_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_STATE_INITIAL_HANDTRACKING))
            {
                bool handTrackingState = (bool)_list[0];

                if (_fingerTipPokeToolView != null) _fingerTipPokeToolView.EnableState = handTrackingState;
                if (_rayToolView != null) _rayToolView.EnableState = handTrackingState;
            }
            if (_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_ROTATION_CAMERA_APPLIED)
            {
                Vector3 rotationApplied = (Vector3)_list[0];
                m_rotationAcumulated += rotationApplied;
            }
        }

        private IEnumerator AttachTriggerLogic()
        {
            while (!HandsManager.Instance || !HandsManager.Instance.IsInitialized())
            {
                yield return null;
            }

            OVRSkeleton handSkeleton = IsRightHandedTool ? HandsManager.Instance.RightHandSkeleton : HandsManager.Instance.LeftHandSkeleton;

            OVRSkeleton.BoneId boneToTestCollisions = OVRSkeleton.BoneId.Hand_Pinky3;
            switch (_fingerToFollow)
            {
                case OVRPlugin.HandFinger.Thumb:
                    boneToTestCollisions = OVRSkeleton.BoneId.Hand_Index3;
                    break;
                case OVRPlugin.HandFinger.Index:
                    boneToTestCollisions = OVRSkeleton.BoneId.Hand_Index3;
                    break;
                case OVRPlugin.HandFinger.Middle:
                    boneToTestCollisions = OVRSkeleton.BoneId.Hand_Middle3;
                    break;
                case OVRPlugin.HandFinger.Ring:
                    boneToTestCollisions = OVRSkeleton.BoneId.Hand_Ring3;
                    break;
                default:
                    boneToTestCollisions = OVRSkeleton.BoneId.Hand_Pinky3;
                    break;
            }

            List<BoneCapsuleTriggerLogic> boneCapsuleTriggerLogic = new List<BoneCapsuleTriggerLogic>();
            List<OVRBoneCapsule> boneCapsules = HandsManager.GetCapsulesPerBone(handSkeleton, boneToTestCollisions);
            foreach (var ovrCapsuleInfo in boneCapsules)
            {
                var boneCapsuleTrigger = ovrCapsuleInfo.CapsuleRigidbody.gameObject.AddComponent<BoneCapsuleTriggerLogic>();
                ovrCapsuleInfo.CapsuleCollider.isTrigger = true;
                boneCapsuleTrigger.ToolTags = ToolTags;
                boneCapsuleTriggerLogic.Add(boneCapsuleTrigger);
            }

            _boneCapsuleTriggerLogic = boneCapsuleTriggerLogic.ToArray();
            // finger tip should have only one capsule
            if (boneCapsules.Count > 0)
            {
                _capsuleToTrack = boneCapsules[0];
            }

            _isInitialized = true;
        }

        private void UpdateAverageVelocity()
        {
            var prevPosition = _position;
            var currPosition = transform.position;
            var currentVelocity = (currPosition - prevPosition) / Time.deltaTime;
            _position = currPosition;
            _velocityFrames[_currVelocityFrame] = currentVelocity;
            // if sampled more than allowed, loop back toward the beginning
            _currVelocityFrame = (_currVelocityFrame + 1) % NUM_VELOCITY_FRAMES;

            Velocity = Vector3.zero;
            // edge case; when we first start up, we will have only sampled less than the
            // max frames. so only compute the average over that subset. After that, the
            // frame samples will act like an array that loops back toward to the beginning
            if (!_sampledMaxFramesAlready && _currVelocityFrame == NUM_VELOCITY_FRAMES - 1)
            {
                _sampledMaxFramesAlready = true;
            }

            int numFramesToSamples = _sampledMaxFramesAlready ? NUM_VELOCITY_FRAMES : _currVelocityFrame + 1;
            for (int frameIndex = 0; frameIndex < numFramesToSamples; frameIndex++)
            {
                Velocity += _velocityFrames[frameIndex];
            }

            Velocity /= numFramesToSamples;
        }

        private void CheckAndUpdateScale()
        {
            float currentScale = IsRightHandedTool?HandsManager.Instance.RightHand.HandScale:HandsManager.Instance.LeftHand.HandScale;
            if (Mathf.Abs(currentScale - _lastScale) > Mathf.Epsilon)
            {
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                _lastScale = currentScale;
            }
        }

        private void Update()
		{
            if (!HandsManager.Instance || !HandsManager.Instance.IsInitialized() || !_isInitialized || _capsuleToTrack == null)
            {
                return;
			}

            OVRHand hand = IsRightHandedTool ? HandsManager.Instance.RightHand : HandsManager.Instance.LeftHand;
            float currentScale = hand.HandScale;
            var pointer = hand.PointerPose;

            // push tool into the tip based on how wide it is. so negate the direction
            Transform capsuleTransform = _capsuleToTrack.CapsuleCollider.transform;
            Vector3 capsuleDirection = capsuleTransform.right;
            Vector3 trackedPosition = capsuleTransform.position + _capsuleToTrack.CapsuleCollider.height * 0.5f * capsuleDirection;
            Vector3 sphereRadiusOffset = currentScale * _fingerTipPokeToolView.SphereRadius * capsuleDirection;
            
            // push tool back so that it's centered on transform/bone
            Vector3 toolPosition = trackedPosition + sphereRadiusOffset;
            transform.position = toolPosition;
            transform.rotation = Quaternion.Euler(m_rotationAcumulated.x, m_rotationAcumulated.y, m_rotationAcumulated.z) * pointer.rotation;
            InteractionPosition = trackedPosition;

            UpdateAverageVelocity();

            CheckAndUpdateScale();

            _pinchStateCustom.UpdateState(hand, _focusedInteractable);
            if (_rayToolView != null)
            {
                if (_rayToolView.EnableState)
                {
                    _rayToolView.ToolActivateState = _pinchStateCustom.PinchSteadyOnFocusedObject || _pinchStateCustom.PinchDownOnFocusedObject;
                    if (_previousStatePinch && !_rayToolView.ToolActivateState)
                    {
                        OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_UP, true, (IsRightHandedTool ? HAND.right : HAND.left), _rayToolView.ReferenceRay.gameObject, true);
                        OculusEventObserver.Instance.DispatchOculusEvent(EVENT_PINCHINTERACTIONTOOL_PINCH_RELEASED, (IsRightHandedTool ? HAND.right : HAND.left), _fingerTipPokeToolView.gameObject.transform, _rayToolView.ReferenceRay);
                    }
                    else if (!_previousStatePinch && _rayToolView.ToolActivateState)
                    {
                        OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_INDEX_TRIGGER_DOWN, true, (IsRightHandedTool ? HAND.right : HAND.left), _rayToolView.ReferenceRay.gameObject, true);
                        OculusEventObserver.Instance.DispatchOculusEvent(EVENT_PINCHINTERACTIONTOOL_PINCH_PRESSED, (IsRightHandedTool ? HAND.right : HAND.left), _fingerTipPokeToolView.gameObject.transform, _rayToolView.ReferenceRay);        
                    }
                    _previousStatePinch = _rayToolView.ToolActivateState;

                    if (m_pressedStablePinch)
                    {
                        if (!_rayToolView.ToolActivateState)
                        {
                            m_pressedStablePinch = false;
                            OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_UP, true, (IsRightHandedTool ? HAND.right : HAND.left), _rayToolView.ReferenceRay.gameObject, true, true);
                        }
                    }
                    else
                    {
                        if (_rayToolView.ToolActivateState)
                        {
                            m_timeAcumDetectStablePinch += Time.deltaTime;
                            if (m_timeAcumDetectStablePinch > TIME_FOR_HAND_TRIGGER)
                            {
                                m_timeAcumDetectStablePinch = 0;
                                m_pressedStablePinch = true;
                                OculusEventObserver.Instance.DispatchOculusEvent(OculusHandsManager.EVENT_OCULUSHANDMANAGER_HAND_TRIGGER_DOWN, true, (IsRightHandedTool ? HAND.right : HAND.left), _rayToolView.ReferenceRay.gameObject, true, true);
                            }
                        }
                    }
                }
            }
        }

        public override List<InteractableCollisionInfo> GetNextIntersectingObjects()
        {
            return null;
        }

        public override void FocusOnInteractable(OculusSampleFramework.Interactable focusedInteractable, ColliderZone colliderZone)
        {
        }

        public override void DeFocus()
        {
        }
#endif
    }
}