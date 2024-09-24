using UnityEngine;

//
// Game state that changes during runtime
//

[CreateAssetMenu(fileName = "GameState", menuName = "ScriptableObject/Data/GameState")]
class GameState : ScriptableObject
{
    //TODO this variables are being used as individual scriptable objects
    //public uint Score;
    //public uint Fuel;
    public bool HasStarted = false;
    public bool OutOfFuel = false;

    public void StartGame()
    {
        /*
        This is NOT the best solution, it better to handle this as an event, 
        I just could manage to wrap my head around the SO event logic.
        This is being trigger by thr button on the Lobby screne on the Game Scene.
        - David
        */
        HasStarted = true;
        OutOfFuel = false;
    }
}
