using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnScript : MonoBehaviour
{
    [Header("Player and Spawn Settings")]
    [SerializeField] private Color[] _playerColors;
    [SerializeField] private Players m_playersSO;

    private void Start()
    {
        ServerManager.OnPlayerSpawn += SpawnPlayer;
    }

    private void SpawnPlayer(PlayerData playerData)
    {
        SetPlayerColor(playerData);
    }

    // currently hardcoded (and not working)
    private void SetPlayerColor(PlayerData playerData)
    {
        int colorIndex = (int)((playerData.clientID - 1) % (ulong)_playerColors.Length);
        if (playerData.playerObject.TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material.SetColor("_BaseColor", _playerColors[colorIndex]);
        }
    }

    void OnDestroy()
    {
        ServerManager.OnPlayerSpawn -= SpawnPlayer;
    }
}
