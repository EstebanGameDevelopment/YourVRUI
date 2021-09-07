using UnityEngine;
#if ENABLE_PICONEO
using Pvr_UnitySDKAPI;
#endif

namespace YourCommonTools
{
    /******************************************
	 * 
	 * PicoNeoHeadController
	 * 
	 * @author Esteban Gallardo
	 */
    public class PicoNeoHeadController : MonoBehaviour
    {
        public GameObject ControlledHead;

#if ENABLE_PICONEO
        // -------------------------------------------
        /* 
		 * Update
		 */
        void Update()
        {
            if (ControlledHead != null)
            {
                ControlledHead.transform.position = Pvr_UnitySDKSensor.Instance.HeadPose.Position;
                ControlledHead.transform.localRotation = Pvr_UnitySDKSensor.Instance.HeadPose.Orientation;
            }
        }
#endif
    }
}

