using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform player;
	private float zOffset;
    void Awake()
    {
        zOffset = transform.position.z - player.position.z;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, zOffset);
		
    }
}
