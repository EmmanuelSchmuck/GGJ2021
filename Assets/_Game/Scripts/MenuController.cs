using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	
	public bool fadeOnLeave;
	public float fadeDuration = 1f;

	private void Start()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}
	
    public void LoadScene(string sceneName)
	{
		Debug.Log($"Loading scene {sceneName}");
		StartCoroutine(LoadSceneCoroutine(sceneName));
		
	}

	private IEnumerator LoadSceneCoroutine(string sceneName)
	{
		if(fadeOnLeave)
		{
			ScreenFader.Instance?.Fade(Color.clear, Color.black,fadeDuration);
			yield return new WaitForSeconds(fadeDuration);
		}	
		
		SceneManager.LoadScene(sceneName);
	}	

	public void ExitGame()
	{
		Debug.Log("Exiting game");
		Application.Quit();
	}
}
