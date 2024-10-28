using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlightRecordGUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text m_names;
    [SerializeField] private TMP_Text m_score;
    public TMP_Text Names { get { return m_names; } set { m_names = value; } }
    public TMP_Text Score { get { return m_score; } set { m_score = value; } }
}
