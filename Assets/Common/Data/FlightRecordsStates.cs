using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "FlightRecordsStates", menuName = "ScriptableObject/Data/FlightRecordsStates")]
public class FlightRecordsStates : ScriptableObject
{
    [SerializeField] private LinkedList<FlightRecord> m_flightRecords = new LinkedList<FlightRecord>();
    public LinkedList<FlightRecord> FlightRecords { get => m_flightRecords;}
    [SerializeField] private int m_records;
    private const int MAX_RECORDS = 3;
    [SerializeField] private float m_shortestDistance = 0;
    [SerializeField] private float m_longestDistance = 0;

    public void Init()
    {
        m_records = 0;
        m_shortestDistance = 0;
        m_longestDistance = 0;
        //m_flightRecords = new LinkedList<FlightRecord>();
    }
    public struct FlightRecord
    {
        public float m_distance;
        public string m_names; //Store the names in one string in the form: "name | name | ..."
        public FlightRecord(float distance, string names)
        {
            m_distance = distance;
            m_names = names;
        }
    }

    public bool AddNewRecord(float distance, string names)
    {
        //Early return
        if (distance < m_shortestDistance && m_flightRecords.Count == MAX_RECORDS)
            return false;

        // If the LinkedList is empty OR the new distance is bigger than the longest record, save it
        if (m_flightRecords.Count == 0 || distance > m_longestDistance)
        {
            m_flightRecords.AddFirst(new FlightRecord(distance, names));
            m_longestDistance = m_flightRecords.First.Value.m_distance;

            // If the list is now bigger than the max records, remove the last one
            if (m_flightRecords.Count > MAX_RECORDS)
            {
                m_flightRecords.RemoveLast();
                m_shortestDistance = m_flightRecords.Last.Value.m_distance;
            }
        }
        else if (distance > m_shortestDistance)
        {
            // If the new distance is not bigger, but it is bigger than the shortest 
            //and we have at least one record already, save it, and update
            m_flightRecords.AddLast(new FlightRecord(distance, names));
            m_shortestDistance = m_flightRecords.Last.Value.m_distance;
        }
        else if (distance < m_shortestDistance && m_flightRecords.Count < MAX_RECORDS)
        {
            //If we new distance is smaller than the shortest and we still have space, save it
            m_flightRecords.AddLast(new FlightRecord(distance, names));
            m_shortestDistance = m_flightRecords.Last.Value.m_distance;
        }

        m_records = m_flightRecords.Count;
        return true;
    }

    public FlightRecord GetFlightRecord(int index)
    {

        IEnumerator<FlightRecord> IFlightRecords = m_flightRecords.GetEnumerator();
        while (IFlightRecords.MoveNext() != false)
        {

        }

        return new FlightRecord(0, "");
    }
}