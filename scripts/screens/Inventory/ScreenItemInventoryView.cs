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
	 * ScreenItemInventoryView
	 * 
	 * (DEBUG CODE) It's only used for debug purposes. 
	 * Inventory screen to select items.
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenItemInventoryView : MonoBehaviour, IBasicView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_PANELINVENTROY_OPEN_VIEW = "EVENT_PANELINVENTROY_OPEN_VIEW";
		public const string EVENT_PANELINVENTROY_CLOSE_VIEW = "EVENT_PANELINVENTROY_CLOSE_VIEW";

		// ----------------------------------------------
		// PREFABS
		// ----------------------------------------------	
		public GameObject InventoryItemPrefab;
		public Sprite[] ImageItems;

		// ----------------------------------------------
		// VARIABLE MEMBERS
		// ----------------------------------------------	
		private GameObject m_grid;
		private List<GameObject> m_elements = new List<GameObject>();
		private GameObject m_root;
		private Transform m_container;

		private GameObject m_buttonInteract;
		private GameObject m_buttonClose;
		private GameObject m_buttonDropItem;

		private Sprite m_selectedItem;

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

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			// GET ALL THE REFERENCES TO LAYOUT
			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_buttonInteract = m_container.Find("Base_Map/Button_Interact").gameObject;
			m_buttonClose = m_container.Find("Base_Map/Button_Close").gameObject;
			m_buttonDropItem = m_container.Find("Base_Map/Button_Drop").gameObject;
			m_buttonDropItem.SetActive(false);

			m_grid = m_container.Find("Base_Map/ScrollList/Grid").gameObject;

			UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);

			// BUILD ALL LAYOUT ELEMENTS AND INTERACTIONS
			m_buttonInteract.GetComponent<Button>().onClick.AddListener(InteractItemButton);
			m_buttonDropItem.GetComponent<Button>().onClick.AddListener(DropItemButton);

			for (int i = 0; i < ImageItems.Length; i++)
			{
				Sprite itemSprite = ImageItems[i];
				GameObject instance = Utilities.AddChild(m_grid.transform, InventoryItemPrefab);
				instance.GetComponent<ItemView>().Initialization(i, itemSprite.name, itemSprite);
				m_elements.Add(instance);
			}

			m_buttonClose.GetComponent<Button>().onClick.AddListener(DestroyScreen);

			// FIRST INITIALIZATION
			if (m_elements.Count > 0)
			{
				m_elements[0].GetComponent<ItemView>().ButtonPressed();
			}
			else
			{
				m_root.transform.Find("Content/Base_Map/Name_Label").GetComponent<Text>().text = "Name Item";
				m_root.transform.Find("Content/Base_Map/Description_Label").GetComponent<Text>().text = "Description Item";
				m_root.transform.Find("Content/Base_Map/ImageIcon").GetComponent<Image>().sprite = null;
			}
		}

		// -------------------------------------------
		/* 
		 * Remove all the references
		 */
		private void DestroyScreen()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Remove all the references
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
			if (_nameEvent == EVENT_PANELINVENTROY_CLOSE_VIEW)
			{
				Destroy();
			}
			if (_nameEvent == ItemView.EVENT_INVENTORY_ITEM_SELECTED)
			{
				m_selectedItem = ImageItems[(int)_list[0]];
				m_root.transform.Find("Content/Base_Map/Name_Label").GetComponent<Text>().text = m_selectedItem.name;
				m_root.transform.Find("Content/Base_Map/Description_Label").GetComponent<Text>().text = m_selectedItem.name;
				m_root.transform.Find("Content/Base_Map/ImageIcon").GetComponent<Image>().sprite = m_selectedItem;
			}
			if (_nameEvent == KeysEventInputController.ACTION_CANCEL_BUTTON)
			{
				if ((this.gameObject.GetComponent<BaseVRScreenView>() != null) 
					&& this.gameObject.GetComponent<BaseVRScreenView>().DisableActionButtonInteraction)
				{
					UIEventController.Instance.DispatchUIEvent(BaseVRScreenView.EVENT_SCREEN_DISABLE_ACTION_BUTTON_INTERACTION, false);
				}
				else
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
			}
		}

		// -------------------------------------------
		/* 
		 * ClearList
		 */
		private void ClearList()
		{
			for (int i = 0; i < m_elements.Count; i++)
			{
				GameObject element = m_elements[i];
				Destroy(element);
			}
			m_elements.Clear();
		}

		// -------------------------------------------
		/* 
		 * InteractItemButton
		 */
		private void InteractItemButton()
		{
			if (m_selectedItem != null)
			{
				Debug.Log("ScreenInventoryView::InteractItemButton::m_selectedItem=" + m_selectedItem.name);
			}
		}

		// -------------------------------------------
		/* 
		 * DropItemButton
		 */
		private void DropItemButton()
		{
		}
	}
}