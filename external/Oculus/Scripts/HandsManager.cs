using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourVRUI
{
#if !ENABLE_OCULUS
    public class HandsManager : MonoBehaviour
    {
        [SerializeField] GameObject _leftHand = null;
        [SerializeField] GameObject _rightHand = null;
    }
#endif
}
