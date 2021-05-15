using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourVRUI
{
    /******************************************
	 * 
	 * ScreenOculusControlSelectionView
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenOculusControlSelectionView : ScreenBaseView, IBasicView
    {
        public const string SCREEN_NAME = "SCREEN_OCULUS_CONTROL";

        public const string OCULUSCONTROL_MODE = "OCULUSCONTROL_MODE";

        // -------------------------------------------
        /* 
		 * SaveOculusControlMode
		 */
        public static void SaveOculusControlMode(bool _isHandMode)
        {
            PlayerPrefs.SetInt(OCULUSCONTROL_MODE, (_isHandMode?1:0));
        }
        
        private static int IsControlByHandsDesired = -1;
        public static bool ShouldCheckTheHandControl = false;

        // -------------------------------------------
        /* 
		 * ControOculusWithHands
		 */
        public static bool ControOculusWithHands()
        {
            if (!ShouldCheckTheHandControl)
            {
                return false;
            }

            if (IsControlByHandsDesired == -1)
            {
                IsControlByHandsDesired = PlayerPrefs.GetInt(OCULUSCONTROL_MODE, 0);
            }
#if FORCE_OCULUS_HANDS
            IsControlByHandsDesired = 1;
#endif
            return (IsControlByHandsDesired == 1);
        }

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
        private Transform m_container;

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
        {
            base.Initialize(_list);

            m_root = this.gameObject;
            m_container = m_root.transform.Find("Content");

            m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.oculus.control.selection");

            GameObject optionOne = m_container.Find("Button_Hands").gameObject;
            optionOne.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.oculus.control.hands");
            optionOne.GetComponent<Button>().onClick.AddListener(ControlWithHands);

            GameObject optionTwo = m_container.Find("Button_Controller").gameObject;
            optionTwo.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.oculus.control.controller");
            optionTwo.GetComponent<Button>().onClick.AddListener(ControlWithController);

            UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);
        }

        // -------------------------------------------
        /* 
		 * GetGameObject
		 */
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public override bool Destroy()
        {
            if (base.Destroy()) return true;

            UIEventController.Instance.UIEvent -= OnMenuEvent;
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject, SCREEN_NAME);

            return false;
        }

        // -------------------------------------------
        /* 
		 * ControlWithHands
		 */
        private void ControlWithHands()
        {
            SaveOculusControlMode(true);
            Invoke("DestroyWithDelay", 0.2f);
        }

        // -------------------------------------------
        /* 
		 * ControlWithController
		 */
        private void ControlWithController()
        {
            SaveOculusControlMode(false);
            Invoke("DestroyWithDelay", 0.2f);
        }

        // -------------------------------------------
        /* 
		 * DestroyWithDelay
		 */
        public void DestroyWithDelay()
        {
            Destroy();
        }
    }
}
