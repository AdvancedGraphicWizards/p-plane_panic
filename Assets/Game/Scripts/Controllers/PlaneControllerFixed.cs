using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// The controller for the Plane object
/// </summary>

public class PlaneControllerFixed : MonoBehaviour
{
    [Header("Scriptable Object References")]

    [Tooltip("Holds the values of pre-runtime variables, this do not change during gameplay")]
    [SerializeField] private GameSettings _gameSettings;
    [Tooltip("Holds the state of runtime varibales")]
    [SerializeField] private GameState gameStateSO;
    [Header("Manager References")]
    [SerializeField] private WeightManager _weightManager;
    [SerializeField] private PlaneCameraController _planeCameraController;

    [Header("Plane Particle effects & Animation")]
    [SerializeField] private Animation m_bladeRotateAnim;
    [SerializeField] private ParticleSystem[] m_exhaustParticles;
    [SerializeField] private Material m_SpeedLines;

    [Header("Plane Attributes")]
    [Tooltip("The Forward Speed is controll on a higher level via GameSettings scriptable object")]
    [SerializeField] private float maxForwardSpeed = 20f;
    [SerializeField] private float maxHorizontalSpeed = 10f;
    [SerializeField] private float maxVerticalSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deacceleration = 1f;
    [SerializeField] private float startLossOfControlTime = 1.33f;

    [Header("Plane tilt Settings")]
    [SerializeField] private float rollSpeed = 1f;
    [Range(0f, 90f)][SerializeField] private float maxRoll = 40f;
    [SerializeField] private float pitchSpeed = 1f;
    [Range(0f, 90f)][SerializeField] private float maxPitch = 30f;

    [Header("Turbo Settings")]
    [SerializeField] private float _turboTime = 4f;
    [SerializeField] private float _turboSpeed = 4f;

    [Header("Camera values")]
    [SerializeField] private float cameraDefaultFov = 50f;
    [SerializeField] private float cameraTurboFov = 65f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource engineSoundSource;
    [SerializeField] private AudioSource alarmSoundSource;
    [SerializeField] private float maxEngineSound = 1f;
    [SerializeField] private float defaultSoundPitch = 1f;
    [SerializeField] private float turboSoundPitch = 1.5f;

    // Plane variables
    private float startPosition = 0f;
    private float baseSpeed = 0f;
    private float forwardSpeed = 0f;
    private float horizontalSpeed = 0f;
    private float verticalSpeed = 0f;
    private float currPitch = 0f;
    private float currRoll = 0f;
    public int _isTurbo = 0;
    private float currentEngineSoundPitch = 0f;
    private bool m_recoverState = false;
    private bool m_startLossOfControl = false;

    // Turbo variables
    private float m_turboTimer = 0f;
    private float m_maxForwardSpeed = 20f;
    private float m_maxHorizontalSpeed = 10f;
    private float m_maxVerticalSpeed = 10f;

    private void Start() {   
        // Check for missing references
        if (!gameStateSO)
            throw new NullReferenceException("Missing GameState");

        // Set the initial values of the plane
        maxForwardSpeed = _gameSettings ? _gameSettings.PlaneBaseSpeed : 20f;
        forwardSpeed = maxForwardSpeed;
        baseSpeed = maxForwardSpeed;
        currentEngineSoundPitch = defaultSoundPitch;

        // Add the turbo callback on ring enter
        HoopScript.OnRingEnter += ActivateTurbo;

        // Set the game state to not starteda
        gameStateSO.HasStarted = false;
        startPosition = transform.position.z;
    }

    private void Update() {

        // Update variables based on boost and increase base speed based on distance
        m_turboTimer = Mathf.Max(0, m_turboTimer - Time.deltaTime);
        if (m_turboTimer == 0) { _isTurbo = 0; }
        m_maxForwardSpeed = (maxForwardSpeed + _turboSpeed * _isTurbo);
        m_maxHorizontalSpeed = (maxHorizontalSpeed + _turboSpeed * _isTurbo / 2);
        m_maxVerticalSpeed = (maxVerticalSpeed + _turboSpeed * _isTurbo / 2);

        // Update plane rotation and movement
        if (gameStateSO.HasStarted) {
            RotatePlane();
            MovePlane();
        }

        // Update the engine sound, particles and animations
        if (gameStateSO.OutOfFuel && !m_recoverState) {
            foreach (ParticleSystem ps in m_exhaustParticles) { ps.Stop(); }
            m_bladeRotateAnim.Stop();
            m_recoverState = true;
            if (gameStateSO.HasStarted){
                alarmSoundSource.volume = 0.3f;
                alarmSoundSource.Play();
            }
        } else if (!gameStateSO.OutOfFuel && m_recoverState) {
            foreach (ParticleSystem ps in m_exhaustParticles) { ps.Play(); }
            m_bladeRotateAnim.Play();
            m_recoverState = false;
            alarmSoundSource.Stop();
        }

        // Update the engine sound
        if (!gameStateSO.OutOfFuel) {
            if (engineSoundSource != null) {
                currentEngineSoundPitch = (forwardSpeed - baseSpeed) / _turboSpeed * turboSoundPitch + defaultSoundPitch;
                engineSoundSource.pitch = Mathf.Lerp(engineSoundSource.pitch, currentEngineSoundPitch, 10f * Time.deltaTime);
                engineSoundSource.volume = Mathf.Lerp(engineSoundSource.volume, maxEngineSound, 1f * Time.deltaTime);
            }
        }
        else {
            if (alarmSoundSource.isPlaying) { alarmSoundSource.pitch += 0.1f * Time.deltaTime;}
        }

        // Update the camera FOV
        Camera.main.fieldOfView = cameraDefaultFov + Mathf.Max(0, Mathf.Log(forwardSpeed - baseSpeed, 10)) * (cameraTurboFov - cameraDefaultFov);

        // Update speed lines screen effect
        m_SpeedLines.SetFloat("_SpeedLinesEnabled", (forwardSpeed - baseSpeed) / _turboSpeed);
    }

    // Rotate the plane transform about the z and x axes according to weight values
    private void RotatePlane() {

        // Lose pitch control when out of fuel
        if (!m_startLossOfControl) {
            currRoll = Mathf.Lerp(currRoll, -_weightManager.TotalRollWeight * maxRoll, rollSpeed * Time.deltaTime);
            currPitch = Mathf.Lerp(currPitch, (gameStateSO.OutOfFuel ? 1 : _weightManager.TotalPitchWeight) * maxPitch, pitchSpeed * Time.deltaTime);
        }

        // Rotate the plane
        transform.localRotation = Quaternion.Euler(currPitch, 0, currRoll);

        // Speed up or slow down the plane
        forwardSpeed = Mathf.Lerp(forwardSpeed, m_maxForwardSpeed, (forwardSpeed <= m_maxForwardSpeed ? acceleration : deacceleration) * Time.deltaTime);
        horizontalSpeed += (horizontalSpeed <= m_maxHorizontalSpeed ? acceleration : -deacceleration) * Time.deltaTime;
        verticalSpeed += (verticalSpeed <= m_maxVerticalSpeed ? acceleration : -deacceleration) * Time.deltaTime;
    }

    // Moves the plane forward
    private void MovePlane() {
        transform.Translate(
            new Vector3(
                -currRoll / maxRoll * horizontalSpeed, 
                -currPitch / maxPitch * verticalSpeed, 
                forwardSpeed
            ) * Time.deltaTime
        );
    }

    // Activates turbo boost, strength modifier currently unused
    public void ActivateTurbo(float strength) {
        m_turboTimer = _turboTime;
        _isTurbo = Mathf.Min(3, _isTurbo + 1);
    }

    public void ActivateLossOfControl() {
        StartCoroutine(InitialLossOfControl());
    }

    private IEnumerator InitialLossOfControl() {
        m_startLossOfControl = true;
        yield return new WaitForSeconds(startLossOfControlTime);
        m_startLossOfControl = false;
    }

    void OnDestroy() {
        HoopScript.OnRingEnter -= ActivateTurbo;
    }
}
