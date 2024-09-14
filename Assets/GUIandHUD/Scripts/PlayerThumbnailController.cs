using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerThumbnailController : MonoBehaviour
{
    [Header("Internal Members")]
    //[SerializeField] private RenderTexture m_playerRenderTexture;
    [SerializeField] private TMP_Text m_playerName;
    [SerializeField] private bool m_isAssigned;
    private PlayerData m_playerData;

    private void Awake()
    {
        m_playerName.enabled = false;
    }

    public void AssignPlayer(PlayerData playerData)
    {
        // Change this when we actually send the player name
        m_playerName.text = "Player " + playerData.clientID;
        m_playerName.enabled = true;
        m_isAssigned = true;
        m_playerData = playerData;
    }

    public void UnassignPlayer()
    {
        m_playerName.text = "";
        m_playerName.enabled = false;
        m_isAssigned = false;
    }

    public bool IsAssigned()
    {
        return m_isAssigned;
    }

    public bool CheckClientID(ulong clientID)
    {
        if (!m_isAssigned) return false;

        if (m_playerData.clientID == clientID) return true;

        return false;
    }
}
