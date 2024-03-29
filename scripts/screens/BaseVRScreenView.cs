﻿using System;
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
	 * BaseVRScreenView
	 * 
	 * This class should be added to all the screen we want to use in the virtual world
	 * 
	 * @author Esteban Gallardo
	 */
	public class BaseVRScreenView : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREEN_OPEN_VIEW = "EVENT_SCREEN_OPEN_VIEW";
		public const string EVENT_SCREEN_CLOSE_VIEW = "EVENT_SCREEN_CLOSE_VIEW";
		public const string EVENT_SCREEN_DESTROYED_VIEW = "EVENT_SCREEN_DESTROYED_VIEW";

		public const string EVENT_SCREEN_CHECK_ELEMENT_BELONGS_TO_SCROLLRECT = "EVENT_SCREEN_CHECK_ELEMENT_BELONGS_TO_SCROLLRECT";
		public const string EVENT_SCREEN_RESPONSE_ELEMENT_BELONGS_TO_SCROLLRECT = "EVENT_SCREEN_RESPONSE_ELEMENT_BELONGS_TO_SCROLLRECT";
		public const string EVENT_SCREEN_MOVED_SCROLL_RECT = "EVENT_SCREEN_MOVED_SCROLL_RECT";
		public const string EVENT_SCREEN_DISABLE_ACTION_BUTTON_INTERACTION = "EVENT_SCREEN_DISABLE_ACTION_BUTTON_INTERACTION";

        public const string EVENT_SCREEN_ENABLE_ALPHA_ZERO_IMAGE = "EVENT_SCREEN_ENABLE_ALPHA_ZERO_IMAGE";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        public const float FADE_TIME = 1f;
		public const float DELAY_TO_REFOCUS = 0.2f;
        public const float SCROLL_RECT_VR_SPEED_WITH_JOYSTICK = 1F;

        public const string CONTENT_COMPONENT_NAME = "Content";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		[Tooltip("By default all the interaction elements of YourVRUI package will be added automatically. Turn it off if you prefer to take full control and do it manually")]
		public bool AutomaticallyAddButtons = true;

		// ----------------------------------------------
		// PRIVATE VARIABLE MEMBERS
		// ----------------------------------------------	
		private GameObject m_screen;
		private CanvasGroup m_canvasGroup;

		private GameObject m_characterOrigin;

		private int m_selectionButton;
		private List<GameObject> m_selectors;

		private Vector3 m_normal;
		private float m_distance = -1;
		private bool m_refocus = false;

		private bool m_enabledSelector = true;
		private float m_timeToRefocus = -1;

		private bool m_destroyMessageOnDistance = true;
		private GameObject m_playerInteracted = null;
		private float m_distanceToDestroy = -1;

		private bool m_blockOtherScreens = true;
		private bool m_highlightSelector = true;
		private bool m_firstHighlightConsumed = false;

		private List<ScrollRectVR> m_scrollRectsVR = null;
		private List<ScrollRectVR> m_staticScrollRectsVR = null;

		private float m_initialPositionOnScrollList = -1;

		private bool m_disableActionButtonInteraction = false;

        private int m_layerScreen = 0;

        private bool m_enableAlphaZero = true;

        private GameObject m_dotProjectedUI = null;

        private float m_timeToAlpha = 0;

        private bool m_enableScrollDown = false;
        private bool m_enableScrollUp = false;
        private bool m_enableScrollLeft = false;
        private bool m_enableScrollRight = false;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public GameObject Screen
		{
			get { return m_screen; }
		}

		public CanvasGroup CanvasGroup
		{
			get { return m_canvasGroup; }
		}

		public string NameCharacter
		{
			get { return m_characterOrigin.name; }
		}

		public GameObject OriginCharacter
		{
			get { return m_characterOrigin; }
		}

		public bool DisableActionButtonInteraction
		{
			get { return m_disableActionButtonInteraction; }
            set { m_disableActionButtonInteraction = value; }
        }

        public int SelectionButton
        {
            get { return m_selectionButton; }
            set { m_selectionButton = value; }
        }
        public int LayerScreen
        {
            get { return m_layerScreen; }
        }
        public GameObject DotProjectedUI
        {
            get { return m_dotProjectedUI; }
        }

        // -------------------------------------------
        /* 
		 * Initialitzation
		 */
        public void InitBaseScreen(params object[] _list)
		{
			m_characterOrigin = (GameObject)_list[0];
			m_blockOtherScreens = (bool)_list[1];
			m_highlightSelector = (bool)_list[2];
			m_selectionButton = 0;
			m_selectors = new List<GameObject>();
			m_screen = this.gameObject;
			if (m_screen.transform.Find(CONTENT_COMPONENT_NAME) != null)
			{
                GameObject contentBaseRef = m_screen.transform.Find(CONTENT_COMPONENT_NAME).gameObject;
                m_canvasGroup = contentBaseRef.GetComponent<CanvasGroup>();
				if (m_canvasGroup != null)
				{
					m_canvasGroup.alpha = YourVRUIScreenController.Instance.DefaultInitialAlpha;
				}
                if (YourVRUIScreenController.Instance.DotProjectionUI != null)
                {
                    if (contentBaseRef.GetComponent<Collider>() == null)
                    {
                        contentBaseRef.AddComponent<BoxCollider>();
                        if ((contentBaseRef.GetComponent<RectTransform>().rect.width > 0) && (contentBaseRef.GetComponent<RectTransform>().rect.height > 0))
                        {
                            contentBaseRef.GetComponent<BoxCollider>().size = new Vector3(contentBaseRef.GetComponent<RectTransform>().rect.width, contentBaseRef.GetComponent<RectTransform>().rect.height, 0.1f);
                        }
                        else
                        {
                            if (contentBaseRef.GetComponent<LayoutElement>() != null)
                            {
                                contentBaseRef.GetComponent<BoxCollider>().size = new Vector3(contentBaseRef.GetComponent<LayoutElement>().preferredWidth, contentBaseRef.GetComponent<LayoutElement>().preferredHeight, 0.1f);
                            }
                        }

                        contentBaseRef.GetComponent<BoxCollider>().isTrigger = true;
                        contentBaseRef.GetComponent<BoxCollider>().center += this.transform.root.forward * 20;
                    }
                }
            }


            UIEventController.Instance.UIEvent += new UIEventHandler(OnBaseScreenBasicEvent);
			UIEventController.Instance.DispatchUIEvent(EVENT_SCREEN_OPEN_VIEW, this.gameObject, m_blockOtherScreens);
			UIEventController.Instance.DispatchUIEvent(InteractionController.EVENT_INTERACTIONCONTROLLER_SCREEN_CREATED, m_characterOrigin);

			UIEventController.Instance.DispatchUIEvent(YourVRUIScreenController.EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT, false);

			if (AutomaticallyAddButtons)
			{
				AddAutomaticallyButtons(m_screen);
			}

            m_staticScrollRectsVR = new List<ScrollRectVR>();
            Utilities.CalculateScrollRect(this.gameObject, ref m_staticScrollRectsVR);
        }

        // -------------------------------------------
        /* 
		 * Reset the reference with player object
		 */
        public void DestroyMessageOnDistance(GameObject _playerInteracted, float _distanceToDestroy)
		{
			m_playerInteracted = _playerInteracted;
			m_distanceToDestroy = _distanceToDestroy;
		}

		// -------------------------------------------
		/* 
		 * Called on the destroy method of the object
		 */
		void OnDestroy()
		{
            // Debug.LogError("YourVRUI::BaseVRScreenView::OnDestroy::NAME OBJECT DESTROYED[" + this.gameObject.name + "]::+++++++++++BEGIN+++++++++++++");

            if (this.gameObject.GetComponent<IBasicView>() != null)
			{
				this.gameObject.GetComponent<IBasicView>().Destroy();
			}

			m_distance = -1;
			UIEventController.Instance.UIEvent -= OnBaseScreenBasicEvent;

			ClearListSelectors();
			m_selectors = null;

			UIEventController.Instance.DispatchUIEvent(YourVRUIScreenController.EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT, false);
			UIEventController.Instance.DispatchUIEvent(EVENT_SCREEN_CLOSE_VIEW, this.gameObject);
			UIEventController.Instance.DispatchUIEvent(InteractionController.EVENT_INTERACTIONCONTROLLER_SCREEN_DESTROYED, m_characterOrigin);

			m_characterOrigin = null;

			m_playerInteracted = null;
			m_screen = null;

            // Debug.LogError("YourVRUI::BaseVRScreenView::OnDestroy::NAME OBJECT DESTROYED[" + this.gameObject.name + "]::+++++++++++END+++++++++++++");
        }

		// -------------------------------------------
		/* 
		 * It will go recursively through all the childs 
		 * looking for interactable elements to add 
		 * the beahavior of YourVRUI
		 */
		public void AddAutomaticallyButtons(GameObject _go)
		{
			if (((_go.GetComponent<Button>() != null) && (_go.GetComponent<Button>().enabled) && (_go.GetComponent<Button>().interactable)) || 
                (_go.GetComponent<ICustomButton>() != null) || 
                ((_go.GetComponent<InputField>() != null) && (_go.GetComponent<InputField>().enabled) && (_go.GetComponent<InputField>().interactable)))
            {
                AddButtonToList(_go);
			}
			foreach (Transform child in _go.transform)
			{
				AddAutomaticallyButtons(child.gameObject);
			}
		}

		// -------------------------------------------
		/* 
		 * It will add the interactable element to the list
		 */
		private GameObject AddButtonToList(GameObject _button)
		{
            if (!m_selectors.Contains(_button))
            {
                m_selectors.Add(_button);
                if (m_enabledSelector)
                {
                    if (_button != null)
                    {
                        if (_button.GetComponent<ButtonVRView>() == null)
                        {
                            _button.AddComponent<ButtonVRView>();
                        }
                        _button.GetComponent<ButtonVRView>().Initialize(YourVRUIScreenController.Instance.SelectorGraphic, YourVRUIScreenController.UI_TRIGGERER, m_layerScreen, this.gameObject.name);
                    }
                }
            }
            return _button;
		}

        // -------------------------------------------
        /* 
		 * EmptyListSelectors
		 */
        public void EmptyListSelectors()
        {
            if (m_selectors != null) m_selectors.Clear();
        }

        // -------------------------------------------
        /* 
        * It will remove and clean the interactable element and all his references
        */
        private void ClearListSelectors()
		{
			try
			{
				if (m_selectors != null)
				{
					for (int i = 0; i < m_selectors.Count; i++)
					{
						if (m_selectors[i] != null)
						{
							if (m_selectors[i].GetComponent<ButtonVRView>() != null)
							{
								m_selectors[i].GetComponent<ButtonVRView>().Destroy();
							}
						}
					}
					m_selectors.Clear();
				}
			}
			catch (Exception err)
			{
                if (YourVRUIScreenController.Instance.DebugMode)
				{
					Debug.LogError(err.StackTrace);
				}
			};
		}

        // -------------------------------------------
        /* 
		 * UpdateScrollRectItemsVisibility
		 */
        private void UpdateScrollRectItemsVisibility(ScrollRectVR _scrollRect, GameObject _target)
        {
            Transform allChilds = _target.transform.parent;
            int hidingTotalNumber = 0;
            for (int k = 0; k < allChilds.childCount; k++)
            {
                Transform childScroll = allChilds.GetChild(k);
                if (childScroll.gameObject.GetComponent<CanvasGroup>() != null)
                {
                    if (Utilities.IsVisibleFrom(_scrollRect, childScroll.GetComponent<RectTransform>()))
                    {
                        childScroll.gameObject.GetComponent<CanvasGroup>().alpha = 1;
                    }
                    else
                    {
                        childScroll.gameObject.GetComponent<CanvasGroup>().alpha = 0;
                        hidingTotalNumber++;
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * HideSelectorsOnScrollRect
		 */
        private void HideSelectorsOnScrollRect(GameObject _target)
        {
            ButtonVRView[] allChilds = _target.GetComponentsInChildren<ButtonVRView>();
            for (int k = 0; k < allChilds.Length; k++)
            {
                if (allChilds[k] != null)
                {
                    allChilds[k].DisableSelectable();
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Global manager of events
		 */
        private void OnBaseScreenBasicEvent(string _nameEvent, params object[] _list)
        {
            if (this == null) return;
            if (this.gameObject == null) return;
            if (!this.gameObject.activeSelf) return;

            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_CLEAR_LIST_SELECTORS)
            {
                if (this.gameObject == (GameObject)_list[0])
                {
                    ClearListSelectors();
                    if (_list.Length > 1) m_disableActionButtonInteraction = (bool)_list[1];
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_RECALCULATE_LIST_SELECTORS)
            {
                if (this.gameObject == (GameObject)_list[0])
                {
                    ClearListSelectors();
                    AddAutomaticallyButtons(m_screen);
                    if (_list.Length > 1) m_disableActionButtonInteraction = (bool)_list[1];
                }
            }

            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_RELOAD_SCREEN_DATA)
            {
                bool forceReaload = false;
                if (_list.Length > 0)
                {
                    forceReaload = true;
                }
                if (AutomaticallyAddButtons || forceReaload)
                {
                    ClearListSelectors();
                    AddAutomaticallyButtons(m_screen);
                    if (forceReaload)
                    {
                        Utilities.ApplyMaterialOnImages(m_screen, YourVRUIScreenController.Instance.MaterialDrawOnTop);
                    }                    
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_RELOAD_SCREEN_AND_DISABLE)
            {
                ClearListSelectors();
                AddAutomaticallyButtons(m_screen);
                Utilities.ApplyMaterialOnImages(m_screen, YourVRUIScreenController.Instance.MaterialDrawOnTop);
                GameObject targetObject = (GameObject)_list[0];
                if (Utilities.FindGameObjectInChilds(this.gameObject, targetObject))
                {
                    HideSelectorsOnScrollRect(targetObject);
                }
            }
            if (m_disableActionButtonInteraction)
            {
                if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
                {
                    m_disableActionButtonInteraction = false;
                }
            }

            if (m_selectors == null) return;
            if (m_selectors.Count == 0) return;

            if (_nameEvent == EVENT_SCREEN_DESTROYED_VIEW)
            {
                GameObject characterOrigin = (GameObject)_list[0];
                if (m_characterOrigin == characterOrigin)
                {
                    GameObject.Destroy(m_screen);
                    return;
                }
            }

            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_CLEAR_SELECTOR_DATA)
            {
                ClearListSelectors();
            }

            if (!m_disableActionButtonInteraction)
            {
                if (m_highlightSelector)
                {
                    if (_nameEvent == ButtonVRView.EVENT_SELECTED_VR_BUTTON_COMPONENT)
                    {
                        if (!YourVRUIScreenController.Instance.KeysEnabled)
                        {
                            GameObject componentSelected = (GameObject)_list[0];
                            int indexSelected = IsComponentInsideScreen(componentSelected);
                            if (((indexSelected != m_selectionButton) || !m_firstHighlightConsumed) && (indexSelected != -1))
                            {
                                m_firstHighlightConsumed = true;
                                EnableSelectedComponent(componentSelected);
                            }
                        }
                    }
                }
            }

            if (_nameEvent == KeysEventInputController.ACTION_BACK_BUTTON)
            {
                if (YourVRUIScreenController.Instance.KeysEnabled)
                {
                    UIEventController.Instance.DispatchUIEvent(YourVRUIScreenController.EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT, false);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_CANCEL_BUTTON);
                }
            }

            if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
            {
                if (!m_disableActionButtonInteraction)
                {
                    if ((m_selectionButton >= 0) && (m_selectionButton < m_selectors.Count))
                    {
                        if (m_selectors[m_selectionButton] != null)
                        {
                            if (m_selectors[m_selectionButton].GetComponent<ButtonVRView>() != null)
                            {
                                if (m_selectors[m_selectionButton].activeSelf)
                                {
                                    // Debug.LogError("INVOKE BUTTON[" + m_selectors[m_selectionButton].transform.name + "]");
                                    m_selectors[m_selectionButton].GetComponent<ButtonVRView>().InvokeButton();
                                }
                            }
                        }
                    }
                }
            }

            if (_nameEvent == ButtonVRView.EVENT_CHECK_ELEMENT_CORNER_OUT_OF_LIST)
            {
                /*
                if (!m_disableActionButtonInteraction)
                {
                    if (YourVRUIScreenController.Instance.DisplayHighlightedItemInList)
                    {
                        GameObject targetObject = (GameObject)_list[0];
                        Vector3 dataElement = GetGameObjectElementInsideScrollRect(targetObject);
                        if ((dataElement.x != -1) && (dataElement.y != -1) && (dataElement.z != -1))
                        {
                            Utilities.MoveScrollWithSiblings(m_scrollRectsVR[(int)dataElement.y], targetObject);
                            UpdateScrollRectItemsVisibility(m_scrollRectsVR[(int)dataElement.y], targetObject);
                        }
                    }
                }
                */
            }

            if (_nameEvent == KeysEventInputController.JOYSTICK_DOWN_PRESSED)
            {
                m_enableScrollDown = true;
            }
            if (_nameEvent == KeysEventInputController.JOYSTICK_UP_PRESSED)
            {
                m_enableScrollUp = true;
            }
            if (_nameEvent == KeysEventInputController.JOYSTICK_LEFT_PRESSED)
            {
                m_enableScrollLeft = true;
            }
            if (_nameEvent == KeysEventInputController.JOYSTICK_RIGHT_PRESSED)
            {
                m_enableScrollRight = true;
            }
            if ((_nameEvent == KeysEventInputController.JOYSTICK_DOWN_RELEASED)
                || (_nameEvent == KeysEventInputController.JOYSTICK_UP_RELEASED)
                || (_nameEvent == KeysEventInputController.JOYSTICK_RIGHT_RELEASED)
                || (_nameEvent == KeysEventInputController.JOYSTICK_LEFT_RELEASED))
            {
                m_enableScrollDown = false;
                m_enableScrollUp = false;
                m_enableScrollLeft = false;
                m_enableScrollRight = false;
            }
            if (_nameEvent == EVENT_SCREEN_CHECK_ELEMENT_BELONGS_TO_SCROLLRECT)
            {
                GameObject targetObject = (GameObject)_list[0];
                m_initialPositionOnScrollList = -1;
                Vector3 dataElement = GetGameObjectElementInsideScrollRect(targetObject);
                bool elementIsInsideScrollRect = ((dataElement.x != -1) && (dataElement.y != -1) && (dataElement.z != -1));
                bool isVerticalGrid = false;
                if (elementIsInsideScrollRect)
                {
                    isVerticalGrid = m_scrollRectsVR[(int)dataElement.y].IsVerticalGrid();
                }
                UIEventController.Instance.DispatchUIEvent(EVENT_SCREEN_RESPONSE_ELEMENT_BELONGS_TO_SCROLLRECT, targetObject, elementIsInsideScrollRect, isVerticalGrid);
            }

            if (_nameEvent == EVENT_SCREEN_MOVED_SCROLL_RECT)
            {
                GameObject targetObject = (GameObject)_list[0];
                float angleDiferenceFromOrigin = (float)_list[1];
                bool directionToMove = (bool)_list[2];
                Vector3 dataElement = GetGameObjectElementInsideScrollRect(targetObject);
                bool elementIsInsideScrollRect = ((dataElement.x != -1) && (dataElement.y != -1) && (dataElement.z != -1));
                if (elementIsInsideScrollRect)
                {
                    ScrollRectVR scrollRectVR = m_scrollRectsVR[(int)dataElement.y];
                    if (m_initialPositionOnScrollList == -1)
                    {
                        if (scrollRectVR.IsVerticalGrid())
                        {
                            m_initialPositionOnScrollList = scrollRectVR.ScrollRectObject.verticalNormalizedPosition;
                        }
                        else
                        {
                            m_initialPositionOnScrollList = scrollRectVR.ScrollRectObject.horizontalNormalizedPosition;
                        }
                    }
                    float finalNormalizedPosition = 0;
                    float angleBaseMovement = (scrollRectVR.IsVerticalGrid() ? 45 : 90);
                    if (directionToMove)
                    {
                        finalNormalizedPosition = m_initialPositionOnScrollList + (angleDiferenceFromOrigin / angleBaseMovement);
                    }
                    else
                    {
                        finalNormalizedPosition = m_initialPositionOnScrollList - (angleDiferenceFromOrigin / angleBaseMovement);
                    }
                    if (finalNormalizedPosition < 0) finalNormalizedPosition = 0;
                    if (finalNormalizedPosition > 1) finalNormalizedPosition = 1;
                    if (scrollRectVR.IsVerticalGrid())
                    {
                        scrollRectVR.ScrollRectObject.verticalNormalizedPosition = finalNormalizedPosition;
                    }
                    else
                    {
                        scrollRectVR.ScrollRectObject.horizontalNormalizedPosition = finalNormalizedPosition;
                    }
                    UpdateScrollRectItemsVisibility(scrollRectVR, targetObject);
                }
            }

            if (_nameEvent == EVENT_SCREEN_DISABLE_ACTION_BUTTON_INTERACTION)
            {
                m_disableActionButtonInteraction = (bool)_list[0];
                if (m_disableActionButtonInteraction)
                {
                    EnableSelectedComponent(null);
                }
            }

            bool keepSearching = true;

            // KEYS ACTION
            if (_nameEvent == KeysEventInputController.ACTION_KEY_UP_PRESSED)
            {
                UIEventController.Instance.DispatchUIEvent(YourVRUIScreenController.EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT, true);
                do
                {
                    m_selectionButton--;
                    if (m_selectionButton < 0)
                    {
                        m_selectionButton = m_selectors.Count - 1;
                    }
                    if (m_selectors[m_selectionButton] == null)
                    {
                        keepSearching = true;
                    }
                    else
                    {
                        keepSearching = !m_selectors[m_selectionButton].activeSelf;
                    }
                } while (keepSearching);
                EnableSelector();
            }
            if (_nameEvent == KeysEventInputController.ACTION_KEY_DOWN_PRESSED)
            {
                UIEventController.Instance.DispatchUIEvent(YourVRUIScreenController.EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT, true);
                do
                {
                    m_selectionButton++;
                    if (m_selectionButton > m_selectors.Count - 1)
                    {
                        m_selectionButton = 0;
                    }
                    if (m_selectors[m_selectionButton] == null)
                    {
                        keepSearching = true;
                    }
                    else
                    {
                        keepSearching = !m_selectors[m_selectionButton].activeSelf;
                    }
                } while (keepSearching);
                EnableSelector();
            }
            if (_nameEvent == KeysEventInputController.ACTION_KEY_LEFT_PRESSED)
            {
                UIEventController.Instance.DispatchUIEvent(YourVRUIScreenController.EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT, true);
                do
                {
                    m_selectionButton--;
                    if (m_selectionButton < 0)
                    {
                        m_selectionButton = m_selectors.Count - 1;
                    }
                    if (m_selectors[m_selectionButton] == null)
                    {
                        keepSearching = true;
                    }
                    else
                    {
                        keepSearching = !m_selectors[m_selectionButton].activeSelf;
                    }
                } while (keepSearching);
                EnableSelector();
            }
            if (_nameEvent == KeysEventInputController.ACTION_KEY_RIGHT_PRESSED)
            {
                UIEventController.Instance.DispatchUIEvent(YourVRUIScreenController.EVENT_SCREENMANAGER_ENABLE_KEYS_INPUT, true);
                do
                {
                    m_selectionButton++;
                    if (m_selectionButton > m_selectors.Count - 1)
                    {
                        m_selectionButton = 0;
                    }
                    if (m_selectors[m_selectionButton] == null)
                    {
                        keepSearching = true;
                    }
                    else
                    {
                        keepSearching = !m_selectors[m_selectionButton].activeSelf;
                    }
                } while (keepSearching);
                EnableSelector();
            }
            if (_nameEvent == KeysEventInputController.ACTION_RECENTER)
            {
                RunRefocusScreen(true, true);
            }
            if (_nameEvent == EVENT_SCREEN_ENABLE_ALPHA_ZERO_IMAGE)
            {
                m_enableAlphaZero = (bool)_list[0];
                for (int i = 0; i < m_selectors.Count; i++)
                {
                    if (m_selectors[i].GetComponent<Image>() != null)
                    {
                        if (m_selectors[i].GetComponent<Image>().color.a == 0)
                        {
                            m_selectors[i].GetComponent<BoxCollider>().enabled = m_enableAlphaZero;
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
        * Enable the hightlight of the selected component
        */
        private void EnableSelectedComponent(GameObject _componentSelected)
		{
            try
            {
                for (int i = 0; i < m_selectors.Count; i++)
                {
                    if (m_selectors[i] == _componentSelected)
                    {
                        m_selectionButton = i;
                        bool enableDisplaySelector = true;
                        if (m_selectors[i].GetComponent<Image>() != null)
                        {
                            if (m_selectors[i].GetComponent<Image>().color.a == 0)
                            {
                                enableDisplaySelector = false;
                            }
                        }
                        m_selectors[i].GetComponent<ButtonVRView>().EnableSelector(enableDisplaySelector);
                    }
                    else
                    {
                        m_selectors[i].GetComponent<ButtonVRView>().EnableSelector(false);
                    }
                }
            }
            catch (Exception err) {
            };
        }

		// -------------------------------------------
		/* 
		 * Check if the GameObject is inside the scrollrect
		 */
		private Vector3 GetGameObjectElementInsideScrollRect(GameObject _element)
		{
			if (m_scrollRectsVR == null)
			{
				Utilities.CalculateScrollRect(this.gameObject, ref m_scrollRectsVR);
			}

			Vector3 output = new Vector3(-1, -1, -1);
			int indexSelector = IsComponentInsideScreen(_element);
			if (indexSelector != -1)
			{
				output = new Vector3(indexSelector, -1, -1);
				if (m_scrollRectsVR != null)
				{
					for (int i = 0; i < m_scrollRectsVR.Count; i++)
					{
						if (Utilities.FindGameObjectInChilds(m_scrollRectsVR[i].BaseObject, _element))
						{
							output = new Vector3(indexSelector, i, _element.transform.GetSiblingIndex());
						}
					}
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * Enables the selectors to show with what elements
		 * the user is interacting with
		 */
		private void EnableSelector()
		{
			if (m_selectors == null) return;
			if (m_selectors.Count == 0) return;
			if (!((m_selectionButton >= 0) && (m_selectionButton < m_selectors.Count))) return;

			for (int i = 0; i < m_selectors.Count; i++)
			{
				if (m_selectors[i] != null)
				{
					if (m_selectors[i].transform != null)
					{
						if (m_selectors[i].GetComponent<ButtonVRView>() != null)
						{
							if (m_selectionButton == i)
							{
								m_selectors[i].GetComponent<ButtonVRView>().EnableSelector(true);
							}
							else
							{
								m_selectors[i].GetComponent<ButtonVRView>().EnableSelector(false);
							}
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Disable the selectors
		 */
		private void DisableSelectors()
		{
			if (m_selectors == null) return;
			if (m_selectors.Count == 0) return;
			if (!((m_selectionButton >= 0) && (m_selectionButton < m_selectors.Count))) return;

			for (int i = 0; i < m_selectors.Count; i++)
			{
				if (m_selectors[i].GetComponent<ButtonVRView>() != null)
				{
					m_selectors[i].GetComponent<ButtonVRView>().EnableSelector(false);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * SetAlpha
		 */
		private void SetAlpha(float _newAlpha)
		{
			if (m_canvasGroup != null)
			{
				if (m_canvasGroup.alpha != 1)
				{
					m_canvasGroup.alpha = _newAlpha;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Set a link with the player
		 */
		public void SetLinkToPlayer(Vector3 _normal, float _distance, bool _refocus)
		{
			m_normal = _normal;
			m_distance = _distance;
			m_refocus = _refocus;
			m_timeToRefocus = DELAY_TO_REFOCUS;
            RunRefocusScreen(false);
            Invoke("ResetScreenPosition", 0.1f);
        }

        // -------------------------------------------
        /* 
		 * ResetScreenPosition
		 */
        public void ResetScreenPosition()
        {
            RunRefocusScreen(false);
        }

        // -------------------------------------------
        /* 
        * RunRefocusScreen
        */
        public void RunRefocusScreen(bool _enableAnimation, bool _force = false)
        {
            if (m_refocus || _force)
            {
                if (YourVRUIScreenController.Instance == null) return;
                if (YourVRUIScreenController.Instance.GameCamera == null) return;
                if (m_screen == null) return;
                if (this == null) return;
                if (this.gameObject == null) return;

                m_normal = Utilities.ClonePoint(YourVRUIScreenController.Instance.GameCamera.transform.forward.normalized);
                Vector3 targetPosition = YourVRUIScreenController.Instance.GameCamera.transform.position + (m_normal * m_distance);
                this.gameObject.transform.forward = Utilities.ClonePoint(YourVRUIScreenController.Instance.GameCamera.transform.forward);
                if (_enableAnimation)
                {
                    InterpolatorController.Instance.Interpolate(this.gameObject, targetPosition, 0.95f * DELAY_TO_REFOCUS);
                }
                else
                {
                    this.gameObject.transform.position = targetPosition;
                }
                m_timeToRefocus = 0;
            }
        }

        // -------------------------------------------
        /* 
		 * Calculate the logic of realigning the screen when it's not visible
		 */
        private void RefocusScreen()
		{
			if (m_distance != -1)
			{
				if (m_refocus)
				{
					if (m_timeToRefocus > DELAY_TO_REFOCUS)
					{
						Bounds canvasBounds = new Bounds(this.gameObject.transform.position, Vector3.one);
						if (!Utilities.IsVisibleFrom(canvasBounds, YourVRUIScreenController.Instance.GameCamera))
						{
                            RunRefocusScreen(true);
                        }
					}

                    m_timeToRefocus += Time.deltaTime;
                    if (m_timeToRefocus > DELAY_TO_REFOCUS)
                    {
                        this.gameObject.transform.position = YourVRUIScreenController.Instance.GameCamera.transform.transform.position + (m_normal * m_distance);
                    }
                }
            }
		}

		// -------------------------------------------
		/* 
		 * Will check if the screen should be destroyed by the distance with the player
		 */
		private void DestroyOnDistanceScreen()
		{
			if (m_destroyMessageOnDistance && (m_distanceToDestroy > 0) && (m_playerInteracted != null) && (m_characterOrigin != null))
			{
				if (Vector3.Distance(m_playerInteracted.transform.position, m_characterOrigin.transform.position) > m_distanceToDestroy)
				{
					GameObject.Destroy(m_screen);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Check if the component is in the list of selectors
		 */
		private int IsComponentInsideScreen(GameObject _component)
		{
			for (int i = 0; i < m_selectors.Count; i++)
			{
				if (m_selectors[i] == _component)
				{
					return i;
				}
			}
			return -1;
		}

        // -------------------------------------------
        /* 
		 * SetLayer
		 */
        public void SetLayer(int _layer)
        {
            m_layerScreen = _layer;
        }

        // -------------------------------------------
        /* 
		 * DotProjectedUIUpdate
		 */
        private void DotProjectedUIUpdate()
        {
            if (YourVRUIScreenController.Instance.EnableProjectionDot && (YourVRUIScreenController.Instance.DotProjectionUI != null))
            {
                if (m_canvasGroup != null)
                {
                    if (m_dotProjectedUI == null)
                    {
                        m_dotProjectedUI = GameObject.Instantiate(YourVRUIScreenController.Instance.DotProjectionUI, m_canvasGroup.transform);
                    }
                    RaycastHit hitSurface;
                    if (!YourVRUIScreenController.Instance.IsDayDreamActivated)
                    {
                        hitSurface = Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.GameCamera.transform.position, YourVRUIScreenController.Instance.GameCamera.transform.forward, "UI");
                    }
                    else
                    {
                        hitSurface = Utilities.GetRaycastHitInfoByRayWithMask(YourVRUIScreenController.Instance.LaserPointer.transform.position, YourVRUIScreenController.Instance.LaserPointer.transform.forward, "UI");
                    }

                    if (hitSurface.collider != null)
                    {
                        if (!m_dotProjectedUI.activeSelf)
                        {
                            m_dotProjectedUI.SetActive(true);
                        }
                        m_dotProjectedUI.transform.position = hitSurface.point;
                    }
                }
            }
            else
            {
                if (m_dotProjectedUI != null)
                {
                    m_dotProjectedUI.SetActive(false);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * AlphaProgressLogic
		 */
        private void AlphaProgressLogic()
        {
            if (m_canvasGroup != null)
            {
                if (m_canvasGroup.alpha != YourVRUIScreenController.Instance.DefaultFinalAlpha)
                {
                    m_timeToAlpha += Time.deltaTime;
                    float progressAlpha = m_timeToAlpha / YourVRUIScreenController.Instance.DefaultTimeoutToAlpha;
                    m_canvasGroup.alpha = (YourVRUIScreenController.Instance.DefaultFinalAlpha - YourVRUIScreenController.Instance.DefaultInitialAlpha) * progressAlpha;
                    if (m_timeToAlpha > YourVRUIScreenController.Instance.DefaultTimeoutToAlpha)
                    {
                        m_canvasGroup.alpha = YourVRUIScreenController.Instance.DefaultFinalAlpha;
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * ScrollRectVRWithJoystick
		 */
        private void ScrollRectVRWithJoystick()
        {
            if (m_staticScrollRectsVR != null)
            {
                foreach (ScrollRectVR item in m_staticScrollRectsVR)
                {
                    if (item.IsVerticalGrid())
                    {
                        if (m_enableScrollDown)
                        {
                            item.ScrollRectObject.verticalNormalizedPosition += Time.deltaTime * SCROLL_RECT_VR_SPEED_WITH_JOYSTICK;
                        }
                        if (m_enableScrollUp)
                        {
                            item.ScrollRectObject.verticalNormalizedPosition -= Time.deltaTime * SCROLL_RECT_VR_SPEED_WITH_JOYSTICK;
                        }
                    }
                    else
                    {
                        if (m_enableScrollRight)
                        {
                            item.ScrollRectObject.horizontalNormalizedPosition += Time.deltaTime * SCROLL_RECT_VR_SPEED_WITH_JOYSTICK;
                        }
                        if (m_enableScrollLeft)
                        {
                            item.ScrollRectObject.horizontalNormalizedPosition -= Time.deltaTime * SCROLL_RECT_VR_SPEED_WITH_JOYSTICK;
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Runs the logic for realigning and destroying on distance
		 */
        void Update()
        {
            if (YourVRUIScreenController.Instance == null) return;

            RefocusScreen();

            DestroyOnDistanceScreen();

            DotProjectedUIUpdate();

            AlphaProgressLogic();

            ScrollRectVRWithJoystick();
        }

    }
}