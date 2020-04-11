using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourVRUI
{
    public class Key : MonoBehaviour
    {
        protected Text key;

        public virtual void Awake()
        {
            key = transform.Find("Text").GetComponent<Text>();
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (this.gameObject.GetComponent<ButtonVRView>().IsSelected)
                {
                    if (key.text.Equals("ENTER"))
                    {
                        UIEventController.Instance.DispatchUIEvent(KeyboardManager.EVENT_KEYBOARDMANAGER_ENTER);
                    }
                    else if (key.text.Equals("BACKSPACE"))
                    {
                        UIEventController.Instance.DispatchUIEvent(KeyboardManager.EVENT_KEYBOARDMANAGER_BACKSPACE);
                    }
                    else if (key.text.Equals("CLEAR"))
                    {
                        UIEventController.Instance.DispatchUIEvent(KeyboardManager.EVENT_KEYBOARDMANAGER_CLEAR);
                    }
                    else if (key.text.Equals("CAPSLOCK"))
                    {
                        UIEventController.Instance.DispatchUIEvent(KeyboardManager.EVENT_KEYBOARDMANAGER_CAPSLOCK);
                    }
                    else if (key.text.Equals("SHIFT"))
                    {
                        UIEventController.Instance.DispatchUIEvent(KeyboardManager.EVENT_KEYBOARDMANAGER_SHIFT);
                    }
                    else
                    {
                        UIEventController.Instance.DispatchUIEvent(KeyboardManager.EVENT_KEYBOARDMANAGER_KEYCODE, key.text);
                    }                        
                }                
            });
        }

        public virtual void CapsLock(bool isUppercase) { }
        public virtual void ShiftKey() { }
    };
}