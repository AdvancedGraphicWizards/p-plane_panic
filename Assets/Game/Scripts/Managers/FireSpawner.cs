using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpawner : MonoBehaviour
{
    [Tooltip("Holds the state of runtime varibales")]
    [SerializeField] private GameState gameStateSO;

    [Tooltip("Fire Prefab to be spawned")]

    [SerializeField] private GameObject _firePrefab;
    [Tooltip("Parent that fires will be spawned under (defaults to attached gameObject)")]
    [SerializeField] private Transform _targetParent;
    [Tooltip("BoxCollider that determines potential spawn locations (defaults to attached gameObject)")]
    [SerializeField] private BoxCollider _fireSpawnZone;


    [Header("Fire Spawning Attributes")]

    [SerializeField] private float _timeToFirstFire;
    [SerializeField] private float _timeBetweenFireSpawns;
    [Tooltip("Maximum Fires that can occur at once")]
    [SerializeField] private int _maxFires;


    [Header("Fire Clearing Attributes")]
    
    [Tooltip("Time taken for fire to cause damage")]
    [SerializeField] private float _fireDamageSpeed;
    [Tooltip("Time taken to clear fire if required number of people are at it")]
    [SerializeField] private float _fireClearSpeed;


    private GameObject[] _fireArray;
    private int _numActiveFires;
    private Vector3 _spawnLocation;
    private float _timeToFire;
    
    public bool canSpawnFires = false;


    private void Awake()
    {
        if (_fireSpawnZone == null) _fireSpawnZone = GetComponent<BoxCollider>();
        if (_targetParent == null) _targetParent = gameObject.transform;
        _fireArray = new GameObject[_maxFires];
        _timeToFire = _timeToFirstFire;
    }

    private void Update()
    {
        if (!gameStateSO.HasStarted) return;
        if (!canSpawnFires) return;
        if (_numActiveFires >= _maxFires) return;

        if (_timeToFire <= 0) {
            SpawnFire();

            _timeToFire = _timeBetweenFireSpawns;
        }

        _timeToFire -= Time.deltaTime;
    }


    // Spawns a fire range in the specified BoxCollider
    private void SpawnFire() {
        _spawnLocation = new Vector3(
            Random.Range(-_fireSpawnZone.size.x, _fireSpawnZone.size.x), 
            0,
            Random.Range(-_fireSpawnZone.size.z, _fireSpawnZone.size.z));

        _fireArray[_numActiveFires] = Instantiate(_firePrefab, Vector3.zero, _targetParent.rotation);
        _fireArray[_numActiveFires].transform.SetParent(_targetParent);
        _fireArray[_numActiveFires].transform.localPosition = _spawnLocation;
        _numActiveFires++;
    }
}
