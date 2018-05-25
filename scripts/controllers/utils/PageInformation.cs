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
	 * PageInformation
	 * 
	 * It is used only for the example code, it's not necessary for your project
	 * 
	 * @author Esteban Gallardo
	 */
	[System.Serializable]
	public class PageInformation
	{
		public string MyTitle;
		public string MyText;
		public Sprite MySprite;
		public string EventData;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public PageInformation(string _title, string _text, Sprite _sprite, string _eventData)
		{
			MyTitle = _title;
			MyText = _text;
			MySprite = _sprite;
			EventData = _eventData;
		}

		// -------------------------------------------
		/* 
		 * Clone
		 */
		public PageInformation Clone()
		{
			return new PageInformation(MyTitle, MyText, MySprite, EventData);
		}
	}
}