using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseGenerator))]
public class NoiseGeneratorEditor : Editor
{
    public string filePath = "";
    [HideInInspector] public Texture2D texture2D;
    [HideInInspector] public Texture3D texture3D;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        NoiseGenerator g = (NoiseGenerator)target;

        GUILayout.Space(10);

        //
        // Generate
        // 

        bool bufferExists = g.noiseBuffer != null;
        bool textureExists = !(texture2D == null && texture3D == null);
        // if (!bufferExists || !textureExists)
        // {
        //     g.GenerateNoise();
        //     CreatePreviewTexture(g);
        // }

        if (GUILayout.Button("Generate"))
        {
            g.GenerateNoise();

            CreatePreviewTexture(g);
        }

        GUILayout.Space(10);

        // 
        // Preview
        //

        if (textureExists)
        {
            if (texture2D != null)
            {
                GUILayout.Label("2D Preview");

                float inspectorWidth = EditorGUIUtility.currentViewWidth;
                Rect previewRect = GUILayoutUtility.GetRect(inspectorWidth, inspectorWidth);

                EditorGUI.DrawPreviewTexture(previewRect, texture2D);
            }

            if (texture2D != null)
            {
                GUILayout.Label("2D Tiling Preview:");

                float inspectorWidth = EditorGUIUtility.currentViewWidth;
                int tiles = 2;
                Rect tiledPreviewRect = GUILayoutUtility.GetRect(inspectorWidth, inspectorWidth);

                float tileSize = inspectorWidth / tiles;
                for (int y = 0; y < tiles; y++)
                {
                    for (int x = 0; x < tiles; x++)
                    {
                        Rect tileRect = new(
                            tiledPreviewRect.x + x * tileSize,
                            tiledPreviewRect.y + y * tileSize,
                            tileSize,
                            tileSize);

                        EditorGUI.DrawPreviewTexture(tileRect, texture2D);
                    }
                }
            }
        }

    }

    void CreatePreviewTexture(NoiseGenerator g)
    {
        // Write to texture
        float[] data = new float[g.size * g.size * g.size];
        g.noiseBuffer.GetData(data);

        // 2D
        texture2D = new Texture2D(g.size, g.size, TextureFormat.RGBA32, false);
        for (int y = 0; y < g.size; y++)
        {
            for (int x = 0; x < g.size; x++)
            {
                int index = x + y * g.size;
                float value = data[index];
                texture2D.SetPixel(x, y, new Color(value, value, value));
            }
        }
        texture2D.Apply();
    }
}
