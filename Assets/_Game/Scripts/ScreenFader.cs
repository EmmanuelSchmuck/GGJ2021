using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
	public bool fadeOnStart;
	public float fadeOnStartDuration;
    public Image image;
	public AnimationCurve easingCurve;

	public static ScreenFader Instance;

	private void Awake(){
		Instance = this;
		if(fadeOnStart)
		{
			image.color = Color.black;
			Fade(Color.black,Color.clear,fadeOnStartDuration);
		}
	}

	public void Fade(Color from, Color to, float duration)
	{
		StopAllCoroutines();
		StartCoroutine(FadeAnimation(from, to, duration));
	}

	private IEnumerator FadeAnimation(Color from, Color to, float duration)
	{
		float timer = 0f;
		float alpha = 0;
		while(timer<duration)
		{
			yield return new WaitForSecondsRealtime(0.01f);
			timer += 0.01f;
			alpha = easingCurve.Evaluate(timer/duration);
			image.color = Color.Lerp(from, to, alpha);
		}
	}
}
