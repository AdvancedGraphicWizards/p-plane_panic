using System.Collections;
using System.Collections.Generic;
using Rellac.Audio;
using UnityEngine;


// Adapted from: https://github.com/Sopiro/Unity-Procedural-Animation/tree/master 
// By Soprio 2021

public class IKLeg : MonoBehaviour
{
    [SerializeField] private Transform m_bodyTransform;
    [SerializeField] private Transform m_raySource;
    [SerializeField] private Transform m_offsetTransform;
    [SerializeField] private SoundManager m_soundManager;
    public GameObject ikTarget; // foot object to be moved
    public GameObject stepTarget; // foot object to be moved

    [SerializeField] private AnimationCurve m_speedCurve; // should start at 0 and end at 1
    [SerializeField] private AnimationCurve m_heightCurve; // should start and end at 0
    [SerializeField] private AnimationCurve m_rotationCurve; // should start and end at 0

    [SerializeField] private float m_maxFootHeight = 0.2f; // Max height ikTarget (foot) can reach
    [SerializeField] private float m_animationTime = 0.1f; // total animation time
    [SerializeField] private float m_AnimationFrameTime = 1 / 60.0f; // Increments of time that the animation is played by (default 60fps)

    [SerializeField] private float m_maxDistFromTarget = 0.55f;
    [SerializeField] private float m_maxRaycastDist = 7.0f;
    [SerializeField] private float m_footOvershoot = 0.55f / 2.0f; // Overshoot passed target along foot forward direction
    [SerializeField] private float m_footOffset = 0f; // offset of ikTarget away from the hit position Parallel to the normal
    [SerializeField] private int m_LayerInclude = 3; // Layer target for raycast

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
        // convert layer to bitmask (invert to ONLY include selected layer)
        m_LayerInclude = 1 << m_LayerInclude;

        if (stepTarget == null)
            stepTarget = new GameObject($"{gameObject.name}_stepTarget");
    }

    private void Start()
    {
        m_currentPos = ikTarget.transform.localPosition;
        UpdateIKTargetTransform();
    }
        
    private void Update()
    {
        // Calculate target position
        if (!Animating) {
            RaycastHit hit;
            if (Physics.Raycast(m_raySource.position, m_bodyTransform.up.normalized * -1, out hit, m_maxRaycastDist, m_LayerInclude))
            {
                stepTarget.transform.position = hit.point;
                stepTarget.transform.up = hit.normal;
                m_raycastHitPos = stepTarget.transform.localPosition;
                m_raycastHitNormal = stepTarget.transform.up;
            }
        }

        DistFromTarget = (m_raycastHitPos - m_currentPos).magnitude;

        // If the distance gets too far, animate and move the foot
        if (!Animating && (DistFromTarget > m_maxDistFromTarget && Movable))
        {
            // Debug.Log($"{gameObject.name} step");
            StartCoroutine(AnimateLeg());
            Movable = false;
        }
    }


    // note that raycasthitPos may be recalculated if the movement is too slow (making the leg move furhter than expected)
    private IEnumerator AnimateLeg()
    {
        Animating = true;

        float timer = 0.0f;
        float animTime;

        Vector3 startingFootPos = m_currentPos;
        m_footForwardDir = m_raycastHitPos - m_currentPos;
        m_footForwardDir += m_footForwardDir.normalized * m_footOvershoot;

        Vector3 right = Vector3.Cross(m_bodyTransform.up, m_footForwardDir.normalized).normalized;
        m_footUpDir = Vector3.Cross(m_footForwardDir.normalized, right);
        m_rotatedForwardDir = m_footForwardDir;

        while (timer < m_animationTime + m_AnimationFrameTime)
        {
            animTime = m_speedCurve.Evaluate(timer / m_animationTime);

            // If the target is keep moving, apply acceleration to correct the end point
            float footAcceleration = Mathf.Max((m_raycastHitPos - startingFootPos).magnitude / m_footForwardDir.magnitude, 1.0f);

            m_currentPos = startingFootPos + m_footForwardDir * footAcceleration * animTime; // Forward direction of foot vector
            m_currentPos += m_footUpDir * m_heightCurve.Evaluate(animTime) * m_maxFootHeight; // Upward direction of foot vector

            m_rotatedForwardDir = Quaternion.AngleAxis(m_rotationCurve.Evaluate(animTime) * 90f, right) * m_footForwardDir; // rotate rotationvector about axis 

            UpdateIKTargetTransform();

            timer += m_AnimationFrameTime;

            yield return new WaitForSeconds(m_AnimationFrameTime);
        }

        m_soundManager.PlayOneShotRandomPitch("footstep",0.1f);

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
