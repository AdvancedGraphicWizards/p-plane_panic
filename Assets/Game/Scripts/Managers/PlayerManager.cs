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
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _targetParent;
    [SerializeField] private Color[] _playerColors;
    [SerializeField] private int _maxPlayerCount;

    private List<GameObject> _playerCharacters = new List<GameObject>();

    private void Awake()
    {
        ServerManager.OnPlayerSpawn += playerData => SpawnPlayer(playerData);
        ServerManager.OnPlayerDisconnect += playerData => DespawnPlayer(playerData.playerObject);
    }

    private void Start()
    {
        //SpawnPlayers();

        //SetPlayerColor();
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

    // currently hardcoded (and not working)
    private void SetPlayerColor(PlayerData playerData)
    {
        int colorIndex = (int)((playerData.clientID - 1) % (ulong)_playerColors.Length);
        if (playerData.playerObject.TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material.SetColor("_BaseColor", _playerColors[colorIndex]);
        }
    }
}
