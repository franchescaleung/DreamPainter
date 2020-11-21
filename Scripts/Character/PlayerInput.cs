using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Dreamwork{
public class PlayerInput : MonoBehaviour
{
	public static

	Dictionary<string, KeyCode> keyMapping = new Dictionary<string, KeyCode>
	{
		{ "Left", KeyCode.A },
		{ "Right", KeyCode.D },
		{ "Jump1", KeyCode.W },
		{ "Jump2", KeyCode.Space },
		{ "Crouch", KeyCode.S },
		{ "Run", KeyCode.LeftShift },
		{ "Escape", KeyCode.Escape },
		{ "RotateItem", KeyCode.Q },
		{ "UseItem", KeyCode.E },
		{ "ToggleSketchpad", KeyCode.Tab}
	};


    
    public static float horizontal
	{
		get
		{
			float horizontal = 0;
			horizontal += Input.GetKey(keyMapping["Left"]) ? -1 : 0;
			horizontal += Input.GetKey(keyMapping["Right"]) ? 1 : 0;
			return horizontal;
		}
	}
	public static bool jump
	{
		get{return Input.GetKey(keyMapping["Jump1"]) || Input.GetKey(keyMapping["Jump2"]);}
	}

	public static bool crouch
	{
		get{return Input.GetKey(keyMapping["Crouch"]);}
	}

	public static bool run
	{
		get{return Input.GetKey(keyMapping["Run"]);}
	}

	public static bool rotate_item
	{
		get{return Input.GetKeyDown(keyMapping["RotateItem"]);}
	}

	public static bool use_item
	{
		get{return Input.GetKeyDown(keyMapping["UseItem"]);}
	}

	public static bool up
	{
		get{return Input.GetKey(keyMapping["Jump1"]);}
	}

	public static bool sketchpad
	{
		get{return Input.GetKeyDown(keyMapping["ToggleSketchpad"]);}
	}


}
}
