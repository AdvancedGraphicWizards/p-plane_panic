using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RingSpawnScript : MonoBehaviour
{

    [SerializeField] private GameObject _hoopPrefab;
    [SerializeField] private GameObject _superHoopPrefab;

    [SerializeField] private float _distanceBetweenRings = 100f;
    [SerializeField] private float _increaseDistanceFactor = 10f;
    [SerializeField] private float _increaseOffsetFactor = 1f;
    [SerializeField] private float _ringOffsetRange = 10f;
    [SerializeField] private float _firstRingDistance = 500f;
    [SerializeField] private float _baseHeight = 10f;
    [SerializeField] private float _baseHeightIncreaseFactor = 10f;

    // Random list of integers [-1,1] used for spawning
    private float[] _randomSeed = new float[200];
    private GameObject[] _hoopArray = new GameObject[50];
    private GameObject[] _superHoopArray = new GameObject[50];
    private float _zValue = 50f;


    private void Start()
    {
        InstantiateRings();
    }

    // TODO: Add an object pooling system to place additional rings ahead of the player
    // Instantiate initial rings present
    private void InstantiateRings(){
        _zValue = _firstRingDistance;

        for (int i = 0; i < 200; i++)
        {
            _randomSeed[i] = Random.value * 2 - 1;
        }

        for (int i = 0; i < 50; i++) {
            _hoopArray[i] = Instantiate(_hoopPrefab, new Vector3(_randomSeed[i]*_ringOffsetRange, _randomSeed[i+1]*(_ringOffsetRange/2f) + _baseHeight, _zValue), Quaternion.identity);
            _superHoopArray[i] = Instantiate(_superHoopPrefab, new Vector3(_randomSeed[100-i]*_ringOffsetRange*2, _randomSeed[100-i+1]*_ringOffsetRange*2 + _baseHeight*2, _zValue*3 - 20), Quaternion.identity);
            
            _ringOffsetRange += _increaseOffsetFactor;
            _zValue += _distanceBetweenRings;
            _distanceBetweenRings += _increaseDistanceFactor;
            _baseHeight += _baseHeightIncreaseFactor;
        }
    }
}
