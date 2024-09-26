using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Rellac.Audio;


/// <summary>
/// Component that controls the behaviour of the Hoop Prefab
/// Gives the plane fuel upon contact, plays an animation and sound effect
/// </summary>

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Animation))]
public class HoopScript : MonoBehaviour
{
    [Header("Variables")]
    [Tooltip("Amount of fuel gained")]
    [SerializeField] private float _fuelRecoverAmount;
    [Tooltip("Time until the gameObject becomes inactive after collision")]
    [SerializeField] private float _deactivationTimer;
    [Tooltip("Tag used to identify player colliders")]
    [SerializeField] private string _playerTag;

    [Header("Component References")]
    [SerializeField] private SoundManager m_soundManager;

    private Animation _ringCollectAnim;

    public static event Action<float> OnRingEnter;

    void Awake()
    {
        _ringCollectAnim = GetComponent<Animation>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_playerTag)){
            m_soundManager.PlayOneShotRandomPitch("ringPickup",0.1f);
            OnRingEnter?.Invoke(_fuelRecoverAmount);
            StartCoroutine(Deactivate());
        }
    }

    IEnumerator Deactivate() {
        _ringCollectAnim.Play();
        yield return new WaitForSeconds(_deactivationTimer);
        gameObject.SetActive(false);
    }
}

