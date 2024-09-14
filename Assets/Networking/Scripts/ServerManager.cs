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
}

public class ServerManager : MonoBehaviour
{

    [Header("Server Settings")]
    public GameObject playerPrefab;     // The player prefab to spawn in the scene
    public int maxPlayers = 8;          // The maximum number of players in the server

    // Server data
    private string m_joinCode;          // Code to join the relay server, defined when the relay is allocated.
    private Dictionary<ulong, PlayerData> m_players = new Dictionary<ulong, PlayerData>();

    // UnityEvents, replace with event system?
    public static event Action<GameObject> OnPlayerSpawn;
    public static event Action<GameObject> OnPlayerDisconnect;

    // Setup
    private async void Start()
    {

        // Create a new relay allocation with a maximum number of participants.
        m_joinCode = await RelayManager.CreateRelay(maxPlayers);
        Debug.Log("Join code: " + m_joinCode);

        // Set the callbacks for the network manager.
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
    }

    // Called when a client connects to the server.
    private void OnClientConnected(ulong clientID)
    {
        Debug.Log("Client connected: " + clientID);

        // Create a new player object and add it to the dictionary.
        m_players.Add(clientID, new PlayerData()
        {
            clientID = clientID,
            playerName = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.GetComponent<PhoneController>().playerName.Value.ToString(),
            playerObject = Instantiate(playerPrefab),
            networkObject = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject
        });

        OnPlayerSpawn?.Invoke(m_players[clientID].playerObject);

        // Assign the player data to the player object
        if (m_players[clientID].playerObject.TryGetComponent<InputManager>(out InputManager playerInput)) {
            playerInput.AssignPhoneController(NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.GetComponent<PhoneController>());
        }
    }

    // Called when a client disconnects from the server.
    private void OnClientDisconnect(ulong clientID)
    {
        Debug.Log("Client disconnected: " + clientID);

        // Remove the player from the dictionary and destroy the player object.
        if (m_players.ContainsKey(clientID))
        {
            OnPlayerDisconnect?.Invoke(m_players[clientID].playerObject);
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
}
