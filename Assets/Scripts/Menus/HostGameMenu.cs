using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Logging;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Steamworks;
using System.Collections.Generic;

public class HostGameMenu : MonoBehaviour
{

	public ScrollRect players;
	public PlayerInServerListEntry playerListEntryTemplate;
	public RectTransform playerListContentRect;
	public Button playButton; //So we can update the text on the button to make it clear we're playing solo or with friends
	public Button inviteButton;
	public Button backButton;

	private List<PlayerInServerListItemData> playerList = new List<PlayerInServerListItemData>();
	private float playerListEntryTemplateHeight;
	private float nextListUpdateTime = 0f;
	private NetworkController networkController;
	private Steamworks.Data.Lobby lobby;

	[SerializeField]
	private TextMeshProUGUI titleText;

	private void Start()
	{
		var gC = FindObjectOfType<GameController>();
		networkController = (NetworkController)gC.GetController(typeof(NetworkController)) as NetworkController;
		if (!networkController)
		{
			BMSLog.LogWarning("Could not find networkController - should not see me");
		}

		// Init the MainThreadManager
		MainThreadManager.Create();

		playButton.enabled = false;
		playerListEntryTemplateHeight = ((RectTransform)playerListEntryTemplate.transform).rect.height;
		RefreshPlayers();
		SetLobbyEvents();
		if (!networkController.GetIsHost())
		{
			LoadedAsClient();
		}

		GetLobby();
	}

	private void Update()
	{
		if (Time.time > nextListUpdateTime)
		{
			RefreshPlayers();
			nextListUpdateTime = Time.time + 5.0f + UnityEngine.Random.Range(0.0f, 1.0f);
		}
	}

	private Steamworks.Data.Lobby GetLobby()
	{
		if (lobby.Id.Value > 0)
		{
			return lobby;
		}
		else
		{
			lobby = networkController.GetLobby();
			if (lobby.Id.Value > 0)
			{
				return lobby;
			}
			else
			{
				BMSLog.LogWarning("Could not find this lobby");
				return default;
			}
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

	private void OnDestroy()
	{
		SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
	}

	private void OnLobbyMemberJoined(Steamworks.Data.Lobby lobby, Friend friendWhoJoined)
	{
		RefreshPlayers();
	}

	private void OnLobbyDataChanged(Steamworks.Data.Lobby lobby)
	{
		RefreshPlayers();
	}

	/// <summary>
	/// Add a steam Lobby to the list of visible lobbies
	/// </summary>
	/// <param name="lobby">The Steamworks Lobby to add to the list</param>
	private void AddPlayer(Friend friend)
	{
		var friendName = friend.Name;

		for (int i = 0; i < playerList.Count; ++i)
		{
			var player = playerList[i];
			if (player.playerName == friendName)
			{
				// Already have that player listed - nothing else to do
				return;
			}
		}

		var playerListItemData = new PlayerInServerListItemData {
			ListItem = GameObject.Instantiate<PlayerInServerListEntry>(playerListEntryTemplate, players.content),
			playerName = friendName,
			steamId = friend.Id,
			friend = friend,
		};

		playerListItemData.ListItem.gameObject.SetActive(true);

		UpdateItem(playerListItemData);
		playerListItemData.NextUpdate = Time.time + 5.0f + UnityEngine.Random.Range(0.0f, 1.0f);

		playerList.Add(playerListItemData);
		SetListItemSelected(playerListItemData, false);

		RepositionItems();
	}

	private async void GetSteamAvatarAsync(PlayerInServerListItemData playerInServerListItemData)
	{
		await GetSteamAvatar(playerInServerListItemData);
	}

	private async Task GetSteamAvatar(PlayerInServerListItemData playerInServerListItemData)
	{
		var friend = playerInServerListItemData.friend;
		var img = await friend.GetMediumAvatarAsync();
		if (!img.HasValue)
			return;
		playerInServerListItemData.ListItem.steamImage.LoadTextureFromImage(img.Value);
	}

	/// <summary>
	/// Remove a lobby from the list
	/// </summary>
	/// <param name="item">Lobby listItemData to remove</param>
	private void RemovePlayer(PlayerInServerListItemData item)
	{
		Destroy(item.ListItem.gameObject);
		playerList.Remove(item);
		RepositionItems();
	}

	/// <summary>
	/// Reposition the server list items after a add/remove operation
	/// </summary>
	private void RepositionItems()
	{
		for (int i = 0; i < playerList.Count; i++)
		{
			PositionItem(playerList[i].ListItem.gameObject, i);
		}

		var sizeDelta = playerListContentRect.sizeDelta;
		sizeDelta.y = playerList.Count * playerListEntryTemplateHeight;
		playerListContentRect.sizeDelta = sizeDelta;
	}

	/// <summary>
	/// Set the position of an item in the server list
	/// </summary>
	/// <param name="item"></param>
	/// <param name="index"></param>
	private void PositionItem(GameObject item, int index)
	{
		var rectTransform = (RectTransform)item.transform;
		rectTransform.localPosition = new Vector3(0.0f, -playerListEntryTemplateHeight * index, 0.0f);
	}

	/// <summary>
	/// Set the border around the selected server entry
	/// </summary>
	/// <param name="data"></param>
	/// <param name="selected"></param>
	private void SetListItemSelected(PlayerInServerListItemData data, bool selected)
	{
		data.ListItem.GetComponent<Image>().enabled = selected;
	}

	/// <summary>
	/// Update a specific server's details on the server list.
	/// </summary>
	/// <param name="option">The server display information to update</param>
	private void UpdateItem(PlayerInServerListItemData option)
	{
		option.ListItem.playerName.text = option.playerName;
		option.ListItem.playerSteamId.text = option.steamId.Value.ToString();
		GetSteamAvatarAsync(option);
	}

	private void RefreshPlayers()
	{
		int numInLobby = 0;
		foreach (var friend in lobby.Members)
		{
			AddPlayer(friend);
			numInLobby++;
		}

		playButton.enabled = numInLobby > 0;
	}

}

internal class PlayerInServerListItemData
{
	public string playerName;
	public Friend friend;
	public SteamId steamId;
	public float NextUpdate;
	public PlayerInServerListEntry ListItem;
}
