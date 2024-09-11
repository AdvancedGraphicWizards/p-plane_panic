using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugManager", menuName = "ScriptableObject/Manager/DebugManager")]
class DebugManager : ScriptableObject
{
    public void DebugMessage(String message)
    {
        Debug.Log(message);
    }
}
