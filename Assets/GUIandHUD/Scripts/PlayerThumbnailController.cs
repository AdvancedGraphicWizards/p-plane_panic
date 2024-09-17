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
    private PhoneController m_phoneController;
    private Coroutine updateNameCoroutine;

    private void Awake()
    {
        m_playerName.enabled = false;
    }

    public void AssignPlayer(PlayerData playerData)
    {
        m_isAssigned = true;
        m_playerData = playerData;
        m_phoneController = m_playerData.networkObject.GetComponent<PhoneController>();

        // Start updating the player's name every second
        if (updateNameCoroutine == null)
        {
            updateNameCoroutine = StartCoroutine(UpdatePlayerNameRoutine());
        }
    }

    public void UnassignPlayer()
    {
        // Stop updating the player's name
        if (updateNameCoroutine != null)
        {
            StopCoroutine(updateNameCoroutine);
            updateNameCoroutine = null;
        }

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


    private void UpdatePlayerName()
    {
        if (m_phoneController != null)
        {
            m_playerData.playerName = m_phoneController.GetName();
            m_playerData.playerObject.GetComponentInChildren<TMP_Text>().text = m_playerData.playerName;
            m_playerName.text = m_playerData.playerName;
            m_playerName.enabled = true;
        }
    }

    private IEnumerator UpdatePlayerNameRoutine()
    {
        while (m_isAssigned)
        {
            UpdatePlayerName();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
