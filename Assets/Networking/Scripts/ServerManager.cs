using System.Collections.Generic;
using Unity.Netcode;
using System;
using UnityEngine;

public struct PlayerData
{
    public ulong clientID;              // The client ID of the player
    public string playerName;           // The name of the player
    public GameObject playerObject;     // The player object in the scene
    public GameObject networkObject;    // The networked object
    public Color playerColor;           // The player colour
}

public class ServerManager : Singleton<ServerManager>
{

    [Header("Server Settings")]
    public GameObject playerPrefab;     // The player prefab to spawn in the scene
    public int maxPlayers = 9;          // The maximum number of players in the server

    // Server data
    private string m_joinCode;          // Code to join the relay server, defined when the relay is allocated.
    private Dictionary<ulong, PlayerData> m_players = new Dictionary<ulong, PlayerData>();

    // UnityEvents, replace with event system?
    public static event Action<PlayerData> OnPlayerSpawn;
    public static event Action<PlayerData> OnPlayerDisconnect;
    public static event Action<string> OnGameCode;

    [Header("Game State Scriptable Objects")]
    [Tooltip("Holds the state of runtime variables.")]
    [SerializeField] private GameState m_gameStateSO;
    [Tooltip("Holds the amount of connected players.")]
    [SerializeField] private IntVariable m_connectedPlayersSO;
    [Tooltip("Holds the Color Manager Scriptable Object.")]
    [SerializeField] private ColorManager m_colorManager;

    // Setup
    private async void Start()
    {
        // VERY TEMPORARY, REPLACE LATER
        if (!m_gameStateSO) throw new NullReferenceException("Missing GameState, HelloWorld purposes");
        if (!m_connectedPlayersSO) throw new NullReferenceException("Missing connected players scriptable object.");
        if (!m_colorManager) throw new NullReferenceException("Missing color manager scriptable object.");

        // Create a new relay allocation with a maximum number of participants.
        m_joinCode = await RelayManager.CreateRelay(maxPlayers);
        Debug.Log("Join code: " + m_joinCode);
        OnGameCode?.Invoke(m_joinCode);

        // Set the callbacks for the network manager.
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
    }

    // Called when a client connects to the server.
    private void OnClientConnected(ulong clientID)
    {
        // Because the ApprovalCheck needs to be on the same object as the
        // network manager it is easier for now to just handle it in this script
        if (m_players.Count >= maxPlayers || m_gameStateSO.HasStarted)
        {
            DisconnectPlayer(clientID);
            return;
        }

        Debug.Log("Client connected: " + clientID);

        // Create a new player object and add it to the dictionary.
        m_players.Add(clientID, new PlayerData()
        {
            clientID = clientID,
            playerName = "Player " + clientID,
            playerObject = Instantiate(playerPrefab),
            networkObject = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject,
            playerColor = m_colorManager.GetColor()
        });

        m_connectedPlayersSO.Value = m_players.Count;

        OnPlayerSpawn?.Invoke(m_players[clientID]);
        PhoneController phoneController = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.GetComponent<PhoneController>();
        phoneController.SetColor(m_players[clientID].playerColor);

        // Assign the player data to the player object
        if (m_players[clientID].playerObject.TryGetComponent<InputManager>(out InputManager playerInput))
        {
            playerInput.AssignPhoneController(phoneController);
        }
    }

    // Called when a client disconnects from the server.
    private void OnClientDisconnect(ulong clientID)
    {
        Debug.Log("Client disconnected: " + clientID);

        // Remove the player from the dictionary and destroy the player object.
        if (m_players.ContainsKey(clientID))
        {
            m_connectedPlayersSO.Value--;
            OnPlayerDisconnect?.Invoke(m_players[clientID]);
            m_players.Remove(clientID);
        }
    }

    // Called when the server is started.
    private void OnServerStarted()
    {
        Debug.Log("Server started");
    }

    // Called when the server is stopped.
    private void OnServerStopped(bool wasHost)
    {
        Debug.Log("Server stopped");
    }

    // Called to disconnect a player
    public void DisconnectPlayer(ulong clientID)
    {
        NetworkManager.Singleton.DisconnectClient(clientID);
        if (m_players.ContainsKey(clientID))
        {
            OnPlayerDisconnect?.Invoke(m_players[clientID]);
            Debug.Log($"Client {clientID} has been disconnected.");
        }
    }
}
