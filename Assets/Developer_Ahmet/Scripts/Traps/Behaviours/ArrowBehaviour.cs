using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class ArrowBehaviour : MonoBehaviour, ICanDamage, ITrapMovable, IHaveVisualEffect
{
    ArrowDispenser m_Dispenser;
    public void SetDispenser( ArrowDispenser dispenser)
    {
        m_Dispenser = dispenser;
        Damage = m_Dispenser.ArrowsDamage;
        Speed = m_Dispenser.ArrowsSpeed;
        Material = m_Dispenser.Material;
        EffectOnTarget = m_Dispenser.EffectOnTarget;
        ChangeTime = m_Dispenser.ChangeTime;
        followWay = true;
    }
    public float Damage { get; set; }
    public float Speed { get; set; }
    public Vector3 Direction { get; set; } = Vector3.forward;
    public Material Material { get; set; }
    public TrapEffectType EffectOnTarget { get ; set; }
    public float ChangeTime { get; set; }

    public IEnumerator IEHit(HealthHandler targetHealth, float waiting)
    {
        yield return new WaitForSeconds(waiting);
        targetHealth.TakeDamage(this);
    }

    private void OnEnable()
    {
        transform.localPosition = Vector3.zero;
    }
    bool followWay;
    private void Update()
    {
        if (m_Dispenser != null && followWay)
        {
            transform.Translate((transform.position + Direction) * (Speed * Time.deltaTime));
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthHandler healthHandler))
        {
            StartCoroutine(IEHit(healthHandler, 0));
        }
        followWay = false;
    }
}
