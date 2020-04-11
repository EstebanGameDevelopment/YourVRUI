using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourVRUI
{
    /******************************************
	 * 
	 * ScreenVRKeyboardView
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenVRKeyboardView : ScreenBaseView, IBasicView
    {
        public const string SCREEN_NAME = "SCREEN_VR_KEYBOARD";

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREENVRKEYBOARD_CONFIRM_INPUT = "EVENT_SCREENVRKEYBOARD_CONFIRM_INPUT";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
        private InputField m_inputFieldReference;
        private KeyboardManager m_keyboardManager;

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
        {
            base.Initialize(_list);

            m_inputFieldReference = (InputField)_list[0];

            m_root = this.gameObject;
            m_container = m_root.transform.Find("Content");

            m_keyboardManager = m_container.GetComponentInChildren<KeyboardManager>();
            m_keyboardManager.inputText.text = m_inputFieldReference.text;
            m_keyboardManager.Initialize();

            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
		}

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public override bool Destroy()
        {
            if (base.Destroy()) return true;

            m_keyboardManager.Destroy();
            m_keyboardManager = null;

            UIEventController.Instance.UIEvent -= OnUIEvent;
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

            return false;
        }

        // -------------------------------------------
        /* 
		 * OnUIEvent
		 */
        private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == KeysEventInputController.ACTION_CANCEL_BUTTON)
			{
				Destroy();
			}
            if (_nameEvent == EVENT_SCREENVRKEYBOARD_CONFIRM_INPUT)
            {
                m_inputFieldReference.text = m_keyboardManager.inputText.text;
                // m_inputFieldReference.transform.GetComponentInParent<ScreenBaseView>().gameObject.SetActive(true);
                Destroy();
            }
		}
	}
}