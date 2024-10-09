using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "FlightRecordsStates", menuName = "ScriptableObject/Data/FlightRecordsStates")]
public class FlightRecordsStates : ScriptableObject
{
    [SerializeField] private List<FlightRecord> m_flightRecords = new List<FlightRecord>();
    public List<FlightRecord> FlightRecords { get => m_flightRecords; }
    private const int MAX_RECORDS = 3;
    [SerializeField] private float m_shortestDistance;
    [SerializeField] private float m_longestDistance;
    private const string SaveKey = "FlightRecords_SaveKey";

    // Reset the leaderboard
    public void Init()
    {
        Debug.Log("Resetting flight leaderboard!");
        m_shortestDistance = 0;
        m_longestDistance = 0;
        m_flightRecords.Clear();
        SaveRecords();
    }

    [System.Serializable]
    public struct FlightRecord
    {
        public float m_distance;
        public string m_names;
        public FlightRecord(float distance, string names)
        {
            m_distance = distance;
            m_names = names;
        }
    }

    [System.Serializable]
    private class SerializableFlightRecords
    {
        public List<FlightRecord> records;

        public SerializableFlightRecords(List<FlightRecord> records)
        {
            this.records = records;
        }
    }

    public bool AddNewRecord(float distance, string names)
    {
        Debug.Log("AddNewRecord() -> Distance: " + distance + " | Team name: " + names);

        if (m_flightRecords.Count == MAX_RECORDS && distance <= m_flightRecords[m_flightRecords.Count - 1].m_distance)
        {
            return false;
        }

        m_flightRecords.Add(new FlightRecord(distance, names));
        m_flightRecords.Sort((a, b) => b.m_distance.CompareTo(a.m_distance));

        if (m_flightRecords.Count > MAX_RECORDS)
        {
            m_flightRecords.RemoveAt(m_flightRecords.Count - 1);
        }

        m_longestDistance = m_flightRecords[0].m_distance;
        m_shortestDistance = m_flightRecords[m_flightRecords.Count - 1].m_distance;

        SaveRecords();
        return true;
    }

    // Save the records to PlayerPrefs
    public void SaveRecords()
    {
        Debug.Log("Saved flight records to PlayerPrefs!");
        string json = JsonUtility.ToJson(new SerializableFlightRecords(m_flightRecords));
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    // Load the leaderboard data from PlayerPrefs
    public void LoadRecords()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("Loaded flight records from PlayerPrefs!");
            string json = PlayerPrefs.GetString(SaveKey);
            SerializableFlightRecords loadedData = JsonUtility.FromJson<SerializableFlightRecords>(json);
            m_flightRecords = loadedData.records.ToList();

            if (m_flightRecords.Count > 0)
            {
                m_longestDistance = m_flightRecords[0].m_distance;
                m_shortestDistance = m_flightRecords[m_flightRecords.Count - 1].m_distance;
            }
        }
    }
}