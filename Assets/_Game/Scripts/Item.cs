using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// can be grabbed & released by dog
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Item : MonoBehaviour
{
	public AudioSource whoosh;
	public float movementAnimationDuration = 0.5f;
	public AnimationCurve movementAnimationCurve;
	public int CatchCount; // how many time was I caught by the dog
	public float lastTimeOfDrop { get; set; }

	private Rigidbody2D m_Rigidbody;
	private Collider2D m_Collider;

	public bool IsAnimating{get; private set;}

	protected void Awake()
	{
		this.gameObject.layer = LayerMask.NameToLayer("Item");

		this.m_Rigidbody = GetComponent<Rigidbody2D>();
		this.m_Collider = GetComponent<Collider2D>();
	}

	// must be in local space already
	private IEnumerator LerpPositionAndRotation()
	{
		// IsAnimating = true;
		float duration = movementAnimationDuration;
		float timer = 0f;
		Vector3 startPosition = transform.localPosition;
		Vector3 targetPosition = Vector3.zero;
		Quaternion startRotation = transform.localRotation;
		Quaternion targetRotation = Quaternion.identity;
		while(timer < duration)
		{	
			float alpha = movementAnimationCurve.Evaluate(timer/duration);
			transform.localPosition = Vector3.Lerp(startPosition, targetPosition, alpha);
			transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, alpha);
			timer += Time.deltaTime;
			yield return null;
		}
		
		// IsAnimating = false;
	}

	public void OnCatch(Transform anchor)
	{
		StopAllCoroutines();
		CatchCount++;
		transform.SetParent(anchor);
		m_Collider.enabled = false;
		m_Rigidbody.simulated = false;
		whoosh.Play();
		StartCoroutine(LerpPositionAndRotation());
		// transform.localPosition = Vector3.zero;
	}

	public void OnRelease(Transform newParent = null)
	{
		StopAllCoroutines();
		transform.SetParent(newParent);
		m_Collider.enabled = true;
		m_Rigidbody.simulated = true;
		lastTimeOfDrop = Time.time;
	}
}
