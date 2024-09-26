using System;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : Singleton<ClientUI>
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField m_nameInputField;
    [SerializeField] private TMP_InputField m_gameCodeInputField;
    //[SerializeField] private TMP_Text m_joinButtonText;
    [SerializeField] private GameObject m_joinScreen;
    [SerializeField] private GameObject m_playScreen;
    [Tooltip("UI Element that shows the player color, can be an image or a the background.")]
    [SerializeField] private Image m_playerColorObject;
    [SerializeField] private TMP_Text m_playerNameText;
    [SerializeField] private TMP_Text m_playerNameBackdrop;

    [Header("Connection Variables")]
    public string playerName = "Player";
    [SerializeField] private Color m_playerColor = new Color(0.5235849f, 1.0f, 0.6483989f);
    [SerializeField] private bool m_isConnected = false;

    [DllImport("__Internal")]
    private static extern void RequestWakeLock();
    private bool m_requestedWakeLock = false;

    private void Awake()
    {
        if (m_joinScreen != null)
        {
            m_joinScreen.SetActive(true);
        }

        else Debug.LogError("JoinScreen not assigned.");

        if (m_playScreen != null)
        {
            m_playScreen.SetActive(false);
        }

        else Debug.LogError("PlayScreen not assigned.");

        if (m_playerColorObject != null)
        {
            m_playerColor = m_playerColorObject.color;
        }

        else Debug.LogError("Player Color object not assigned.");
    }

    private void Start()
    {
        if (SystemInfo.supportsGyroscope) Input.gyro.enabled = true;
    }

    public void JoinGame()
    {
        Debug.Log("Joining game");

        string name = m_nameInputField.text;
        string code = m_gameCodeInputField.text;

        if (name == "")
        {
            Debug.Log("Name cannot be empty!");
            return;
        }

        if (code == "")
        {
            Debug.Log("Game code cannot be empty!");
            return;
        }

        if (name.Length > 12)
        {
            Debug.Log("Name must be 12 characters or less!");
            return;
        }

        Debug.Log($"Attempting to join game with code {code} as {name}");

        //phoneController.SetName(name);
        m_playerNameText.text = m_playerNameBackdrop.text = playerName = name;
        Connect(code);
    }

    public void IsConnected(bool connectionStatus)
    {
        if (connectionStatus != m_isConnected)
        {
            m_isConnected = connectionStatus;
            m_playScreen.SetActive(connectionStatus);
            m_joinScreen.SetActive(!connectionStatus);

            // Do something

            if (m_isConnected)
            {
                // Do something
            }

            else
            {
                // Do something
            }
        }

        // Else same status we had previously, do nothing.
    }

    public void ChangeColor(Color newColor)
    {
        m_playerColor = newColor;
        m_playerColorObject.material.SetColor("_BaseColor", m_playerColor);
    }

    public void Connect(string joinCode)
    {
        if (!m_requestedWakeLock)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        RequestWakeLock();
#endif
            m_requestedWakeLock = true;
        }

        Debug.Log("Joining relay server with join code: " + joinCode);
        RelayManager.JoinRelay(joinCode);
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Disconnecting from server...");
            NetworkManager.Singleton.Shutdown();
            IsConnected(false);
        }
    }
}
