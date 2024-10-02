using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public enum ControlMode
    {
        Default,
        Landscape,
        Portrait
    }

    [Header("Locally Scoped Members")]
    [SerializeField] private PhoneController m_phoneController;

    [Header("Input Filter Variables")]
    [Tooltip("Low pass factor, higher values make movement more responsive but add jitter from the gyroscope.")]
    [SerializeField] private float lowPassFactor = 0.1f;
    public Vector2 Direction = Vector2.zero;
    public Vector3 euler = Vector3.zero;

    [Header("Control Mode")]
    [Tooltip("Select the control mode: Default (used in the FF demo), Landscape (two-handed) or Portrait (one-handed, held vertically).")]
    [SerializeField] private ControlMode m_controlMode = ControlMode.Default;

    void FixedUpdate()
    {
        if (m_phoneController)
        {
            Quaternion rotation = m_phoneController.GetRotation();
            //transform.rotation = rotation;

            Vector3 targetMovement = GetMovementFromRotation(rotation);
            Direction = Vector3.Lerp(Direction, targetMovement, lowPassFactor);
        }
    }

    private Vector2 GetMovementFromRotation(Quaternion rotation)
    {
        if (m_controlMode == ControlMode.Portrait)
        {
            // TODO: Fix this, doesn't register rotation properly
            // because the Y-axis shifts when we rotate along the X-axis
            // rotation = Quaternion.Euler(0, 0, 90) * rotation;
        }

        euler = rotation.eulerAngles;
        float tiltX = (euler.x > 180) ? euler.x - 360 : euler.x; // Tilt forward/backward
        float tiltZ = (euler.z > 180) ? euler.z - 360 : euler.z; // Tilt left/right

        // Normalise the tilts to the range -1 to 1 for movement
        tiltX = Mathf.Clamp(tiltX, -45, 45) / 45f;
        tiltZ = Mathf.Clamp(tiltZ, -45, 45) / 45f;

        Vector2 movement = new Vector2(tiltZ, -tiltX);

        if (m_controlMode == ControlMode.Landscape)
        {
            movement = new Vector2(tiltX, tiltZ);
        }

        return movement;
    }

    public void AssignPhoneController(PhoneController phoneController)
    {
        m_phoneController = phoneController;
    }
}
