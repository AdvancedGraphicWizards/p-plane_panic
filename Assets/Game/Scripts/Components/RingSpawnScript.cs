using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Handles Spawning of Rings/Hoops in the game scene
/// Uses object pooling to spawn scripts in front of the players
/// </summary>

public class RingSpawnScript : MonoBehaviour
{

    [Header("Component References")]
    [SerializeField] private GameObject _hoopPrefab;
    [SerializeField] private GameObject _superHoopPrefab;
    [SerializeField] private Transform m_planeLocation;

    [Header("Spawning Variables")]
    [SerializeField] private float _distanceBetweenRings = 100f;
    [SerializeField] private float _maxDistanceBetweenRings = 400f;
    [SerializeField] private float _increaseDistanceFactor = 10f;
    [SerializeField] private float _increaseOffsetFactor = 1.5f;
    [SerializeField] private float _ringOffsetRange = 10f;
    [SerializeField] private float _firstRingDistance = 500f;
    [SerializeField] private float _baseHeight = 10f;
    [SerializeField] private float _baseHeightIncreaseFactor = 2f;

    [SerializeField] private float _period =2500f;
    [SerializeField] private float _amplitude = 80f;
    [SerializeField] private float _periodOffset = 0;


    [Header("Object Pooling Variables")]
    [SerializeField] private int m_backCullingDistance = 20;
    [SerializeField] private int m_ringBatchingAmount = 50;


    // Random list of integers [-1,1] used for spawning
    private float[] _randomSeed = new float[200];
    private GameObject[] _hoopArray = new GameObject[100];
    private float _zValue = 50f;
    private int m_currentBackPosition = 0;
    private int m_seedNum = 0;



    private void Start()
    {
        InstantiateRings();

        // Find and assign Plane Object
        if (m_planeLocation == null) {
            if (GameObject.Find("Plane") != null) {
                m_planeLocation = GameObject.Find("Plane").transform;
            }
        }
    }


    void FixedUpdate()
    {
        // Regular hoops
        if (m_planeLocation.position.z - m_backCullingDistance > _hoopArray[m_currentBackPosition].transform.position.z) {

            
            
            //reactivate ring and change position
            // Note currently does not change to reflect if super-rings are used or not
            _hoopArray[m_currentBackPosition].SetActive(true);
            _hoopArray[m_currentBackPosition].transform.position = new Vector3(_randomSeed[m_seedNum]*_ringOffsetRange + WaveOffset(_zValue), _randomSeed[m_seedNum + 1]*(_ringOffsetRange/2f) + _baseHeight, _zValue);


            _ringOffsetRange += _increaseOffsetFactor;
            _zValue += _distanceBetweenRings;

            if (_distanceBetweenRings < _maxDistanceBetweenRings)
                _distanceBetweenRings += _increaseDistanceFactor;

            _baseHeight += _baseHeightIncreaseFactor;

            // update next ring to pool
            m_currentBackPosition++;
            m_currentBackPosition = m_currentBackPosition % m_ringBatchingAmount;
        }
        
    }
    
    

    // Instantiate initial rings present
    private void InstantiateRings(){
        _zValue = _firstRingDistance;

        for (int i = 0; i < 200; i++)
        {
            _randomSeed[i] = Random.value * 2 - 1;
        }

        for (int i = 0; i < 100; i++) {
            if (i % 5 == 0){ 
                // every fifth ring is a super-hoop
                _hoopArray[i] = Instantiate(
                    _superHoopPrefab, 
                    new Vector3(_randomSeed[100-i]*_ringOffsetRange*1.3f + WaveOffset(_zValue), _randomSeed[100-i+1]*_ringOffsetRange*1.3f + _baseHeight, _zValue + _distanceBetweenRings/2), 
                    Quaternion.identity);
            }
            else {
                // spawn a regular hoop
                _hoopArray[i] = Instantiate(
                    _hoopPrefab, 
                    new Vector3(_randomSeed[i]*_ringOffsetRange + WaveOffset(_zValue), _randomSeed[i+1]*(_ringOffsetRange/2f) + _baseHeight, _zValue), 
                    Quaternion.identity);


                // Update offsetvalues
                _ringOffsetRange += _increaseOffsetFactor;
                _zValue += _distanceBetweenRings;
                _distanceBetweenRings += _increaseDistanceFactor;
                _baseHeight += _baseHeightIncreaseFactor;
            }
            
            _hoopArray[i].transform.SetParent(transform);
        }
        m_seedNum = 100;
    }

    private float WaveOffset(float x) {
        return Mathf.Sin(x*2*Mathf.PI/_period + _periodOffset)*_amplitude;
    }
}
