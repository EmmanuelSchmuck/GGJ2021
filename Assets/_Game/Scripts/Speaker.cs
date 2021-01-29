using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour
{
    public SpeechBubble speechBubblePrefab;
	public Transform dialogAnchor;

	private SpeechBubble currentBubble;

    public void Speak(string text)
	{
		if (currentBubble == null)
		{
			currentBubble = Instantiate<SpeechBubble>(speechBubblePrefab);
			currentBubble.InitAttached(text, dialogAnchor);
		}
		else
		{
			currentBubble.Text = text;
		}
	}
}
