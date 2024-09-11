using UnityEngine;

class ScoreManager : MonoBehaviour
{
    public GameState gameState;

    void OnEnable()
    {
        gameState.Score = 0;
    }

    void FixedUpdate()
    {
        gameState.Score += 1;
    }
}
