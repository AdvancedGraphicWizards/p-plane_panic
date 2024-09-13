using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TotalFlightDistance : MonoBehaviour
{
    [SerializeField] private UIntVariable m_flightDistance; // so_ stands for Scriptable Object
    [SerializeField] private TMP_Text m_distanceCounter;

    private void Start()
    {
        m_distanceCounter.text = m_flightDistance.Value.ToString() + " m";
    }
}
