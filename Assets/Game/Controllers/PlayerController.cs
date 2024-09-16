using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(WeightComponent))]
public class PlayerController : MonoBehaviour
{
    [Header("Manager References")]
    private CharacterController m_characterController;
    private WeightComponent m_weightComponent;
    private Vector3 m_currentMovementVector;
    [SerializeField] private PlatformData _platformData;
    [SerializeField] private InputManager m_inputManager;

    [SerializeField] private GameObject _playerModel;

    [Space(5)]

    //* Player Attributes
    [Range(0f, 20f)]
    [SerializeField] private float m_walkingSpeed  = 1.0f;

    [Header("Gravity")]
    [SerializeField] private float m_gravity = -9.80f;
    [SerializeField] private float m_groundedGravity = -0.05f;
    [SerializeField] private bool m_isGrounded = true;
    [Header("Jump")]
    [SerializeField] private float m_maxJumpHeight = 1.0f;
    [SerializeField] private float m_maxJumpTime = 0.5f;

    [Header("Player Grounded")]
    [SerializeField] private float m_groundedOffset = 0.4f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float m_groundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask m_groundLayers;

    [Header("Read Only: Relative position")]
    [SerializeField] private Vector2 _relPosition;

    private bool m_isJumping = false;
    private float m_initialJumpVelocity;
    [SerializeField] private Vector3 currentLoc;
    [SerializeField] private Vector3 nextLoc;
    [SerializeField] private Vector3 directionVector;


    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_weightComponent = GetComponent<WeightComponent>();

        m_characterController.detectCollisions = true;
        m_currentMovementVector = Vector3.zero;
        _relPosition = Vector2.zero;
    }

    private void FixedUpdate()
    {
        CheckIfGrounded(); // currently unused
        Move();
        UpdateWeight();
    }

    private void UpdateModelDirection(Vector3 directionVector){
        _playerModel.transform.forward = directionVector;
    }

    private void UpdateWeight(){
        _relPosition.x = (currentLoc.x / _platformData.BoundsX);
        _relPosition.y = (currentLoc.z / _platformData.BoundsY);

        m_weightComponent.UpdateWeights(_relPosition);

    }

    private void Move()
    {
        m_currentMovementVector.z =  m_inputManager.Direction.y * m_walkingSpeed * Time.deltaTime;
        m_currentMovementVector.x =  m_inputManager.Direction.x * m_walkingSpeed * Time.deltaTime;

        nextLoc = currentLoc + m_currentMovementVector;


        // If change in movement Translate relative to localposition
        // If resulting position would be out of bounds set to closest in-bounds position
        if (nextLoc == currentLoc){return;}
        else {UpdateModelDirection(nextLoc - currentLoc);}

        if (nextLoc.z < _platformData.BoundsY && nextLoc.z > -_platformData.BoundsY) {
            transform.position += transform.forward * m_currentMovementVector.z;

            // m_characterController.Move(transform.forward * m_currentMovementVector.z);
        }
        else {
            nextLoc.z = _platformData.BoundsY * Math.Sign(nextLoc.z);
            transform.position += transform.forward * (nextLoc.z - currentLoc.z);

            // m_characterController.Move(transform.forward * (nextLoc.z - currentLoc.z));
        }
        currentLoc.z = nextLoc.z;

        if (nextLoc.x < _platformData.BoundsX && nextLoc.x > -_platformData.BoundsX){
            transform.position += transform.right * m_currentMovementVector.x;

            // m_characterController.Move(transform.right * m_currentMovementVector.x);
        } else {
            nextLoc.x = _platformData.BoundsX * Math.Sign(nextLoc.x);
            transform.position += transform.right * (nextLoc.x - currentLoc.x);

            // m_characterController.Move(transform.right * (nextLoc.x - currentLoc.x));
        }

        
        currentLoc.x = nextLoc.x;
    }


    // CURRENTLY UNUSED
    /*
    private void HandleJump()
    {
        if (!m_isJumping && m_inputManager.Jump && m_isGrounded)
        {
            m_isJumping = true;
            m_currentMovementVector.y = m_initialJumpVelocity;
        }
        else if (m_isJumping && !m_inputManager.Jump && m_isGrounded)
        {
            m_isJumping = false;
        }
    }
    */

    // CURRENTLY UNUSED
    private void ApplyGravity()
    {
        if (m_isGrounded)
            m_currentMovementVector.y = m_groundedGravity;
        else
            m_currentMovementVector.y += m_gravity * Time.deltaTime;
    }

    // CURRENTLY UNUSED
    private void CheckIfGrounded()
    {
        /*
        In this model the body lays on the [0,0,0]
        The radious of the character controller is 0.5
        The sphere radious is 0.5, and is elevated 0.4 units from the ground, meaning, 0.1 units are always in contact with the ground.
        */
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + m_groundedOffset, transform.position.z);
        Debug.DrawLine(transform.position, spherePosition, Color.yellow);
        m_isGrounded = Physics.CheckSphere(spherePosition, m_groundedRadius, m_groundLayers, QueryTriggerInteraction.Ignore);
    }
}
