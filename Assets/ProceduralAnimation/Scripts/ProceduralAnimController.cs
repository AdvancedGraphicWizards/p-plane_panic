using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component controlling a body attatched to several IKLegs
/// 
/// Adapted from: https://github.com/Sopiro/Unity-Procedural-Animation/tree/master 
/// By Soprio 2021
/// </summary>

public class ProceduralAnimController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Transform m_bodyTransform;
    [SerializeField] private Transform m_rcTransform;
    [SerializeField] private IKLeg[] m_legs;
    [SerializeField] private ParticleSystem m_cloudTrail;
    
    [Header("Body Position Variables")]
    [SerializeField] private float m_bodyHeightBase = 0.3f;
    [SerializeField] private float m_minStepWait = 0.5f;
    [SerializeField] private float m_posAdjustRatio = 0.1f;
    [SerializeField] private float m_rotAdjustRatio = 0.2f;

    [Header("Idle Action Variables")]
    [SerializeField] private float m_idleBounceRange = 0.5f;
    [SerializeField] private float m_idleBouncePeriod = 1f;
    [SerializeField] private float m_idleTime = 1f;
    [SerializeField] private float m_lookTime = 1f;
    // [SerializeField] private float m_idleGlanceAngleRange = 45f;

    [Header("Raycast Variables")]
    [SerializeField] private int m_LayerInclude = 3; // Layer target for raycast
    
    private bool m_isIdle = false;
    private float m_idleBounceTimer = 0f;
    private float m_stepWaitTimer = 0.5f;
    private float m_idleTimer = 1f;
    private float m_lookTimer = 1f;
    // private float m_idleGlanceAngle = 0f;
    private int stepOrder = 1;

    private Vector3 m_bodyPos;
    private Vector3 m_bodyUp;
    private Vector3 m_bodyForward;
    private Vector3 m_bodyRight;
    private Quaternion m_bodyRotation;

    void Awake()
    {
        // convert layer to bitmask (invert to ONLY include selected layer)
        m_LayerInclude = 1 << m_LayerInclude;
    }

    private void Start()
    {
        m_stepWaitTimer = m_minStepWait;
        m_idleBounceTimer = m_idleBouncePeriod;
        m_idleTimer = m_idleTime;
        m_lookTimer = m_lookTime;

        // Start coroutine to adjust body transform
        StartCoroutine(AdjustBodyTransform());
    }

    private void Update()
    {
        // If the opposite foot step completes, switch the order to make a new step
        if (m_stepWaitTimer <= 0) {

            if (!m_legs[stepOrder].Animating && !m_legs[stepOrder].Movable)
            {
                stepOrder = (stepOrder + 1) % 2;

                for (int i = 0; i < m_legs.Length; i++)
                {
                    m_legs[i].Movable = (stepOrder == i);
                }
                m_stepWaitTimer = m_minStepWait;

                m_isIdle = false;
                m_idleTimer = m_idleTime;
            }

            // If not animating start idle timer
            if (!m_isIdle && !m_legs[stepOrder].Animating) {
                m_idleTimer -= Time.deltaTime;
                if (m_idleTimer <= 0){
                    m_isIdle = true;
                }
            }
        }
        else {
            m_stepWaitTimer -= Time.deltaTime;
        }
    }

    private IEnumerator AdjustBodyTransform()
    {
        while (true)
        {
            // Centering Body based on leg position

            Vector3 footCenter = Vector3.zero;
            m_bodyUp = Vector3.zero;

            // Collect leg information to calculate body transform
            foreach (IKLeg leg in m_legs)
            {
                footCenter += leg.m_currentPos;
            }

            if (Physics.Raycast(m_rcTransform.position, m_rcTransform.up * -1, out RaycastHit hit, 10.0f))
            {
                m_bodyUp += hit.normal;
            }

            footCenter /= m_legs.Length;
            m_bodyUp.Normalize();


            float bounceConst = 0f;
            var emission = m_cloudTrail.emission;

            // Passive Bounce (currently only active during idle)
            if (m_isIdle) {
                m_idleBounceTimer += Time.deltaTime;
                // TODO: Open to be animatable with a curve 
                // TODO: transition more smoothly perhaps using a float for idle state instead of a binary
                bounceConst = Mathf.Sin((m_idleBounceTimer * 2 * Mathf.PI) / m_idleBouncePeriod); 
                emission.enabled = false;
            }
            else {
                emission.enabled = true;
            }
            
            //Idle lookaround (NOT CURRENTLY IMPLEMENTED)
            // if (m_isIdle){
            //     m_lookTimer -= Time.deltaTime;
                
            //     if (m_lookTimer <= 0){
            //         // update lookvector
            //         m_idleGlanceAngle = Random.Range(-m_idleGlanceAngleRange, m_idleGlanceAngleRange);
            //         m_lookTimer = m_lookTime;
            //     } else {
            //         m_idleGlanceAngle = 0f;
            //     }
            // }
            // else {
            //     m_idleGlanceAngle = 0f;   
            // }

            // Interpolate postition from old to new
            m_bodyPos = footCenter + m_bodyUp * m_bodyHeightBase + m_bodyUp * bounceConst * m_idleBounceRange;

            m_bodyTransform.localPosition = Vector3.Lerp(m_bodyTransform.position, m_bodyPos, m_posAdjustRatio);

            // Currently just chase rc_transformLocation
            m_bodyTransform.rotation = Quaternion.Slerp(m_bodyTransform.rotation, m_rcTransform.rotation, m_rotAdjustRatio);

            yield return new WaitForFixedUpdate();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_bodyPos, m_bodyPos + m_bodyRight);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(m_bodyPos, m_bodyPos + m_bodyUp);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(m_bodyPos, m_bodyPos + m_bodyForward);
    }
}
