using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelperBox : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> textBoxes;
	public static HelperBox Instance;
	private void Awake()
	{
		Instance = this;
	}
    public void DisplayText(HelperBoxText text)
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
