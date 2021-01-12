using UnityEngine;
using System.Collections;
using YourCommonTools;

namespace YourVRUI
{

    /******************************************
	 * 
	 * PlayerRaycasterController
	 * 
	 * You should add this class to the player with the camera.
	 * This class will do the raycasting and it will check the
	 * world for objects with InteractionController script
	 * 
	 * @author Esteban Gallardo
	 */
    public class PlayerRaycasterController : MonoBehaviour
    {
        // ----------------------------------------------
        // PUBLIC EVENTS
        // ----------------------------------------------	
        public const string EVENT_PLAYERRAYSCASTER_REQUEST_RAYCAST = "EVENT_PLAYERRAYSCASTER_REQUEST_RAYCAST";

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        [Tooltip("Layers you want to ignore from the raycasting")]
        public string[] IgnoreLayers = new string[] { "UI" };

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private InteractionController m_previousCollidedObject = null;

        private GameObject m_referenceElementInScrollRect;
        private Vector3 m_positionCollisionAnchor;
        private Vector3 m_forwardCollisionAnchor;
        private float m_distanceToCollisionScrollRect = -1;
        private float m_referenceAngleForDirection = -1;
        private bool m_isVerticalGridToMove = false;
        private bool m_detectionMovementScrollRect = false;

        // -------------------------------------------
        /* 
		 * Initialitzation listener
		 */
        public void Initialize()
        {
            // IF ENABLED COLLISION DETECTION IT WILL LOOK FOR A COLLIDER, IF THERE IS NO COLLIDER IT WILL ADD ONE
            if (YourVRUIScreenController.Instance.EnableCollisionDetection)
            {
                if (!Utilities.IsThereABoxCollider(this.gameObject))
                {
                    Bounds bounds = Utilities.CalculateBounds(this.gameObject);
                    this.gameObject.AddComponent<BoxCollider>();
                    float scaleBoxCollider = 2.5f;
                    this.gameObject.GetComponent<BoxCollider>().size = new Vector3(bounds.size.x * scaleBoxCollider, bounds.size.y, bounds.size.z * scaleBoxCollider);
                    this.gameObject.GetComponent<BoxCollider>().isTrigger = true;
                    if (YourVRUIScreenController.Instance.DebugMode)
                    {
                        Debug.Log("PlayerRaycasterController::Start::ADDED A BOX COLLIDER WITH SIZE=" + this.gameObject.GetComponent<BoxCollider>().size.ToString());
                    }
                }
                if (YourVRUIScreenController.Instance.TagPlayerDetectionCollision != null)
                {
                    if (YourVRUIScreenController.Instance.TagPlayerDetectionCollision.Length > 0)
                    {
                        this.gameObject.tag = YourVRUIScreenController.Instance.TagPlayerDetectionCollision;
                    }
                }
            }

            UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);
        }

        // -------------------------------------------
        /* 
		 * Destroy all references
		 */
        void OnDestroy()
        {
            Destroy();
        }

        // -------------------------------------------
        /* 
		 * Destroy all references
		 */
        public void Destroy()
        {
            UIEventController.Instance.UIEvent -= OnBasicEvent;
        }

        // -------------------------------------------
        /* 
		 * Reset all the reference with objects of the world
		 */
        private void ResetLinkedElementScrollRect()
        {
            m_referenceElementInScrollRect = null;
            m_positionCollisionAnchor = Vector3.zero;
            m_distanceToCollisionScrollRect = -1;
            m_referenceAngleForDirection = -1;
            m_detectionMovementScrollRect = false;
            // ScreenVREventController.Instance.DelayScreenVREvent(BaseVRScreenView.EVENT_SCREEN_DISABLE_ACTION_BUTTON_INTERACTION, 0.1f, true);
        }

        // -------------------------------------------
        /* 
		 * Manager of global events
		 */
        private void OnBasicEvent(string _nameEvent, params object[] _list)
        {
            if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
            {
                ResetLinkedElementScrollRect();
                bool triggeredDispatchScreen = false;
                if (YourVRUIScreenController.Instance.EnableCollisionDetection)
                {
                    if (m_previousCollidedObject != null)
                    {
                        InteractionController objectCollided = GetControllerCollided();
                        if (objectCollided == m_previousCollidedObject)
                        {
                            m_previousCollidedObject.DispatchScreen(this.gameObject, IgnoreLayers, true);
                            triggeredDispatchScreen = true;
                        }
                    }
                }
                if (!triggeredDispatchScreen)
                {
                    if (!YourVRUIScreenController.Instance.IsDayDreamActivated)
                    {
                        CheckRaycastingNormal(true);
                    }
                    else
                    {
                        CheckRaycastingDaydream(true);
                    }
                }
            }
            if (_nameEvent == KeysEventInputController.ACTION_SET_ANCHOR_POSITION)
            {
                ResetLinkedElementScrollRect();
                RaycastHit objectCollided;
                if (!YourVRUIScreenController.Instance.IsDayDreamActivated)
                {
                    objectCollided = Utilities.GetRaycastHitInfoByRay(YourVRUIScreenController.Instance.GameCamera.transform.position, YourVRUIScreenController.Instance.GameCamera.transform.forward);
                }
                else
                {
                    objectCollided = Utilities.GetRaycastHitInfoByRay(YourVRUIScreenController.Instance.LaserPointer.transform.position, YourVRUIScreenController.Instance.LaserPointer.transform.forward);
                }
                if (objectCollided.collider != null)
                {
                    m_referenceElementInScrollRect = objectCollided.collider.gameObject;
                    m_positionCollisionAnchor = objectCollided.point;
                    m_distanceToCollisionScrollRect = -1;
                    m_referenceAngleForDirection = -1;
                    m_detectionMovementScrollRect = false;
                    UIEventController.Instance.DispatchUIEvent(BaseVRScreenView.EVENT_SCREEN_CHECK_ELEMENT_BELONGS_TO_SCROLLRECT, objectCollided.collider.gameObject);
                }
            }
            if (_nameEvent == BaseVRScreenView.EVENT_SCREEN_RESPONSE_ELEMENT_BELONGS_TO_SCROLLRECT)
            {
                GameObject gameObjectChecked = (GameObject)_list[0];
                if (m_referenceElementInScrollRect == gameObjectChecked)
                {
                    bool isInsideScrollRect = (bool)_list[1];
                    m_isVerticalGridToMove = (bool)_list[2];
                    if (!isInsideScrollRect)
                    {
                        ResetLinkedElementScrollRect();
                    }
                    else
                    {
                        if (!YourVRUIScreenController.Instance.IsDayDreamActivated)
                        {
                            m_forwardCollisionAnchor = YourVRUIScreenController.Instance.GameCamera.transform.forward;
                            m_distanceToCollisionScrollRect = Vector3.Distance(m_positionCollisionAnchor, YourVRUIScreenController.Instance.GameCamera.transform.position);
                        }
                        else
                        {
                            m_forwardCollisionAnchor = YourVRUIScreenController.Instance.LaserPointer.transform.forward;
                            m_distanceToCollisionScrollRect = Vector3.Distance(m_positionCollisionAnchor, YourVRUIScreenController.Instance.LaserPointer.transform.position);
                        }
                    }
                }
            }
            if (_nameEvent == EVENT_PLAYERRAYSCASTER_REQUEST_RAYCAST)
            {
                CheckRaycastingNormal(true, true);
            }

            if (_nameEvent == InteractionController.EVENT_INTERACTIONCONTROLLER_COLLIDED_WITH_PLAYER)
            {
                if (YourVRUIScreenController.Instance.EnableCollisionDetection)
                {
                    m_previousCollidedObject = (InteractionController)_list[0];
                }
            }

            if (_nameEvent == EventSystemController.EVENT_EVENTSYSTEMCONTROLLER_RAYCASTING_SYSTEM)
            {
#if !ENABLE_OCULUS && !ENABLE_HTCVIVE && UNITY_HAS_GOOGLEVR && ENABLE_PARTY_2018
                if (this.gameObject.GetComponent<GvrPointerPhysicsRaycaster>() != null)
                {
                    this.gameObject.GetComponent<GvrPointerPhysicsRaycaster>().enabled = (bool)_list[0];
                }
#endif
            }
        }

        // -------------------------------------------
        /* 
		 * Check if there is a InteractionController object in the sight of the camera
		 */
        private InteractionController GetControllerCollided()
        {
            RaycastHit objectCollided = new RaycastHit();
            if (YourVRUIScreenController.Instance.LayersToRaycast.Length == 0)
            {
                objectCollided = Utilities.GetRaycastHitInfoByRay(YourVRUIScreenController.Instance.GameCamera.transform.position, YourVRUIScreenController.Instance.GameCamera.transform.forward, IgnoreLayers);
            }
            else
            {
                Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.GameCamera.transform.position, YourVRUIScreenController.Instance.GameCamera.transform.forward, ref objectCollided, YourVRUIScreenController.Instance.LayersToRaycast);
            }
            if (objectCollided.collider != null)
            {
                GameObject goCollided = objectCollided.collider.gameObject;
                InteractionController interactedObject = goCollided.GetComponent<InteractionController>();
                return interactedObject;
            }
            return null;
        }

        // -------------------------------------------
        /* 
		 * Check the raycasting using the gaze of the camera
		 */
        private void CheckRaycastingNormal(bool _actionButtonPressed, bool _force = false)
        {
            RaycastHit objectCollided = new RaycastHit();
            if ((YourVRUIScreenController.Instance.LayersToRaycast == null) || (YourVRUIScreenController.Instance.LayersToRaycast.Length == 0))
            {
                objectCollided = Utilities.GetRaycastHitInfoByRay(YourVRUIScreenController.Instance.GameCamera.transform.position, YourVRUIScreenController.Instance.GameCamera.transform.forward, IgnoreLayers);
            }
            else
            {
                Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.GameCamera.transform.position, YourVRUIScreenController.Instance.GameCamera.transform.forward, ref objectCollided, YourVRUIScreenController.Instance.LayersToRaycast);
            }
            CheckRaycasting(_actionButtonPressed, objectCollided, _force);
        }

        // -------------------------------------------
        /* 
		 * Check the raycasting using the draydream laser pointer
		 */
        private void CheckRaycastingDaydream(bool _actionButtonPressed)
        {
            RaycastHit objectCollided = new RaycastHit();
            if (YourVRUIScreenController.Instance.LayersToRaycast.Length == 0)
            {
                objectCollided = Utilities.GetRaycastHitInfoByRay(YourVRUIScreenController.Instance.LaserPointer.transform.position, YourVRUIScreenController.Instance.LaserPointer.transform.forward, IgnoreLayers);
            }
            else
            {
                Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.LaserPointer.transform.position, YourVRUIScreenController.Instance.LaserPointer.transform.forward, ref objectCollided, YourVRUIScreenController.Instance.LayersToRaycast);
            }
            CheckRaycasting(_actionButtonPressed, objectCollided);
        }

        // -------------------------------------------
        /* 
		 * Calculate the raycasting operation to look for InteractionController objects
		 */
        private void CheckRaycasting(bool _actionButtonPressed, RaycastHit _objectCollided, bool _force = false)
        {
            if (_objectCollided.collider != null)
            {
                GameObject goCollided = _objectCollided.collider.gameObject;
                InteractionController interactedObject = goCollided.GetComponent<InteractionController>();
                if (interactedObject != null)
                {
                    float distanceToCollidedObject = Vector3.Distance(YourVRUIScreenController.Instance.GameCamera.transform.position, goCollided.transform.position);
                    if (distanceToCollidedObject < interactedObject.DetectionDistance)
                    {
                        if (((interactedObject.TriggerMessageOnDetection) && (m_previousCollidedObject != interactedObject))
                            || ((interactedObject.TriggerMessageOnDetection) && (m_previousCollidedObject == interactedObject) && _actionButtonPressed)
                            || (!interactedObject.TriggerMessageOnDetection && _actionButtonPressed))
                        {
                            if (YourVRUIScreenController.Instance.EnableRaycastDetection || _force)
                            {
                                if (_actionButtonPressed)
                                {
                                    m_previousCollidedObject = interactedObject;
                                    if (!interactedObject.ScreenIsDisplayed)
                                    {
                                        if (YourVRUIScreenController.Instance.DebugMode)
                                        {
                                            Debug.Log("PlayerRaycasterController::CheckRaycasting::COLLIDED WITH AN OBJECT[" + interactedObject.name + "] IS [" + distanceToCollidedObject + "] AWAY FROM PLAYER");
                                        }
                                        interactedObject.DispatchScreen(this.gameObject, IgnoreLayers, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Raycast detection
		 */
        void Update()
        {
            if (YourVRUIScreenController.Instance == null) return;

            // HIGHTLIGHT
            if (!YourVRUIScreenController.Instance.IsDayDreamActivated)
            {
                CheckRaycastingNormal(false);

                if (!YourVRUIScreenController.Instance.KeysEnabled)
                {
                    RaycastHit objectCollided = Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.GameCamera.transform.position, YourVRUIScreenController.Instance.GameCamera.transform.forward, IgnoreLayers);
                    if (objectCollided.collider != null)
                    {
                        UIEventController.Instance.DispatchUIEvent(ButtonVRView.EVENT_SELECTED_VR_BUTTON_COMPONENT, objectCollided.collider.gameObject);
                    }
                }
            }
            else
            {
                CheckRaycastingDaydream(false);

                if (!YourVRUIScreenController.Instance.KeysEnabled)
                {
                    RaycastHit objectCollided = Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.LaserPointer.transform.position, YourVRUIScreenController.Instance.LaserPointer.transform.forward, IgnoreLayers);
                    if (objectCollided.collider != null)
                    {
                        UIEventController.Instance.DispatchUIEvent(ButtonVRView.EVENT_SELECTED_VR_BUTTON_COMPONENT, objectCollided.collider.gameObject);
                    }
                }
            }

            // RESET ANY PREVIOUS CONNECTION BY DISTANCE BY THE OBJECT
            if (m_previousCollidedObject != null)
            {
                if (Vector3.Distance(m_previousCollidedObject.gameObject.transform.position, YourVRUIScreenController.Instance.GameCamera.transform.position) > 1.2f * m_previousCollidedObject.DetectionDistance)
                {
                    if (YourVRUIScreenController.Instance.DebugMode)
                    {
                        Debug.Log("PlayerRaycasterController::Update::REFERENCE DESTROYED BY DISTANCE");
                    }
                    m_previousCollidedObject = null;
                }
            }

            // WE ARE MOVING A SCROLL LIST
            if (m_distanceToCollisionScrollRect != -1)
            {
                Vector3 originPosition;
                Vector3 newPositionMoved;
                Vector3 newForwardMoved;
                if (!YourVRUIScreenController.Instance.IsDayDreamActivated)
                {
                    originPosition = YourVRUIScreenController.Instance.GameCamera.transform.position;
                    newPositionMoved = YourVRUIScreenController.Instance.GameCamera.transform.position + (YourVRUIScreenController.Instance.GameCamera.transform.forward.normalized * m_distanceToCollisionScrollRect);
                }
                else
                {
                    originPosition = YourVRUIScreenController.Instance.LaserPointer.transform.position;
                    newPositionMoved = YourVRUIScreenController.Instance.LaserPointer.transform.position + (YourVRUIScreenController.Instance.LaserPointer.transform.forward.normalized * m_distanceToCollisionScrollRect);
                }
                newForwardMoved = newPositionMoved - originPosition;
                bool directionToScroll = false;
                float newAngleForward = (Mathf.Atan2(originPosition.x - newPositionMoved.x, originPosition.z - newPositionMoved.z) * 180) / Mathf.PI;
                if (m_isVerticalGridToMove)
                {
                    directionToScroll = (newPositionMoved.y < m_positionCollisionAnchor.y);
                }
                else
                {
                    if (m_referenceAngleForDirection == -1)
                    {
                        m_referenceAngleForDirection = newAngleForward;
                    }
                    directionToScroll = (m_referenceAngleForDirection > newAngleForward);
                }
                float angleBetweenForwardVectors = Vector3.Angle(m_forwardCollisionAnchor, newForwardMoved);
                if (!m_detectionMovementScrollRect)
                {
                    if (angleBetweenForwardVectors > 3)
                    {
                        m_detectionMovementScrollRect = true;
                        UIEventController.Instance.DispatchUIEvent(BaseVRScreenView.EVENT_SCREEN_DISABLE_ACTION_BUTTON_INTERACTION, true);
                    }
                }
                UIEventController.Instance.DispatchUIEvent(BaseVRScreenView.EVENT_SCREEN_MOVED_SCROLL_RECT, m_referenceElementInScrollRect, angleBetweenForwardVectors, directionToScroll);
            }
        }
    }
}