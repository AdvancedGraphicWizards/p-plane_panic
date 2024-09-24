using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Rellac.Audio;
using TMPro;
using UnityEditor.Networking.PlayerConnection;
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
    [SerializeField] private IntVariable m_connectedPlayers;
    [SerializeField] private SoundManager m_SoundManager;
    

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
        _requiredPlayersLabel.text = _extinguishingPlayers + " / " + _playersToPutOut[m_connectedPlayers.Value];
        _timeToBurn.text = Mathf.Round(_fireDamageTimer).ToString();
    }

    private void FireDamage() {
        Debug.Log("FireDamage!");
        m_SoundManager.PlayOneShotRandomPitch("fireDamage",0.05f);
        FireDamageEvent?.Invoke(_fireFuelDamage);
        Destroy(gameObject);
    }

    private void FireExtinguish() {
        Debug.Log("FireExtinguish!");
        m_SoundManager.PlayOneShotRandomPitch("fireExtinguish",0.05f);
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
        if (_extinguishingPlayers >= _playersToPutOut[m_connectedPlayers.Value]){
            _extinguishing = true;
            _fireRenderer.material.color = Color.green;
        }
        else {
            _extinguishing = false;
            _fireRenderer.material.color = Color.red;
        }
    }
}
