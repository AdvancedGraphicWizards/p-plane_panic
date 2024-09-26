using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Locally Scoped Members")]
    [SerializeField] private PhoneController m_phoneController;

    [Header("Input Filter Variables")]
    [Tooltip("Low pass factor, higher values make movement more responsive but add jitter from the gyroscope.")]
    [SerializeField] private float lowPassFactor = 0.1f;
    public Vector2 Direction = Vector2.zero;

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
        Vector3 euler = rotation.eulerAngles;
        float tiltX = (euler.x > 180) ? euler.x - 360 : euler.x; // Tilt forward/backward
        float tiltZ = (euler.z > 180) ? euler.z - 360 : euler.z; // Tilt left/right

        // Normalize the tilts to the range -1 to 1 for movement
        tiltX = Mathf.Clamp(tiltX, -45, 45) / 45f;
        tiltZ = Mathf.Clamp(tiltZ, -45, 45) / 45f;

        Vector2 movement = new Vector2(tiltZ, -tiltX);
        return movement;
    }

    public void AssignPhoneController(PhoneController phoneController)
    {
        m_phoneController = phoneController;
    }
}
