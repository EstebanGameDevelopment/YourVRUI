#if ENABLE_OCULUS
using OculusSampleFramework;
#endif
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;

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
		 * OnDestroy
		 */
        private void OnDestroy()
        {
            if (Instance != null)
            {
                GameObject.Destroy(_instance.gameObject);
                _instance = null;
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
            if (_controller ==  OVRInput.Controller.All)
            {
                OVRHaptics.LeftChannel.Preempt(clip);
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
            if (_controller ==  OVRInput.Controller.All)
            {
                OVRHaptics.LeftChannel.Preempt(clip);
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
            if (_controller ==  OVRInput.Controller.All)
            {
                OVRHaptics.LeftChannel.Preempt(clip);
                OVRHaptics.RightChannel.Preempt(clip);
            }
        }

        // -------------------------------------------
        /* 
		 * TriggerCouritineVibration
		 */
        public void TriggerCouritineVibration(float _frequency, float _amplitude, float _duration, OVRInput.Controller _controller)
        {
            StartCoroutine(Haptics(_frequency, _amplitude, _duration, _controller));
        }

        // -------------------------------------------
        /* 
		 * Haptics
		 */
        IEnumerator Haptics(float _frequency, float _amplitude, float _duration, OVRInput.Controller _controller)
        {
            if ((_controller ==  OVRInput.Controller.RTouch) || (_controller ==  OVRInput.Controller.All)) OVRInput.SetControllerVibration(_frequency, _amplitude, OVRInput.Controller.RTouch);
            if ((_controller ==  OVRInput.Controller.LTouch) || (_controller ==  OVRInput.Controller.All)) OVRInput.SetControllerVibration(_frequency, _amplitude, OVRInput.Controller.LTouch);

            yield return new WaitForSeconds(_duration);

            if ((_controller ==  OVRInput.Controller.RTouch) || (_controller ==  OVRInput.Controller.All)) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            if ((_controller ==  OVRInput.Controller.LTouch) || (_controller ==  OVRInput.Controller.All)) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }
#endif
    }
}

