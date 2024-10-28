using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeamNameResult : MonoBehaviour
{
    [SerializeField] private Players m_players;
    [SerializeField] private TMP_Text m_teamNameText;

    private void Start()
    {
        if (m_players != null) m_teamNameText.text = "The " + m_players.playerTeamName;
        else m_teamNameText.text = "Missing team name!";
    }
}
