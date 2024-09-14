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

    [SerializeField] private int _playerCount = 4;
    [SerializeField] private int _currentPlayerColorIndex = 0;

    [SerializeField] private List<GameObject> _playerCharacters;
    private int _activePlayerCount = 0;


    private void Awake()
    {
        _playerCharacters = new List<GameObject>();
        ServerManager.OnPlayerSpawn += playerObject => SpawnPlayer(playerObject);
        ServerManager.OnPlayerDisconnect += playerObject => DespawnPlayer(playerObject);
    }

    private void Start()
    {
        //SpawnPlayers();

        //SetPlayerColor();
    }

    private void SpawnPlayer(GameObject playerObject)
    {
        if (_activePlayerCount < _maxPlayerCount)
        {
            _playerCharacters.Add(playerObject);
            _activePlayerCount++;

            ParentPlayer(playerObject, _targetParent);
            SetPlayerColor(playerObject);
        }
    }

    private void DespawnPlayer(GameObject playerObject)
    {
        if (_playerCharacters.Contains(playerObject))
        {
            _playerCharacters.Remove(playerObject);
            Destroy(playerObject);
            if (_activePlayerCount > 0) _activePlayerCount--;
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
    private void SetPlayerColor(GameObject playerObject)
    {
        playerObject.GetComponent<Renderer>().material.SetColor("_BaseColor", _playerColors[_currentPlayerColorIndex]);
        _currentPlayerColorIndex = _currentPlayerColorIndex < _playerColors.Length - 1 ? _currentPlayerColorIndex + 1 : 0;
    }
}
