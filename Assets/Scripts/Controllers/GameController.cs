using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;
using System;
using BeardedManStudios.Forge.Logging;

public class GameController : MonoBehaviour
{

	//Prefab GameObjects and their controller scripts
	[SerializeField]
	private GameObject levelManagerPrefab;
	private LevelManager levelManager;

	/// <summary>
	/// NetworkController is the project's master network manager, controlling both Forge NetworkManager and Facepunch Steamworks Client
	/// </summary>
	[SerializeField]
	private GameObject networkControllerPrefab;
	private GameObject networkControllerGO;
	private NetworkController networkController;

	private List<Controller> allControllers = new List<Controller>();

	public Controller GetController(Type type)
	{
		Controller returnController = CheckControllersForType(type);
		if (returnController != null)
		{
			return returnController;
		}
		RefreshControllers();
		returnController = CheckControllersForType(type);
		if (returnController != null)
		{
			return returnController;
		}
		returnController = CreateController(type);
		return returnController;
	}

	private Controller CreateController(Type type)
	{
		GameObject temp;
		Controller output;
		if (type == typeof(LevelManager))
		{
			temp = Instantiate(levelManagerPrefab, transform) as GameObject;
			output = temp.GetComponent<LevelManager>();
		}
		else if (type == typeof(NetworkController))
		{
			temp = Instantiate(networkControllerPrefab, transform) as GameObject;
			output = temp.GetComponent<NetworkController>();
		}
		else
		{
			return null;
		}
		return output;
	}

	private Controller CheckControllersForType(Type type)
	{
		for (int i = 0; i < allControllers.Count; i++)
		{
			if (allControllers[i].GetControllerType() == type)
			{
				return allControllers[i];
			}
		}
		return null;
	}
	private void RefreshControllers()
	{
		Controller[] output = GetComponentsInChildren<Controller>();
		allControllers.Clear();
		for (int i = 0; i < output.Length; i++)
		{
			allControllers.Add(output[i]);
		}
	}

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);				
	}

	public void LoadGameAsHost()
	{
		BMSLog.Log("LoadGameAsHost");
	}

	public void LoadGameAsClient()
	{
		BMSLog.Log("LoadGameAsClient");
	}

	/// <summary>
	/// When we return to the main menu, delete everything from the game and reset to initial state.
	/// </summary>
	/// 	/// NEEDS TO BE REFACTORED OUT
	public void ResetGame()
	{
		BMSLog.Log("GameController.ResetGame called");

		for (int i = 0; i < allControllers.Count; i++)
		{
			if (allControllers[i] != null)
			{
				if (allControllers[i].GetControllerType() != typeof(LevelManager) && allControllers[i].GetControllerType() != typeof(NetworkController))
				{	
					Destroy(allControllers[i].gameObject);
				}
			}
		}

	}

}
