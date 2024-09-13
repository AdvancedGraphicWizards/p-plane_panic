using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlaneCameraController : MonoBehaviour
{
    private CinemachineBrain brain;

    [Header("References")]
    [SerializeField] private PlaneControllerFixed airPlaneController;
    [SerializeField] private CinemachineFreeLook freeLook;

    [Header("Camera values")]
    [SerializeField] private float cameraDefaultFov = 60f;
    [SerializeField] private float cameraTurboFov = 40f;
    
}
