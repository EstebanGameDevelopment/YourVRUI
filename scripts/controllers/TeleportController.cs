using System;
using System.Collections.Generic;
using UnityEngine;
using YourCommonTools;

namespace YourVRUI
{
	public class TeleportController : MonoBehaviour
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_TELEPORTCONTROLLER_ACTIVATION     = "EVENT_TELEPORTCONTROLLER_ACTIVATION";
        public const string EVENT_TELEPORTCONTROLLER_COMPLETED      = "EVENT_TELEPORTCONTROLLER_COMPLETED";
        public const string EVENT_TELEPORTCONTROLLER_DEACTIVATION   = "EVENT_TELEPORTCONTROLLER_DEACTIVATION";
        public const string EVENT_TELEPORTCONTROLLER_TELEPORT       = "EVENT_TELEPORTCONTROLLER_TELEPORT";
        public const string EVENT_TELEPORTCONTROLLER_KEY_RELEASED   = "EVENT_TELEPORTCONTROLLER_KEY_RELEASED";

        private const int PARABOLA_PRECISION = 450;

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static TeleportController _instance;

        public static TeleportController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(TeleportController)) as TeleportController;
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public GameObject CameraController;
        public GameObject MarkerDestination;
        public Material LineMaterial;
        public LayerMask AllowedLayers;
        public float MaxTeleportDistance = 4f;
        public float MatScale = 5;
        public Vector3 DestinationNormal;
        public float LineWidth = 0.05f;
        public float Curvature = 0.2f;
        public Color GoodDestinationColor = new Color(0, 0.6f, 1f, 0.2f);
        public Color BadDestinationColor = new Color(0.8f, 0, 0, 0.2f);
        public List<GameObject> TargetsAllowedDestination = new List<GameObject>();

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private Vector3 m_FinalHitLocation;
        private Vector3 m_FinalHitNormal;        
        private GameObject m_FinalHitGameObject;
        private LineRenderer m_lineRenderer;

        private GameObject c_lineParent;
        private GameObject c_line1;
        private GameObject c_line2;

        private GameObject m_markerDestination;

        private Transform m_forwardDirection;
        private bool m_forceRotateBecauseHand = false;

        private bool m_activateTeleport = false;
        private bool m_calculateParabola = false;

        private bool m_hitSomething = false;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool ActivateTeleport
        {
            get { return m_activateTeleport; }
        }
        public Transform ForwardDirection
        {
            get { return m_forwardDirection; }
            set {
                m_forwardDirection = value;
                m_forceRotateBecauseHand = true;
            }
        }

        // -------------------------------------------
        /* 
		 * Start
		 */
        public void Start()
        {
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
            
            c_lineParent = new GameObject("Line");
            Utilities.AttachChild(this.gameObject.transform, c_lineParent);
            c_lineParent.transform.localScale = CameraController.transform.localScale;
            c_line1 = new GameObject("Line1");

            c_line1.transform.SetParent(c_lineParent.transform);
            m_lineRenderer = c_line1.AddComponent<LineRenderer>();
            c_line2 = new GameObject("Line2");
            c_line2.transform.SetParent(c_lineParent.transform);
            m_lineRenderer.startWidth = LineWidth * CameraController.transform.localScale.magnitude;
            m_lineRenderer.endWidth = LineWidth * CameraController.transform.localScale.magnitude;
            m_lineRenderer.material = LineMaterial;
            m_lineRenderer.SetPosition(0, Vector3.zero);
            m_lineRenderer.SetPosition(1, Vector3.zero);

            m_forwardDirection = this.transform;
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        public void OnDestroy()
        {
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
        }

        // -------------------------------------------
        /* 
        * DestroyMarkerTeleport
        */
        private void DestroyMarkerTeleport()
        {
            if (m_markerDestination != null)
            {
                GameObject.Destroy(m_markerDestination);
                m_markerDestination = null;
            }
            if (m_lineRenderer != null)
            {
                if (m_lineRenderer.gameObject != null)
                {
                    m_lineRenderer.gameObject.SetActive(false);
                }
            }            
        }

        // -------------------------------------------
        /* 
        * AllowDestination
        */
        private bool AllowDestination(Ray _ray, out RaycastHit _hit, float _raycastLength)
        {
            bool hitSomething = Physics.Raycast(_ray, out _hit, _raycastLength, AllowedLayers);

            if (hitSomething)
            {
                if (TargetsAllowedDestination == null)
                {
                    return true;
                }
                else
                {
                    if (TargetsAllowedDestination.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        foreach (GameObject item in TargetsAllowedDestination)
                        {
                            if (item == _hit.collider.gameObject)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        // -------------------------------------------
        /* 
        * ComputeParabola
        */
        internal void ComputeParabola()
        {
            if (m_markerDestination == null)
            {
                m_markerDestination = Instantiate(MarkerDestination);
                m_lineRenderer.gameObject.SetActive(true);
            }

            //	Line renderer position storage (two because line renderer texture will stretch if one is used)
            List<Vector3> positions1 = new List<Vector3>();

            //	first Vector3 positions array will be used for the curve and the second line renderer is used for the straight down after the curve
            float totalDistance1 = 0;

            //	Variables need for curve
            Quaternion currentRotation = m_forwardDirection.transform.rotation;
            Vector3 originalPosition = Utilities.Clone(transform.position);
            Vector3 currentPosition = transform.position;
            Vector3 lastPostion;
            positions1.Add(currentPosition);

            lastPostion = transform.position - m_forwardDirection.forward;
            Vector3 currentDirection = m_forwardDirection.forward;
            Vector3 downForward = new Vector3(m_forwardDirection.forward.x * 0.01f, -1, m_forwardDirection.forward.z * 0.01f);
            if (m_forceRotateBecauseHand)
            {
                // Curvature = 1;
                // downForward = new Vector3(m_forwardDirection.forward.x * 0.01f, -1, m_forwardDirection.forward.z * 0.01f);
            }
            RaycastHit hit = new RaycastHit();
            m_FinalHitLocation = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int step = 0; step < PARABOLA_PRECISION; step++)
            {
                Quaternion downRotation = Quaternion.LookRotation(downForward);
                currentRotation = Quaternion.RotateTowards(currentRotation, downRotation, Curvature);

                Ray newRay = new Ray(currentPosition, currentPosition - lastPostion);

                float length = (MaxTeleportDistance * 0.01f) * CameraController.transform.localScale.magnitude;
                if (currentRotation == downRotation)
                {
                    length = (MaxTeleportDistance * MatScale) * CameraController.transform.localScale.magnitude;
                    positions1.Add(currentPosition);
                }

                float raycastLength = length * 1.1f;

                //	Check if we hit something
                m_hitSomething = AllowDestination(newRay, out hit, raycastLength);

                // don't allow to teleport to negative normals (we don't want to be stuck under floors)
                if ((hit.normal.y > 0) && (Vector3.Distance(originalPosition, hit.point) > 1))
                {
                    m_FinalHitLocation = hit.point;
                    m_FinalHitNormal = hit.normal;
                    m_FinalHitGameObject = hit.collider.gameObject;

                    totalDistance1 += (currentPosition - m_FinalHitLocation).magnitude;
                    positions1.Add(m_FinalHitLocation);

                    DestinationNormal = m_FinalHitNormal;

                    break;
                }

                //	Convert the rotation to a forward vector and apply to our current position
                currentDirection = currentRotation * Vector3.forward;
                lastPostion = currentPosition;
                currentPosition += currentDirection * length;

                totalDistance1 += length;
                positions1.Add(currentPosition);

                if (currentRotation == downRotation)
                    break;
            }

            m_lineRenderer.enabled = true;

            m_lineRenderer.material.color = (m_hitSomething?GoodDestinationColor:BadDestinationColor);

            m_lineRenderer.positionCount = positions1.Count;
            m_lineRenderer.SetPositions(positions1.ToArray());

            m_markerDestination.transform.position = positions1[positions1.Count - 1];
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_TELEPORTCONTROLLER_ACTIVATION)
            {
                m_activateTeleport = true;
                m_calculateParabola = true;
            }
            if (_nameEvent == EVENT_TELEPORTCONTROLLER_DEACTIVATION)
            {
                m_activateTeleport = false;
                m_calculateParabola = false;
                DestroyMarkerTeleport();
            }
            if (_nameEvent == EVENT_TELEPORTCONTROLLER_KEY_RELEASED)
            {
                if (m_activateTeleport)
                {
                    if (m_calculateParabola)
                    {
                        if (m_markerDestination != null)
                        {
                            m_calculateParabola = false;
                            Vector3 shiftToTarget = m_markerDestination.transform.position - transform.position;
                            DestroyMarkerTeleport();

                            if (m_hitSomething)
                            {
                                BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_TELEPORTCONTROLLER_TELEPORT, 0.01f, Utilities.Vector3ToString(shiftToTarget));
                            }
                            BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_TELEPORTCONTROLLER_DEACTIVATION, 0.2f);
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        private void Update()
        {
            if (m_activateTeleport)
            {
                if (m_calculateParabola)
                {
                    ComputeParabola();
                }
            }            
        }
    }
}