using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;

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

    [Header("Plane Attributes")]
    [Tooltip("The Forward Speed is controll on a higher level via GameSettings scriptable object")]
    [SerializeField] private float _forwardSpeed = 120f;
    [SerializeField] private float _maxHorizontalSpeed = 10f;
    [SerializeField] private float _maxVerticalSpeed = 10f;
    [SerializeField] private float _accelerating = 1f;
    [SerializeField] private float _decelerating = 1f;
    [SerializeField] private float rollSpeed = 1f;
    [SerializeField] private float pitchSpeed = 1f;

    [Range(0f, 90f)]
    [SerializeField] private float maxRoll = 30f;
    [Range(0f, 90f)]
    [SerializeField] private float maxPitch = 30f;
    [SerializeField] private float _turboTime = 4f;
    [SerializeField] private float _turboSpeed = 2f;
    [Range(0f, 100f)]
    [SerializeField] private float _weightDampingSpeed = 5;
    [SerializeField] private float _weightDampingThreshold = 0.1f; // weight threshold to begin Damping to zero

    [Header("Audio Settings")]
    [SerializeField] private AudioSource engineSoundSource;
    [SerializeField] private float maxEngineSound = 1f;
    [SerializeField] private float defaultSoundPitch = 1f;
    [SerializeField] private float turboSoundPitch = 1.5f;

    [Header("Read Only")]
    private float _maxForwardSpeed = 0f;
    private float _maxHorizSpeed = 0f;
    private float _maxVertSpeed = 0f;
    private float currPitch = 0f;
    private float currRoll = 0f;
    public int _isTurbo = 0;
    private float currentEngineSoundPitch = 0f;

    private void Start()
    {
        if (_gameSettings)
            _forwardSpeed = _gameSettings.PlaneBaseSpeed;
        else
            _forwardSpeed = 20f;

        _maxHorizSpeed = _maxHorizontalSpeed;
        _maxVertSpeed = _maxVerticalSpeed;
        _maxForwardSpeed = _forwardSpeed;
        currentEngineSoundPitch = defaultSoundPitch;

        HoopScript.OnRingEnter += amt => ActivateTurbo(amt);

        //TODO include the exception, once we agree on using the SO
        //throw new NullReferenceException("GameSettings reference is missing, this context needs it to define the speed of the plane");
        if (!gameStateSO)
            throw new NullReferenceException("Missing GameState, HelloWorld purposes");

        gameStateSO.HasStarted = false; //TODO This should be done by a hight entity, a singleton probably that handles the GamePlay.

    }
    private void FixedUpdate()
    {
        if (!gameStateSO.HasStarted) return;
        AudioSystem();
        RotatePlane();
        MovePlane();
    }


    // Rotate the plane transform about the z and x axes according to weight values
    private void RotatePlane()
    {
        // Calculate the new roll and pitch values
        currRoll = Mathf.Lerp(currRoll, -_weightManager.TotalRollWeight * maxRoll, rollSpeed * Time.deltaTime);
        currPitch = Mathf.Lerp(currPitch, _weightManager.TotalPitchWeight * maxPitch, pitchSpeed * Time.deltaTime);

        // Rotate the plane
        transform.localRotation = Quaternion.Euler(currPitch, 0, currRoll);

        // Speed up or slow down the plane
        _forwardSpeed += (_forwardSpeed < _maxForwardSpeed ? _accelerating : -_decelerating) * Time.deltaTime;
        _maxHorizontalSpeed += (_maxHorizontalSpeed < _maxHorizSpeed ? _accelerating : -_decelerating) * Time.deltaTime;
        _maxVerticalSpeed += (_maxVerticalSpeed < _maxVertSpeed ? _accelerating : -_decelerating) * Time.deltaTime;
    }

    // Activates turbo boost, strength modifier currently unused
    public void ActivateTurbo(float strength) {
        StartCoroutine(TurboBoost());
    }

    // Coroutine that updates the speed and sounds values when boosting
    private IEnumerator TurboBoost() {
        _isTurbo ++;
        currentEngineSoundPitch = turboSoundPitch;
        _maxForwardSpeed += _turboSpeed * 2;
        _maxHorizSpeed += _turboSpeed;
        _maxVertSpeed += _turboSpeed;
        yield return new WaitForSeconds(_turboTime);
        _maxForwardSpeed -= _turboSpeed * 2;
        _maxHorizSpeed -= _turboSpeed;
        _maxVertSpeed -= _turboSpeed;
        currentEngineSoundPitch = defaultSoundPitch;
        _isTurbo --;
    }

    // Update speed values according to current roll and pitch
    // Then move plane forward via translation according to speed values
    private void MovePlane()
    {
        float _horizSpeed = currRoll / maxRoll * _maxHorizontalSpeed;
        float _vertSpeed = currPitch / maxPitch * _maxVerticalSpeed;

        transform.Translate(new Vector3(-_horizSpeed, -_vertSpeed, _forwardSpeed) * Time.deltaTime);
    }

    // Update engine sound depending on pitch and volume variables
    private void AudioSystem(){
        if (engineSoundSource != null) {
            engineSoundSource.pitch = Mathf.Lerp(engineSoundSource.pitch, currentEngineSoundPitch, 10f * Time.deltaTime);
            engineSoundSource.volume = Mathf.Lerp(engineSoundSource.volume, maxEngineSound, 1f * Time.deltaTime);
        }
    }
}
