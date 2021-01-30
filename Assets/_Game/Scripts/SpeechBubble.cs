using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
	public string Text
	{
		get => textMesh.text;
		set => textMesh.text = value;
	}

	public float FontSize
	{
		get => textMesh.fontSize;
		set => textMesh.fontSize = value;
	}

	public TextMeshPro textMesh;
	public AnimationCurve opacityOverLifetime;

	private float lifetime;
	private float remainingTime;
	private Vector3 driftDirection;

	public void Init(string text, Transform target, float duration, bool attached = true, Vector3? drift = null)
	{
		if (attached)
		{
			// Become a child of the target.
			transform.parent = target;
			transform.localPosition = Vector3.zero;
		}
		else
		{
			// Stay in world space but take the position of the target.
			transform.parent = null;
			transform.position = target.position;
		}

		this.driftDirection = drift ?? Random.onUnitSphere;

		// Sets the content.
		Text = text;
		remainingTime = duration;
		lifetime = duration;
	}

	private void LateUpdate()
	{
		if (transform.parent != null)
		{
			textMesh.rectTransform.eulerAngles = Vector3.zero;
		}

		// animates opacity
		remainingTime -= Time.deltaTime;
		Color textColor = textMesh.color;
		textColor.a = Mathf.Clamp01(opacityOverLifetime.Evaluate(Mathf.InverseLerp(0, lifetime, lifetime - remainingTime)));
		textMesh.color = textColor;
		if (remainingTime <= 0f)
		{
			Destroy(gameObject);
		}

		// drifting
		transform.position += Time.deltaTime * driftDirection;
	}
}
