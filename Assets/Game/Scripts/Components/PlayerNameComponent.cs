using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameComponent : MonoBehaviour
{
    [Header("Player and Spawn Settings")]
    [SerializeField] private TMP_Text m_playerNameText;
    [SerializeField] private string m_playerName = "Player";
    [Header("Locally Scoped Members")]
    [SerializeField] private PlayerData m_playerData;
    [SerializeField] private PhoneController m_phoneController;


    private void Awake()
    {
        ServerManager.OnPlayerName += name => SetPlayerName();

        if (m_playerNameText != null) m_playerNameText.text = m_playerName;
        else Debug.LogError ("Player Name Text object not assigned");
    }

    private void SetPlayerName() {
        m_playerNameText.text = m_playerName = m_playerData.playerName = m_phoneController.GetName();
    }

    private void OnDestroy()
    {
        ServerManager.OnPlayerName -= name => SetPlayerName();
    }

    public void AssignPlayerData(PlayerData playerData)
    {
        m_playerData = playerData;
    }

    public void AssignPhoneController(PhoneController phoneController)
    {
        m_phoneController = phoneController;
    }
}
