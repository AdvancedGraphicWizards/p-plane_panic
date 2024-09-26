using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the spawning of Fire Prefabs
/// </summary>

public class FireSpawner : MonoBehaviour
{
    [Tooltip("Holds the state of runtime varibales")]
    [SerializeField] private GameState gameStateSO;

    [Tooltip("Fire Prefab to be spawned")]

    [SerializeField] private GameObject _firePrefab;
    [Tooltip("Parent that fires will be spawned under (defaults to attached gameObject)")]
    [SerializeField] private Transform _targetParent;
    [SerializeField] private Vector2 m_fireSpawnBounds;


    [Header("Fire Spawning Attributes")]

    [SerializeField] private float _timeToFirstFire;
    [SerializeField] private float _timeBetweenFireSpawns;
    [SerializeField] private float _timeBetweenSpawnDecrease = 1f;
    [SerializeField] private float _minTimeBetweenFireSpawns = 5f;
    [Tooltip("Maximum Fires that can occur at once")]
    [SerializeField] private int _maxFires;


    [Header("Fire Clearing Attributes")]
    
    [Tooltip("Time taken for fire to cause damage")]
    [SerializeField] private float _fireDamageSpeed;
    [Tooltip("Time taken to clear fire if required number of people are at it")]
    [SerializeField] private float _fireClearSpeed;


    private int _numActiveFires;
    private Vector3 _spawnLocation;
    private float _timeToFire;
    public bool canSpawnFires = false;


    private void Awake()
    {
        if (_targetParent == null) _targetParent = gameObject.transform;
        _timeToFire = _timeToFirstFire;
    }


    private void Start()
    {
        FireComponent.FireDamageEvent += amt=> DespawnFire();
    }

    private void Update()
    {
        if (!gameStateSO.HasStarted) return;
        if (!canSpawnFires) return;
        if (_numActiveFires >= _maxFires) return;

        if (_timeToFire <= 0) {
            SpawnFire();

            // Reduce time between fire spawns
            if (_timeBetweenFireSpawns >= _minTimeBetweenFireSpawns) {
                _timeBetweenFireSpawns -= _timeBetweenSpawnDecrease;
            }
            else {
                _timeBetweenFireSpawns = _minTimeBetweenFireSpawns;
            }
            
            _timeToFire = _timeBetweenFireSpawns;

        }

        _timeToFire -= Time.deltaTime;
    }


    // Spawns a fire range in the specified BoxCollider
    private void SpawnFire() {
        _spawnLocation = new Vector3(
            Random.Range(-m_fireSpawnBounds.x, m_fireSpawnBounds.x), 
            0,
            Random.Range(-m_fireSpawnBounds.y, m_fireSpawnBounds.y));

        GameObject fire = Instantiate(_firePrefab, Vector3.zero, _targetParent.rotation);
        fire.transform.SetParent(_targetParent);
        fire.transform.localPosition = _spawnLocation;
        _numActiveFires++;
    }

    private void DespawnFire() {
        _numActiveFires--;
    }
    
    void OnDestroy()
    {
        FireComponent.FireDamageEvent -= amt=> DespawnFire();
    }
}
