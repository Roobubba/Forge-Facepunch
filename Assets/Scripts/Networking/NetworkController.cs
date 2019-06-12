using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Logging;
using Steamworks;
using System.Threading.Tasks;
using System;

public class NetworkController : Controller
{
	public override Type GetControllerType()
	{
		return this.GetType();
	}

	/// <summary>
	/// NetworkController controls the Forge Networking NetworkManager prefab, game object and script.
	/// </summary>
	[SerializeField]
	private GameObject networkManagerPrefab;
	private GameObject networkManagerGO;
	private NetworkManager networkManager;
	private bool haveNetworkManager = false;
	public bool useMainThreadManagerForRPCs = false;

	private bool isHosting = false;
	public bool GetIsHost()
	{
		return isHosting;
	}

	private FacepunchP2PClient steamP2PClient = null;
	private FacepunchP2PServer steamP2PServer = null;
	//private NetWorker steamP2PClient = null;
	//private NetWorker steamP2PServer = null;

	private Steamworks.Data.Lobby lobby;

	private LevelManager levelManager;
	private GameController gameController;

	// Do these once
	private void Start()
	{
		gameController = FindObjectOfType<GameController>();
		levelManager = (LevelManager)gameController.GetController(typeof(LevelManager)) as LevelManager;
		if (useMainThreadManagerForRPCs)
		{
			Rpc.MainThreadRunner = MainThreadManager.Instance;
		}
		SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
		SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;

	}

	/// <summary>
	/// Reset our network status eg on reload main menu
	/// </summary>
	public void Setup()
	{
		isHosting = false;
	}

	private void UnsubscribeP2PClientEvents()
	{
		if (steamP2PClient != null)
		{
			((FacepunchP2PClient)steamP2PClient).bindSuccessful -= OnClientBindSuccessful;
			((FacepunchP2PClient)steamP2PClient).serverAccepted -= OnClientServerAccepted;
			((FacepunchP2PClient)steamP2PClient).disconnected -= OnClientDisconnected;

		}
	}
	private void UnsubscribeP2PServerEvents()
	{
		if (steamP2PServer != null)
		{
			((FacepunchP2PServer)steamP2PServer).playerTimeout -= OnServerPlayerTimeout;
			((FacepunchP2PServer)steamP2PServer).playerConnected -= OnServerPlayerConnected;
			((FacepunchP2PServer)steamP2PServer).playerAccepted -= OnServerPlayerAccepted;
			((FacepunchP2PServer)steamP2PServer).playerDisconnected -= OnServerPlayerDisconnected;
			((FacepunchP2PServer)steamP2PServer).disconnected -= OnServerDisconnected;
		}
	}

	private void OnDestroy()
	{
		SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
		SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
		UnsubscribeP2PClientEvents();
		UnsubscribeP2PServerEvents();
		//lobby.Leave();
		//lobby = default;
	}

	public void CancelGame(bool loadMenu = true)
	{
		BMSLog.Log("Cancelling game start and reloading main menu");
		UnsubscribeP2PClientEvents();
		UnsubscribeP2PServerEvents();
		lobby.Leave();
		lobby = default;
		if (haveNetworkManager)
		{
			BMSLog.Log("Calling networkManager.Disconnect()");
			networkManager.Disconnect();
			haveNetworkManager = false;
		}
		if (loadMenu) LevelManager.LoadMainMenu();
	}

	private NetworkManager GetNetworkManager()
	{
		if (NetworkManager.Instance == null)
		{
			networkManagerGO = Instantiate(networkManagerPrefab) as GameObject;
		}
		haveNetworkManager = true;
		return NetworkManager.Instance;
	}

	public void HostGame()
	{
		//Set hosting
		isHosting = true;
		if (!haveNetworkManager)
		{
			networkManager = GetNetworkManager();
		}
		steamP2PServer = new FacepunchP2PServer(4);
		networkManager.Initialize(steamP2PServer);
		((FacepunchP2PServer)steamP2PServer).playerTimeout += OnServerPlayerTimeout;
		((FacepunchP2PServer)steamP2PServer).playerConnected += OnServerPlayerConnected;
		((FacepunchP2PServer)steamP2PServer).playerAccepted += OnServerPlayerAccepted;
		((FacepunchP2PServer)steamP2PServer).playerDisconnected += OnServerPlayerDisconnected;
		((FacepunchP2PServer)steamP2PServer).disconnected += OnServerDisconnected;
		BMSLog.Log("Starting Host fuction");
		((FacepunchP2PServer)steamP2PServer).Host();
		Connected(steamP2PServer);
		CreateLobbyAsync();
		
		//Load 02aLobbyHost scene
		levelManager.LoadLevel("02aLobbyHost");
	}


	private async void CreateLobbyAsync()
	{
		await CreateLobby();
	}

	private async Task CreateLobby()
	{
		Steamworks.Data.Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(4);
		if (!lobby.HasValue)
		{
			BMSLog.Log("Error creating lobby");
			return;
		}
		BMSLog.Log("Created Lobby Async: lobby Id = " + lobby.Value.Id);
		var lobbyVal = lobby.Value;
		lobbyVal.SetPublic();
		lobbyVal.SetData("FNR-FP","blob");
		this.lobby = lobbyVal;
	}

	private void OnLobbyMemberJoined(Steamworks.Data.Lobby lobby, Friend friend)
	{
		BMSLog.Log("Player joined lobby: " + friend.Name);
	}

	private void OnLobbyMemberLeave(Steamworks.Data.Lobby lobby, Friend friend)
	{
		BMSLog.Log("Player left lobby: " + friend.Name);
	}

	private void OnLobbyDataChanged(Steamworks.Data.Lobby lobby)
	{
		BMSLog.Log("Lobby data changed, new data:");
		foreach (KeyValuePair<string, string> data in lobby.Data)
		{
			BMSLog.Log(data.Key + "; " + data.Value);
		}
	}

	private void HandlePlayerDisconnect(NetworkingPlayer player, NetWorker sender)
	{
		NetworkObject networkObjectToDestroy = null;
		//Loop through all players and find the player who disconnected
		foreach (var no in sender.NetworkObjectList)
		{
			if (no.Owner == player)
			{
				//Found him
				networkObjectToDestroy = no;
			}
		}

		//Remove the actual network object outside of the foreach loop, as we would modify the collection at runtime elsewise. (could also use a return, too late)
		if (networkObjectToDestroy != null)
		{
			sender.NetworkObjectList.Remove(networkObjectToDestroy);
			networkObjectToDestroy.Destroy();
		}
	}

	public void InviteFriends()
	{
		if (lobby.Id.Value > 0)
		{
			SteamFriends.OpenGameInviteOverlay(lobby.Id);
		}
		else
		{
			BMSLog.Log("lobby id value is not set, is the lobby created yet?");
		}
	}

	public void JoinGame()
	{
		isHosting = false;



		//Load 02bLobbyJoin scene
		levelManager.LoadLevel("02bLobbyJoin");
	}

	public void ChangeLevel(string scene)
	{
		levelManager.LoadLevel(scene);
	}

	public void StartGame()
	{
		BMSLog.Log("StartGame called. IsHosting = " + isHosting);
		if (isHosting)
		{
			LevelManager.StartGameLevel();
		}
		else
		{
			BMSLog.Log("Only the host can start the game");
		}
	}

	public void ConnectToLobby(Steamworks.Data.Lobby lobbyToJoin)
	{
		JoinLobbyAsync(lobbyToJoin);
	}

	private async void JoinLobbyAsync(Steamworks.Data.Lobby lobbyToJoin)
	{
		await JoinLobby(lobbyToJoin);
	}
	
	private async Task JoinLobby(Steamworks.Data.Lobby lobbyToJoin)
	{
		RoomEnter x = await lobbyToJoin.Join();
		if (x != RoomEnter.Success)
		{
			BMSLog.Log("Error connecting to lobby returned: " + x.ToString());
			return;
		}
		this.lobby = lobbyToJoin;
		BMSLog.Log("Connected to lobby, owner.Id = " + lobbyToJoin.Owner.Id.Value);
		ConnectToHost(lobbyToJoin.Owner.Id, null);


	}

	public void ConnectToHost(SteamId hostSteamId, UnityEngine.UI.Text connectionText)
	{
		string info = "Starting Client connection to lobby owner (host): " + hostSteamId.Value.ToString();
		BMSLog.Log(info);
		if (connectionText != null)
		{
			connectionText.text = info;
		}
		networkManager = GetNetworkManager();
		steamP2PClient = new FacepunchP2PClient();
		networkManager.Initialize(steamP2PClient);
		((FacepunchP2PClient)steamP2PClient).bindSuccessful += OnClientBindSuccessful;
		((FacepunchP2PClient)steamP2PClient).serverAccepted += OnClientServerAccepted;
		((FacepunchP2PClient)steamP2PClient).disconnected += OnClientDisconnected;
		((FacepunchP2PClient)steamP2PClient).Connect(hostSteamId);
		Connected(steamP2PClient);

	}


	/// <summary>
	/// 
	/// 
	/// Called by LevelManager when we've loaded into the Game Scene
	/// </summary>
	public void GameSceneLoaded()
	{
		if (isHosting)
		{
			gameController.LoadGameAsHost();
		}
		else
		{
			gameController.LoadGameAsClient();
		}
		networkManager.InstantiatePlayerController();
	}

	/// <summary>
	/// Connected is called whenever we set up a new server or client
	/// </summary>
	/// <param name="networker"></param>
	public void Connected(NetWorker networker, UnityEngine.UI.Text connectionText = null)
	{
		string info = "Networker is bound";
		if (!networker.IsBound)
		{
			BMSLog.LogWarning("NetWorker failed to bind");
			//return;
		}
		else
		{
			BMSLog.Log(info);
			if (connectionText != null)
			{
				connectionText.text = info;
			}
		}

		if (!haveNetworkManager)
		{
			BMSLog.LogWarning("Network Manager could not be found. This should never happen!");
			if (networkManagerGO != null) Destroy(networkManagerGO);
			networkManager = GetNetworkManager();
			networkManager.Initialize(networker);
		}
		info = "Networker Connection Complete";
		BMSLog.Log(info);
		if (connectionText != null)
		{
			connectionText.text = info;
		}
	}


	#region FacepunchP2PServer Events

	private void OnServerPlayerTimeout(NetworkingPlayer player, NetWorker sender)
	{
		MainThreadManager.Run(() =>
		{
			BMSLog.Log("Player " + player.NetworkId + " timed out");
		});
	}

	private void OnServerPlayerConnected(NetworkingPlayer player, NetWorker sender)
	{
		MainThreadManager.Run(() =>
		{
			BMSLog.Log("Player " + player.NetworkId + " connecting");
		});
	}

	private void OnServerPlayerAccepted(NetworkingPlayer player, NetWorker sender)
	{
		MainThreadManager.Run(() =>
		{
			BMSLog.Log("Player " + player.NetworkId + " connected");
		});
	}

	private void OnServerPlayerDisconnected(NetworkingPlayer player, NetWorker sender)
	{
		MainThreadManager.Run(() =>
		{
			BMSLog.Log("Player " + player.NetworkId + " disconnected.");
			HandlePlayerDisconnect(player, sender);
		});
	}

	private void OnServerDisconnected(NetWorker sender)
	{
		MainThreadManager.Run(() =>
		{
			BMSLog.Log("steamP2PServer.disconnected called");
		});
	}
	#endregion

	#region FacepuncP2PClient Events
	
	private void OnClientBindSuccessful(NetWorker server)
	{
		MainThreadManager.Run(() =>
		{
			isHosting = false;
			//Connected(steamP2PClient);
			string info = "Networker bound, connecting to server...";
			BMSLog.Log(info);
			/*if (connectionText != null)
			{
				connectionText.text = info;
			}*/
		});
	}

	private void OnClientDisconnected (NetWorker server)
	{
		MainThreadManager.Run(() =>
		{
			BMSLog.Log("Client disconnected");
			CancelGame(true);
		});
	}

	private void OnClientServerAccepted (NetWorker server)
	{
		MainThreadManager.Run(() =>
		{
			string info = "Server accepted the connection, loading server scene";
			BMSLog.Log(info);
			/*if (connectionText != null)
			{
				connectionText.text = info;
			}*/
		});
	}

	#endregion


}
