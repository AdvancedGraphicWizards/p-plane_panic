using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public enum AirplaneState
    {
        Flying,
        Landing,
        Takeoff,
    }

    private float maxSpeed = 0.6f;
    private float currentYawSpeed;
    private float currentPitchSpeed;
    private float currentRollSpeed;
    private float currentSpeed;
    private bool planeIsDead = false;

    [SerializeField] private Rigidbody rb;

    public AirplaneState airplaneState;

    [Header("Player input targets and current")]
    public float rollRotation;
    public float pitchRotation;
    [SerializeField] public float rollChange;
    [SerializeField] public float pitchChange;

    [Header("Rotating speeds")]
    [Range(5f, 500f)]
    [SerializeField] private float yawSpeed = 50f;

    [Range(5f, 500f)]
    [SerializeField] private float pitchSpeed = 100f;

    [Range(5f, 500f)]
    [SerializeField] private float rollSpeed = 200f;

    [Header("Rotating speeds multiplers (Turbo Active)")]
    [SerializeField] private bool turbo = false;

    [Range(0.1f, 5f)]
    [SerializeField] private float yawTurboMultiplier = 0.3f;

    [Range(0.1f, 5f)]
    [SerializeField] private float pitchTurboMultiplier = 0.5f;

    [Range(0.1f, 5f)]
    [SerializeField] private float rollTurboMultiplier = 1f;

    [Header("Moving speed")]
    [Range(5f, 100f)]
    [SerializeField] private float defaultSpeed = 10f;

    [Range(10f, 200f)]
    [SerializeField] private float turboSpeed = 20f;

    [Range(0.1f, 50f)]
    [SerializeField] private float accelerating = 10f;

    [Range(0.1f, 50f)]
    [SerializeField] private float deaccelerating = 5f;


    [Header("Sideway force")]
    [Range(0.1f, 15f)]
    [SerializeField] private float sidewaysMovement = 15f;

    [Range(0.001f, 0.05f)]
    [SerializeField] private float sidewaysMovementXRot = 0.012f;

    [Range(0.1f, 5f)]
    [SerializeField] private float sidewaysMovementYRot = 1.5f;

    [Range(-1, 1f)]
    [SerializeField] private float sidewaysMovementYPos = 0.1f;



    private void Start()
    {
        //Setup speeds
        maxSpeed = defaultSpeed;
        currentSpeed = defaultSpeed;

        //Get and set rigidbody
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

    }

    private void Update()
    {
        //AudioSystem();

        switch (airplaneState)
        {
            case AirplaneState.Flying:
                FlyingUpdate();
                break;

            case AirplaneState.Landing:
                //LandingUpdate();
                break;

            case AirplaneState.Takeoff:
                //TakeoffUpdate();
                break;
        }
    }

    private void FlyingUpdate()
    {
        if (!planeIsDead)
        {
            Movement();
            SidewaysForceCalculation();
        }
    }


    // controls sideway force from roll rotation
    private void SidewaysForceCalculation()
    {
        float _mutiplierXRot = sidewaysMovement * sidewaysMovementXRot;
        float _mutiplierYRot = sidewaysMovement * sidewaysMovementYRot;

        float _mutiplierYPos = sidewaysMovement * sidewaysMovementYPos;

        //Right side 
        if (transform.localEulerAngles.z > 270f && transform.localEulerAngles.z < 360f)
        {
            float _angle = (transform.localEulerAngles.z - 270f) / (360f - 270f);
            float _invert = 1f - _angle;

            transform.Rotate(Vector3.up * (_invert * _mutiplierYRot) * Time.deltaTime);
            transform.Rotate(Vector3.right * (-_invert * _mutiplierXRot) * currentPitchSpeed * Time.deltaTime);

            transform.Translate(transform.up * (_invert * _mutiplierYPos) * Time.deltaTime);
        }

        //Left side
        if (transform.localEulerAngles.z > 0f && transform.localEulerAngles.z < 90f)
        {
            float _angle = transform.localEulerAngles.z / 90f;

            transform.Rotate(-Vector3.up * (_angle * _mutiplierYRot) * Time.deltaTime);
            transform.Rotate(Vector3.right * (-_angle * _mutiplierXRot) * currentPitchSpeed * Time.deltaTime);

            transform.Translate(transform.up * (_angle * _mutiplierYPos) * Time.deltaTime);
        }

        //Right side down
        if (transform.localEulerAngles.z > 90f && transform.localEulerAngles.z < 180f)
        {
            float _angle = (transform.localEulerAngles.z - 90f) / (180f - 90f);
            float _invert = 1f - _angle;

            transform.Translate(transform.up * (_invert * _mutiplierYPos) * Time.deltaTime);
            transform.Rotate(Vector3.right * (-_invert * _mutiplierXRot) * currentPitchSpeed * Time.deltaTime);
        }

        //Left side down
        if (transform.localEulerAngles.z > 180f && transform.localEulerAngles.z < 270f)
        {
            float _angle = (transform.localEulerAngles.z - 180f) / (270f - 180f);

            transform.Translate(transform.up * (_angle * _mutiplierYPos) * Time.deltaTime);
            transform.Rotate(Vector3.right * (-_angle * _mutiplierXRot) * currentPitchSpeed * Time.deltaTime);
        }
    }


    private void Movement()
    {
        //Move forward
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        //Rotate airplane by inputs
        //Debug.Log($"RC{rollChange}, PC{pitchChange}");
        transform.Rotate(Vector3.forward * -rollChange * currentRollSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right * pitchChange * currentPitchSpeed * Time.deltaTime);


        //Accelerate and deacclerate
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += accelerating * Time.deltaTime;
        }
        else
        {
            currentSpeed -= deaccelerating * Time.deltaTime;
        }

        //Turbo
        if (turbo)
        {
            maxSpeed = turboSpeed;
            currentPitchSpeed = pitchSpeed * pitchTurboMultiplier;
            currentRollSpeed = rollSpeed * rollTurboMultiplier;

        }
        else
        {
            maxSpeed = defaultSpeed;
            currentPitchSpeed = pitchSpeed;
            currentRollSpeed = rollSpeed;
        }
    }

}
