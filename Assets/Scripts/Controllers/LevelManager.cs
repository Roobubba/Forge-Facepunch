using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using BeardedManStudios.Forge.Networking.Unity;

public class LevelManager : Controller
{

	public override Type GetControllerType()
	{
		return this.GetType();
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneWasLoaded;

	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneWasLoaded;

	}


	void OnSceneWasLoaded(Scene scene, LoadSceneMode sceneMode)
	{
		GameController gameController = FindObjectOfType<GameController>();
		NetworkController networkController = (NetworkController) gameController.GetController(typeof(NetworkController)) as NetworkController;
		if (scene.name == "01aMainMenu")
		{
			//Load main menu and reset network status to initial state
			gameController.ResetGame();
			networkController.Setup();

		}
		else if (scene.name == "03aGame")
		{
			networkController.GameSceneLoaded();
		}
	}


	public void LoadLevel(string name)
	{
		Debug.Log ("Level load requested for: " + name);
		SceneManager.LoadScene(name);
	}

	public static void LoadMainMenu()
	{
		Debug.Log("Level load requested for Main Menu");
		SceneManager.LoadScene("01aMainMenu");
	}

	public static void ReloadCurrentLevel()
	{
		SceneManager.LoadScene("03aGame");
	}

	public static void StartGameLevel()
	{
		SceneManager.LoadScene("03aGame");
	}

	public void QuitRequest()
	{
		Application.Quit();
#if UNITY_EDITOR
		Debug.Log("Application Quit Requested");
		if (UnityEditor.EditorApplication.isPlaying)
		{
			UnityEditor.EditorApplication.isPlaying = false;
		}
#endif

	}


}
