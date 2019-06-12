using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIController : MonoBehaviour
{
	public Button[] lobbyButtons;
	private NetworkController networkController;

	private List<Steamworks.Data.Lobby> lobbies = new List<Steamworks.Data.Lobby>();

    // Start is called before the first frame update
    void Start()
    {
		GameController gameController = FindObjectOfType<GameController>();
		networkController = (NetworkController)gameController.GetController(typeof(NetworkController)) as NetworkController;
    }

	public bool AddLobby(Steamworks.Data.Lobby lobby)
	{
		if (lobbies.Count > 9)
			return false;

		lobbies.Add(lobby);
		int newIndex = lobbies.Count - 1;
		Text newDisplayText = lobbyButtons[newIndex].GetComponentInChildren<Text>();
		newDisplayText.text = lobby.Id.Value.ToString();
		lobbyButtons[newIndex].onClick.AddListener(() => {
			networkController.ConnectToLobby(lobby);
		});

		return true;
	}
	
	public void ClearLobbies()
	{
		for (int i = 0; i < lobbyButtons.Length; i++)
		{
			lobbyButtons[i].onClick.RemoveAllListeners();
			Text text = lobbyButtons[i].GetComponentInChildren<Text>();
			text.text = "LOBBY ID";
		}
		lobbies = new List<Steamworks.Data.Lobby>();
	}
}
