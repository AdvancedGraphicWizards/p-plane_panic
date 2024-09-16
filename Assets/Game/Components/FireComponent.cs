using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireComponent : MonoBehaviour
{
    // Replace with serailizedObjectValue
    [SerializeField] private int[] _playersToPutOut;
    [SerializeField] private TMP_Text _requiredPlayersLabel;
    [SerializeField] private TMP_Text _timeToBurn;
    [SerializeField] private MeshRenderer _fireRenderer;
    [SerializeField] private String _playerTag = "Player";
    [SerializeField] private float _fireFuelDamage = -10;
    private int _activePlayers = 4; 
    

    // Should be determined by spawner!
    private float _fireDamageTimer = 10f;
    private float _fireExtinguishTimer = 2f;
    private bool _extinguishing = false;
    private int _extinguishingPlayers = 0;

    public static event Action<float> FireDamageEvent;

    void Update()
    {
        UpdateText();
        // halt timer if extinguishing
        if (!_extinguishing) 
        {
            _fireDamageTimer -= Time.deltaTime;

            if (_fireDamageTimer <= 0) {
                FireDamage();
            }
        }
        else {
            _fireExtinguishTimer -= Time.deltaTime;
            if (_fireExtinguishTimer <= 0) {
                FireExtinguish();
            }
        }
    }

    private void UpdateText(){
        _requiredPlayersLabel.text = _extinguishingPlayers + " / " + _playersToPutOut[_activePlayers];
        _timeToBurn.text = Math.Round(_fireDamageTimer,2).ToString();
    }

    private void FireDamage() {
        // cause damage-event and delete instance WIP
        Debug.Log("FireDamage!");
        FireDamageEvent?.Invoke(_fireFuelDamage);
        Destroy(gameObject);
    }

    private void FireExtinguish() {
        // cause damage-event and delete instance WIP
        Debug.Log("FireExtinguish!");
        FireDamageEvent?.Invoke(-_fireFuelDamage); // gain fuel on success? (Should use its own event)
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == _playerTag) {
            _extinguishingPlayers++;
            CheckExtinguishingStatus();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == _playerTag) {
            _extinguishingPlayers--;
            CheckExtinguishingStatus();
        }
    }

    private void CheckExtinguishingStatus() {
        if (_extinguishingPlayers >= _playersToPutOut[_activePlayers]){
            _extinguishing = true;
            _fireRenderer.material.color = Color.green;
        }
        else {
            _extinguishing = false;
            _fireRenderer.material.color = Color.red;
        }
    }
}
