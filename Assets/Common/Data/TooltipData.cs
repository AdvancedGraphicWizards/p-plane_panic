using UnityEngine;

[CreateAssetMenu(fileName = "TooltipData", menuName = "ScriptableObject/Data/TooltipData")]
public class TooltipData : ScriptableObject
{
    public int ringsPassed;
    public int firesExtinguished;

    public void ResetStats() {
        ringsPassed = firesExtinguished = 0;
    }
}
