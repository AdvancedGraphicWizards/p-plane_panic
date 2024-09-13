using TMPro;
using UnityEngine;

public class DistanceCounterController : MonoBehaviour
{
    [Header("Scriptable Object References")]
    [SerializeField] private GameSettings _gameSettings;
    [Tooltip("Scriptable Object of type UIntVariable, named FlightDistance")]
    [SerializeField] private UIntVariable m_flightDistance; // so_ stands for Scriptable Object
    [SerializeField] private TMP_Text m_distanceCounter;
    
    [Tooltip("The speed m/s of the plane, this probably should be an Scriptable Object that hold all the player gameplay settings")]
    [SerializeField] private float m_flyingSpeed = 50.0f;
    private float m_distance = 0.0f;

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

        m_flyingSpeed = _gameSettings.PlaneBaseSpeed;

    }
    private void Update()
    {
        CountDistance();
    }

    private void CountDistance()
    {
        m_distance += m_flyingSpeed * Time.deltaTime;

        m_flightDistance.Value = (uint)m_distance;
        m_distanceCounter.text = m_flightDistance.Value.ToString() + " m";
    }
}
