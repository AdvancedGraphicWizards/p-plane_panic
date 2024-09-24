using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Rellac.Audio;


public class HoopScript : MonoBehaviour
{
    [SerializeField] private string _playerTag;
    [SerializeField] private float _deactivationTimer;
    [SerializeField] private float _fuelRecoverAmount;
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

