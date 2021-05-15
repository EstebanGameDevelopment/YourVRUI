#if ENABLE_OCULUS
using OculusSampleFramework;
#endif
using UnityEngine;
using UnityEngine.Assertions;

namespace YourVRUI
{
	public class OculusVibrationController : MonoBehaviour
	{
#if ENABLE_OCULUS
        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static OculusVibrationController _instance;

        public static OculusVibrationController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(OculusVibrationController)) as OculusVibrationController;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        container.name = "OculusVibrationController";
                        _instance = container.AddComponent(typeof(OculusVibrationController)) as OculusVibrationController;
                    }
                }
                return _instance;
            }
        }

        // -------------------------------------------
        /* 
		 * TriggerVibration
		 */
        public void TriggerVibration(AudioClip _audioclip, OVRInput.Controller _controller)
        {
            OVRHapticsClip clip = new OVRHapticsClip(_audioclip);

            if (_controller == OVRInput.Controller.LTouch)
            {
                OVRHaptics.LeftChannel.Preempt(clip);
            }
            if (_controller == OVRInput.Controller.RTouch)
            {
                OVRHaptics.RightChannel.Preempt(clip);
            }
        }

        // -------------------------------------------
        /* 
		 * TriggerVibration
		 */
        public void TriggerVibration(int _iteration, int _frequency, int _strength, OVRInput.Controller _controller)
        {
            OVRHapticsClip clip = new OVRHapticsClip();

            for (int i = 0; i < _iteration; i++)
            {
                clip.WriteSample(i % _frequency == 0 ? (byte)_strength : (byte)0);
            }

            if (_controller ==  OVRInput.Controller.LTouch)
            {
                OVRHaptics.LeftChannel.Preempt(clip);
            }
            if (_controller == OVRInput.Controller.RTouch)
            {
                OVRHaptics.RightChannel.Preempt(clip);
            }
        }

        // -------------------------------------------
        /* 
		 * TriggerVibration
		 */
        public void TriggerVibration(int[] _map, OVRInput.Controller _controller)
        {
            OVRHapticsClip clip = new OVRHapticsClip();

            for (int i = 0; i < _map.Length; i++)
            {
                clip.WriteSample((byte)_map[i]);
            }

            if (_controller == OVRInput.Controller.LTouch)
            {
                OVRHaptics.LeftChannel.Preempt(clip);
            }
            if (_controller == OVRInput.Controller.RTouch)
            {
                OVRHaptics.RightChannel.Preempt(clip);
            }
        }
#endif
    }
}

