using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YourVRUI
{
	/******************************************
	 * 
	 * ButtonVRView
	 * 
	 * This class will be added automatically (or manually)
	 * to all interactable elements of an screen
	 * 
	 * @author Esteban Gallardo
	 */
	public class ButtonVRView : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SELECTED_VR_BUTTON_COMPONENT = "EVENT_SELECTED_VR_BUTTON_COMPONENT";
		public const string EVENT_CLICKED_VR_BUTTON = "EVENT_CLICKED_VR_BUTTON";
		public const string EVENT_CHECK_ELEMENT_CORNER_OUT_OF_LIST = "EVENT_CHECK_ELEMENT_CORNER_OUT_OF_LIST";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string SELECTOR_COMPONENT_NAME = "Selector";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_selector;

		// -------------------------------------------
		/* 
		 * We add a visual selector (if there is not already one with the name "Selector")
		 * and we also add a box collider to be able for the screen to be used
		 * with systems like Leap Motion
		 */
		public void Initialize()
		{
			if (transform.Find(SELECTOR_COMPONENT_NAME) != null)
			{
				m_selector = transform.Find(SELECTOR_COMPONENT_NAME).gameObject;
				m_selector.SetActive(false);
			}
			else
			{
				GameObject nodeImage = new GameObject();
				nodeImage.transform.SetParent(transform, false);
				Rect rectBase = GetComponent<RectTransform>().rect;
				Rect mySpriteRect = new Rect(0, 0, YourVRUIScreenController.Instance.SelectorGraphic.rect.width, YourVRUIScreenController.Instance.SelectorGraphic.rect.height);
				UtilitiesYourVRUI.AddSprite(nodeImage, YourVRUIScreenController.Instance.SelectorGraphic, mySpriteRect, rectBase, new Vector2(0.5f, 0.5f));
				nodeImage.name = SELECTOR_COMPONENT_NAME;
				nodeImage.transform.SetAsFirstSibling();
				m_selector = transform.Find(SELECTOR_COMPONENT_NAME).gameObject;
				m_selector.SetActive(false);
			}
			m_selector.layer = LayerMask.NameToLayer("UI");

			if (GetComponent<Collider>() == null)
			{
				this.gameObject.AddComponent<BoxCollider>();
				GetComponent<BoxCollider>().size = new Vector3(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height, 0.1f);
				GetComponent<BoxCollider>().isTrigger = true;
			}

			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.AddListener(OnClickedButton);
			}
			else
			{
				if (this.gameObject.GetComponent<Toggle>() != null)
				{
					this.gameObject.GetComponent<Toggle>().onValueChanged.AddListener(OnValueChangedToggle);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy all the references
		 */
		public void Destroy()
		{
			this.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
			m_selector = null;
		}

		// -------------------------------------------
		/* 
		 * Triggered when there is collision
		 */
		public void OnTriggerEnter(Collider _collision)
		{
			if (_collision != null)
			{
				if (_collision.gameObject != null)
				{
					if (_collision.gameObject.tag == YourVRUIScreenController.UI_TRIGGERER)
					{
						if (YourVRUIScreenController.Instance.DebugThrowProjectile)
						{
							Debug.Log("ButtonVRView::OnTriggerEnter::NAME[" + this.gameObject.name + "]::_collision.collider.gameObject=" + _collision.gameObject.name);
						}
						InvokeButton();
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Will be called to invoke the button functionality
		 */
		public void InvokeButton()
		{
			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.Invoke();
			}
			else
			{
				if (this.gameObject.GetComponent<Toggle>() != null)
				{
					this.gameObject.GetComponent<Toggle>().onValueChanged.Invoke(!this.gameObject.GetComponent<Toggle>().isOn);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Clicked the button
		 */
		public void OnClickedButton()
		{
			ScreenVREventController.Instance.DispatchScreenVREvent(EVENT_CLICKED_VR_BUTTON, this.gameObject);
		}


		// -------------------------------------------
		/* 
		 * Changed the value of toggle
		 */
		public void OnValueChangedToggle(bool _value)
		{
			ScreenVREventController.Instance.DispatchScreenVREvent(EVENT_CLICKED_VR_BUTTON, this.gameObject, _value);
		}

		// -------------------------------------------
		/* 
		 * Will enable the selector component
		 */
		public void EnableSelector(bool _value)
		{
			if (m_selector != null)
			{
				m_selector.SetActive(_value);
				if (_value)
				{
					if (this.gameObject.GetComponent<RectTransform>() != null)
					{
						Rect corners = UtilitiesYourVRUI.GetCornersRectTransform(this.gameObject.GetComponent<RectTransform>());
						ScreenVREventController.Instance.DispatchScreenVREvent(EVENT_CHECK_ELEMENT_CORNER_OUT_OF_LIST, this.gameObject, corners);
					}
				}
			}
		}
	}
}