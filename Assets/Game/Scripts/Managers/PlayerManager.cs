using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Handles Player character despawning and spawning behaviour
/// </summary>

public class PlayerSpawnScript : MonoBehaviour
{

    [Header("Player and Spawn Settings")]
    [SerializeField] private Transform _targetParent;
    [SerializeField] private List<GameObject> _playerCharacters;


    private void Awake()
    {
        ServerManager.OnPlayerSpawn += playerData => SpawnPlayer(playerData);
        ServerManager.OnPlayerDisconnect += playerData => DespawnPlayer(playerData.playerObject);
    }

    private void SpawnPlayer(PlayerData playerData)
    {
        _playerCharacters.Add(playerData.playerObject);

        ParentPlayer(playerData.playerObject, _targetParent);
        SetPlayerColor(playerData);
    }

    private void DespawnPlayer(GameObject playerObject)
    {
        if (_playerCharacters.Contains(playerObject))
        {
            _playerCharacters.Remove(playerObject);
            Destroy(playerObject);
        }
    }

    private void ParentPlayer(GameObject playerObject, Transform targetParent)
    {
        if (targetParent != null)
        {
            playerObject.transform.SetParent(targetParent);
            playerObject.transform.position = targetParent.position;
        }
    }

    private void SetPlayerColor(PlayerData playerData)
    {
        if (playerData.playerObject.TryGetComponent<SetColorComponent>(out SetColorComponent colorComponent))
        {
            colorComponent.SetColor(playerData.playerColor);
        }
    }
}
