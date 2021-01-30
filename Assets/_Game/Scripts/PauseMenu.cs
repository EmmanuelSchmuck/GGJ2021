using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	public bool fadeOnLeave;
	public float fadeDuration = 1f;
	public static PauseMenu Instance;

	[SerializeField] private RectTransform menu;

	private bool isPaused;
	// Start is called before the first frame update
	void Awake()
	{
		Instance = this;
		menu.gameObject.SetActive(false);
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void BackToStartMenu()
	{
		StopAllCoroutines();
		StartCoroutine(BackToStartMenuCoroutine());
	}

	private IEnumerator BackToStartMenuCoroutine()
	{
		if (fadeOnLeave)
		{
			ScreenFader.Instance?.Fade(Color.clear, Color.black, fadeDuration);
			yield return new WaitForSecondsRealtime(fadeDuration);
		}
		Time.timeScale = 1f;
		SceneManager.LoadScene("_StartMenu");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isPaused) Resume();
			else Pause();
		}
	}

	public void Resume()
	{
		isPaused = false;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		menu.gameObject.SetActive(false);
		Time.timeScale = 1f;
	}


	public void Pause()
	{
		isPaused = true;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		menu.gameObject.SetActive(true);
		Time.timeScale = 0f;
	}
}
