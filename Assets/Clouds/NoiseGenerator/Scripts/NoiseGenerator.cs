using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "NoiseGenerator", menuName = "ScriptableObject/NoiseGenerator")]
public class NoiseGenerator : ScriptableObject
{
    [SerializeField] private ComputeShader worleyShader;
    [SerializeField] private ComputeShader perlinShader;
    public int size = 128;
    [SerializeField] private int cells = 8;
    public bool worleyNoise = true;

    public ComputeBuffer noiseBuffer;
    private ComputeBuffer pointsBuffer;

    public void GenerateNoise() {
        if (worleyNoise)   {
            GenerateWorleyNoise();
        } else {
            GeneratePerlinNoise();
        }
    }

    public void GeneratePerlinNoise() {
        noiseBuffer?.Release();    
        noiseBuffer = new ComputeBuffer(size * size * size, sizeof(float));

        // Generate noise buffer
        int kernelIndex = perlinShader.FindKernel("CSMain");
        perlinShader.SetInt("Size", size);
        perlinShader.SetBuffer(kernelIndex, "Result", noiseBuffer);
        perlinShader.Dispatch(kernelIndex, size, size, size);
    }


    public void GenerateWorleyNoise() {
        // Points buffer
        pointsBuffer?.Release();
        pointsBuffer = new ComputeBuffer(cells * cells * cells, 3 * sizeof(float));

        // Generate points buffer
        float cellSize = size / cells;
        float3[] data = new float3[cells * cells * cells];
        for (int y = 0; y < cells; y++) {
            for (int x = 0; x < cells; x++) {
                for (int z = 0; z < cells; z++) {
                    int index = x + y * cells + z * cells * cells;
                    float3 minPos = new(x * cellSize, y * cellSize, z * cellSize);
                    float3 maxPos = new((x + 1) * cellSize, (y + 1) * cellSize, (z + 1) * cellSize);
                    float3 pos = new(
                        UnityEngine.Random.Range(minPos.x, maxPos.x),
                        UnityEngine.Random.Range(minPos.y, maxPos.y),
                        UnityEngine.Random.Range(minPos.z, maxPos.z)
                    );
                    data[index] = pos;
                }
            }
        }
        pointsBuffer.SetData(data, 0, 0, data.Length);

        // Noise buffer
        noiseBuffer?.Release();    
        noiseBuffer = new ComputeBuffer(size * size * size, sizeof(float));

        // Generate noise buffer
        int kernelIndex = worleyShader.FindKernel("CSMain");
        worleyShader.SetInt("Size", size);
        worleyShader.SetInt("Cells", cells);
        worleyShader.SetBuffer(kernelIndex, "Points", pointsBuffer);
        worleyShader.SetBuffer(kernelIndex, "Result", noiseBuffer);
        worleyShader.Dispatch(kernelIndex, size, size, size);
    }

    void OnDisable() {
        noiseBuffer?.Release();
        pointsBuffer?.Release();
    }
}
