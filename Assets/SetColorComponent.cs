using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component to set the color of a target material
/// </summary>

public class SetColorComponent : MonoBehaviour
{
    [SerializeField] private FeathersController feathersController;

    public void SetColor(Color color)
    {
        feathersController.featherColor = color;
    }
}
