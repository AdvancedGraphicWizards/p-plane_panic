using System;
using System.Collections;
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
    [SerializeField] private TMP_Text m_debugPingText;
    [SerializeField] private TMP_Text m_debugInputText;
    [SerializeField] private TMP_Text m_debugWakeLockText;

    [Header("Connection Variables")]
    public string playerName = "Player";
    [SerializeField] private Color m_playerColor = new Color(0.5235849f, 1.0f, 0.6483989f);
    [SerializeField] private bool m_isConnected = false;
    [SerializeField] private bool m_canConnect = true;

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
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            m_debugInputText.text = $"Input: Gyro";
        }

        //UNUSED
        m_debugWakeLockText.text = "";
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

        // Set canConnect to false and start the coroutine to reset it
        if (m_canConnect)
        {
            m_canConnect = false;
            m_playerNameText.text = playerName = name;
            StartCoroutine(ResetCanConnect());
            Connect(code);
        }
        else
        {
            Debug.Log("Already attempting to connect, please wait.");
        }
    }

    private IEnumerator ResetCanConnect()
    {
        // Wait for 3 seconds or connection success
        float elapsedTime = 0f;
        while (elapsedTime < 3f && !m_isConnected)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        m_canConnect = true;
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
                m_canConnect = true; // Ensure we can connect again if disconnected later
                StopCoroutine(ResetCanConnect());
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

    public void UpdatePing(float newPing)
    {
        m_debugPingText.text = $"Ping: {newPing} ms";
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
