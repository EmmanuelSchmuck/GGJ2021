using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brice_WoofTest : MonoBehaviour
{
    public void OnPlayerActionKeyDown()
	{
		GetComponent<Speaker>().Speak("woof");
	}
}
