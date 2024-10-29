using UnityEngine;

[CreateAssetMenu(fileName = "NoiseCombiner", menuName = "ScriptableObject/NoiseCombiner")]
public class NoiseCombiner : ScriptableObject
{
    public NoiseGenerator textureR;
    public NoiseGenerator textureG;
    public NoiseGenerator textureB;
    public NoiseGenerator textureA;

    // Takes the red channel from each texture
    public Texture3D Combine() {
        int size = textureR.size;

        float[] dataR = new float[size * size * size];
        if (textureR != null) { 
            if (textureR.noiseBuffer == null) {
                textureR.GenerateWorleyNoise();
            }
            textureR.noiseBuffer.GetData(dataR); 
        }
        float[] dataG = new float[size * size * size];
        if (textureG != null) { 
            if (textureG.noiseBuffer == null) {
                textureG.GenerateWorleyNoise();
            }
            textureG.noiseBuffer.GetData(dataG); 
        }
        float[] dataB = new float[size * size * size];
        if (textureB != null) { 
            if (textureB.noiseBuffer == null) {
                textureB.GenerateWorleyNoise();
            }
            textureB.noiseBuffer.GetData(dataB); 
        }
        float[] dataA = new float[size * size * size];
        if (textureA != null) { 
            if (textureA.noiseBuffer == null) {
                textureA.GenerateWorleyNoise();
            }
            textureA.noiseBuffer.GetData(dataA); 
        }

        Texture3D texture = new(size, size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                for (int z = 0; z < size; z++) {
                    int index = x + y * size + z * size * size;
                    Color color = new(0, 0, 0, 0);
                    if (textureR != null) { color.r = dataR[index]; }
                    if (textureG != null) { color.g = dataG[index]; }
                    if (textureB != null) { color.b = dataB[index]; }
                    if (textureA != null) { color.a = dataA[index]; }
                    texture.SetPixel(x, y, z, color);
                }
            }
        }
        texture.Apply();

        return texture;
    }
}
