using UnityEngine;

[CreateAssetMenu(fileName = "PlatformData", menuName = "ScriptableObjects/PlatformDataObject", order = 1)]
public class PlatformData : ScriptableObject
{
    public string platformName;

    public float BoundsX;
    public float BoundsY;
}