using TMPro;
using UnityEngine;
class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private GameState gameState;

    void Update()
    {
        text.text = gameState.Score.ToString();
    }
}
