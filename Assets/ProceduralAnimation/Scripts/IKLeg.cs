using System.Collections;
using System.Collections.Generic;
using Rellac.Audio;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Component controlling a singular IK target (aka "foot")
/// 
/// Adapted from: https://github.com/Sopiro/Unity-Procedural-Animation/tree/master 
/// By Soprio 2021
/// </summary>

public class IKLeg : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Transform m_bodyTransform;
    [SerializeField] private Transform m_raySource;
    [SerializeField] private Transform m_offsetTransform;
    [SerializeField] private SoundManager m_soundManager;
    public GameObject ikTarget; // foot object to be moved
    public GameObject stepTarget; // foot object to be moved

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve m_speedCurve; // should start at 0 and end at 1
    [SerializeField] private AnimationCurve m_heightCurve; // should start and end at 0
    [SerializeField] private AnimationCurve m_rotationCurve; // should start and end at 0

    [Header("Step Variables")]
    [SerializeField] private float m_maxFootHeight = 0.2f; // Max height ikTarget (foot) can reach
    [SerializeField] private float m_animationTime = 0.1f; // total animation time
    [SerializeField] private float m_AnimationFrameTime = 1 / 60.0f; // Increments of time that the animation is played by (default 60fps)
    [SerializeField] private float m_maxDistFromTarget = 0.55f; // maximum distance until step will occur
    [SerializeField] private float m_maxRaycastDist = 7.0f; // Maximum raycast distance for checking for ground
    [SerializeField] private float m_footOvershoot = 0.55f / 2.0f; // Overshoot passed target along foot forward direction
    [SerializeField] private float m_footOffset = 0f; // offset of ikTarget away from the hit position Parallel to the normal
    [SerializeField] private int m_LayerInclude = 3; // Layer target for raycast
    [SerializeField] float m_stepSpeedPerFixedUpdate = 0.1f; // Speed of the foot per FixedUpdate tick
    [SerializeField] private float m_footRotationSpeed = 0.1f; // Speed of the foot rotation per FixedUpdate tick


    // TODO: fix naming violation
    public Vector3 m_currentPos { get; private set; }
    public Vector3 m_footUpDir { get; private set; }
    public Vector3 m_footForwardDir { get; private set; }
    public Vector3 m_rotatedForwardDir { get; private set; }
    public Vector3 m_raycastHitPos { get; private set; }
    public Vector3 m_raycastHitNormal { get; private set; }

    public bool Animating { get; private set; } = false;
    public bool Movable { get; set; } = false;
    public float DistFromTarget { get; private set; }


    private void Awake()
    {
        // convert layer to bitmask
        m_LayerInclude = 1 << m_LayerInclude;

        if (stepTarget == null)
            stepTarget = new GameObject($"{gameObject.name}_stepTarget");
    }

    private void Start()
    {
        m_currentPos = ikTarget.transform.localPosition;
        UpdateIKTargetTransform();
    }
        
private void FixedUpdate()
{
    // Calculate target position
    if (!Animating)
    {
        RaycastHit hit;
        if (Physics.Raycast(m_raySource.position, m_bodyTransform.up.normalized * -1, out hit, m_maxRaycastDist, m_LayerInclude))
        {
            stepTarget.transform.position = hit.point;
            stepTarget.transform.up = hit.normal;
            m_raycastHitPos = stepTarget.transform.localPosition;
            m_raycastHitNormal = stepTarget.transform.up;
        }
        DistFromTarget = (m_raycastHitPos - m_currentPos).magnitude;
    }


    // If the distance gets too far, animate and move the foot
    if (!Animating && (DistFromTarget > m_maxDistFromTarget && Movable))
    {
        StartCoroutine(AnimateLeg());
        Movable = false;
    }
}

private IEnumerator AnimateLeg()
{
    Animating = true;

    Vector3 startingFootPos = m_currentPos;
    m_footForwardDir = m_raycastHitPos - m_currentPos;
    m_footForwardDir += m_footForwardDir.normalized * m_footOvershoot;

    Vector3 right = Vector3.Cross(m_bodyTransform.up, m_footForwardDir.normalized).normalized;
    m_footUpDir = Vector3.Cross(m_footForwardDir.normalized, right);
    m_rotatedForwardDir = m_footForwardDir;

    float totalDistance = m_footForwardDir.magnitude;
    float distanceMoved = 0f;

    float footAcceleration = Mathf.Max((m_raycastHitPos - startingFootPos).magnitude / m_footForwardDir.magnitude, 1.0f);

    // This is your configurable "speed" per FixedUpdate tick
    float stepDistancePerUpdate = m_stepSpeedPerFixedUpdate; // <-- new var you define

    while (distanceMoved < totalDistance)
    {
        // Clamp to avoid overshooting the end
        float step = Mathf.Min(stepDistancePerUpdate, totalDistance - distanceMoved);
        distanceMoved += step;

        float normalizedDistance = distanceMoved / totalDistance;
        float curveValue = m_speedCurve.Evaluate(normalizedDistance);

        m_currentPos = startingFootPos + m_footForwardDir * footAcceleration * curveValue;
        m_currentPos += m_footUpDir * m_heightCurve.Evaluate(curveValue) * m_maxFootHeight;

        m_rotatedForwardDir = Quaternion.AngleAxis(m_rotationCurve.Evaluate(curveValue) * 90f, right) * m_footForwardDir;

        UpdateIKTargetTransform();

        yield return new WaitForFixedUpdate();
    }

    m_soundManager.PlayOneShotRandomPitch("footstep", 0.1f);
    Animating = false;
}

    private void UpdateIKTargetTransform()
    {
        ikTarget.transform.localPosition = m_currentPos + m_bodyTransform.up.normalized * m_footOffset;
        ikTarget.transform.rotation = Quaternion.LookRotation(m_rotatedForwardDir, m_raycastHitNormal);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(m_raycastHitPos, 0.1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(m_currentPos, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_currentPos, m_raycastHitPos);

        Gizmos.color = Color.green;
        if (stepTarget != null)
            Gizmos.DrawSphere(stepTarget.transform.position, 0.2f);

        Gizmos.color = Color.yellow;
        if (ikTarget != null)
            Gizmos.DrawSphere(ikTarget.transform.position, 0.1f);
    }

}
