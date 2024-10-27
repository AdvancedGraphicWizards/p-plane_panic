using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class ParticleCollectionComponent : MonoBehaviour
{
    [SerializeField] private Transform m_particleGroupParent;
    [SerializeField] private CirclePlacementComponent m_cpComponent;
    [SerializeField] private Camera m_mainCamera;
    private Transform[] m_particles;
    private Vector3[] m_screenLoc;
    [SerializeField] private float m_zValue;
    [SerializeField] private Transform m_offsetTransform;
    private int m_activeParticles = 0;

    private void Start() {
        if (m_particleGroupParent == null)
            return;

        if (m_mainCamera == null && Camera.main != null)
            m_mainCamera = Camera.main;

        int numParticles = m_particleGroupParent.childCount;
        m_particles = new Transform[numParticles];
        m_screenLoc = new Vector3[numParticles];

        for (int i = 0; i < numParticles; ++i)
            m_particles[i] = m_particleGroupParent.GetChild(i);

        // TEMPORARY: Find the platform object and set the offset to its position
        if (m_offsetTransform == null)
        {
            if (GameObject.Find("Plane") != null)
            {
                m_offsetTransform = GameObject.Find("Plane").transform;
            }
        }
    }

    public void RegisterParticlesInScreenSpace() {
        if (m_cpComponent != null) m_cpComponent.UpdatesLocally = false;
        for (int i = 0; i < m_particles.Length; i++) {
            m_screenLoc[i] =  m_particles[i].position - m_offsetTransform.position; // get pos relative to plane
        }
        m_activeParticles = m_particles.Length;
    }

    private void Update() {

        if (m_activeParticles <= 0) return;

        AnimateParticlesInScreenSpace();
    }

    private void AnimateParticlesInScreenSpace() {
        for (int i = 0; i < m_particles.Length; i++) {
            if (m_cpComponent.Rotates) {
                foreach (Vector3 pos in m_cpComponent.CalculateCircleArrayPositions()) {
                    // project the group offset onto plane
                    m_screenLoc[i] = Vector3.ProjectOnPlane(m_particleGroupParent.position, new Vector3(0,0,1f)) + pos;
                }
            }
            m_particles[i].position = m_offsetTransform.position + m_screenLoc[i];
        }
    }

    // void MoveToCollection() {

    //     for (int i = 0; i < pSystem.particleCount; i++) {
    //         ParticleSystem.Particle p = particles[i];

    //         Vector3 particlePos = p.position;
    //         Vector3 particleTargetPos = particleTarget.localPosition;

    //         float passedTime = p.startLifetime - p.remainingLifetime;
    //         if (passedTime > delay ) {
    //             Vector3 currentDir = p.velocity.normalized;
    //             Vector3 newDir = (particleTargetPos - particlePos).normalized;
    //             Vector3 dir = Vector3.RotateTowards(currentDir, newDir, maxRadians, maxMagnitude);

    //             float velocity = baseSpeed + passedTime * speedup;
    //             p.velocity = dir * velocity;
    //         } else {
    //             p.velocity = p.velocity.normalized * baseSpeed;
    //         }

    //         particles[i] = p;
    //     }

    //     pSystem.SetParticles(particles, pSystem.particleCount);
    // }
}
