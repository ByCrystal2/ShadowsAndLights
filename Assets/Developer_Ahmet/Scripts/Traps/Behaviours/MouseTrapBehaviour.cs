using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public partial class MouseTrapBehaviour : TrapBehaviour
{
    MouseTrap Me;
    void Start()
    {
        Me = new MouseTrap(ID, Damage, TrapType, EffectType, ChangeTime,MinActiveLevel,MaxActiveLevel);
        SetTrap(Me);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthHandler health))
        {
            Me.IEHit(health);
            //AudioSource.PlayOneShot(LightPuzzleHandler.instance.GetSoundInEffectOnTarget(TrapType));
            GameAudioManager.instance.PlayTrapSound(AudioSourceHelper);
        }
    }
}
public partial class MouseTrapBehaviour : TrapBehaviour
{    
    [Range(0,100)] public float Damage;
    
}//SerializeFieds...
