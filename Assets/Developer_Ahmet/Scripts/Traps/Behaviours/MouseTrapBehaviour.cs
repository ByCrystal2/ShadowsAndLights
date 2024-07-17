using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public partial class MouseTrapBehaviour : TrapBehaviour
{
    MouseTrap Me;
    void Start()
    {
        Me = new MouseTrap(ID, Damage, TrapEffectType, ChangeTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthHandler health))
        {
            StartCoroutine(Me.IEHit(health, 0f));
            AudioSource.PlayOneShot(LightPuzzleHandler.instance.GetSoundInEffectOnTarget(TrapEffectType));
        }
    }
}
public partial class MouseTrapBehaviour : TrapBehaviour
{    
    [Range(0,100)] public float Damage;
    
}//SerializeFieds...
