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
    [SerializeField] private float cameraDefaultFov = 40f;
    [SerializeField] private float cameraTurboFov = 60f;


    void Update()
    {
        if (airPlaneController._isTurbo > 0) {
            ChangeCameraFov(cameraTurboFov);
        } else {
            ChangeCameraFov(cameraDefaultFov);
        }
        
    }

    public void ChangeCameraFov(float _fov)
    {
        float _deltatime = Time.deltaTime * 100f;
        freeLook.m_Lens.FieldOfView = Mathf.Lerp(freeLook.m_Lens.FieldOfView, _fov, 0.01f * _deltatime);
    }
}
