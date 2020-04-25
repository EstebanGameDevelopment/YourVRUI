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
                if (this.gameObject.GetComponentInParent<BaseVRScreenView>() != null)
                {
                    BaseVRScreenView parentScreen = this.gameObject.GetComponentInParent<BaseVRScreenView>();
                    this.gameObject.AddComponent<ButtonVRView>();
                    this.gameObject.GetComponent<ButtonVRView>().Initialize(YourVRUIScreenController.Instance.SelectorGraphic, YourVRUIScreenController.UI_TRIGGERER, parentScreen.LayerScreen, parentScreen.gameObject.name);
                }
            }
		}
	}
}