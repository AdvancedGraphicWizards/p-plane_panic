using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component to set the color of a target material
/// </summary>

public class SetColorComponent : MonoBehaviour
{
    [SerializeField] private Renderer[] m_colorTargets;

    public void SetColor(Color color) {
        foreach (Renderer target in m_colorTargets) {
            if (target.material != null)
                target.material.SetColor("_BaseColor", color);
            else Debug.LogError("Targeted Renderer has no material");
        }
    }
}
