using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brice_WoofTest : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<PlayerController>().ActionKeyDown += OnPlayerActionKeyDown;
	}

	public void OnPlayerActionKeyDown(PlayerController player)
	{
		GetComponent<Speaker>().Speak("woof");
	}
}
