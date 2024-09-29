using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FuelParticleManager : MonoBehaviour
{
    [SerializeField] ParticleSystem pSystem;
    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[128];
    [SerializeField] private Transform particleTarget;
    [SerializeField] private float maxRadians = 0.1f;
    [SerializeField] private float maxMagnitude = 1f;
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private float speedup = 10.0f;
    [SerializeField] private float baseSpeed = 15.0f;
    [SerializeField] private int particleCount = 20;

    [SerializeField] private FloatEvent OnEnterRing;
    [SerializeField] private FloatEvent OnAddFuel;

    void Start() {
        pSystem = GetComponent<ParticleSystem>();

        OnEnterRing.Subscribe(EmitParticles);
    }
    
    void OnDestroy() {
        OnEnterRing.Unsubscribe(EmitParticles);
    }

    private float fuelPerParticle = 0f;
    void EmitParticles(float fuel) {
        pSystem.Emit(particleCount);
        fuelPerParticle = fuel / particleCount;
    }

    void Update() {
        // move particles towrads target collider
        pSystem.GetParticles(particles);

        for (int i = 0; i < pSystem.particleCount; i++) {
            ParticleSystem.Particle p = particles[i];

            Vector3 particlePos = transform.position + p.position;
            Vector3 particleTargetPos = particleTarget.position;

            float passedTime = p.startLifetime - p.remainingLifetime;
            if (passedTime > delay ) {
                Vector3 currentDir = p.velocity.normalized;
                Vector3 newDir = (particleTargetPos - particlePos).normalized;
                Vector3 dir = Vector3.RotateTowards(currentDir, newDir, maxRadians, maxMagnitude);

                float velocity = baseSpeed + passedTime * speedup;
                p.velocity = dir * velocity;
            } else {
                p.velocity = p.velocity.normalized * baseSpeed;
            }

            particles[i] = p;
        }

        pSystem.SetParticles(particles, pSystem.particleCount);
    }
    void OnParticleTrigger() {
        Debug.Log($"Add {fuelPerParticle} fuel");
        OnAddFuel?.Raise(fuelPerParticle);
    }
}
