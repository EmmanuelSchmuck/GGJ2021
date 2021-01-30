using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperBox : MonoBehaviour
{
    public static void DisplayText(HelperBoxText text)
	{
		Debug.Log($"Displaying helper box text: {text}");
	}
}

public enum HelperBoxText
{
	None,
	PlanetMovement,
	CatchItem,
	GiveItem,
	JumpToSpace,
	SpaceMovement
}
