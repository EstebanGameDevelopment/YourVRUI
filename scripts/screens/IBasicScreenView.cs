using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourVRUI
{
	/******************************************
	 * 
	 * IMenus
	 * 
	 * Basic interface that force the programmer to 
	 * initialize and free resources to avoid memory leaks
	 * 
	 * @author Esteban Gallardo
	 */
	public interface IBasicScreenView
	{
		// FUNCTIONS
		void Initialize(params object[] _list);
		void Destroy();
	}
}