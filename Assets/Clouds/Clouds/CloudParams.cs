using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

// Size  = 4 + 4 = 8 floats
public struct Shape {
    public float3 origin;
    public float type;
    public float3 dim;
    public float padding;

    public Shape(float3 origin, float3 dim) {
        this.origin = origin;
        this.dim = dim;

        // temp
        this.type = 0;
        this.padding = 0;
    }

    static public int Stride() {
        return 8 * sizeof(float);
    }
}

[CreateAssetMenu(fileName ="CloudParams", menuName = "ScriptableObject/CloudParams")]
public class CloudParams: ScriptableObject {
    [Header("Material")]
    [SerializeField] private Material material;

    [Header("Ray marching")]
    [SerializeField] private float raymarchHitDist = 0.001f;
    [SerializeField] private int raymarchMaxSteps = 100;

    [Header("Cloud marching")]
    [SerializeField] private int cloudMaxSteps = 100;
    [SerializeField] private float cloudStepSize = 0.5f;
    [SerializeField] private float cloudRandomOffsetMultiplier = 2.0f;

    [Header("Cloud looks")]
    [SerializeField] private Texture3D cloudNoiseTexture;
    [SerializeField] private float cloudNoiseSampleMultiplier = 1.0f;
    [SerializeField] private float cloudAbsorption = 1.0f;
    [SerializeField] private Vector3 cloudScrollSpeed = new(0,0,0);
    [SerializeField] private float cloudEdgeBlend = 100f;
    [SerializeField] private float cloudDistBlend = 100f;

    [Header("Cloud spawning")]
    public float minDistance = 1000 ;
    public float maxDistance = 2000;

    private ComputeBuffer shapeBuffer;

    public void UploadShape(List<Shape> shapes) {
        if (shapeBuffer == null || shapeBuffer.count != shapes.Count) {
            shapeBuffer = new ComputeBuffer(shapes.Count, Shape.Stride());
        }

        shapeBuffer.SetData(shapes.ToArray());
        material.SetBuffer("_ShapeBuffer", shapeBuffer);
        material.SetInt("_ShapeBufferLen", shapeBuffer.count);
    }

    public void UploadInternal() {
        // Ray marching
        material.SetInt("_RaymarchMaxSteps", raymarchMaxSteps);
        material.SetFloat("_RaymarchHitDist", raymarchHitDist);

        // Cloud marching
        if (cloudStepSize > 0.00001) material.SetFloat("_CloudStepSize", cloudStepSize);
        material.SetInt("_CloudMaxSteps", cloudMaxSteps);
        material.SetFloat("_CloudRandomOffsetMultiplier", cloudRandomOffsetMultiplier);

        // Cloud looks
        material.SetTexture("_CloudNoiseTexture", cloudNoiseTexture);
        material.SetFloat("_CloudNoiseSampleMultiplier", cloudNoiseSampleMultiplier);
        material.SetFloat("_CloudAbsorption", cloudAbsorption);
        material.SetVector("_CloudScrollSpeed", cloudScrollSpeed);
        material.SetFloat("_CloudEdgeBlend", cloudEdgeBlend);
        material.SetFloat("_CloudDistBlend", cloudDistBlend);

    }

    void OnEnable() {
        UploadInternal();
    }
    void OnValidate() {

        UploadInternal();
    }
}