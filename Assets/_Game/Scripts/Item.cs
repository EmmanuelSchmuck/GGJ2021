using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// can be grabbed & released by dog
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Item : MonoBehaviour
{
	public int CatchCount; // how many time was I caught by the dog

	private Rigidbody2D m_Rigidbody;
	private Collider2D m_Collider;

	protected void Awake()
	{
		this.gameObject.layer = LayerMask.NameToLayer("Item");

		this.m_Rigidbody = GetComponent<Rigidbody2D>();
		this.m_Collider = GetComponent<Collider2D>();
	}

	public void OnCatch(Transform anchor)
	{
		CatchCount++;
		transform.SetParent(anchor);
		transform.localPosition = Vector3.zero;

		m_Collider.enabled = false;
		m_Rigidbody.simulated = false;
	}

	public void OnRelease()
	{
		transform.SetParent(null);
		m_Collider.enabled = true;
		m_Rigidbody.simulated = true;
	}
}
