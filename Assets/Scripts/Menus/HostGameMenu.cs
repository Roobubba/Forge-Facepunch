using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking.Lobby;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Steamworks;


public class HostGameMenu : MonoBehaviour {

	public Button playButton; //So we can update the text on the button to make it clear we're playing solo or with friends
	public Button inviteButton;
	public Button backButton;
	private NetworkController networkController;
	private GameController gameController;

	[SerializeField]
	private Text[] playerTexts;

	[SerializeField]
	private TextMeshProUGUI titleText;

	private void Start()
	{
		gameController = FindObjectOfType<GameController>();
		networkController = (NetworkController)gameController.GetController(typeof(NetworkController)) as NetworkController;
		SetLobbyEvents();
		if (!networkController.GetIsHost())
		{
			LoadedAsClient();
		}
	}

	public void BackButton()
	{
		networkController.CancelGame();
	}

	public void InviteFriendsButton()
	{
		networkController.InviteFriends();
	}

	public void StartGameButton()
	{
		networkController.StartGame();
	}
	
	public void LoadedAsClient()
	{
		titleText.text = "WAITING FOR HOST";
	}

	private void SetLobbyEvents()
	{
		SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
	}

	private void OnLobbyDataChanged(Steamworks.Data.Lobby lobby)
	{
		UpdateUI(lobby);
	}

	private void OnLobbyMemberJoined(Steamworks.Data.Lobby lobby, Friend friendWhoJoined)
	{
		UpdateUI(lobby);
	}

	private void UpdateUI(Steamworks.Data.Lobby lobby)
	{
		int index = 0;
		foreach (Friend friend in lobby.Members)
		{
			playerTexts[index].text = friend.Name;
			index++;
		}
		if (index < playerTexts.Length - 1)
		{
			for (int i = index; i < playerTexts.Length; i++)
			{
				playerTexts[i].text = "PLAYER NAME";
			}
		}
	}

	private void OnDestroy()
	{
		SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
	}

}
