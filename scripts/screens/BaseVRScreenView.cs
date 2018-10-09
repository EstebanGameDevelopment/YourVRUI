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

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const float FADE_TIME = 1f;
		public const float DELAY_TO_REFOCUS = 0.2f;

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

		private float m_initialPositionOnScrollList = -1;

		private bool m_disableActionButtonInteraction = false;

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
				m_canvasGroup = m_screen.transform.Find(CONTENT_COMPONENT_NAME).GetComponent<CanvasGroup>();
				if (m_canvasGroup != null)
				{
					m_canvasGroup.alpha = 1;
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
			if (this.gameObject.GetComponent<IBasicView>() != null)
			{
				this.gameObject.GetComponent<IBasicView>().Destroy();
			}

			Debug.Log("YourVRUI::BaseVRScreenView::OnDestroy::NAME OBJECT DESTROYED[" + this.gameObject.name + "]");

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
		}

		// -------------------------------------------
		/* 
		 * It will go recursively through all the childs 
		 * looking for interactable elements to add 
		 * the beahavior of YourVRUI
		 */
		private void AddAutomaticallyButtons(GameObject _go)
		{
			if (_go.GetComponent<Button>() != null)
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
			m_selectors.Add(_button);
			if (m_enabledSelector)
			{
				if (_button != null)
				{
                    _button.AddComponent<ButtonVRView>();
					_button.GetComponent<ButtonVRView>().Initialize(YourVRUIScreenController.Instance.SelectorGraphic, YourVRUIScreenController.UI_TRIGGERER);
				}
			}
			return _button;
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
		 * Global manager of events
		 */
		private void OnBaseScreenBasicEvent(string _nameEvent, params object[] _list)
		{
            if (this == null) return;
            if (this.gameObject == null) return;
			if (!this.gameObject.activeSelf) return;
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

            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_RELOAD_SCREEN_DATA)
            {
                if (AutomaticallyAddButtons)
                {
                    ClearListSelectors();
                    AddAutomaticallyButtons(m_screen);
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
									m_selectors[m_selectionButton].GetComponent<ButtonVRView>().InvokeButton();
								}
							}
						}
					}
				}
			}

			if (_nameEvent == ButtonVRView.EVENT_CHECK_ELEMENT_CORNER_OUT_OF_LIST)
			{
				if (!m_disableActionButtonInteraction)
				{
					GameObject targetObject = (GameObject)_list[0];
					Vector3 dataElement = GetGameObjectElementInsideScrollRect(targetObject);
					if ((dataElement.x != -1) && (dataElement.y != -1) && (dataElement.z != -1))
					{
						Utilities.MoveScrollWithSiblings(m_scrollRectsVR[(int)dataElement.y], targetObject);
					}
				}
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
		}

		// -------------------------------------------
		/* 
		 * Enable the hightlight of the selected component
		 */
		private void EnableSelectedComponent(GameObject _componentSelected)
		{
			for (int i = 0; i < m_selectors.Count; i++)
			{
				if (m_selectors[i] == _componentSelected)
				{
					m_selectionButton = i;
					m_selectors[i].GetComponent<ButtonVRView>().EnableSelector(true);
				}
				else
				{
					m_selectors[i].GetComponent<ButtonVRView>().EnableSelector(false);
				}
			}
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
        private void RunRefocusScreen(bool _enableAnimation)
        {
            if (m_refocus)
            {
                if (YourVRUIScreenController.Instance == null) return;
                if (YourVRUIScreenController.Instance.GameCamera == null) return;

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
				}
				m_timeToRefocus += Time.deltaTime;
				if (m_timeToRefocus > DELAY_TO_REFOCUS)
				{
					this.gameObject.transform.position = YourVRUIScreenController.Instance.GameCamera.transform.transform.position + (m_normal * m_distance);
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
		 * Runs the logic for realigning and destroying on distance
		 */
		void Update()
		{
            if (YourVRUIScreenController.Instance == null) return;

            RefocusScreen();

			DestroyOnDistanceScreen();
		}
	}
}