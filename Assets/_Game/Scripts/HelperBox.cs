using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HelperBox : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> textBoxes;
	public static HelperBox Instance;
	private void Awake()
	{
		Instance = this;
	}
	public void Hide()
	{
		foreach(GameObject textbox in textBoxes)
		{
			textbox.SetActive(false);
		}
	}
    public void DisplayText(HelperBoxText text)
	{
		if(textBoxes.FirstOrDefault(x => x.name == text.ToString()) == null)
		{
			Debug.LogError("No text box of this name");
		}
		foreach(GameObject textbox in textBoxes)
		{
			textbox.SetActive(textbox.name == text.ToString());
		}
	}
}

public enum HelperBoxText
{
	PlanetMovement,
	CatchItem,
	GiveItem,
	JumpToSpace,
	SpaceMovement
}
