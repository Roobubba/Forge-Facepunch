﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Steamworks;

/// <summary>
/// Initial game setup loading script
/// </summary>
public class SplashScreenLoader : MonoBehaviour {

	[SerializeField]
	private GameObject gameControllerPrefab;

	// Roo's amazing unbelievable stunning wonderful project
	//private const int STEAM_APP_ID = 1090590;
	// Spacewar steam test project
	private const int STEAM_APP_ID = 480;

	private void Awake()
	{
		DontDestroyOnLoad(this);
	}

	void Start()
	{
		try
		{
			BeardedManStudios.Forge.Logging.BMSLog.Log("Initialising Steam Client");
			SteamClient.Init(STEAM_APP_ID);
		}
		catch (System.Exception e)
		{
			// Couldn't init for some reason (steam is closed etc)
			BeardedManStudios.Forge.Logging.BMSLog.Log("Error, could not initialise the steam client, is Steam running?");
			Debug.LogException(e);
		}
		LoadControllers();
	}

	private void LoadControllers()
	{
		GameObject gameControllerGO = Instantiate(gameControllerPrefab) as GameObject;
		GameController gameController = gameControllerGO.GetComponent<GameController>();
		LevelManager levelManager = (LevelManager)gameController.GetController(typeof(LevelManager)) as LevelManager;
		NetworkController networkController = (NetworkController)gameController.GetController(typeof(NetworkController)) as NetworkController;

		Debug.Log("gameControllerGO = " + gameControllerGO);
		Debug.Log("gameController = " + gameController);
		Debug.Log("levelManager = " + levelManager);
		Debug.Log("networkController = " + networkController);

		LevelManager.LoadMainMenu();
	}

	private void OnDestroy()
	{
		SteamClient.Shutdown();
	}

	private void OnApplicationQuit()
	{
		SteamClient.Shutdown();
	}
}
