using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calculates the total weight from a number of WeightComponent GameObjects
/// </summary>

public class WeightManager : MonoBehaviour
{
    [SerializeField] private List<WeightComponent> _weightedObjects;
    [SerializeField] private int _numWeightedObjects;
    [SerializeField] private Players m_playersSO;

    public float TotalRollWeight { get; private set; }
    public float TotalPitchWeight { get; private set; }

    private void Awake()
    {   
        // Listen for existing players
        foreach (PlayerData playerData in m_playersSO.players.Values)
        {
            TrackWeightObject(playerData.playerObject);
        }

        // Listen for player spawn and disconnect events
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
