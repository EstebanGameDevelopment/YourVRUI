/***
 * Author: Yunhan Li
 * Any issue please contact yunhn.lee@gmail.com
 ***/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using YourCommonTools;
using System;

namespace YourVRUI
{
    public class KeyboardManager : MonoBehaviour
    {
        public const string EVENT_KEYBOARDMANAGER_KEYCODE = "EVENT_KEYBOARDMANAGER_KEYCODE";
        public const string EVENT_KEYBOARDMANAGER_ENTER = "EVENT_KEYBOARDMANAGER_ENTER";
        public const string EVENT_KEYBOARDMANAGER_BACKSPACE = "EVENT_KEYBOARDMANAGER_BACKSPACE";
        public const string EVENT_KEYBOARDMANAGER_CLEAR = "EVENT_KEYBOARDMANAGER_CLEAR";
        public const string EVENT_KEYBOARDMANAGER_CAPSLOCK = "EVENT_KEYBOARDMANAGER_CAPSLOCK";
        public const string EVENT_KEYBOARDMANAGER_SHIFT = "EVENT_KEYBOARDMANAGER_SHIFT";

        [Header("User defined")]
        [Tooltip("If the character is uppercase at the initialization")]
        public bool isUppercase = false;
        public int maxInputLength;

        [Header("UI Elements")]
        public Text inputText;

        [Header("Essentials")]
        public Transform keys;

        private string Input
        {
            get { return inputText.text; }
            set { inputText.text = value; }
        }
        private Key[] keyList;
        private bool capslockFlag;

        public void Initialize()
        {
            keyList = keys.GetComponentsInChildren<Key>();
            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
            capslockFlag = isUppercase;
            CapsLocK();
        }
        
        public void Destroy()
        {
            UIEventController.Instance.UIEvent -= OnUIEvent;
        }

        private void CapsLocK()
        {
            foreach (var key in keyList)
            {
                if (key is Alphabet)
                {
                    key.CapsLock(capslockFlag);
                }
            }
            capslockFlag = !capslockFlag;
        }

        private void OnUIEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_KEYBOARDMANAGER_KEYCODE)
            {
                if (Input.Length > maxInputLength) { return; }
                Input += (string)_list[0];
            }
            if (_nameEvent == EVENT_KEYBOARDMANAGER_ENTER)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenVRKeyboardView.EVENT_SCREENVRKEYBOARD_CONFIRM_INPUT);
            }
            if (_nameEvent == EVENT_KEYBOARDMANAGER_BACKSPACE)
            {
                if (Input.Length > 0)
                {
                    Input = Input.Remove(Input.Length - 1);
                }
                else
                {
                    return;
                }
            }
            if (_nameEvent == EVENT_KEYBOARDMANAGER_CLEAR)
            {
                Input = "";
            }
            if (_nameEvent == EVENT_KEYBOARDMANAGER_CAPSLOCK)
            {
                CapsLocK();
            }
            if (_nameEvent == EVENT_KEYBOARDMANAGER_SHIFT)
            {
                foreach (var key in keyList)
                {
                    if (key is Shift)
                    {
                        key.ShiftKey();
                    }
                }
            }
        }
    }
}