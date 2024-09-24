using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetColorComponent : MonoBehaviour
{
    [SerializeField] private Renderer[] m_colorTargets;

    public void SetColor(Color color) {
        foreach (Renderer target in m_colorTargets) {
            target.material.SetColor("_BaseColor", color);
        }
    }
}
