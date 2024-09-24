using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;


public class TerrainCollider : MonoBehaviour
{
    [SerializeField] private string m_playerTag = "Plane";
    [SerializeField] private VoidEvent m_planeCrashEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(m_playerTag)){
            m_planeCrashEvent.Raise();
        }
    }
}

