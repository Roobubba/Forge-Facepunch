using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using BeardedManStudios.Forge.Logging;
using System.Threading.Tasks;

public class JoinGameMenu : MonoBehaviour {

	private NetworkController networkController;
	[SerializeField]
	private LobbyUIController lobbyUIController;

	[SerializeField]
	private Button refreshListButton;



	private void Start()
	{
		GameController gC = FindObjectOfType<GameController>();
		networkController = (NetworkController)gC.GetController(typeof(NetworkController)) as NetworkController;
		if (!networkController)
		{
			BMSLog.LogWarning("Could not find networkController - should not see me");
		}
		RefreshLobbiesAsync();
	}

	public void BackButton()
	{
		networkController.CancelGame();
	}

	public void RefreshList()
	{
		lobbyUIController.ClearLobbies();
		RefreshLobbiesAsync();
	}

	private async void RefreshLobbiesAsync()
	{
		await RefreshLobbies();
	}

	private async Task RefreshLobbies()
	{
		Steamworks.Data.LobbyQuery lobbyQuery = new Steamworks.Data.LobbyQuery();
		//lobbyQuery.FilterDistanceClose();
		var lobbyList = await lobbyQuery.RequestAsync();
		if (lobbyList == null)
		{
			BMSLog.Log("Lobbylist is null!");
			return;
		}

		foreach (var lobby in lobbyList)
		{
			if (lobby.GetData("FNR-FP") == "blob")
			{
				if (!lobbyUIController.AddLobby(lobby))
				{
					BMSLog.Log("Could not add lobby to lobbyUIController: " + lobby.Id.Value.ToString());
					continue;
				}
				else
				{
					BMSLog.Log("Added lobby to lobbyUIController: " + lobby.Id.Value.ToString());
				}
			}
		}
	}


}
