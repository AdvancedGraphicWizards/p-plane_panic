using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour

{
    [Header("Locally Scoped Members")]
    [SerializeField] private PhoneController m_phoneController;

    [Header("Movement Variables")]
    [Tooltip("Maximum movement speed of the player character.")]
    [SerializeField] private float moveSpeed = 0.03f;
    [Tooltip("Low pass factor, lower values make movement more responsive but add jitter from the gyroscope.")]
    [SerializeField] private float lowPassFactor = 0.1f;
    [Tooltip("Tilt dead zone, when tilting within this zone we treat the input as zero.")]
    [SerializeField] private float tiltDeadZone = 10f;
    [Tooltip("Threshold for ignoring small movement values.")]
    [SerializeField] private float movementThreshold = 0.001f;
    public Vector2 Direction = Vector2.zero;

    void FixedUpdate()
    {
        if (m_phoneController)
        {
            Quaternion rotation = m_phoneController.GetRotation();
            //transform.rotation = rotation;

            Vector3 targetMovement = GetMovementFromRotation(rotation);
            Direction = Vector3.Lerp(Direction, targetMovement, lowPassFactor);
            Direction = ApplyMovementThreshold(Direction);
        }
    }

    private Vector2 GetMovementFromRotation(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        float tiltX = (euler.x > 180) ? euler.x - 360 : euler.x; // Tilt forward/backward
        float tiltZ = (euler.z > 180) ? euler.z - 360 : euler.z; // Tilt left/right

        if (Mathf.Abs(tiltX) < tiltDeadZone) tiltX = 0;
        if (Mathf.Abs(tiltZ) < tiltDeadZone) tiltZ = 0;

        // Normalize the tilts to the range -1 to 1 for movement
        tiltX = Mathf.Clamp(tiltX, -45, 45) / 45;
        tiltZ = Mathf.Clamp(tiltZ, -45, 45) / 45;

        Vector2 movement = new Vector2(tiltZ, -tiltX);
        return movement;
    }

    private Vector2 ApplyMovementThreshold(Vector2 movement)
    {
        if (Mathf.Abs(movement.x) < movementThreshold)
        {
            movement.x = 0;
        }

        if (Mathf.Abs(movement.y) < movementThreshold)
        {
            movement.y = 0;
        }

        return movement;
    }

    public void AssignPhoneController(PhoneController phoneController)
    {
        m_phoneController = phoneController;
    }
}
