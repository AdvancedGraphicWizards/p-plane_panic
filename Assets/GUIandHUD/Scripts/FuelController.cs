using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FuelController : MonoBehaviour
{
    [SerializeField] private GameState gameStateSO;
    [Tooltip("The Slide Game Object FuelSliderIndicator")]
    [SerializeField] private Slider m_slider;
    [Tooltip("You can find it on the Game Object hierarchy FuelSliderIndicator -> FillArea -> Fill ")]
    [SerializeField] private Image m_fuelImage;
    [Tooltip("Difine how much fuel does the plane consume. Better if it is a Scriptable Object")]
    [SerializeField] private float m_consumptionRate = 5.0f;
    [Tooltip("The starting fuel level. Better if it is a Scriptable Object")]
    [SerializeField] private float m_startingFuel = 100.0f;
    private float m_fuel = 0;

    [Tooltip("Scriptable Object of type UIntVariable, named Fuel")]
    [SerializeField] private UIntVariable m_currentFuel;
    [SerializeField] private float[] fuelBreakPoints = new float[] { 0.3f, 0.5f };
    [SerializeField] private Color[] fuelStatesColor;

    private void Awake()
    {
        if (!m_slider || !m_fuelImage)
        {
            throw new System.NullReferenceException("Slider references are missing, the CountDownController needs a m_slider and m_fuelImage component to display the counter");
        }
        if (!m_currentFuel)
        {
            throw new System.NullReferenceException("The Scriptable Object CurrentFuel is missing, the DistanceCounterController needs is to keep track of the distance");
        }

        if (!gameStateSO)
            throw new System.NullReferenceException("Missing GameState, HelloWorld purposes");


        m_currentFuel.Value = (uint)m_startingFuel;
        m_fuel = m_startingFuel;

    }

    private void Update()
    {
        if (!gameStateSO.HasStarted) return; //TODO quick fix for the HelloWorld

        ConsumeFuel();
        UpdateColor();
    }

    private void ConsumeFuel()
    {

        if (m_fuel <= 0)
        {
            gameStateSO.OutOfFuel = true;
            //TriggerLoseEvent();
        }
        else
        {
            gameStateSO.OutOfFuel = false;
        }

        m_fuel -= m_consumptionRate * Time.deltaTime;
        m_slider.normalizedValue = m_fuel / m_startingFuel;
        m_currentFuel.Value = (uint)m_fuel;
    }
    public void UpdateColor()
    {
        if(fuelStatesColor.Length == 0) return;
        float normalizedValue = m_fuel / m_startingFuel;
        if (normalizedValue < fuelBreakPoints[0])
            m_fuelImage.color = fuelStatesColor[2];//Color.red;
        else if (normalizedValue < fuelBreakPoints[1])
            m_fuelImage.color = fuelStatesColor[1];//Color.yellow;
        else
            m_fuelImage.color = fuelStatesColor[0];//Color.green;

    }

    public void UpdateFuel(float amt)
    {
        m_fuel += amt;
        m_fuel = Math.Min(m_fuel, m_startingFuel);
    }

    [SerializeField] private FloatEvent OnAddFuel;
    void OnEnable()
    {
        //HoopScript.OnRingEnter += fuel_amt => UpdateFuel(fuel_amt);
        OnAddFuel.Subscribe(UpdateFuel);
        FireComponent.FireDamageEvent += fuel_amt => UpdateFuel(fuel_amt);
    }
    void OnDisable() {
        OnAddFuel.Unsubscribe(UpdateFuel);
    }
}
