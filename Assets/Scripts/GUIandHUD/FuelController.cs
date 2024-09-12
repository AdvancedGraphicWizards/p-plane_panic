using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FuelController : MonoBehaviour
{
    [SerializeField] private Slider m_slider;
    [Tooltip("Difine how much fuel does the plane consume. Better if it is a Scriptable Object")]
    [SerializeField] private float m_consumptionRate = 5.0f;
    [Tooltip("The starting fuel level. Better if it is a Scriptable Object")]
    [SerializeField] private float m_startingFuel = 100.0f;
    private float m_fuel = 0;

    [Tooltip("Scriptable Object of type UIntVariable, named Fuel")]
    [SerializeField] private UIntVariable m_currentFuel;

    private void OnEnable()
    {
        if (!m_slider)
        {
            throw new System.NullReferenceException("m_slider reference is missing, the CountDownController needs a m_slider component to display the counter");
        }
        if (!m_currentFuel)
        {
            throw new System.NullReferenceException("The Scriptable Object CurrentFuel is missing, the DistanceCounterController needs is to keep track of the distance");
        }

        m_currentFuel.Value = (uint)m_startingFuel;
        m_fuel = m_startingFuel;

    }
    private void Update()
    {
        ConsumeFuel();
    }

    private void ConsumeFuel()
    {

        if(m_fuel <= 0) return;

        m_fuel -= m_consumptionRate * Time.deltaTime;
        m_slider.normalizedValue = m_fuel / m_startingFuel;
        m_currentFuel.Value = (uint)m_fuel;
    }
}
