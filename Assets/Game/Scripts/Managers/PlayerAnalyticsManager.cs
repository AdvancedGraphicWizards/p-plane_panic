using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public struct PlayData 
{ 
    public DateTime timeStamp;
    public int numberOfPlayers;
    public DateTime endTimeStamp;
    public int ringsCollected;
    public int superRingsCollected;
    public int firesExtinguished;
    public int fireDamage;

}

[CreateAssetMenu(menuName = "p-plane_panic/PlayerAnalyticsManager")]
public class PlayerAnalyticsManager : ScriptableObject
{
    public PlayData[] playDatas = new PlayData[400];
    public int runNumber = 0;

    [SerializeField] VoidEvent onPlaneCrash;
    [SerializeField] IntVariable playerNumber;

    private void OnEnable() {
        FireComponent.FireDamageEvent += amt => UpdateFireStats(amt);
        HoopScript.OnRingEnter += amt => UpdateRingStats(amt);
    }

    private void OnDisable() {
        FireComponent.FireDamageEvent -= amt => UpdateFireStats(amt);
        HoopScript.OnRingEnter -= amt => UpdateRingStats(amt);
    }

    public void AddNewPlayData() {
        runNumber++;
        playDatas[runNumber].timeStamp = DateTime.Now;
        playDatas[runNumber].numberOfPlayers = playerNumber.Value;
        playDatas[runNumber].endTimeStamp = DateTime.Now;
        playDatas[runNumber].ringsCollected = 0;
        playDatas[runNumber].superRingsCollected = 0;
        playDatas[runNumber].firesExtinguished = 0;
        playDatas[runNumber].fireDamage = 0;
    }

    public void UpdateFireStats(float amt) {
        if (amt > 0) {
            playDatas[runNumber].firesExtinguished++;
        }
        else {
            playDatas[runNumber].fireDamage++;
        }
    }

    public void UpdateRingStats(float amt) {
        if (amt == 20) {
            playDatas[runNumber].ringsCollected++;
        }
        else if (amt == 40) {
            playDatas[runNumber].superRingsCollected++;
        }
    }

    public void UpdateEndStats() {
        playDatas[runNumber].endTimeStamp = DateTime.Now;
    }
}