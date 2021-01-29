using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
	public float extraFuelMin, extraFuelMax;
	public float speedBoostIntensity = 5;
	public float speedBoostDuration = 5;

	public float rotationSpeed;

	private void Update()
	{
		transform.RotateAround(transform.position, Vector3.back, rotationSpeed * Time.deltaTime);
	}

    public void OnBeingCollected()
	{
		Destroy(gameObject);
	}
}
