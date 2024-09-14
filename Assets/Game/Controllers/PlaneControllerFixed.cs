using UnityEngine;
using System;

public class PlaneControllerFixed : MonoBehaviour
{
    [Header("Scriptable Object References")]

    [Tooltip("Holds the values of pre-runtime variables, this do not change during gameplay")]
    [SerializeField] private GameSettings _gameSettings;
    [Tooltip("Holds the state of runtime varibales")]
    [SerializeField] private GameState gameStateSO;
    [Header("Manager References")]
    [SerializeField] private WeightManager _weightManager;

    [Header("Plane Attributes")]
    [Tooltip("The Forward Speed is controll on a higher level via GameSettings scriptable object")]
    [SerializeField] private float _forwardSpeed;
    [SerializeField] private float _rollRotationSpeed = 5f;
    [SerializeField] private float _pitchRotationSpeed = 5f;
    [SerializeField] private float _maxHorizontalSpeed = 10f;
    [SerializeField] private float _maxVerticalSpeed = 10f;
    [Range(0f, 90f)]
    [SerializeField] private float _maxRollAngle = 40f;
    [Range(0f, 90f)]
    [SerializeField] private float _maxPitchAngle = 40f;
    [Range(0f, 100f)]
    [SerializeField] private float _weightDampingSpeed = 5;
    [SerializeField] private float _weightDampingThreshold = 0.1f; // weight threshold to begin Damping to zero

    [Header("Read Only")]
    [SerializeField] private float _horizSpeed = 0f;
    [SerializeField] private float _vertSpeed = 0f;
    [SerializeField] private float _rollChange = 0f;
    [SerializeField] private float _pitchChange = 0f;
    [SerializeField] private float _currentPitchAngle = 0f;
    [SerializeField] private float _currentRollAngle = 0f;
    [SerializeField] private float _nextPitchAngle = 0f;
    [SerializeField] private float _nextRollAngle = 0f;
    [SerializeField] private float _totalRollWeight = 0f;
    [SerializeField] private float _totalPitchWeight = 0f;

    private void Start()
    {
        if (_gameSettings)
            _forwardSpeed = _gameSettings.PlaneBaseSpeed;
        else
            _forwardSpeed = 20f;
        //TODO include the exception, once we agree on using the SO
        //throw new NullReferenceException("GameSettings reference is missing, this context needs it to define the speed of the plane");
        if (!gameStateSO)
            throw new NullReferenceException("Missing GameState, HelloWorld purposes");

        gameStateSO.HasStarted = false; //TODO This should be done by a hight entity, a singleton probably that handles the GamePlay.

    }
    private void FixedUpdate()
    {
        if (!gameStateSO.HasStarted) return;
        RotatePlane();
        MovePlane();
    }

    private void RotatePlane()
    {
        _totalRollWeight = _weightManager.TotalRollWeight;
        _totalPitchWeight = _weightManager.TotalPitchWeight;

        // if (_totalRollWeight < _weightDampingThreshold) {
        //     _nextRollAngle += _weightDampingSpeed * Math.Sign(_rollChange) * Time.deltaTime;
        // }

        // if (_totalPitchWeight < _weightDampingThreshold) {
        //     _nextPitchAngle += -_weightDampingSpeed * Math.Sign(_pitchChange) * Time.deltaTime;
        // }


        _rollChange = _totalRollWeight * Time.deltaTime;
        _pitchChange = _totalPitchWeight * Time.deltaTime;

        _nextRollAngle += -_rollChange * _rollRotationSpeed;
        _nextPitchAngle += _pitchChange * _pitchRotationSpeed;



        // Clamp rotations to maximum defined values
        if (_nextRollAngle == _currentRollAngle && _nextPitchAngle == _currentPitchAngle) return;

        if (_nextRollAngle < _maxRollAngle && _nextRollAngle > -_maxRollAngle)
        {
            transform.Rotate(Vector3.forward * -_rollChange * _rollRotationSpeed);
        }
        else
        {
            transform.Rotate(Vector3.forward * (_maxRollAngle * Math.Sign(_nextRollAngle) - _currentRollAngle));
            _nextRollAngle = _maxRollAngle * Math.Sign(_nextRollAngle);
        }
        _currentRollAngle = _nextRollAngle;

        if (_nextPitchAngle < _maxPitchAngle && _nextPitchAngle > -_maxPitchAngle)
        {
            transform.Rotate(Vector3.right * _pitchChange * _pitchRotationSpeed);
        }
        else
        {
            transform.Rotate(Vector3.right * (_maxPitchAngle * Math.Sign(_nextPitchAngle) - _currentPitchAngle));
            _nextPitchAngle = _maxPitchAngle * Math.Sign(_nextPitchAngle);
        }
        _currentPitchAngle = _nextPitchAngle;
    }

    private void MovePlane()
    {
        _horizSpeed = _currentRollAngle / _maxRollAngle * _maxHorizontalSpeed;
        _vertSpeed = _currentPitchAngle / _maxPitchAngle * _maxVerticalSpeed;

        transform.Translate((Vector3.forward * _forwardSpeed + Vector3.right * -_horizSpeed + Vector3.up * -_vertSpeed) * Time.deltaTime);
    }
}
