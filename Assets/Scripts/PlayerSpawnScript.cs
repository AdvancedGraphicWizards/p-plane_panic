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
    [SerializeField] private String[] _controlSchemes = new String[4] {"Keyboard_wasd", "keyboard_ijkl", "keyboard_arrows", "keyboard_numpad"};

    [SerializeField] private GameObject[] _playerCharacters;
    private int _activePlayerCount = 0;

    public static event Action<GameObject> OnPlayerSpawn;


    private void Awake()
    {
        _playerCharacters = new GameObject[_maxPlayerCount];
    }

    private void Start()
    {
        SpawnPlayers();

        if (_targetParent != null) ParentPlayers(_targetParent);

        SetPlayerColor();
    }

    // Instantiate all Players
    // Currently hardcoded to only spawn 4 player characters from a set of 4 possible keyboard controls
    private void SpawnPlayers(){
        for (int i = 0; i < _playerCount; i++)
        {
            _playerCharacters[i] = PlayerInput.Instantiate(_playerPrefab, controlScheme: _controlSchemes[i], pairWithDevice: Keyboard.current).gameObject;
            OnPlayerSpawn?.Invoke(_playerCharacters[i]);
            _activePlayerCount++;
        }
    }

    private void DespawnPlayer(){
        //TODO
    }


    private void ParentPlayers(Transform targetParent){
        for (int i = 0; i < _activePlayerCount; i++) {
            _playerCharacters[i].transform.SetParent(targetParent);
            _playerCharacters[i].transform.position = _targetParent.position;
        }
    }

    // currently hardcoded (and not working)
    private void SetPlayerColor(){
        for (int i = 0; i < _activePlayerCount; i++) {
            _playerCharacters[i].GetComponent<Renderer>().material.SetColor("_BaseColor", _playerColors[i]);
        }
    }
}
