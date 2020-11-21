using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Dreamwork
{
    public class InputManager : MonoBehaviour
    {
        static Dictionary<string, KeyCode> keyMapping;
        #region Initialization
        static string[] keyMaps = new string[7]
        {
        "Left",
        "Right",
        "Jump1",
        "Jump2",
        "Crouch",
        "Speedup",
        "Escape"
        };
        static KeyCode[] defaults = new KeyCode[7]
        {
        KeyCode.A,
        KeyCode.D,
        KeyCode.W,
        KeyCode.Space,
        KeyCode.S,
        KeyCode.LeftShift,
        KeyCode.Escape
        };

        private static void InitializeDictionary()
        {
            keyMapping = new Dictionary<string, KeyCode>();
            for (int i = 0; i < keyMaps.Length; ++i)
            {
                keyMapping.Add(keyMaps[i], defaults[i]);
            }
        }

        public static void SetKeyMap(string keyMap, KeyCode key)
        {
            if (!keyMapping.ContainsKey(keyMap))
                throw new ArgumentException("Invalid KeyMap in SetKeyMap: " + keyMap);
            keyMapping[keyMap] = key;
        }

        static InputManager()
        {
            InitializeDictionary();
        }
        #endregion
        #region Get Support Functions
        public static bool GetKeyDown(string keyMap)
        {
            return Input.GetKeyDown(keyMapping[keyMap]);
        }

        public static bool GetKeyUp(string keyMap)
        {
            return Input.GetKeyUp(keyMapping[keyMap]);
        }

        public static bool GetKey(string keyMap)
        {
            return Input.GetKey(keyMapping[keyMap]);
        }

        public static float GetAxis(string whichaxis)
        {
            if (whichaxis == "Horizontal")
            {
                if (Input.GetKey(keyMapping["Left"]) && !Input.GetKey(keyMapping["Right"]))
                {
                    return -1;
                }
                else if (Input.GetKey(keyMapping["Right"]) && !Input.GetKey(keyMapping["Left"]))
                {
                    return 1;
                }
                else
                    return 0;
            }
            return 0;
        }
        #endregion
        #region Return keybind function
        public static float Horizontal
        {
            get
            {
                return GetAxis("Horizontal");
            }
        }

        public static bool Pause
        {
            get
            {
                    return GetKeyDown("Escape");
            }
        }
        #endregion
    }
}