using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourVRUI
{

	/******************************************
	 * 
	 * ScrollRectVR
	 * 
	 * Class used to keep the needed information of the ScrollRect
	 * in order for the user to be able to scroll with the gaze/daydream controller
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScrollRectVR
	{
		private GameObject m_baseObject;
		private ScrollRect m_scrollRectObject;
		private RectTransform m_rectTransformObject;
		private HorizontalLayoutGroup m_horizontalGrid;
		private VerticalLayoutGroup m_verticalGrid;

		private float m_initialPositionHorizontalGrid;
		private float m_initialPositionVerticalGrid;

		private int m_visibleItemsPerPage = 0;

		private int m_lastSibling = 0;
		private int m_currentSibling = 0;

		public GameObject BaseObject
		{
			get { return m_baseObject; }
		}
		public ScrollRect ScrollRectObject
		{
			get { return m_scrollRectObject; }
		}
		public RectTransform RectTransformObject
		{
			get { return m_rectTransformObject; }
		}
		public HorizontalLayoutGroup HorizontalGrid
		{
			get { return m_horizontalGrid; }
		}
		public VerticalLayoutGroup VerticalGrid
		{
			get { return m_verticalGrid; }
		}
		public float InitialPositionHorizontalGrid
		{
			get { return m_initialPositionHorizontalGrid; }
		}
		public float InitialPositionVerticalGrid
		{
			get { return m_initialPositionVerticalGrid; }
		}
		public int VisibleItemsPerPage
		{
			get { return m_visibleItemsPerPage; }
			set { m_visibleItemsPerPage = 0; }
		}
		public int LastSibling
		{
			get { return m_lastSibling; }
			set { m_lastSibling = value; }
		}
		public int CurrentSibling
		{
			get { return m_currentSibling; }
			set { m_currentSibling = value; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public ScrollRectVR(GameObject _gameObject)
		{
			m_baseObject = _gameObject;
			m_scrollRectObject = m_baseObject.GetComponent<ScrollRect>();
			m_rectTransformObject = m_baseObject.GetComponent<RectTransform>();
			m_horizontalGrid = _gameObject.GetComponentInChildren<HorizontalLayoutGroup>();
			m_verticalGrid = _gameObject.GetComponentInChildren<VerticalLayoutGroup>();

			if (m_horizontalGrid)
			{
				m_initialPositionHorizontalGrid = m_horizontalGrid.gameObject.GetComponent<RectTransform>().localPosition.x;
			}
			if (m_verticalGrid)
			{
				m_initialPositionVerticalGrid = m_verticalGrid.gameObject.GetComponent<RectTransform>().localPosition.y;
			}
		}

		// -------------------------------------------
		/* 
		 * Reports if the scrollRect is vertical
		 */
		public bool IsVerticalGrid()
		{
			return m_verticalGrid != null;
		}

		// -------------------------------------------
		/* 
		 * Reports if the scrollRect is horizontal
		 */
		public bool IsHorizontalGrid()
		{
			return m_horizontalGrid != null;
		}

	}
}