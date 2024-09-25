using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetColorComponent : MonoBehaviour
{
    [SerializeField] private FeathersController feathersController;

    public void SetColor(Color color)
    {
        feathersController.featherColor = color;
    }
}
