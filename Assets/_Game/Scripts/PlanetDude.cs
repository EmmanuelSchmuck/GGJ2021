using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetDude : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
	{
		Item item = other.GetComponent<Item>();
		if (item != null)
		{
			Debug.Log($"The item {item.name} is close to me, yay !!");
		}
	}
}
