using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour
{
    public SpeechBubble speechBubblePrefab;
	public Transform dialogAnchor;
	public bool spawnDetached;

	private SpeechBubble currentBubble;

    public void Speak(string text)
	{
		if (currentBubble == null || spawnDetached)
		{
			var bubble = Instantiate<SpeechBubble>(speechBubblePrefab);
			bubble.Init(text, dialogAnchor, 5f, !spawnDetached);

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
}
