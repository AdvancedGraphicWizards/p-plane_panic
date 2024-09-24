using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public struct LabColor
{
    public float L;
    public float A;
    public float B;

    public LabColor(float l, float a, float b)
    {
        L = l;
        A = a;
        B = b;
    }

    // Conversion from RGB to LAB
    public static LabColor FromColor(Color color)
    {
        // Convert RGB to XYZ
        float r = PivotRgb(color.r);
        float g = PivotRgb(color.g);
        float b = PivotRgb(color.b);

        // Observer = 2 degree, Illuminant = D65
        float x = r * 0.4124564f + g * 0.3575761f + b * 0.1804375f;
        float y = r * 0.2126729f + g * 0.7151522f + b * 0.0721750f;
        float z = r * 0.0193339f + g * 0.1191920f + b * 0.9503041f;

        // Normalise for D65 white point
        x /= 0.95047f;
        y /= 1.00000f;
        z /= 1.08883f;

        // Convert XYZ to LAB
        x = PivotXyz(x);
        y = PivotXyz(y);
        z = PivotXyz(z);

        float l = 116f * y - 16f;
        float a = 500f * (x - y);
        float bLab = 200f * (y - z);

        // Debug.Log("LAB Colour L: " + l + ", A: " + a + ", B:" + bLab);
        return new LabColor(l, a, bLab);
    }

    // Conversion from LAB to RGB
    public readonly Color ToColor()
    {
        // Convert LAB to XYZ
        float y = (L + 16f) / 116f;
        float x = A / 500f + y;
        float z = y - B / 200f;

        x = InversePivotXyz(x) * 0.95047f;
        y = InversePivotXyz(y) * 1.00000f;
        z = InversePivotXyz(z) * 1.08883f;

        // Convert XYZ to RGB
        float r = x * 3.2404542f + y * -1.5371385f + z * -0.4985314f;
        float g = x * -0.9692660f + y * 1.8760108f + z * 0.0415560f;
        float b = x * 0.0556434f + y * -0.2040259f + z * 1.0572252f;

        r = Mathf.Clamp01(InversePivotRgb(r));
        g = Mathf.Clamp01(InversePivotRgb(g));
        b = Mathf.Clamp01(InversePivotRgb(b));

        return new Color(r, g, b);
    }

    // Helper function to convert an RGB component
    private static float PivotRgb(float n)
    {
        return (n > 0.04045f) ? Mathf.Pow((n + 0.055f) / 1.055f, 2.4f) : n / 12.92f;
    }

    // Helper function to apply XYZ pivot
    private static float PivotXyz(float n)
    {
        return (n > 0.008856f) ? Mathf.Pow(n, 1f / 3f) : (7.787f * n) + (16f / 116f);
    }

    // Inverse pivot for XYZ conversion
    private static float InversePivotXyz(float n)
    {
        float n3 = Mathf.Pow(n, 3);
        return (n3 > 0.008856f) ? n3 : (n - 16f / 116f) / 7.787f;
    }

    // Inverse pivot for RGB conversion
    private static float InversePivotRgb(float n)
    {
        return (n > 0.0031308f) ? 1.055f * Mathf.Pow(n, 1f / 2.4f) - 0.055f : 12.92f * n;
    }
}

// https://github.com/tompazourek/Colourful/blob/master/src/Colourful/Difference/CIEDE2000ColorDifference.cs
public static class ColorDifference
{
    private const float k_H = 1f;
    private const float k_L = 1f;
    private const float k_C = 1f;

    public static float CIEDE2000(LabColor lab1, LabColor lab2)
    {
        // 1. Calculate C_prime, h_prime
        Calculate_a_prime(lab1.A, lab2.A, lab1.B, lab2.B, out float a_prime0, out float a_prime1);
        Calculate_C_prime(a_prime0, a_prime1, lab1.B, lab2.B, out float C_prime0, out float C_prime1);
        Calculate_h_prime(a_prime0, a_prime1, lab1.B, lab2.B, out float h_prime0, out float h_prime1);

        // 2. Calculate dL_prime, dC_prime, dH_prime
        float dL_prime = lab2.L - lab1.L;
        float dC_prime = C_prime1 - C_prime0;
        float dh_prime = Calculate_dh_prime(C_prime0, C_prime1, h_prime0, h_prime1);
        float dH_prime = 2f * Mathf.Sqrt(C_prime0 * C_prime1) * Mathf.Sin(Mathf.Deg2Rad * (dh_prime / 2f));

        // 3. Calculate CIEDE2000 Colour-Difference dE00
        float L_prime_mean = (lab1.L + lab2.L) / 2f;
        float C_prime_mean = (C_prime0 + C_prime1) / 2f;
        float h_prime_mean = Calculate_h_prime_mean(h_prime0, h_prime1, C_prime0, C_prime1);

        float T = 1f - 0.17f * Mathf.Cos(Mathf.Deg2Rad * (h_prime_mean - 30f))
                     + 0.24f * Mathf.Cos(Mathf.Deg2Rad * (2f * h_prime_mean))
                     + 0.32f * Mathf.Cos(Mathf.Deg2Rad * (3f * h_prime_mean + 6f))
                     - 0.20f * Mathf.Cos(Mathf.Deg2Rad * (4f * h_prime_mean - 63f));

        float dTheta = 30f * Mathf.Exp(-Mathf.Pow((h_prime_mean - 275f) / 25f, 2f));
        float R_C = 2f * Mathf.Sqrt(Pow7(C_prime_mean) / (Pow7(C_prime_mean) + Pow7(25f)));

        float S_L = 1f + 0.015f * Mathf.Pow(L_prime_mean - 50f, 2f) / Mathf.Sqrt(20f + Mathf.Pow(L_prime_mean - 50f, 2f));
        float S_C = 1f + 0.045f * C_prime_mean;
        float S_H = 1f + 0.015f * C_prime_mean * T;
        float R_T = -Mathf.Sin(2f * Mathf.Deg2Rad * dTheta) * R_C;

        float dE00 = Mathf.Sqrt(
            Mathf.Pow(dL_prime / (k_L * S_L), 2f) +
            Mathf.Pow(dC_prime / (k_C * S_C), 2f) +
            Mathf.Pow(dH_prime / (k_H * S_H), 2f) +
            R_T * (dC_prime / (k_C * S_C)) * (dH_prime / (k_H * S_H))
        );

        // Debug.Log("Colour difference = " + dE00);
        return dE00;
    }

    private static void Calculate_a_prime(float a0, float a1, float b0, float b1, out float a_prime0, out float a_prime1)
    {
        float C_ab0 = Mathf.Sqrt(a0 * a0 + b0 * b0);
        float C_ab1 = Mathf.Sqrt(a1 * a1 + b1 * b1);

        float C_ab_mean = (C_ab0 + C_ab1) / 2f;

        float G = 0.5f * (1f - Mathf.Sqrt(Pow7(C_ab_mean) / (Pow7(C_ab_mean) + Pow7(25f))));

        a_prime0 = (1f + G) * a0;
        a_prime1 = (1f + G) * a1;
    }

    private static void Calculate_C_prime(float a_prime0, float a_prime1, float b0, float b1, out float C_prime0, out float C_prime1)
    {
        C_prime0 = Mathf.Sqrt(a_prime0 * a_prime0 + b0 * b0);
        C_prime1 = Mathf.Sqrt(a_prime1 * a_prime1 + b1 * b1);
    }

    private static void Calculate_h_prime(float a_prime0, float a_prime1, float b0, float b1, out float h_prime0, out float h_prime1)
    {
        h_prime0 = NormalizeDegree(Mathf.Rad2Deg * Mathf.Atan2(b0, a_prime0));
        h_prime1 = NormalizeDegree(Mathf.Rad2Deg * Mathf.Atan2(b1, a_prime1));
    }

    private static float Calculate_dh_prime(float C_prime0, float C_prime1, float h_prime0, float h_prime1)
    {
        if (C_prime0 * C_prime1 == 0f)
            return 0f;

        float delta = h_prime1 - h_prime0;
        if (Mathf.Abs(delta) <= 180f)
            return delta;

        return delta > 180f ? delta - 360f : delta + 360f;
    }

    private static float Calculate_h_prime_mean(float h_prime0, float h_prime1, float C_prime0, float C_prime1)
    {
        if (C_prime0 * C_prime1 == 0f)
            return h_prime0 + h_prime1;

        float delta = Mathf.Abs(h_prime0 - h_prime1);
        if (delta <= 180f)
            return (h_prime0 + h_prime1) / 2f;

        return (h_prime0 + h_prime1 + (h_prime0 + h_prime1 < 360f ? 360f : -360f)) / 2f;
    }

    private static float Pow7(float n)
    {
        return Mathf.Pow(n, 7f);
    }

    private static float NormalizeDegree(float degree)
    {
        while (degree < 0) degree += 360;
        while (degree >= 360) degree -= 360;
        return degree;
    }
}

// https://github.com/connorgr/colorgorical/blob/master/src/model/model.py
public class PaletteGenerator
{
    private List<LabColor> _colorSpace;
    private Dictionary<string, float> _weights;
    private float _minLightness;
    private float _maxLightness;

    public PaletteGenerator(Dictionary<string, float> weights, float minLightness = 25f, float maxLightness = 85f)
    {
        _weights = weights;
        _minLightness = minLightness;
        _maxLightness = maxLightness;
        InitialiseColorSpace();
    }

    private void InitialiseColorSpace()
    {
        _colorSpace = new List<LabColor>();

        // Intervals along the each LAB axis to sample from when
        // constructing the starting colour subspace to sample from.
        float[] LValues = { 10.0f, 25.0f, 40.0f, 55.0f, 70.0f, 85.0f, 100.0f };
        float[] aValues = { -105.0f, -90.0f, -75.0f, -60.0f, -45.0f, -30.0f, -15.0f, 0.0f, 15.0f,
            30.0f, 45.0f, 60.0f, 75.0f, 90.0f };
        float[] bValues = { -105.0f, -90.0f, -75.0f, -60.0f, -45.0f, -30.0f, -15.0f, 0.0f, 15.0f,
            30.0f, 45.0f, 60.0f, 75.0f, 90.0f };

        foreach (float L in LValues)
        {
            foreach (float a in aValues)
            {
                foreach (float b in bValues)
                {
                    LabColor lab = new LabColor(L, a, b);
                    Color rgb = lab.ToColor();
                    if (IsWithinRGBGamut(rgb) && IsWithinLightnessRange(lab.L))
                    {
                        _colorSpace.Add(lab);
                    }
                }
            }
        }

        if (_colorSpace.Count == 0)
        {
            Debug.LogError("Colour space is empty after applying filters.");
        }
    }

    private bool IsWithinRGBGamut(Color rgb)
    {
        return rgb.r >= 0f && rgb.r <= 1f &&
               rgb.g >= 0f && rgb.g <= 1f &&
               rgb.b >= 0f && rgb.b <= 1f;
    }

    private bool IsWithinLightnessRange(float L)
    {
        return L >= _minLightness && L <= _maxLightness;
    }

    public List<Color> GeneratePalette(int paletteSize)
    {
        List<LabColor> palette = new List<LabColor>();

        // Initialise the palette with a starting colour
        LabColor startingColor = SelectStartingColor();
        if (startingColor.Equals(default(LabColor)))
        {
            Debug.LogError("Failed to select a starting colour.");
            return new List<Color>();
        }

        palette.Add(startingColor);
        _colorSpace.Remove(startingColor);
        PruneColorSpace(startingColor);

        while (palette.Count < paletteSize)
        {
            LabColor nextColor = SelectNextColor(palette);
            if (nextColor.Equals(default(LabColor)))
            {
                Debug.LogWarning("Ran out of colours to select.");
                break;
            }

            palette.Add(nextColor);
            _colorSpace.Remove(nextColor);
            PruneColorSpace(nextColor);
        }

        // Convert LAB colours back to Unity colours
        List<Color> unityPalette = palette.Select(lab => lab.ToColor()).ToList();
        return unityPalette;
    }

    private LabColor SelectStartingColor()
    {
        if (_colorSpace.Count == 0)
        {
            Debug.LogError("Colour space is empty.");
            return default;
        }

        int index = Random.Range(0, _colorSpace.Count);
        return _colorSpace[index];
    }

    private LabColor SelectNextColor(List<LabColor> currentPalette)
    {
        float bestScore = float.MinValue;
        LabColor bestColor = default;

        foreach (var candidate in _colorSpace)
        {
            // Skip if candidate is too similar to existing palette colours
            if (!IsNoticeablyDifferent(candidate, currentPalette))
                continue;

            // Calculate total score
            float score = CalculateTotalScore(candidate, currentPalette);

            if (score > bestScore)
            {
                bestScore = score;
                bestColor = candidate;
            }
        }

        if (bestScore == float.MinValue)
            return default; // No suitable candidate found

        return bestColor;
    }

    private bool IsNoticeablyDifferent(LabColor candidate, List<LabColor> palette)
    {
        float threshold = 2f; // Threshold for noticeable difference

        foreach (var existingColor in palette)
        {
            float de = ColorDifference.CIEDE2000(candidate, existingColor);
            if (de < threshold)
                return false; // Candidate is too similar
        }
        return true; // Candidate is noticeably different from all existing colours
    }

    private void PruneColorSpace(LabColor selectedColor)
    {
        _colorSpace.RemoveAll(c => !IsNoticeablyDifferent(c, new List<LabColor> { selectedColor }));
    }

    private float CalculateTotalScore(LabColor candidate, List<LabColor> palette)
    {
        float minDe = float.MaxValue;

        foreach (var existingColor in palette)
        {
            float de = ColorDifference.CIEDE2000(candidate, existingColor);
            if (de < minDe)
                minDe = de;
        }

        // Normalise minDe (since CIEDE2000 ranges approximately from 0 to 100)
        // Since higher color differences are better, we use deScore directly
        // and because we haven't actually implemented the other scorings.
        float deScore = minDe / 100f;
        float totalScore = _weights["ciede2000"] * deScore;

        // NOT IMPLEMENTED PAIR COMBINATION SCORES
        // float ppScore = ...;
        // totalScore += _weights["pairPreference"] * ppScore;

        return totalScore;
    }
}

// https://github.com/agi23-g8/peak-panic/blob/main/Assets/Scripts/Players/ColorPool.cs
[CreateAssetMenu(fileName = "ColorManager", menuName = "ScriptableObject/Manager/ColorManager")]
class ColorManager : ScriptableObject
{
    // Colour lists
    [SerializeField] private List<Color> m_colors = new List<Color>();
    [SerializeField] private int m_numColors = 16;
    private List<Color> m_availableColors = new List<Color>();
    private List<Color> m_usedColors = new List<Color>();

    public void ResetColors()
    {
        m_usedColors.Clear();
        m_availableColors.Clear();
        m_availableColors.AddRange(m_colors);
    }

    public Color GetColor()
    {
        if (m_availableColors.Count == 0)
        {
            Debug.LogWarning("Colour pool is empty and will be reset.");
            ResetColors();
        }

        // Get the first available colour
        Color allocatedColor = m_availableColors[0];

        // Move the colour from available to used list
        m_availableColors.RemoveAt(0);
        m_usedColors.Add(allocatedColor);

        return allocatedColor;
    }

    public Color PeekColor()
    {
        if (m_availableColors.Count == 0)
        {
            Debug.LogWarning("Colour pool is empty.");
            return Color.black;
        }

        // Get the first available color
        Color allocatedColor = m_availableColors[0];

        return allocatedColor;
    }

    // Really only used for the metrics of colour difference.
    public List<Color> GetColors()
    {
        return m_colors;
    }

    public void GenerateColors()
    {
        GenerateColors(m_numColors);
    }

    public void GenerateColors(int paletteSize)
    {
        // Define weights for scoring functions
        var weights = new Dictionary<string, float>
        {
            { "ciede2000", 1f },
            { "pairPreference", 0f }, // NOT IMPLEMENTED
        };

        PaletteGenerator generator = new PaletteGenerator(weights);
        m_colors = generator.GeneratePalette(paletteSize);

        ShuffleColors();
        ResetColors();
    }

    // Shuffle the colour array using Fisher-Yates unbiased shuffling algorithm.
    public void ShuffleColors()
    {
        int n = m_colors.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Color temp = m_colors[i];
            m_colors[i] = m_colors[j];
            m_colors[j] = temp;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ColorManager))]
public class ColorManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);

        ColorManager colorManager = target as ColorManager;

        if (GUILayout.Button("Generate Colours"))
        {
            colorManager.GenerateColors();

            List<Color> colors = colorManager.GetColors();
            ColourStatistics(colors, out float averageDifference, out float minDifference, out float maxDifference);
            Debug.Log($"Average CIEDE2000 difference: {averageDifference:F2}");
            Debug.Log($"Minimum CIEDE2000 difference: {minDifference:F2}");
            Debug.Log($"Maximum CIEDE2000 difference: {maxDifference:F2}");
            EditorUtility.SetDirty(colorManager);
        }
    }

    private void OnEnable()
    {
        // Reset the ColorManager when entering play mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        // Reset the ColorManager when leaving play mode
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ColorManager colorManager = target as ColorManager;
            colorManager.ResetColors();

            EditorUtility.SetDirty(colorManager);
        }
    }

    private void ColourStatistics(List<Color> colors, out float averageDifference, out float minDifference, out float maxDifference)
    {
        int count = colors.Count;
        if (count < 2)
        {
            averageDifference = 0f;
            minDifference = 0f;
            maxDifference = 0f;
            return;
        }

        float totalDifference = 0f;
        int numPairs = 0;
        minDifference = float.MaxValue;
        maxDifference = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            LabColor lab1 = LabColor.FromColor(colors[i]);
            for (int j = i + 1; j < count; j++)
            {
                LabColor lab2 = LabColor.FromColor(colors[j]);
                float difference = ColorDifference.CIEDE2000(lab1, lab2);
                totalDifference += difference;
                numPairs++;

                if (difference < minDifference)
                    minDifference = difference;

                if (difference > maxDifference)
                    maxDifference = difference;
            }
        }

        averageDifference = totalDifference / numPairs;
    }
}
#endif
