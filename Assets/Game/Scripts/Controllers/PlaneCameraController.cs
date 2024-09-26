using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Controls the Camera attached to the plane object
/// </summary>

public class PlaneCameraController : MonoBehaviour
{
    private CinemachineBrain brain;

    [Header("References")]
    [SerializeField] private PlaneControllerFixed _airPlaneController;
    [SerializeField] private Camera _mainCamera;

    [Header("Camera values")]
    [SerializeField] private float cameraDefaultFov = 40f;
    [SerializeField] private float cameraTurboFov = 60f;


    private void Start()
    {
        ChangeCameraFov(cameraDefaultFov);
    }

    private void Update()
    {
        // Change camera FOV while Turbo
        if (_airPlaneController._isTurbo > 0) {
            ChangeCameraFov(cameraTurboFov);
        } else {
            ChangeCameraFov(cameraDefaultFov);
        }
    }

    public void ChangeCameraFov(float _fov)
    {
        float _deltatime = Time.deltaTime * 100f;
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _fov, 0.01f * _deltatime);
    }
}
