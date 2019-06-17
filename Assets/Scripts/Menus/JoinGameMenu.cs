using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Logging;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class JoinGameMenu : MonoBehaviour {

	public ScrollRect servers;
	public FacepunchServerListEntry serverListEntryTemplate;
	public RectTransform serverListContentRect;
	public Button connectButton;

	private int selectedServer = -1;
	private List<FacepunchServerListItemData> serverList = new List<FacepunchServerListItemData>();
	private float serverListEntryTemplateHeight;
	private float nextListUpdateTime = 0f;
	private NetworkController networkController;

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

		connectButton.enabled = false;
		serverListEntryTemplateHeight = ((RectTransform)serverListEntryTemplate.transform).rect.height;
		RefreshLobbiesAsync();
	}

	private void Update()
	{
		if (Time.time > nextListUpdateTime)
		{
			RefreshLobbiesAsync();
			nextListUpdateTime = Time.time + 5.0f + UnityEngine.Random.Range(0.0f, 1.0f);
		}
	}

	public void BackButton()
	{
		networkController.CancelGame();
	}

	public void ConnectButton()
	{
		if (selectedServer >= serverList.Count)
		{
			connectButton.enabled = false;
			return;
		}

		networkController.ConnectToLobby(serverList[selectedServer].lobby);
	}

	/// <summary>
	/// Called when a server list item is clicked. It will automatically connect on double click.
	/// </summary>
	/// <param name="baseEventData"></param>
	public void OnServerItemPointerClick(BaseEventData baseEventData)
	{
		var eventData = (PointerEventData)baseEventData;
		for (int i = 0; i < serverList.Count; ++i)
		{
			if (serverList[i].ListItem.gameObject != eventData.pointerPress)
				continue;

			SetSelectedServer(i);
			if (eventData.clickCount == 2)
				networkController.ConnectToLobby(serverList[i].lobby);

			return;
		}
	}

	/// <summary>
	/// Add a steam Lobby to the list of visible lobbies
	/// </summary>
	/// <param name="lobby">The Steamworks Lobby to add to the list</param>
	private void AddServer(Steamworks.Data.Lobby lobby)
	{
		var hostName = lobby.Id.Value.ToString();

		for (int i = 0; i < serverList.Count; ++i)
		{
			var server = serverList[i];
			if (server.Hostname == hostName)
			{
				// Already have that server listed nothing else to do
				return;
			}
		}

		var serverListItemData = new FacepunchServerListItemData {
			ListItem = GameObject.Instantiate<FacepunchServerListEntry>(serverListEntryTemplate, servers.content),
			Hostname = hostName,
		};
		serverListItemData.ListItem.gameObject.SetActive(true);

		serverListItemData.lobby = lobby;

		UpdateItem(serverListItemData);
		serverListItemData.NextUpdate = Time.time + 5.0f + UnityEngine.Random.Range(0.0f, 1.0f);

		serverList.Add(serverListItemData);
		SetListItemSelected(serverListItemData, false);

		RepositionItems();
	}

	/// <summary>
	/// Remove a lobby from the list
	/// </summary>
	/// <param name="item">Lobby listItemData to remove</param>
	private void RemoveServer(FacepunchServerListItemData item)
	{
		Destroy(item.ListItem.gameObject);
		serverList.Remove(item);
		RepositionItems();
	}

	/// <summary>
	/// Reposition the server list items after a add/remove operation
	/// </summary>
	private void RepositionItems()
	{
		for (int i = 0; i < serverList.Count; i++)
		{
			PositionItem(serverList[i].ListItem.gameObject, i);
		}

		var sizeDelta = serverListContentRect.sizeDelta;
		sizeDelta.y = serverList.Count * serverListEntryTemplateHeight;
		serverListContentRect.sizeDelta = sizeDelta;
	}

	/// <summary>
	/// Set the position of an item in the server list
	/// </summary>
	/// <param name="item"></param>
	/// <param name="index"></param>
	private void PositionItem(GameObject item, int index)
	{
		var rectTransform = (RectTransform)item.transform;
		rectTransform.localPosition = new Vector3(0.0f, -serverListEntryTemplateHeight * index, 0.0f);
	}

	/// <summary>
	/// Select a lobby from the list and set the FacepunchMultiplayerMenu.lobbyToJoin variable
	/// </summary>
	/// <param name="index"></param>
	private void SetSelectedServer(int index)
	{
		if (selectedServer == index)
			return;

		selectedServer = index;
		for (int i = 0; i < serverList.Count; i++)
		{
			SetListItemSelected(serverList[i], index == i);
		}

		connectButton.enabled = index >= 0;
	}

	/// <summary>
	/// Set the border around the selected server entry
	/// </summary>
	/// <param name="data"></param>
	/// <param name="selected"></param>
	private void SetListItemSelected(FacepunchServerListItemData data, bool selected)
	{
		data.ListItem.GetComponent<Image>().enabled = selected;
	}

	/// <summary>
	/// Update a specific server's details on the server list.
	/// </summary>
	/// <param name="option">The server display information to update</param>
	private void UpdateItem(FacepunchServerListItemData option)
	{
		// TODO:  Extract lobby info for display on the menu

		option.ListItem.hostName.text = option.Hostname;
	}

	private async void RefreshLobbiesAsync()
	{
		await RefreshLobbies();
	}

	private async Task RefreshLobbies()
	{
		var lobbyQuery = new Steamworks.Data.LobbyQuery();
		var lobbyList = await lobbyQuery.RequestAsync();
		if (lobbyList == null)
		{
			BMSLog.Log("Lobbylist is null!");
			return;
		}

		foreach (var lobby in lobbyList)
		{
			//if (lobby.GetData("FNR-FP") == "blob")
			//{
				AddServer(lobby);
			//}
		}
	}
}

internal class FacepunchServerListItemData
{
	public string Hostname;
	public FacepunchServerListEntry ListItem;
	public float NextUpdate;
	public Steamworks.Data.Lobby lobby;
}
