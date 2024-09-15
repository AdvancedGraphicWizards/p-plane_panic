using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : Singleton<ClientUI>
{
    [Header("UI Elements")]
    public TMP_InputField nameInputField;
    public TMP_InputField gameCodeInputField;
    public TMP_Text joinButtonText;

    // Private members
    [Header("Phone Controller Reference")]
    public PhoneController phoneController;

    [Header("UI Elements")]
    public string playerName = "Player";
    public Color playerColor = new Color(0.5235849f, 1.0f, 0.6483989f);

    private void Awake()
    {
        if (phoneController == null)
        {
            Debug.LogError("No PhoneController found on the GameObject.");
        }
    }

    private void Start()
    {
        // Nothing for now
    }

    public void JoinGame()
    {
        Debug.Log("Joining game");

        string name = nameInputField.text;
        string code = gameCodeInputField.text;

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

        phoneController.SetName(name);
        playerName = name;
        phoneController.Connect(code);

        joinButtonText.text = "Connecting";
    }
}
