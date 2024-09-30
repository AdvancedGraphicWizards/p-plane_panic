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


    [Header("Plane Attributes")]
    [Tooltip("The Forward Speed is controll on a higher level via GameSettings scriptable object")]
    [SerializeField] private float maxForwardSpeed = 20f;
    [SerializeField] private float maxHorizontalSpeed = 10f;
    [SerializeField] private float maxVerticalSpeed = 10f;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float deacceleration = 1f;
    [SerializeField] private float startLossOfControlTime = 1.33f;

    [Header("Plane tilt Settings")]
    [SerializeField] private float rollSpeed = 1f;
    [Range(0f, 90f)][SerializeField] private float maxRoll = 40f;
    [SerializeField] private float pitchSpeed = 1f;
    [Range(0f, 90f)][SerializeField] private float maxPitch = 30f;

    [Header("Turbo Settings")]
    [SerializeField] private float _turboTime = 4f;
    [SerializeField] private float _turboSpeed = 2f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource engineSoundSource;
    [SerializeField] private float maxEngineSound = 1f;
    [SerializeField] private float defaultSoundPitch = 1f;
    [SerializeField] private float turboSoundPitch = 1.5f;

    // Plane variables
    private float forwardSpeed = 0f;
    private float horizontalSpeed = 0f;
    private float verticalSpeed = 0f;
    private float currPitch = 0f;
    private float currRoll = 0f;
    public int _isTurbo = 0;
    private float currentEngineSoundPitch = 0f;
    private bool m_recoverState = false;
    private bool m_startLossOfControl = false;

    private void Start()
    {
        if (_gameSettings)
            maxForwardSpeed = _gameSettings.PlaneBaseSpeed;
        else
            maxForwardSpeed = 20f;

        // Set the initial values of the plane
        forwardSpeed = maxForwardSpeed;
        currentEngineSoundPitch = defaultSoundPitch;

        // Add the turbo callback on ring enter
        Debug.Log("PlaneControllerFixed created");
        HoopScript.OnRingEnter += ActivateTurbo;

        //TODO include the exception, once we agree on using the SO
        //throw new NullReferenceException("GameSettings reference is missing, this context needs it to define the speed of the plane");
        if (!gameStateSO)
            throw new NullReferenceException("Missing GameState");

        gameStateSO.HasStarted = false;
    }

    private void FixedUpdate()
    {

        if (!gameStateSO.OutOfFuel) AudioSystem(); // dont play engine sounds when out of fuel
        if (gameStateSO.HasStarted && m_startLossOfControl)
        {
            // Make sure plane cannot move for the first few seconds
            currPitch = 0f;
            currRoll = 0f;
            MovePlane();
        }
        else if (gameStateSO.HasStarted)
        {
            RotatePlane();
            MovePlane();
        }
    }

    // Rotate the plane transform about the z and x axes according to weight values
    private void RotatePlane()
    {
        if (gameStateSO.OutOfFuel)
        {
            // If we are out of fuel then we lose pitch control
            currRoll = Mathf.Lerp(currRoll, -_weightManager.TotalRollWeight * maxRoll, rollSpeed * Time.deltaTime);
            currPitch = Mathf.Lerp(currPitch, maxPitch, pitchSpeed * Time.deltaTime);

            // If we are out of fuel disable blade rotation, exhuast particles and engineAudio
            if (!m_recoverState)
            {
                m_bladeRotateAnim.Stop();
                foreach (ParticleSystem ps in m_exhaustParticles)
                {
                    ps.Stop();
                }
                m_recoverState = true;
            }
        }
        else
        {
            // Calculate the new roll and pitch values
            currRoll = Mathf.Lerp(currRoll, -_weightManager.TotalRollWeight * maxRoll, rollSpeed * Time.deltaTime);
            currPitch = Mathf.Lerp(currPitch, _weightManager.TotalPitchWeight * maxPitch, pitchSpeed * Time.deltaTime);

            // If we are recovering switch on all of the special effects
            if (m_recoverState)
            {
                m_bladeRotateAnim.Play();
                foreach (ParticleSystem ps in m_exhaustParticles)
                {
                    ps.Play();
                }
                m_recoverState = false;
            }
        }

        // Rotate the plane
        transform.localRotation = Quaternion.Euler(currPitch, 0, currRoll);

        // Speed up or slow down the plane
        forwardSpeed += (forwardSpeed < maxForwardSpeed ? acceleration : -deacceleration) * Time.deltaTime;
        horizontalSpeed += (horizontalSpeed < maxHorizontalSpeed ? acceleration : -deacceleration) * Time.deltaTime;
        verticalSpeed += (verticalSpeed < maxVerticalSpeed ? acceleration : -deacceleration) * Time.deltaTime;
    }

    // Activates turbo boost, strength modifier currently unused
    public void ActivateTurbo(float strength)
    {
        StartCoroutine(TurboBoost());
    }

    // Coroutine that updates the speed and sounds values when boosting
    private IEnumerator TurboBoost()
    {
        _isTurbo++;
        currentEngineSoundPitch = turboSoundPitch;
        forwardSpeed += _turboSpeed * 2;
        maxHorizontalSpeed += _turboSpeed;
        maxVerticalSpeed += _turboSpeed;
        yield return new WaitForSeconds(_turboTime);
        forwardSpeed -= _turboSpeed * 2;
        maxHorizontalSpeed -= _turboSpeed;
        maxVerticalSpeed -= _turboSpeed;
        currentEngineSoundPitch = defaultSoundPitch;
        _isTurbo--;
    }

    public void ActivateLossOfControl()
    {
        StartCoroutine(InitialLossOfControl());
    }

    private IEnumerator InitialLossOfControl()
    {
        m_startLossOfControl = true;
        yield return new WaitForSeconds(startLossOfControlTime);
        m_startLossOfControl = false;
    }

    // Update speed values according to current roll and pitch
    // Then move plane forward via translation according to speed values
    private void MovePlane()
    {
        float _horizSpeed = currRoll / maxRoll * maxHorizontalSpeed;
        float _vertSpeed = currPitch / maxPitch * maxVerticalSpeed;
        transform.Translate(new Vector3(-_horizSpeed, -_vertSpeed, forwardSpeed) * Time.deltaTime);
    }

    // Update engine sound depending on pitch and volume variables
    private void AudioSystem()
    {
        if (engineSoundSource != null)
        {
            engineSoundSource.pitch = Mathf.Lerp(engineSoundSource.pitch, currentEngineSoundPitch, 10f * Time.deltaTime);
            engineSoundSource.volume = Mathf.Lerp(engineSoundSource.volume, maxEngineSound, 1f * Time.deltaTime);
        }
    }

    void OnDestroy()
    {
        Debug.Log("PlaneControllerFixed destroyed");
        HoopScript.OnRingEnter -= ActivateTurbo;
    }
}
