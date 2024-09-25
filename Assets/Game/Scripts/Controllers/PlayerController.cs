using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Manager References")]
    private CharacterController m_characterController;
    [SerializeField] private WeightComponent m_weightComponent;
    private Vector3 m_currentMovementVector = Vector3.zero;
    [SerializeField] private PlatformData _platformData;
    [SerializeField] private InputManager m_inputManager;
    [SerializeField] private GameObject _playerModel;

    [Space(5)]

    [Header("Player Attributes")]
    [Range(0f, 20f)]
    [SerializeField] private float m_walkingSpeed = 1.0f;
    [SerializeField] private float m_maxJumpHeight = 1.0f;

    [Header("Gravity")]
    [SerializeField] private float m_gravity = -9.80f;
    [SerializeField] private float m_groundedGravity = -0.05f;

    [Header("Player Grounded")]
    [SerializeField] private float m_groundedOffset = 0.4f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float m_groundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask m_groundLayers;

    private bool m_isGrounded = true;
    private bool m_isJumping = false;
    private float m_initialJumpVelocity;

    // Player movement data
    private Transform m_offsetTransform;
    private Vector3 relativePos;

    private void Awake()
    {
        if (m_characterController == null)
            m_characterController = GetComponent<CharacterController>();

        if (m_weightComponent == null)
            m_weightComponent = GetComponent<WeightComponent>();

        m_characterController.detectCollisions = true;

        // TEMPORARY: Find the platform object and set the offset to its position
        if (m_offsetTransform == null)
        {
            if (GameObject.Find("Plane") != null)
            {
                m_offsetTransform = GameObject.Find("Plane").transform;
                InitPlayerPosition();
            }
        }
    }

    private void FixedUpdate()
    {
        // TEMPORARY: Find the platform object and set the offset to its position
        if (m_offsetTransform == null)
        {
            if (GameObject.Find("Plane") != null)
            {
                m_offsetTransform = GameObject.Find("Plane").transform;
            }
        }

        if (m_offsetTransform == null) return;
        Move();
        UpdateWeight();
    }

    private void Move()
    {
        // Update movement vector
        m_currentMovementVector.x = m_inputManager.Direction.x * m_walkingSpeed * Time.deltaTime;
        m_currentMovementVector.z = m_inputManager.Direction.y * m_walkingSpeed * Time.deltaTime;

        // Look towards the direction of movement
        if (m_currentMovementVector != Vector3.zero)
            _playerModel.transform.forward = new Vector3(m_currentMovementVector.x, m_currentMovementVector.y, m_currentMovementVector.z);

        // Update position and move the player
        relativePos += m_currentMovementVector;
        relativePos.x = Mathf.Clamp(relativePos.x, -_platformData.BoundsX, _platformData.BoundsX);
        relativePos.z = Mathf.Clamp(relativePos.z, -_platformData.BoundsY, _platformData.BoundsY);
        transform.position = m_offsetTransform.position + relativePos.x * m_offsetTransform.right + relativePos.z * m_offsetTransform.forward + m_offsetTransform.up * 0.5f;
    }

    public void InitPlayerPosition()
    {
        float boundX = _platformData.BoundsX * 0.9f;
        float boundY = _platformData.BoundsY * 0.9f;

        relativePos.x = Random.Range(-boundX, boundX);
        relativePos.z = Random.Range(-boundY, boundY);
        relativePos.y = 0;

        transform.position = m_offsetTransform.position + relativePos.x * m_offsetTransform.right + relativePos.z * m_offsetTransform.forward + m_offsetTransform.up * 0.5f;
    }

    private void UpdateWeight()
    {
        m_weightComponent.UpdateWeights(new Vector2(relativePos.x / _platformData.BoundsX, relativePos.z / _platformData.BoundsY));
    }

    // // CURRENTLY UNUSED
    // private void HandleJump()
    // {
    //     if (!m_isJumping && m_inputManager.Jump && m_isGrounded)
    //     {
    //         m_isJumping = true;
    //         m_currentMovementVector.y = m_initialJumpVelocity;
    //     }
    //     else if (m_isJumping && !m_inputManager.Jump && m_isGrounded)
    //     {
    //         m_isJumping = false;
    //     }
    // }

    // // CURRENTLY UNUSED
    // private void ApplyGravity()
    // {
    //     if (m_isGrounded)
    //         m_currentMovementVector.y = m_groundedGravity;
    //     else
    //         m_currentMovementVector.y += m_gravity * Time.deltaTime;
    // }

    // // CURRENTLY UNUSED
    // private void CheckIfGrounded()
    // {
    //     /*
    //     In this model the body lays on the [0,0,0]
    //     The radious of the character controller is 0.5
    //     The sphere radious is 0.5, and is elevated 0.4 units from the ground, meaning, 0.1 units are always in contact with the ground.
    //     */
    //     Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + m_groundedOffset, transform.position.z);
    //     Debug.DrawLine(transform.position, spherePosition, Color.yellow);
    //     m_isGrounded = Physics.CheckSphere(spherePosition, m_groundedRadius, m_groundLayers, QueryTriggerInteraction.Ignore);
    // }
}
