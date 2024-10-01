using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Players", menuName = "ScriptableObject/Data/Players")]
public class Players : ScriptableObject
{
    // Keep track of all connected players
    public Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();
    public int playerTeamHash = 0;
    public string playerTeamName = "";

    private int GenerateTeamHash()
    {
        var playerNames = players.Values.Select(p => p.playerName).OrderBy(name => name).ToList();
        string concatenatedNames = string.Join("", playerNames);
        //Debug.Log($"Concatenated names = {concatenatedNames}");

        // Generate a SHA256 hash from the player names
        using SHA256 sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(concatenatedNames));

        return BitConverter.ToInt32(bytes, 0);
    }

    public string GenerateTeamName()
    {
        playerTeamHash = GenerateTeamHash();

        string[] adjectives = { "Fluffy", "Brave", "Cheerful", "Sneaky", "Jolly", "Clever", "Mighty", "Swift", "Legendary", "Epic", "Fearless", "Noble" };
        string[] nouns = { "Penguins", "Owls", "Crows", "Parrots", "Kakapos", "Phoenixes", "Eagles", "Kiwis" };

        string adjective = adjectives[Math.Abs(playerTeamHash) % adjectives.Length];
        string noun = nouns[Math.Abs(playerTeamHash / adjectives.Length) % nouns.Length];

        playerTeamName = $"{adjective} {noun}";
        Debug.Log($"Team name is {playerTeamName}.");
        return playerTeamName;
    }

    public void UpdatePlayerName(string newName, ulong clientID)
    {
        if (players.ContainsKey(clientID))
        {
            PlayerData playerData = players[clientID];

            if (newName == playerData.playerName) return;

            //Debug.Log($"Trying to rename player with ID {clientID} to {newName}");
            playerData.playerName = newName;
            players[clientID] = playerData;
            GenerateTeamName();
        }
    }
}
