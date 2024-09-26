using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Rellac.Audio;
using TMPro;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component that controls the behaviour of the Fire prefab
/// Deals damage unless the required amount of players get in its vicinity
/// </summary>

[RequireComponent(typeof(Collider))]
public class FireComponent : MonoBehaviour
{
    [Header("Fire Attribute Variables")]
    [Tooltip("Required Number of Players to put out a fire for each player count")]
    [SerializeField] private int[] _playersToPutOut;
    [Tooltip("Time to burn and deal damage to the plane")]
    [SerializeField] private float _timeToDamage = 10f;
    [Tooltip("Time to extinguish fire with correct number of players")]
    [SerializeField] private float _timeToExtinguish = 1f;
    [Tooltip("Damage dealt to fuel when burntimer runs out")]
    [SerializeField] private float _fireFuelDamage = -10;

    [Header("Component References")]
    [Tooltip("Tag used to identify player colliders")]
    [SerializeField] private string _playerTag = "Player";
    [Tooltip("Label showing required players")]
    [SerializeField] private TMP_Text _requiredPlayersLabel;
    [Tooltip("Label showing time left until damage")]
    [SerializeField] private TMP_Text _timeToBurn;
    [Tooltip("Mesh colour changed when extinguishing")]
    [SerializeField] private MeshRenderer _fireRenderer;
    [SerializeField] private SoundManager m_SoundManager;
    [SerializeField] private IntVariable m_connectedPlayers;

    private float _fireDamageTimer;
    private float _fireExtinguishTimer;
    private bool _extinguishing = false;
    private int _extinguishingPlayers = 0;

    public static event Action<float> FireDamageEvent;

    private void Awake()
    {
        _fireDamageTimer = _timeToDamage;
        _fireExtinguishTimer = _timeToExtinguish;
    }

    void Update()
    {
        UpdateText();
    
        // --> Perhaps could halt timer if at least 1 player is on the fire

        if (!_extinguishing) 
        {
            // No players, decrease damageTimer
            _fireDamageTimer -= Time.deltaTime;

            if (_fireDamageTimer <= 0) {
                FireDamage();
            }
        }
        else {
            // Enough players, begin extinguishing
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

    // On unsuccessful extinguish deal fuel-damage and destroy fire
    private void FireDamage() {
        Debug.Log("FireDamage!");
        m_SoundManager.PlayOneShotRandomPitch("fireDamage",0.05f);
        FireDamageEvent?.Invoke(_fireFuelDamage);
        Destroy(gameObject);
    }

    // On successful extinguish gain fuel and destroy fire
    private void FireExtinguish() {
        Debug.Log("FireExtinguish!");
        m_SoundManager.PlayOneShotRandomPitch("fireExtinguish",0.05f);
        FireDamageEvent?.Invoke(-_fireFuelDamage); // gain fuel on success? (Should use its own event)
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag)) {
            _extinguishingPlayers++;
            CheckExtinguishingStatus();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_playerTag)) {
            _extinguishingPlayers--;
            CheckExtinguishingStatus();
        }
    }

    // Check if required number of players are present
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
