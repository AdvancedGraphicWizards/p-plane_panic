using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : Singleton<ClientUI>
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField m_nameInputField;
    [SerializeField] private TMP_InputField m_gameCodeInputField;
    [SerializeField] private TMP_Text m_joinButtonText;
    [SerializeField] private GameObject m_joinScreen;
    [SerializeField] private GameObject m_playScreen;
    [Tooltip("UI Element that shows the player color, can be an image or a the background.")]
    [SerializeField] private Image m_playerColorObject;

    // Private members
    [Header("Phone Controller Reference")]
    public PhoneController phoneController;

    [Header("UI Elements")]
    public string playerName = "Player";
    [SerializeField] private Color m_playerColor = new Color(0.5235849f, 1.0f, 0.6483989f);
    [SerializeField] private bool isConnected = false;

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
        // Nothing for now
    }

    public void JoinGame()
    {
        if (!(m_joinButtonText.text == "Connecting"))
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
            playerName = name;
            Connect(code);

            m_joinButtonText.text = "Connecting";
        }

        else
        {
            Disconnect();
            m_joinButtonText.text = "Connect";
        }
    }

    public void IsConnected(bool connectionStatus)
    {
        if (connectionStatus != isConnected)
        {
            isConnected = connectionStatus;
            m_playScreen.SetActive(connectionStatus);
            m_joinScreen.SetActive(!connectionStatus);

            // Do something

            if (isConnected)
            {
                m_joinButtonText.text = "Connect";
            }

            else
            {
                // Do something
            }
        }

        // Else same status we had previously, do nothing.
    }
    
    public void ChangeColor(Color newColor) {
        m_playerColor = newColor;
        m_playerColorObject.color = m_playerColor;
        
    }

    public void Connect(string joinCode)
    {
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
