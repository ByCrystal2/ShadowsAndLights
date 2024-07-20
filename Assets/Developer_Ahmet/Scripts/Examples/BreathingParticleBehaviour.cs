using TMPro;
using UnityEngine;

public class BreathingParticleBehaviour : MonoBehaviour
{

    ParticleSystem particle;
    private Vector3 targetPosition;
    BreathingBehaviour breathingBehaviour;
    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        breathingBehaviour = GetComponentInParent<BreathingBehaviour>();
    }
    void Update()
    {
        if (!particle.isPlaying)
        {
            particle.Play();
        }
        targetPosition = transform.position + transform.forward * breathingBehaviour.myBreathing.BreathSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * breathingBehaviour.myBreathing.BreathSpeed);

        transform.rotation = Quaternion.LookRotation(transform.forward);
    }
}
