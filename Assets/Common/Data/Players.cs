using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Players", menuName = "ScriptableObject/Data/Players")]
public class Players : ScriptableObject
{
    // Keep track of all connected players
    public Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();
}
