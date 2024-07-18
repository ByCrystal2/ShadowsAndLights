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
    public Vector3 DefaultPos { get; set; }

    public IEnumerator IEHit(HealthHandler targetHealth, float waiting)
    {
        yield return new WaitForSeconds(waiting);
        targetHealth.TakeDamage(this);
    }

    private void OnEnable()
    {
        transform.localPosition = Vector3.zero;
        DefaultPos = transform.localPosition;
    }
    bool followWay;
    private void Update()
    {
        if (followWay)
        {
            MoveArrow();
            CheckDistance();
        }
    }
    private void MoveArrow()
    {
        transform.Translate(Direction * Speed * Time.deltaTime,Space.World);
    }

    private void CheckDistance()
    {
        if (Vector3.Distance(transform.localPosition, DefaultPos) > 5f)
        {
            followWay = false;
            DeactivateArrow();
        }
    }

    private void DeactivateArrow()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 1f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthHandler healthHandler))
        {
            StartCoroutine(IEHit(healthHandler, 0));
            Destroy(gameObject);
        }
        else
            followWay = false;
    }
}
