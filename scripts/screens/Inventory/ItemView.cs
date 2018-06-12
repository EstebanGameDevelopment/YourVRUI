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
	 * ItemView
	 * 
	 * (DEBUG CODE) It's only used for debug purposes. 
	 * Inventory item
	 * 
	 * @author Esteban Gallardo
	 */
	public class ItemView : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_INVENTORY_ITEM_SELECTED = "EVENT_INVENTORY_ITEM_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private int m_id;
		private Text m_text;
		private Image m_background;
		private bool m_selected = false;
		private GameObject m_selector;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public virtual bool Selected
		{
			get { return m_selected; }
			set
			{
				m_selected = value;
				if (m_selected)
				{
					m_background.color = Color.cyan;
				}
				else
				{
					m_background.color = Color.white;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Initialization
		 */
		public void Initialization(int _id, string _nameItem, Sprite _spriteItem)
		{
			m_id = _id;
			m_text = transform.Find("Text").GetComponent<Text>();
			m_background = transform.GetComponent<Image>();
			transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

			m_text.text = _nameItem;
			transform.Find("Image").GetComponent<Image>().sprite = _spriteItem;
		}

		// -------------------------------------------
		/* 
		 * ButtonPressed
		 */
		public void ButtonPressed()
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_INVENTORY_ITEM_SELECTED, m_id);
		}
	}
}