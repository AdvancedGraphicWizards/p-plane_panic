using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FuelController : MonoBehaviour
{
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
    [SerializeField] private float[] fuelColorBreakPoints = new float[] { 0.3f, 0.5f };

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

        m_currentFuel.Value = (uint)m_startingFuel;
        m_fuel = m_startingFuel;

    }
    private void Update()
    {
        ConsumeFuel();
        UpdateColor();
    }

    private void ConsumeFuel()
    {

        if (m_fuel <= 0)
            TriggerLoseEvent();

        m_fuel -= m_consumptionRate * Time.deltaTime;
        m_slider.normalizedValue = m_fuel / m_startingFuel;
        m_currentFuel.Value = (uint)m_fuel;
    }
    public void UpdateColor()
    {
        float normalizedValue = m_fuel / m_startingFuel;
        if (normalizedValue < fuelColorBreakPoints[0])
            m_fuelImage.color = Color.red;
        else if (normalizedValue < fuelColorBreakPoints[1])
            m_fuelImage.color = Color.yellow;
        else
            m_fuelImage.color = Color.green;

    }
    //TODO implement the triggering of the Lossing event
    public VoidEvent Event;
    public UnityEvent Response;

    void OnEnable()
    {
        Event.Subscribe(CallResponse);
    }

    void OnDisable()
    {
        Event.Unsubscribe(CallResponse);
    }

    private void CallResponse()
    {
        Response.Invoke();
    }
    public void TriggerLoseEvent()
    {
        CallResponse();
        return;
    }
}
