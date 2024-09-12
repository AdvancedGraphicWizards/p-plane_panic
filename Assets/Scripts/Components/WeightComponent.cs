using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeightComponent : MonoBehaviour
{
    [Header("Weight Value Modifiers (multiplied to end weight)")]
    [Range(0f, 10f)]
    public float rollWeightModifier = 1f;
    [Range(0f, 10f)]
    public float pitchWeightModifier = 1f;

    [Header("Weight Deadzone (weight not counted if under)")]
    [Range(0f, 1f)]
    [SerializeField] private float rollThreshold = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float pitchThreshold = 0f;

    // Final Stored weight values per axis
    public float RollWeight { get; private set; }
    public float PitchWeight { get; private set; }

    public static event Action OnWeightUpdate;


    private void Awake()
    {
        UpdateWeights(Vector2.zero);
    }

    // Update Weights based on given relative position
    // Input relPosition, vec2 <roll-axis, pitch-axis> position in range [-1,1]
    public void UpdateWeights(Vector2 relPosition) {
        if (Math.Abs(relPosition.x) < rollThreshold){
            RollWeight = 0;
        }
        else {
            RollWeight = (Math.Sign(relPosition.x)* (Math.Abs(relPosition.x)- rollThreshold) / (1 - rollThreshold) )* rollWeightModifier;
        }

        if (Math.Abs(relPosition.y) < pitchThreshold){
            PitchWeight = 0;
        }
        else {
            PitchWeight = (Math.Sign(relPosition.y)* (Math.Abs(relPosition.y)- pitchThreshold) / (1 - pitchThreshold) )* pitchWeightModifier;
        }

        OnWeightUpdate?.Invoke();
    }

}
