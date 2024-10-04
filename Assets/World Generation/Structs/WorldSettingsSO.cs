using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshStructure {
    BinaryTree, 
    QuadTree
}

[CreateAssetMenu(fileName = "WorldSettingsSO", menuName = "World/World Settings")]
public class WorldSettingsSO : ScriptableObject {

    [Header("World Generation Data")]
    public long seedX = 35432;
    public long seedZ = 17323;
    public Octave[] octaves = new Octave[5] {
        new Octave{ scale = 0.3f, amplitude = 1f, enabled = true },
        new Octave{ scale = 0.04f, amplitude = 4f, enabled = true },
        new Octave{ scale = 0.008f, amplitude = 30f, enabled = true },
        new Octave{ scale = 0.004f, amplitude = 90f, enabled = true },
        new Octave{ scale = 0.0004f, amplitude = 150f, enabled = true }
    };

    [Header("Mesh Settings")]
    public MeshStructure meshStructure = MeshStructure.BinaryTree;
    [Range(2, 64)]
    public float vertexDistance = 4;
    [Range(2, 250)]
    public int vertexCount = 250;
    [Range(1, 32)]
    public int levelsOfDetail = 7;

    [Header("Material Settings")]
    public Material material;

    // Callback for settings change
    public delegate void OnValidateCallback();
    public OnValidateCallback OnSettingsChange;

    // Validate settings
    private void OnValidate() {
        OnSettingsChange?.Invoke();
    }

    // Scaling factor for mesh generation
    public float MeshScalingFactor() {
        return meshStructure switch {
            MeshStructure.BinaryTree => Mathf.Sqrt(2),
            MeshStructure.QuadTree => 2,
            _ => 1
        };
    }
}
