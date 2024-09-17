using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlaneCameraController : MonoBehaviour
{
    private CinemachineBrain brain;

    [Header("References")]
    [SerializeField] private PlaneControllerFixed _airPlaneController;
    // [SerializeField] private CinemachineFreeLook _freeLook;
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
        if (_airPlaneController._isTurbo > 0) {
            ChangeCameraFov(cameraTurboFov);
        } else {
            ChangeCameraFov(cameraDefaultFov);
        }
    }

    public void ChangeCameraFov(float _fov)
    {
        float _deltatime = Time.deltaTime * 100f;
        // _freeLook.m_Lens.FieldOfView = Mathf.Lerp(_freeLook.m_Lens.FieldOfView, _fov, 0.01f * _deltatime);
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _fov, 0.01f * _deltatime);
    }
}
