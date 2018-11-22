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
	 * ScreenInformationView
	 * 
	 * (DEBUG CODE) It's only used for debug purposes. 
	 * Screen used to allow the user to introduce a code number
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenCodeNumberView : MonoBehaviour, IBasicView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_PANELCODEBLOCK_OPEN_VIEW = "EVENT_PANELCODEBLOCK_OPEN_VIEW";
		public const string EVENT_PANELCODEBLOCK_CLOSE_VIEW = "EVENT_PANELCODEBLOCK_CLOSE_VIEW";
		public const string EVENT_PANELCODEBLOCK_CODE_ENTERED = "EVENT_PANELCODEBLOCK_CODE_ENTERED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private Text m_codeNumber;
		private bool m_isDestroyed = false;

		public string NameOfScreen
		{
			get
			{
				return "";
			}

			set
			{
				
			}
		}
        public int Layer
        {
            get { return 0; }
            set { }
        }
        public bool IsMarkedToBeDestroyed
        {
            get { return false; }
            set { }
        }

        // -------------------------------------------
        /* 
		 * InitScreen
		 */
        public void Initialize(params object[] _list)
		{
			UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");
			m_codeNumber = m_container.transform.Find("CodeNumber").gameObject.GetComponent<Text>();

			m_container.transform.Find("Button_Cancel").gameObject.GetComponent<Button>().onClick.AddListener(CancelButton);
			m_container.transform.Find("Button_Delete").gameObject.GetComponent<Button>().onClick.AddListener(DeleteButton);

			m_container.transform.Find("Numbers/Button_Number0").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber0);
			m_container.transform.Find("Numbers/Button_Number1").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber1);
			m_container.transform.Find("Numbers/Button_Number2").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber2);
			m_container.transform.Find("Numbers/Button_Number3").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber3);
			m_container.transform.Find("Numbers/Button_Number4").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber4);
			m_container.transform.Find("Numbers/Button_Number5").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber5);
			m_container.transform.Find("Numbers/Button_Number6").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber6);
			m_container.transform.Find("Numbers/Button_Number7").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber7);
			m_container.transform.Find("Numbers/Button_Number8").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber8);
			m_container.transform.Find("Numbers/Button_Number9").gameObject.GetComponent<Button>().onClick.AddListener(ButtonNumber9);

			m_container.transform.Find("Button_OK").gameObject.GetComponent<Button>().onClick.AddListener(OkButton);

			m_codeNumber.text = "";
		}


		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
			if (m_isDestroyed) return true;
			m_isDestroyed = true;

			UIEventController.Instance.UIEvent -= OnBasicEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return true;
		}

		// -------------------------------------------
		/* 
		 * SetActivation
		 */
		public void SetActivation(bool _activation)
		{

		}

		// -------------------------------------------
		/* 
		 * InitCamera
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == KeysEventInputController.ACTION_CANCEL_BUTTON)
			{
				if (YourVRUIScreenController.Instance.KeysEnabled)
				{
					YourVRUIScreenController.Instance.KeysEnabled = false;
				}
				else
				{
					Destroy();
				}				
			}
			if (_nameEvent == EVENT_PANELCODEBLOCK_CLOSE_VIEW)
			{
				Destroy();
			}
		}

		// -------------------------------------------
		/* 
		 * OkButton
		 */
		private void OkButton()
		{
			// REPORT CODE
			int responseCode = -1;
			if (int.TryParse(m_codeNumber.text, out responseCode))
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_PANELCODEBLOCK_CODE_ENTERED, responseCode);
			}

			Destroy();
		}

		// -------------------------------------------
		/* 
		 * CancelButton
		 */
		private void CancelButton()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * DeleteButton
		 */
		private void DeleteButton()
		{
			string buf = m_codeNumber.text;

			if (buf.Length > 0)
			{
				buf = buf.Substring(0, buf.Length - 1);
			}

			m_codeNumber.text = buf;
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber0
		 */
		private void ButtonNumber0()
		{
			m_codeNumber.text = m_codeNumber.text + "0";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber1
		 */
		private void ButtonNumber1()
		{
			m_codeNumber.text = m_codeNumber.text + "1";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber0
		 */
		private void ButtonNumber2()
		{
			m_codeNumber.text = m_codeNumber.text + "2";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber3
		 */
		private void ButtonNumber3()
		{
			m_codeNumber.text = m_codeNumber.text + "3";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber4
		 */
		private void ButtonNumber4()
		{
			m_codeNumber.text = m_codeNumber.text + "4";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber5
		 */
		private void ButtonNumber5()
		{
			m_codeNumber.text = m_codeNumber.text + "5";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber6
		 */
		private void ButtonNumber6()
		{
			m_codeNumber.text = m_codeNumber.text + "6";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber7
		 */
		private void ButtonNumber7()
		{
			m_codeNumber.text = m_codeNumber.text + "7";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber8
		 */
		private void ButtonNumber8()
		{
			m_codeNumber.text = m_codeNumber.text + "8";
		}

		// -------------------------------------------
		/* 
		 * ButtonNumber9
		 */
		private void ButtonNumber9()
		{
			m_codeNumber.text = m_codeNumber.text + "9";
		}
	}
}