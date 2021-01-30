using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float minDistanceToFollow;
	public float lerpSpeed;
	public Transform player;
	private float zOffset;

	Vector3 targetPosition;
	
    void Awake()
    {
        zOffset = transform.position.z - player.position.z;
		targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, zOffset);
    }

    void LateUpdate()
    {
		float distance = Vector2.Distance(transform.position,player.transform.position);
		if(distance>minDistanceToFollow)
		{
			targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, zOffset);
		}
		// targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, zOffset);
		transform.position = Vector3.Lerp(transform.position, targetPosition, (distance/10) * Time.deltaTime * lerpSpeed);
        
		
    }
}
