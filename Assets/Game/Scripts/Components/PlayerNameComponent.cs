using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameComponent : MonoBehaviour
{
    [Header("Player and Spawn Settings")]
    [SerializeField] private TMP_Text m_playerNameText;
    [SerializeField] private string m_playerName = "Player";
    [Header("Locally Scoped Members")]
    [SerializeField] private Players m_playersSO;
    [SerializeField] private ulong m_playerID;
    [SerializeField] private PhoneController m_phoneController;


    private void Awake()
    {
        ServerManager.OnPlayerName += name => SetPlayerName();

        if (m_playerNameText != null) m_playerNameText.text = m_playerName;
        else Debug.LogError ("Player Name Text object not assigned");
    }

    private void SetPlayerName() {
        m_playerNameText.text = m_playerName = m_phoneController.GetName();
        m_playersSO.UpdatePlayerName(m_playerName, m_playerID);
    }

    private void OnDestroy()
    {
        ServerManager.OnPlayerName -= name => SetPlayerName();
    }

    public void AssignPlayerData(Players players_SO, ulong playerID)
    {
        m_playersSO = players_SO;
        m_playerID = playerID;
    }

    public void AssignPhoneController(PhoneController phoneController)
    {
        m_phoneController = phoneController;
    }
}
