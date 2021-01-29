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

	public TextMeshPro textMesh;

	public void InitAttached(string text, Transform target)
	{
		// Become a child of the target.
		transform.parent = target;
		transform.localPosition = Vector3.zero;

		// Sets the content.
		Text = text;
	}

	private void LateUpdate()
	{
		if (transform.parent != null)
		{
			transform.eulerAngles = Vector3.zero;

			//float rot = transform.parent.eulerAngles.z % 360f;
			//Debug.Log(rot + " " + Mathf.Deg2Rad * rot);

			//if (rot <= 45 && rot > 315)
			//{
			//	// above, center down
			//	textMesh.rectTransform.pivot = new Vector2(0.5f, 0f);
			//}
			//else if (rot > 45 && rot <= 135)
			//{
			//	// left, right center
			//	textMesh.rectTransform.pivot = new Vector2(1f, 0.5f);
			//}
			//else if (rot > 135 && rot <= 225)
			//{
			//	// below, center up
			//	textMesh.rectTransform.pivot = new Vector2(0.5f, 0f);
			//}
			//else
			//{
			//	// right, left center
			//	textMesh.rectTransform.pivot = new Vector2(0f, 0.5f);
			//}
		}
	}
}
