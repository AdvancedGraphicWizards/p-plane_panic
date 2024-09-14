using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using System;
using Unity.VisualScripting;


public class HoopScript : MonoBehaviour
{
    [SerializeField] private string _playerTag;
    [SerializeField] private float _deactivationTimer;
    [SerializeField] private float _fuelRecoverAmount;

    private Animation _ringCollectAnim;

    public static event Action<float> OnRingEnter;

    void Awake()
    {
        _ringCollectAnim = GetComponent<Animation>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("entered");
        if (other.gameObject.CompareTag(_playerTag)){
            Debug.Log("gaming");
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

