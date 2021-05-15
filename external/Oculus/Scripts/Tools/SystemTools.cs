using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourVRUI
{
    // ----------------------------------------------
    // SYSTEM ENUMS
    // ----------------------------------------------	
    public enum HAND { right = 0, left = 1, both = 2, none = 3 }

    /******************************************
	* 
	* SystemTools
	* 
	* Static function of system tools
	* 
	* @author Esteban Gallardo
	*/
    public class SystemTools
    {

        // ---------------------------------------------------
        /**
		 @brief We get the whole RaycastHit information of the collision, with the mask to consider
		 */
        public static RaycastHit GetRaycastHitInfoByRayWithMask(Vector3 _origin, Vector3 _forward, params string[] _masksToConsider)
        {
            Vector3 fwd = _forward;
            fwd.Normalize();
            RaycastHit hitCollision = new RaycastHit();

            int layerMask = 0;
            if (_masksToConsider != null)
            {
                for (int i = 0; i < _masksToConsider.Length; i++)
                {
                    layerMask |= (1 << LayerMask.NameToLayer(_masksToConsider[i]));
                }
            }
            if (layerMask == 0)
            {
                Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity);
            }
            else
            {
                Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity, layerMask);
            }
            return hitCollision;
        }

        // ---------------------------------------------------
        /**
		 @brief We get the whole RaycastHit information of the collision, with the mask to consider
		 */
        public static RaycastHit GetRaycastHitInfoByRayWithIgnoreMask(Vector3 _origin, Vector3 _forward, params string[] _masksToIgnore)
        {
            Vector3 fwd = _forward;
            fwd.Normalize();
            RaycastHit hitCollision = new RaycastHit();

            int layerMask = 0;
            if (_masksToIgnore != null)
            {
                for (int i = 0; i < _masksToIgnore.Length; i++)
                {
                    layerMask |= (1 << LayerMask.NameToLayer(_masksToIgnore[i]));
                }
                layerMask = ~layerMask;
            }
            if (layerMask == 0)
            {
                Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity);
            }
            else
            {
                Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity, layerMask);
            }
            return hitCollision;
        }


        // -------------------------------------------
        /* 
		 * Adds a child to the parent
		 */
        public static GameObject AddChild(Transform _parent, GameObject _prefab)
        {
            GameObject newObj = GameObject.Instantiate(_prefab);
            newObj.transform.SetParent(_parent, false);
            return newObj;
        }

        // -------------------------------------------
        /* 
		 * Clone
		 */
        public static Rect Clone(Rect _rect)
        {
            Rect output = new Rect();
            output.x = _rect.x;
            output.y = _rect.y;
            output.width = _rect.width;
            output.height = _rect.height;
            return output;
        }


        // -------------------------------------------
        /* 
		 * Will look fot the gameobject in the childs
		 */
        public static bool FindGameObjectInChilds(GameObject _go, GameObject _target)
        {
            if (_go == _target)
            {
                return true;
            }
            bool output = false;
            foreach (Transform child in _go.transform)
            {
                output = output || FindGameObjectInChilds(child.gameObject, _target);
            }
            return output;
        }


        // -------------------------------------------
        /* 
		 * Transponds the 3D coordinates into a 2D plane
		 * https://stackoverflow.com/questions/16699259/retrieve-2d-co-ordinate-from-a-3d-point-on-a-3d-plane
		 */
        public static Vector3 Get2DCoord(Vector3 _target, Vector3 _a, Vector3 _b, Vector3 _c)
        {
            // ab = b - a
            Vector3 ab = _b - _a;
            // ac = c - a
            Vector3 ac = _c - _a;

            // ab = ab / |ab|
            ab.Normalize();
            // ac = ac / |ac|
            ac.Normalize();

            // ap = p - a                      
            Vector3 ap = _target - _a;

            Vector3 position2D = new Vector2(Vector3.Dot(ab, ap), Vector3.Dot(ac, ap));

            // x = ab x ap
            // y = ac x ap
            return position2D;
        }

        // ---------------------------------------------------
        /**
		 * Converst from string to Vector3
		 */
        public static Vector3 StringToVector3(string _data)
        {
            string[] values = _data.Split(',');
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }

        // ---------------------------------------------------
        /**
		 * Converst from string to Vector3
		 */
        public static string Vector3ToString(Vector3 _data)
        {
            return _data.x + "," + _data.y + "," + _data.z;
        }

        // ---------------------------------------------------
        /**
		 * SearchIndexOf
		 */
        public static int SearchIndexOf(int[] _values, int _item)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                if (_values[i] == _item)
                {
                    return i;
                }
            }
            return -1;
        }

        // -------------------------------------------
        /* 
		 * Will generate a random string
		 */
        public static string RandomCodeGeneration(string _idUser)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new System.Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            string finalString = new String(stringChars) + "_" + _idUser;
            return finalString;
        }


        // -------------------------------------------
        /* 
		 * Find out if the object is a number
		 */
        public static bool IsNumber(object _value)
        {
            return (_value is int) || (_value is float) || (_value is double);
        }


        // -------------------------------------------
        /* 
		 * Get the number as an float
		 */
        public static double GetDouble(object _value)
        {
            if (_value is int)
            {
                return (int)_value;
            }
            if (_value is float)
            {
                return (float)_value;
            }
            if (_value is double)
            {
                return (double)_value;
            }
            return -1;
        }

        // -------------------------------------------
        /* 
		 * DebugLogRed
		 */
        public static void DebugLogRed(string _message)
        {
            Debug.Log("<color=red>" + _message + "</color>");
        }

        // -------------------------------------------
        /* 
		 * DebugLogGreen
		 */
        public static void DebugLogGreen(string _message)
        {
            Debug.Log("<color=green>" + _message + "</color>");
        }

        // -------------------------------------------
        /* 
		 * DebugLogBlue
		 */
        public static void DebugLogBlue(string _message)
        {
            Debug.Log("<color=blue>" + _message + "</color>");
        }


        // -------------------------------------------
        /* 
		 * DebugLogYellow
		 */
        public static void DebugLogYellow(string _message)
        {
            Debug.Log("<color=yellow>" + _message + "</color>");
        }

        // -------------------------------------------
        /* 
        * GetFormattedTimeSeconds
        */
        public static string GetFormattedTimeMinutes(long _timestamp)
        {
            int totalSeconds = (int)_timestamp;
            int totalMinutes = (int)Math.Floor((double)(totalSeconds / 60));
            int totalHours = (int)Math.Floor((double)(totalMinutes / 60));
            int restSeconds = (int)(totalSeconds - (totalMinutes * 60));
            int restMinutes = (int)(totalMinutes - (totalHours * 60));

            // SECONDS
            String seconds;
            if (restSeconds < 10)
            {
                seconds = "0" + restSeconds;
            }
            else
            {
                seconds = "" + restSeconds;
            }

            // MINUTES
            String minutes;
            if (restMinutes < 10)
            {
                minutes = "0" + restMinutes;
            }
            else
            {
                minutes = "" + restMinutes;
            }

            return (minutes + ":" + seconds);
        }
    }
}