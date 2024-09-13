using UnityEngine;

[CreateAssetMenu(fileName = "PlatformData", menuName = "ScriptableObjects/PlatformDataObject", order = 1)]
public class PlatformData : ScriptableObject
{
    public string platformName;

    public int BoundsX;
    public int BoundsY;
}