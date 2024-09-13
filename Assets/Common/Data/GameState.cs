using UnityEngine;

//
// Game state that changes during runtime
//

[CreateAssetMenu(fileName = "GameState", menuName = "ScriptableObject/Data/GameState")]
class GameState : ScriptableObject
{
    public uint Score;

    public uint Fuel;
}
