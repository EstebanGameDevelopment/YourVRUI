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
	 * Screen used to display pages of information
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenInformationView : MonoBehaviour, IBasicScreenView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_CHANGED_PAGE_POPUP = "EVENT_CHANGED_PAGE_POPUP";
		public const string EVENT_CONFIRMATION_POPUP = "EVENT_CONFIRMATION_POPUP";
		public const string EVENT_FORCE_DESTRUCTION_POPUP = "EVENT_FORCE_DESTRUCTION_POPUP";
		public const string EVENT_FORCE_TRIGGER_OK_BUTTON = "EVENT_FORCE_TRIGGER_OK_BUTTON";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private Button m_ok;
		private Button m_cancel;
		private Button m_next;
		private Button m_previous;
		private Button m_abort;
		private Text m_text;
		private Text m_title;
		private Image m_imageContent;

		private int m_page = 0;
		private List<PageInformation> m_pagesInfo = new List<PageInformation>();
		private bool m_forceLastPage = false;
		private bool m_lastPageVisited = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool ForceLastPage
		{
			get { return m_forceLastPage; }
			set { m_forceLastPage = value; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			List<PageInformation> listPages = (List<PageInformation>)_list[0];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			if (m_container.Find("Button_OK") != null)
			{
				m_ok = m_container.Find("Button_OK").GetComponent<Button>();
				m_ok.gameObject.GetComponent<Button>().onClick.AddListener(OkPressed);
			}
			if (m_container.Find("Button_Cancel") != null)
			{
				m_cancel = m_container.Find("Button_Cancel").GetComponent<Button>();
				m_cancel.gameObject.GetComponent<Button>().onClick.AddListener(CancelPressed);
			}
			if (m_container.Find("Button_Next") != null)
			{
				m_next = m_container.Find("Button_Next").GetComponent<Button>();
				m_next.gameObject.GetComponent<Button>().onClick.AddListener(NextPressed);
			}
			if (m_container.Find("Button_Previous") != null)
			{
				m_previous = m_container.Find("Button_Previous").GetComponent<Button>();
				m_previous.gameObject.GetComponent<Button>().onClick.AddListener(PreviousPressed);
			}
			if (m_container.Find("Button_Abort") != null)
			{
				m_abort = m_container.Find("Button_Abort").GetComponent<Button>();
				m_abort.gameObject.GetComponent<Button>().onClick.AddListener(AbortPressed);
			}

			if (m_container.Find("Text") != null)
			{
				m_text = m_container.Find("Text").GetComponent<Text>();
			}
			if (m_container.Find("Title") != null)
			{
				m_title = m_container.Find("Title").GetComponent<Text>();
			}

			if (m_container.Find("Image_Background/Image_Content") != null)
			{
				m_imageContent = m_container.Find("Image_Background/Image_Content").GetComponent<Image>();
			}

			if (listPages != null)
			{
				for (int i = 0; i < listPages.Count; i++)
				{
					m_pagesInfo.Add(((PageInformation)listPages[i]).Clone());
				}
			}

			ScreenVREventController.Instance.ScreenVREvent += new ScreenVREventHandler(OnBasicEvent);

			ChangePage(0);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			ScreenVREventController.Instance.ScreenVREvent -= OnBasicEvent;
			GameObject.DestroyObject(this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * OkPressed
		 */
		private void OkPressed()
		{
			if (m_page + 1 < m_pagesInfo.Count)
			{
				ChangePage(1);
				return;
			}

			ScreenVREventController.Instance.DispatchScreenVREvent(EVENT_CONFIRMATION_POPUP, this.gameObject, true, m_pagesInfo[m_page].EventData);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * CancelPressed
		 */
		private void CancelPressed()
		{
			ScreenVREventController.Instance.DispatchScreenVREvent(EVENT_CONFIRMATION_POPUP, this.gameObject, false, m_pagesInfo[m_page].EventData);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * AbortPressed
		 */
		private void AbortPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * NextPressed
		 */
		private void NextPressed()
		{
			ChangePage(1);
		}

		// -------------------------------------------
		/* 
		 * PreviousPressed
		 */
		private void PreviousPressed()
		{
			ChangePage(-1);
		}

		// -------------------------------------------
		/* 
		 * Chage the information page
		 */
		private void ChangePage(int _value)
		{
			m_page += _value;
			if (m_page < 0) m_page = 0;
			if (m_pagesInfo.Count == 0)
			{
				return;
			}
			else
			{
				if (m_page >= m_pagesInfo.Count - 1)
				{
					m_page = m_pagesInfo.Count - 1;
					m_lastPageVisited = true;
				}
			}

			if ((m_page >= 0) && (m_page < m_pagesInfo.Count))
			{
				if (m_title != null) m_title.text = m_pagesInfo[m_page].MyTitle;
				if (m_text != null) m_text.text = m_pagesInfo[m_page].MyText;
				if (m_imageContent != null)
				{
					if (m_pagesInfo[m_page].MySprite != null)
					{
						m_imageContent.sprite = m_pagesInfo[m_page].MySprite;
					}
				}
			}

			if (m_cancel != null) m_cancel.gameObject.SetActive(true);
			if (m_pagesInfo.Count == 1)
			{
				if (m_next != null) m_next.gameObject.SetActive(false);
				if (m_previous != null) m_previous.gameObject.SetActive(false);
				if (m_ok != null) m_ok.gameObject.SetActive(true);
			}
			else
			{
				if (m_page == 0)
				{
					if (m_previous != null) m_previous.gameObject.SetActive(false);
					if (m_next != null) m_next.gameObject.SetActive(true);
				}
				else
				{
					if (m_page == m_pagesInfo.Count - 1)
					{
						if (m_previous != null) m_previous.gameObject.SetActive(true);
						if (m_next != null) m_next.gameObject.SetActive(false);
					}
					else
					{
						if (m_previous != null) m_previous.gameObject.SetActive(true);
						if (m_next != null) m_next.gameObject.SetActive(true);
					}
				}

				ScreenVREventController.Instance.DispatchScreenVREvent(EVENT_CHANGED_PAGE_POPUP, this.gameObject, m_pagesInfo[m_page].EventData);
			}
		}

		// -------------------------------------------
		/* 
		 * SetTitle
		 */
		public void SetTitle(string _text)
		{
			if (m_title != null)
			{
				m_title.text = _text;
			}
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == KeyEventInputController.ACTION_CANCEL_BUTTON)
			{
				if (m_forceLastPage)
				{
					if (m_lastPageVisited)
					{
						ScreenVREventController.Instance.DispatchScreenVREvent(EVENT_CONFIRMATION_POPUP, this.gameObject, false, m_pagesInfo[m_page].EventData);
						Destroy();
					}
				}
				else
				{
					ScreenVREventController.Instance.DispatchScreenVREvent(EVENT_CONFIRMATION_POPUP, this.gameObject, false, m_pagesInfo[m_page].EventData);
					Destroy();
				}
			}
			if (_nameEvent == EVENT_FORCE_DESTRUCTION_POPUP)
			{
				Destroy();
			}
			if (_nameEvent == EVENT_FORCE_TRIGGER_OK_BUTTON)
			{
				OkPressed();
			}
		}
	}
}