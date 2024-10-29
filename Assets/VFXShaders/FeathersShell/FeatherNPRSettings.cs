using UnityEngine;

[CreateAssetMenu(fileName = "FeatherNPRSettings", menuName = "VFX/Feathers/FeatherNPRSettings", order = 0)]
public class FeatherNPRSettings : ScriptableObject
{
    public bool updateStatics;
    [Range(1, 256)]
    public int shellCount = 16;

    [Range(0.0f, 1.0f)]
    public float shellLength = 0.15f;

    [Range(0.01f, 3.0f)]
    public float distanceAttenuation = 1.0f;

    [Range(1.0f, 1000.0f)]
    public float density = 100.0f;

    [Range(0.0f, 1.0f)]
    public float noiseMin = 0.0f;

    [Range(0.0f, 1.0f)]
    public float noiseMax = 1.0f;

    [Range(0.0f, 10.0f)]
    public float thickness = 1.0f;

    [Range(0.0f, 10.0f)]
    public float curvature = 1.0f;

    [Range(0.0f, 1.0f)]
    public float displacementStrength = 0.1f;

    [Header("Illumination")]
    public Color shellColor;
    [Range(0.0f, 5.0f)]
    public float occlusionAttenuation = 1.0f;
    [Range(0.0f, 1.0f)]
    public float occlusionBias = 0.0f;
    [Range(0.0f, 1.0f)]
    public float hardness = 0.0f;
    [Range(0.0f, 100.0f)]
    public float phongExponent = 0.0f;
}
