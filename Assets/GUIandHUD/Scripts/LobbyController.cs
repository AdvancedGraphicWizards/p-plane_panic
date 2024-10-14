using System;
using TMPro;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    [Header("Internal Members")]
    //[SerializeField] private RenderTexture m_playerRenderTexture;
    //[SerializeField] private GameObject[] m_playerThumbnails;
    [SerializeField] private TMP_Text m_playerCounter;
    [SerializeField] private TMP_Text m_gameCode;
    [SerializeField] private Players m_playersSO;
    [Tooltip("Holds the amount of connected players. We hold it like this because we call the disconnect event before we actually remove the player.")]
    [SerializeField] private IntVariable m_connectedPlayersSO;
    [SerializeField] private GameState m_gameStateSO;
    //
    [Tooltip("Reference to the SO that keeps track of the flight records and gets updates when crashing")]
    [SerializeField] private FlightRecordsStates m_flightRecordsStates;
    [SerializeField] private UIntVariable m_flightDistance;


    private void Awake()
    {
        // Listen for existing players
        foreach (PlayerData playerData in m_playersSO.players.Values)
        {
            AssignPlayer(playerData);
        }

        // Listen for player spawn and disconnect events
        ServerManager.OnPlayerSpawn += AssignPlayer;
        ServerManager.OnPlayerDisconnect += UnassignPlayer;
    }

    private void Start()
    {
        if (GameObject.Find("Server") == null)
        {
            Debug.LogError("Setup scene not loaded, no server manager found.");
            return;
        }
        else
        {
            DisplayGameCode(GameObject.Find("Server").GetComponent<ServerManager>().GetGameCode());
        }

        if (!m_connectedPlayersSO) throw new NullReferenceException("Missing connected players scriptable object.");

        m_gameStateSO.HasStarted = false;
    }

    private void AssignPlayer(PlayerData playerData)
    {
        /*
        for (int i = 0; i < m_playerThumbnails.Length; i++)
        {
            if (m_playerThumbnails[i].TryGetComponent<PlayerThumbnailController>(out PlayerThumbnailController thumbnailController))
            {
                if (!thumbnailController.IsAssigned())
                {
                    thumbnailController.AssignPlayer(playerData);
                    UpdatePlayerCounter();
                    return;
                }
            }
        }
        */

        UpdatePlayerCounter();
    }

    private void UnassignPlayer(PlayerData playerData)
    {
        /*
        for (int i = 0; i < m_playerThumbnails.Length; i++)
        {
            if (m_playerThumbnails[i].TryGetComponent<PlayerThumbnailController>(out PlayerThumbnailController thumbnailController))
            {
                if (thumbnailController.CheckClientID(playerData.clientID))
                {
                    thumbnailController.UnassignPlayer();
                    return;
                }
            }
        }
        */

        UpdatePlayerCounter();
    }

    //Leaderboard
    public void UpdateLeaderBoard()
    {
        Debug.Log("======= Updating leaderboard!");
        m_flightRecordsStates.AddNewRecord(m_flightDistance.Value, m_playersSO.playerTeamName);
    }

    private void DisplayGameCode(string gameCode)
    {
        m_gameCode.text = gameCode;
    }

    private void UpdatePlayerCounter()
    {
        m_playerCounter.text = m_connectedPlayersSO.Value + "/9";
    }

    private void OnDestroy()
    {
        ServerManager.OnPlayerSpawn -= AssignPlayer;
        ServerManager.OnPlayerDisconnect -= UnassignPlayer;
    }
}
