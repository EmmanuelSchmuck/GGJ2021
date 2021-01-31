using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
	public bool respawn;
	public float respawnDelay;
	public float extraFuelMin, extraFuelMax;
	public float speedBoostIntensity = 5;
	public float speedBoostDuration = 5;

	private bool isAlive;

	public float rotationSpeed;

	private void Update()
	{
		if(!isAlive) return;
		transform.RotateAround(transform.position, Vector3.back, rotationSpeed * Time.deltaTime);
	}

	private IEnumerator Respawn()
	{
		yield return new WaitForSeconds(respawnDelay);
		GetComponent<CircleCollider2D>().enabled = true;
		GetComponent<SpriteRenderer>().enabled = true;
		isAlive = true;
	}

    public void OnBeingCollected()
	{
		GetComponent<CircleCollider2D>().enabled = false;
		GetComponent<SpriteRenderer>().enabled = false;
		isAlive = false;
		StopAllCoroutines();
		if(respawn) StartCoroutine(Respawn());
	}
}
