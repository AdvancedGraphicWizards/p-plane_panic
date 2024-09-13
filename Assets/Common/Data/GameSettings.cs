using UnityEngine;

//
// Game settings that that are set before running the game
//

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObject/Data/GameSettings")]
class GameSettings : ScriptableObject
{
    public uint MaxFuel;
    public uint RingFuelContribution;
    public uint PlaneBaseSpeed;
}

