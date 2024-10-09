using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Updates the GUI game object whenever there is a new flight that has a higher score than the smallest one
/// Send the new record to the ScriptableObject FlightRecordsStates
/// </summary>
public class LeaderBoardController : MonoBehaviour
{
    [SerializeField] private FlightRecordGUIController[] m_flightRecordGUI;
    [SerializeField] private FlightRecordsStates m_flightRecordsStates;

    private void Awake()
    {
        m_flightRecordGUI = GetComponentsInChildren<FlightRecordGUIController>();

        //m_flightRecordsStates.Init();

        // Debug.Log(m_flightRecordsStates.AddNewRecord(3333, "David | Juan | Pedro | Luis | Alejandro"));
        // Debug.Log(m_flightRecordsStates.AddNewRecord(2222, "Jesus | Pedro | Camila | Alejandro"));
        // Debug.Log(m_flightRecordsStates.AddNewRecord(2222, "Felipe | Jose"));
        // Debug.Log(m_flightRecordsStates.AddNewRecord(4444, "Felipe | Jose"));
        // Debug.Log(m_flightRecordsStates.AddNewRecord(1111, "Felipe | Jose"));
        // Debug.Log(m_flightRecordsStates.AddNewRecord(1111, "Felipe | Jose"));
        // Debug.Log(m_flightRecordsStates.AddNewRecord(5555, "Xulo | David"));
        // Debug.Log(m_flightRecordsStates.AddNewRecord(6666, "Juan | Pedro"));

        UpdateFigthsRecordsGUI();
    }

    public void UpdateFigthsRecordsGUI()
    {
        Debug.Log("==== Updated GUI of the leader board");
        if(m_flightRecordsStates.FlightRecords == null) return;

        IEnumerator<FlightRecordsStates.FlightRecord> IFlighRecord = m_flightRecordsStates.FlightRecords.GetEnumerator();
        int index = 0;
        while (IFlighRecord.MoveNext() != false)
        {
            //Debug.Log(IFlighRecord.Current.m_distance + " " + IFlighRecord.Current.m_names);
            m_flightRecordGUI[index].Names.text = IFlighRecord.Current.m_names;
            m_flightRecordGUI[index].Score.text = IFlighRecord.Current.m_distance.ToString();
            index++;
        }
    }

}
