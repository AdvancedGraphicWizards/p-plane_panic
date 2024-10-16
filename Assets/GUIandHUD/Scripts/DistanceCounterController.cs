using TMPro;
using UnityEngine;

public class DistanceCounterController : MonoBehaviour
{
    [Tooltip("Scriptable Object of type UIntVariable, named FlightDistance")]
    [SerializeField] private UIntVariable m_flightDistance; // so_ stands for Scriptable Object
    [SerializeField] private TMP_Text m_distanceCounter;

    private void OnEnable()
    {
        if (!m_distanceCounter)
        {
            throw new System.NullReferenceException("TextMeshPro reference is missing, the DistanceCounterController needs a TextMeshPro component to display the counter");
        }
        if (!m_flightDistance)
        {
            throw new System.NullReferenceException("The Scriptable Object FlightDistance is missing, the DistanceCounterController needs is to keep track of the distance");
        }
    }
    private void Update()
    {
        CountDistance();
    }

    private void CountDistance()
    {
        m_distanceCounter.text = m_flightDistance.Value.ToString() + " m";
    }
}
