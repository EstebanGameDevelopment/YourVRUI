using System.Collections;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using YourCommonTools;

namespace YourVRUI
{
    public class PlaneAnchorDistance : MonoBehaviour
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_PLANEANCHORDISTANCE_ACTIVATE          = "EVENT_PLANEANCHORDISTANCE_ACTIVATE";
        public const string EVENT_PLANEANCHORDISTANCE_DEACTIVATE        = "EVENT_PLANEANCHORDISTANCE_DEACTIVATE";
        public const string EVENT_PLANEANCHORDISTANCE_MOVED_DISTANCE    = "EVENT_PLANEANCHORDISTANCE_MOVED_DISTANCE";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static PlaneAnchorDistance _instance;

        public static PlaneAnchorDistance Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(PlaneAnchorDistance)) as PlaneAnchorDistance;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        container.name = "PlaneAnchorDistance";
                        _instance = container.AddComponent(typeof(PlaneAnchorDistance)) as PlaneAnchorDistance;
                    }
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private bool m_enabledAnchor = false;
        private bool m_isAnchored = false;
        private Vector3 m_positionCollisionAnchor;
        private Vector3 m_upCollision;
        private Vector3 m_rightCollision;
        private Vector3 m_transpondCoordinates;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool EnabledAnchor
        {
            get { return m_enabledAnchor; }
            set { m_enabledAnchor = value;
                if (!m_enabledAnchor)
                {
                    m_isAnchored = false;
                }
            }
        }
        public bool IsAnchored
        {
            get { return m_isAnchored; }
        }
        public Vector3 TranspondCoordinates
        {
            get { return m_transpondCoordinates; }
        }

        // -------------------------------------------
        /* 
		 * Start
		 */
        private void Start()
        {
            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        private void OnDestroy()
        {
            UIEventController.Instance.UIEvent -= OnUIEvent;
        }

        // -------------------------------------------
        /* 
		 * Transponds the 3D coordinates into a 2D plane
		 * https://stackoverflow.com/questions/16699259/retrieve-2d-co-ordinate-from-a-3d-point-on-a-3d-plane
		 */
        public Vector3 Get2DCoord(Vector3 _target, Vector3 _a, Vector3 _b, Vector3 _c)
        {
            // ab = b - a
            Vector3 ab = _b - _a;
            // ac = c - a
            Vector3 ac = _c - _a;

            // ab = ab / |ab|
            ab.Normalize();
            // ac = ac / |ac|
            ac.Normalize();

            // ap = p - a                      
            Vector3 ap = _target - _a;

            Vector3 position2D = new Vector2(Vector3.Dot(ab, ap), Vector3.Dot(ac, ap));

            // x = ab x ap
            // y = ac x ap
            return position2D;
        }

        // -------------------------------------------
        /* 
		 * OnUIEvent
		 */
        private void OnUIEvent(string _nameEvent, object[] _list)
        {
            if (!m_enabledAnchor) return;

            if (_nameEvent == EVENT_PLANEANCHORDISTANCE_ACTIVATE)
            {
                if (!m_isAnchored)
                {
                    RaycastHit objectCollided = new RaycastHit();
                    if (Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.LaserPointer.transform.position, YourVRUIScreenController.Instance.LaserPointer.transform.forward, ref objectCollided, "UI"))
                    {
                        if (objectCollided.collider != null)
                        {
                            m_positionCollisionAnchor = objectCollided.point;
                            float distanceDots = 0.2f;
                            m_upCollision = m_positionCollisionAnchor + objectCollided.transform.up * distanceDots;
                            m_rightCollision = m_positionCollisionAnchor + objectCollided.transform.right * distanceDots;
                            m_isAnchored = true;
                        }
                    }
                }
            }
            if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
            {
                if (m_isAnchored)
                {
                    m_isAnchored = false;
                    UIEventController.Instance.DispatchUIEvent(EVENT_PLANEANCHORDISTANCE_MOVED_DISTANCE, m_transpondCoordinates);
                }
            }
            if (_nameEvent == EVENT_PLANEANCHORDISTANCE_DEACTIVATE)
            {
                m_isAnchored = false;
                m_transpondCoordinates = Vector3.zero;
            }
        }

        // -------------------------------------------
        /* 
        * Update
        */
        private void Update()
        {
            if (m_enabledAnchor)
            {
                if (m_isAnchored)
                {
                    RaycastHit objectCollided = new RaycastHit();
                    if (Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.LaserPointer.transform.position, YourVRUIScreenController.Instance.LaserPointer.transform.forward, ref objectCollided, "UI"))
                    {
                        if (objectCollided.collider != null)
                        {
                            m_transpondCoordinates = Get2DCoord(objectCollided.point, m_positionCollisionAnchor, m_upCollision, m_rightCollision);
                        }
                    }
                }
            }
        }
    }
}
