using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour
{
	public enum FontSize
	{
		Regular,
		Small,
		Tiny,
		Big,
		Huge
	}

	public SpeechBubble speechBubblePrefab;
	public Transform dialogAnchor;
	public bool spawnDetached;

	private SpeechBubble currentBubble;
	private float nextFontSize = 14;

    public void Speak(string text, float duration = 5f)
	{
		if (currentBubble == null || spawnDetached)
		{
			var bubble = Instantiate<SpeechBubble>(speechBubblePrefab);
			bubble.Init(text, dialogAnchor, duration, !spawnDetached, transform.up);
			bubble.FontSize = nextFontSize;

			if (!spawnDetached)
			{
				currentBubble = bubble;
			}
		}
		else
		{
			currentBubble.Text = text;
		}
	}

	public void SetFontSize(FontSize size)
	{
		switch (size)
		{
			case FontSize.Small:
				nextFontSize = 7;
				break;

			case FontSize.Tiny:
				nextFontSize = 4;
				break;

			case FontSize.Big:
				nextFontSize = 18;
				break;

			case FontSize.Huge:
				nextFontSize = 32;
				break;

			case FontSize.Regular:
			default:
				nextFontSize = 14;
				break;
		}
	}
}
