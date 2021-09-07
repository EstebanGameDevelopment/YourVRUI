using UnityEngine;
#if ENABLE_HTCVIVE
using Wave.Essence;
using Wave.Native;
#endif

namespace YourCommonTools
{
    /******************************************
	 * 
	 * HTCHeadController
	 * 
	 * @author Esteban Gallardo
	 */
    public class HTCHeadController : MonoBehaviour
    {
        public GameObject ControlledHead;

#if ENABLE_HTCVIVE
        // -------------------------------------------
        /* 
		 * Update
		 */
        void Update()
        {
            if (ControlledHead != null)
            {
                var vec = WaveEssence.Instance.GetCurrentControllerPositionOffset(WVR_DeviceType.WVR_DeviceType_Camera);
                var rot = WaveEssence.Instance.GetCurrentControllerRotationOffset(WVR_DeviceType.WVR_DeviceType_Camera);

                ControlledHead.transform.position = vec;
                ControlledHead.transform.localRotation = rot;
            }
        }
#endif
    }
}

