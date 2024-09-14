using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightManager : MonoBehaviour
{
    [Header("Read-Only")]
    [SerializeField] private List<WeightComponent> _weightedObjects;
    [SerializeField] private int _numWeightedObjects;

    public float TotalRollWeight { get; private set; }
    public float TotalPitchWeight { get; private set; }

    private void Awake()
    {
        ServerManager.OnPlayerSpawn += playerData => TrackWeightObject(playerData.playerObject);
        ServerManager.OnPlayerDisconnect += playerData => RemoveWeightObject(playerData.playerObject);

        // update all weights on change (not sure if better than just using Update)
        WeightComponent.OnWeightUpdate += UpdateTotalWeight; 
    }

    public void TrackWeightObject(GameObject playerObject){
        _weightedObjects.Add(playerObject.GetComponent<WeightComponent>());
        _numWeightedObjects++;
    }

    public void RemoveWeightObject(GameObject playerObject){
        _weightedObjects.Remove(playerObject.GetComponent<WeightComponent>());
        _numWeightedObjects--;
    }

    private void UpdateTotalWeight(){
        TotalRollWeight = 0;
        TotalPitchWeight = 0;

        if (_numWeightedObjects == 0) return;

        // Average of tracked weight components
        foreach (WeightComponent wc in _weightedObjects) {
            TotalRollWeight += wc.RollWeight;
            TotalPitchWeight += wc.PitchWeight;
        }

        TotalRollWeight /= _numWeightedObjects;
        TotalPitchWeight /= _numWeightedObjects;
    }

}
