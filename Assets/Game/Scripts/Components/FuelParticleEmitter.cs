using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FuelParticleManager : MonoBehaviour
{
    [SerializeField] ParticleSystem pSystem;
    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[128];
    [SerializeField] private Transform particleTarget;
    [SerializeField] private Transform particleOrigin;
    [SerializeField] private float maxRadians = 0.1f;
    [SerializeField] private float maxMagnitude = 1f;
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private float speedup = 10.0f;
    [SerializeField] private float baseSpeed = 15.0f;
    [SerializeField] private float travelSpeed = 15.0f;
    [SerializeField] private int particleCount = 20;

    [SerializeField] private FloatEvent OnEnterRing;
    [SerializeField] private FloatEvent OnAddFuel;

    [SerializeField]  private float m_staggerTime = 0.5f;
    private float m_staggerStep = 0.5f;

    [SerializeField] private float m_travelTime;
    [SerializeField]  private float m_spreadTime;
    [SerializeField]  private AnimationCurve m_spreadCurve;
    [SerializeField]  private AnimationCurve m_travelCurve;
    [SerializeField]  private AnimationCurve m_mixCurve;

    private float fuelPerParticle = 0f;



    void Start() {
        pSystem = GetComponent<ParticleSystem>();

        OnEnterRing.Subscribe(EmitParticles);

        m_spreadCurve ??= new AnimationCurve(new Keyframe(1, 1), new Keyframe(0, 0));
        m_travelCurve ??= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        m_travelCurve ??= new AnimationCurve(new Keyframe(1, 1), new Keyframe(0, 0));
    }
    
    void OnDestroy() {
        OnEnterRing.Unsubscribe(EmitParticles);
    }

    void EmitParticles(float fuel) {
        StartCoroutine(EmitParticles());
        fuelPerParticle = fuel / particleCount;
        m_staggerStep =  m_staggerTime / particleCount;
    }

    void Update() {
        // for testing
        //delay += Time.deltaTime;
        //if ( delay > 2f){
        //    StartCoroutine(EmitParticles());
        //    delay = 0f;
        //}
            
        if (pSystem.GetParticles(particles) > 0) {
            UpdateParticles();
        }

        // if (pSystem.GetParticles(particles) > 0){
        //     switch (m_pState) {
        //         case FuelParticleEmitterState.SPREAD:
        //             break;
        //         case FuelParticleEmitterState.TRAVEL:
        //             break;
        //         case FuelParticleEmitterState.INACTIVE:
        //             break;
        //         default:
        //             break;
        //     }
        //     StartCoroutine(AnimateParticles());
        // } 
        // move particles towrads target collider
    }

    private IEnumerator EmitParticles() {
        // this solution  will break with multiple overlapping effects...
        pSystem.Emit(particleCount);

        //for (int i = 0; i < particleCount; i++) {
        //    pSystem.Emit(particleCount);
        //    
        //    yield return new WaitForSeconds(m_staggerStep);
        //}
        yield return true;
    }

    private void UpdateParticles() {
        for (int i = 0; i < pSystem.particleCount; i++) {
            ParticleSystem.Particle p = particles[i];

            float st = + i * m_staggerTime;

            // Get normalized lifetime of particle
            float normLife = (p.startLifetime - p.remainingLifetime) / p.startLifetime;

            if (normLife < 0) {
                // do nothing
            }
            else if (normLife < m_spreadTime) {
                // spread state

                // slow down particle velocity closer to switch
                p.velocity = p.velocity.normalized * baseSpeed  * m_spreadCurve.Evaluate(normLife / m_spreadTime); //* (m_spreadTime - normLife)
            }
            else if (normLife < m_travelTime) {
                // travel state
                float normTravelTime = ((normLife - m_spreadTime) / (m_travelTime - m_spreadTime));
                
                Vector3 mixVector = Vector3.down;
                if (i % 2 == 0) {
                    mixVector = Vector3.up;
                }

                p.velocity += (particleTarget.position -transform.TransformPoint(p.position)).normalized * travelSpeed * m_travelCurve.Evaluate(normTravelTime)+ mixVector *m_mixCurve.Evaluate(normTravelTime);
            }
    

            particles[i] = p;
        }
        pSystem.SetParticles(particles, pSystem.particleCount);
    }


    void OnParticleTrigger() {
        //Debug.Log($"Add {fuelPerParticle} fuel");
        OnAddFuel?.Raise(fuelPerParticle);
    }
}
