using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    [Header("Locally Scoped Members")]
    [SerializeField] private PhoneController m_phoneController;
    [SerializeField] private Vector3 m_movement = Vector3.zero;

    [Header("Movement Variables")]
    [Tooltip("Maximum movement speed of the player character.")]
    [SerializeField] private float moveSpeed = 0.03f;
    [Tooltip("Low pass factor, lower values make movement more responsive but add jitter from the gyroscope.")]
    [SerializeField] private float lowPassFactor = 0.1f;
    [Tooltip("Tilt dead zone, when tilting within this zone we treat the input as zero.")]
    [SerializeField] private float tiltDeadZone = 10f;
    [Tooltip("Threshold for ignoring small movement values.")]
    [SerializeField] private float movementThreshold = 0.001f;

    void FixedUpdate()
    {
        if (m_phoneController)
        {
            Quaternion rotation = m_phoneController.GetRotation();
            transform.rotation = rotation;

            Vector3 targetMovement = GetMovementFromRotation(rotation);
            m_movement = Vector3.Lerp(m_movement, targetMovement, lowPassFactor);
            m_movement = ApplyMovementThreshold(m_movement);
        }
    }

    private Vector3 GetMovementFromRotation(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        float tiltX = (euler.x > 180) ? euler.x - 360 : euler.x; // Tilt forward/backward
        float tiltZ = (euler.z > 180) ? euler.z - 360 : euler.z; // Tilt left/right

        if (Mathf.Abs(tiltX) < tiltDeadZone) tiltX = 0;
        if (Mathf.Abs(tiltZ) < tiltDeadZone) tiltZ = 0;

        // Normalize the tilts to the range -1 to 1 for movement
        tiltX = Mathf.Clamp(tiltX, -45, 45) / 45;
        tiltZ = Mathf.Clamp(tiltZ, -45, 45) / 45;

        Vector3 movement = new Vector3(-tiltZ, 0, tiltX);
        return movement;
    }

    private Vector3 ApplyMovementThreshold(Vector3 movement)
    {
        if (Mathf.Abs(movement.x) < movementThreshold)
        {
            movement.x = 0;
        }

        if (Mathf.Abs(movement.z) < movementThreshold)
        {
            movement.z = 0;
        }

        return movement;
    }

    public void AssignPhoneController(PhoneController phoneController)
    {
        m_phoneController = phoneController;
    }
}
