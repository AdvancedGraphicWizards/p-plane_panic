using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Octaves used for perlin noise
[System.Serializable]
public struct Octave {
    public float scale;
    public float amplitude;

    // Toggle
    public bool enabled;
}