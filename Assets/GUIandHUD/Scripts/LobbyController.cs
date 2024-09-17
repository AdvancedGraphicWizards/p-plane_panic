using System;
using TMPro;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    [Header("Internal Members")]
    //[SerializeField] private RenderTexture m_playerRenderTexture;
    [SerializeField] private GameObject[] m_playerThumbnails;
    [SerializeField] private TMP_Text m_playerCounter;
    [SerializeField] private TMP_Text m_gameCode;
    [SerializeField] private Players m_playersSO;
    [SerializeField] private GameState m_gameStateSO;

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
        m_gameStateSO.HasStarted = false;
    }

    private void AssignPlayer(PlayerData playerData)
    {
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

        // If we get here something is wrong
        Debug.LogError("Lobby not full but couldn't assign player.");
    }

    private void UnassignPlayer(PlayerData playerData)
    {
        for (int i = 0; i < m_playerThumbnails.Length; i++)
        {
            if (m_playerThumbnails[i].TryGetComponent<PlayerThumbnailController>(out PlayerThumbnailController thumbnailController))
            {
                if (thumbnailController.CheckClientID(playerData.clientID))
                {
                    thumbnailController.UnassignPlayer();
                    UpdatePlayerCounter();
                    return;
                }
            }
        }

        // If we get here something is wrong
        Debug.LogError("Couldn't find player to unassign.");
    }

    private void DisplayGameCode(string gameCode)
    {
        m_gameCode.text = gameCode;
    }

    private void UpdatePlayerCounter()
    {
        m_playerCounter.text = "Players: " + m_playersSO.players.Count + "/9";
    }

    private void OnDestroy()
    {
        ServerManager.OnPlayerSpawn -= AssignPlayer;
        ServerManager.OnPlayerDisconnect -= UnassignPlayer;
    }
}
