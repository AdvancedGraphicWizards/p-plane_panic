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
        for (int i = 0; i < m_flightRecordGUI.Length; i++)
        {
            m_flightRecordGUI[i].gameObject.SetActive(false);
        }
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
            Debug.Log("Index: " + index + " : " + IFlighRecord.Current.m_names + " = " + IFlighRecord.Current.m_distance );
            m_flightRecordGUI[index].Names.text = IFlighRecord.Current.m_names;
            m_flightRecordGUI[index].Score.text = IFlighRecord.Current.m_distance.ToString();
            m_flightRecordGUI[index].gameObject.SetActive(true);
            index++;
        }
    }

}
