using TMPro;
using UnityEngine;

public class DistanceComponent : MonoBehaviour
{
    [SerializeField] private GameState gameStateSO;
    [SerializeField] private UIntVariable m_flightDistance; // so_ stands for Scriptable Object
    private float m_distance = 0.0f;
    private Vector3 m_initialPosition = Vector3.zero;

    private void OnEnable()
    {
        if (!m_flightDistance)
        {
            throw new System.NullReferenceException("The Scriptable Object FlightDistance is missing, the DistanceCounterController needs is to keep track of the distance");
        }
        if (!gameStateSO)
        {
            throw new System.NullReferenceException("Missing GameState, HelloWorld purposes");
        }
        m_initialPosition = transform.position;
    }
    private void Update()
    {
        CountDistance();
    }

    private void CountDistance()
    {
        // Fair to assume the distance is only in one direction, saves us from taking the magnitude (expensive)
        m_distance = gameStateSO.HasStarted ? Mathf.Abs((transform.position - m_initialPosition).z) : 0.0f;
        m_flightDistance.Value = (uint)m_distance;
    }
}
