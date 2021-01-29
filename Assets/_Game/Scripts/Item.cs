using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// can be grabbed & released by dog
[RequireComponent(typeof(Collider2D))]
public class Item : MonoBehaviour
{
	public int CatchCount; // how many time was I caught by the dog
	protected void Awake()
	{
		this.gameObject.layer = LayerMask.NameToLayer("Item");
	}

	public void OnCatch(Transform anchor)
	{
		CatchCount++;
		transform.SetParent(anchor);
		transform.localPosition = Vector3.zero;
		
		GetComponent<Collider2D>().enabled = false;
		GetComponent<Rigidbody2D>().isKinematic = true;
	}

	public void OnRelease()
	{
		transform.SetParent(null);
		GetComponent<Collider2D>().enabled = true;
		GetComponent<Rigidbody2D>().isKinematic = false;
	}
}
