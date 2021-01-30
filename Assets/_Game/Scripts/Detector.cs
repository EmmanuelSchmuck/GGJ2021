using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
	public PlayerController player;
	private void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log($"Dog enter trigger: {other.transform.parent.gameObject.name}");
		player.OnProximityEnter(other.transform.parent.gameObject);	
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		Debug.Log($"Dog exit trigger: {other.transform.parent.gameObject.name}");
		player.OnProximityLeave(other.transform.parent.gameObject);	
	}
}
