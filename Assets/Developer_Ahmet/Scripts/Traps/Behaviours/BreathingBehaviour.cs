using MalbersAnimations;
using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(CapsuleCollider))]
public partial class BreathingBehaviour : TrapBehaviour, ICanDamage
{
    public Breathing myBreathing;
    CapsuleCollider collider;
    

    public float Damage { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    #region BreathingTypeControl
    public void SetBreathing()
    {
        switch (EffectType)
        {
            case EffectType.Normal:
                myBreathing = null;
                break;
            case EffectType.Fiery:
                myBreathing = new FireBreathing(ID,HowMoneyBreathing,TrapType,EffectType,ChangeTime,BreathShootingContent,BreathDamage,BreathSpeed,BreathDefaultRotate,ExtinctionValue,collider);
                break;
            case EffectType.Freezer:
                myBreathing = new FreezeBreathing(ID, HowMoneyBreathing, TrapType, EffectType, ChangeTime, BreathShootingContent, BreathDamage, BreathSpeed, BreathDefaultRotate, ExtinctionValue, collider);
                break;
            case EffectType.Posion:
                myBreathing = new PosionBreathing(ID, HowMoneyBreathing, TrapType, EffectType, ChangeTime, BreathShootingContent, BreathDamage, BreathSpeed, BreathDefaultRotate, ExtinctionValue, collider);
                break;
            case EffectType.Slowdown:
                //myBreathing = new SlowdownArrow();
                break;
            default:
                break;
        }
        ParticleObject.GetComponent<ParticleSystem>().Stop();
    }

    public Breathing GetBreathing()
    {
        return myBreathing;
    }
    #endregion
    private void Awake()
    {
        SetBreathing();
    }
    private void Start()
    {
        for (int i = 0; i < HowMoneyBreathing; i++)
        {
            GameObject breathingParticle = Instantiate(ParticleObject, myBreathing.BreathContent);
            BreathingParticleBehaviour particleBehaviour = breathingParticle.GetComponent<BreathingParticleBehaviour>();
            breathingParticle.transform.localRotation = BreathDefaultRotate;
            breathingParticle.SetActive(false);
            myBreathing.Objects.Add(particleBehaviour);
        }
    }
    public void Breath(HealthHandler _health)
    {
        if (myBreathing is FireBreathing fire)
        {
            fire.Burn(_health.GetComponent<VisualHandler>());
        }
        else if (myBreathing is FreezeBreathing freeze)
        {
            freeze.Freeze(_health.GetComponent<VisualHandler>());
        }
        else if (myBreathing is PosionBreathing posion)
        {
            posion.Posion(_health.GetComponent<VisualHandler>());
        }
    }
    IEnumerator ShootBreathing()
    {
        for (int i = 0; i < HowMoneyBreathing; i++)
        {
            yield return new WaitForSeconds(ChangeTime);
            ShotBreathing();
        }
    }
    void ShotBreathing()
    {
        myBreathing.Objects[0].gameObject.SetActive(true);
        myBreathing.Objects.RemoveAt(0);
        GameAudioManager.instance.PlayTrapSound(AudioSourceHelper);
        Debug.Log($"This ArrowDispenser ({name}) Fired! Currnet Arrows Count:{myBreathing.Objects.Count}");
    }
    Coroutine shotBreathsCoroutine;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthHandler health))
        {
            if (myBreathing.AllObjectShoot()) return;
            if (other.CompareTag("Animal"))
            {
                Debug.Log($"This ArrowDispenser ({name}) trigged the target ({other.name}).");
                if (IsConsecutiveShots)
                {
                    if (shotBreathsCoroutine == null) { shotBreathsCoroutine = StartCoroutine(ShootBreathing()); }
                }
                else
                    ShotBreathing();
            }
            Breath(health);
            health.TakeDamage(this);
        }
    }

    public void IEHit(HealthHandler targetHealth)
    {
        throw new System.NotImplementedException();
    }
}
public partial class BreathingBehaviour : TrapBehaviour
{
    public float BreathDamage;
    public float BreathSpeed;
    public int HowMoneyBreathing;
    public Transform BreathShootingContent;
    public Quaternion BreathDefaultRotate;
    public bool IsConsecutiveShots;
    public BreathType BreathType;
    [Range(0, 50)] public float ExtinctionValue;

    public abstract class Breathing : Trap, ICanHoldMultipleObjects<BreathingParticleBehaviour>, ITrapMovable
    {
        public abstract GameObject EffectTheTarget(GameObject _arrow, Transform _targetTransform);
        public Breathing(int _id, int _howManyBreaths, TrapType _trapType, EffectType _effectType, float _changeTime, Transform breathContent, float breathDamage, float breathSpeed, Quaternion defaultRotate, float extinctionValue, CapsuleCollider myCollider) : base(_id, _trapType, _effectType, _changeTime)
        {
            HowMany = _howManyBreaths;
            BreathContent = breathContent;
            BreathDamage = breathDamage;
            BreathSpeed = breathSpeed;
            DefaultRotate = defaultRotate;
            ExtinctionValue = extinctionValue;
            MyCollider = myCollider;
        }

        public bool AllObjectShoot()
        {
            return Objects.Count <= 0 ? true : false;
        }
        public CapsuleCollider MyCollider { get; private set; }
        public Quaternion DefaultRotate { get; set; }
        public Transform BreathContent;
        public float BreathSpeed { get; set; }
        public float BreathDamage { get; set; }
        public float ExtinctionValue { get; set; }
        public int HowMany { get; set; }
        public List<BreathingParticleBehaviour> Objects { get; set; } = new List<BreathingParticleBehaviour>();
        public Vector3 DefaultPos { get; set; }
        public float Speed { get; set; }
        public Vector3 Direction { get; set; }
    }
    public class FireBreathing : Breathing
    {
        public FireBreathing(int _id, int _howManyBreaths, TrapType _trapType, EffectType _effectType, float _changeTime, Transform breathContent, float breathDamage, float breathSpeed, Quaternion defaultRotate, float extinctionValue, CapsuleCollider capsuleCollider) : base(_id, _howManyBreaths, _trapType, _effectType, _changeTime, breathContent, breathDamage, breathSpeed, defaultRotate, extinctionValue,capsuleCollider)
        {
        }
        public void Burn(VisualHandler _visualHandler)
        {            
            
        }

        public override GameObject EffectTheTarget(GameObject _breath, Transform _targetTransform)
        {
            GameObject particleEffect = Instantiate(_breath, _targetTransform);
            return particleEffect;
        }
    }
    public class FreezeBreathing : Breathing
    {
        public FreezeBreathing(int _id, int _howManyBreaths, TrapType _trapType, EffectType _effectType, float _changeTime, Transform breathContent, float breathDamage, float breathSpeed, Quaternion defaultRotate, float extinctionValue, CapsuleCollider capsuleCollider) : base(_id, _howManyBreaths, _trapType, _effectType, _changeTime, breathContent, breathDamage, breathSpeed, defaultRotate, extinctionValue, capsuleCollider)
        {
        }

        public override GameObject EffectTheTarget(GameObject _arrow, Transform _targetTransform)
        {
            GameObject particleEffect = Instantiate(_arrow, _targetTransform);
            return particleEffect;
        }

        public void Freeze(VisualHandler _visualHandler)
        {

        }
    }
    public class PosionBreathing : Breathing
    {
        public PosionBreathing(int _id, int _howManyBreaths, TrapType _trapType, EffectType _effectType, float _changeTime, Transform breathContent, float breathDamage, float breathSpeed, Quaternion defaultRotate, float extinctionValue, CapsuleCollider capsuleCollider) : base(_id, _howManyBreaths, _trapType, _effectType, _changeTime, breathContent, breathDamage, breathSpeed, defaultRotate, extinctionValue, capsuleCollider)
        {
        }

        public override GameObject EffectTheTarget(GameObject _arrow, Transform _targetTransform)
        {
            GameObject particleEffect = Instantiate(_arrow, _targetTransform);
            return particleEffect;
        }

        public void Posion(VisualHandler _visualHandler)
        {

        }
    }
}
public enum BreathType
{
    Normal,
    FireBreath,
    FreezerBreath,
    PoisonBreath,
}