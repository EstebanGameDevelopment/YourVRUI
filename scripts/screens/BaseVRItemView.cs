using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourVRUI
{
	public class BaseVRItemView : MonoBehaviour
	{
		void Start()
		{
			if (transform.GetComponent<Button>() != null)
			{
				this.gameObject.AddComponent<ButtonVRView>();
				this.gameObject.GetComponent<ButtonVRView>().Initialize(YourVRUIScreenController.Instance.SelectorGraphic, YourVRUIScreenController.UI_TRIGGERER);
			}
		}
	}
}