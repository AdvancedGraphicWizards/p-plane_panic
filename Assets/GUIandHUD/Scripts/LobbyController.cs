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
    [SerializeField] private IntVariable m_connectedPlayersSO;

    private void Awake()
    {
        ServerManager.OnPlayerSpawn += playerData => AssignPlayer(playerData);
        ServerManager.OnPlayerDisconnect += playerData => UnassignPlayer(playerData);
        ServerManager.OnGameCode += gameCode => DisplayGameCode(gameCode);
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

    private void UpdatePlayerCounter() {
        if (!m_connectedPlayersSO) {
            throw new NullReferenceException("Missing connected players, HelloWorld purposes?");
        }

        m_playerCounter.text = m_connectedPlayersSO.Value + " / 9";
    }
}
