using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trap : IHaveVisualEffect
{
    public int ID;
    public TrapEffectType EffectOnTarget { get; set; }
    public Material Material { get; set; }
    public float ChangeTime { get; set; }

    protected Trap(int _id, TrapEffectType _trapEffectType,float _changeTime)
    {
        ID = _id;
        EffectOnTarget = _trapEffectType;
        Material = LightPuzzleHandler.instance.GetMaterialInEffectOnTarget(EffectOnTarget);
        ChangeTime = _changeTime;
    }
}
public class TrapBehaviour : MonoBehaviour
{
    [SerializeField] protected int ID;
    [SerializeField] protected TrapEffectType TrapEffectType;
    [SerializeField,Range(0, 2)] protected float ChangeTime;
    [SerializeField] protected AudioSource AudioSource;
}
public class MouseTrap : Trap, ICanDamage
{
    public float Damage { get; set; }
    public MouseTrap(int _id, float _damage, TrapEffectType _trapEffectType, float _changeTime) : base(_id, _trapEffectType, _changeTime)
    {
        Damage = _damage;
    }

    public IEnumerator IEHit(HealthHandler _targetHealth, float _waiting)
    {
        yield return new WaitForSeconds(_waiting);
        _targetHealth.TakeDamage(this);
    }
}
public class ArrowDispenser : Trap, ICanHoldMultipleObjects<ArrowBehaviour>
{
    public ArrowDispenser(int _id,int _howManyArrows, TrapEffectType _trapEffectType, float _changeTime, Transform arrowShootingContent, float arrowsDamage, float arrowsSpeed, Quaternion defaultRotate) : base(_id, _trapEffectType, _changeTime)
    {
        HowMany = _howManyArrows;
        ArrowShootingContent = arrowShootingContent;
        ArrowsDamage = arrowsDamage;
        ArrowsSpeed = arrowsSpeed;
        DefaultRotate = defaultRotate;
    }
    
    public Quaternion DefaultRotate { get; set; }
    public Transform ArrowShootingContent;
    public float ArrowsSpeed{ get; set; }
    public float ArrowsDamage{ get; set; }
    public int HowMany { get; set; }
    public List<ArrowBehaviour> Objects { get; set; } = new List<ArrowBehaviour>();
}
[System.Serializable]
public struct TrapEffectHelper
{
    public Material Material;
    public TrapEffectType EffectType;
}
[System.Serializable]
public struct TrapSoundHelper
{
    public AudioClip Clip;
    public TrapEffectType EffectType;
}
public enum TrapEffectType
{
    None,
    MouseTrap,
    ArrowDispenser,
    Burning,
    PosionWater,
}
public interface ICanDamage
{
    public float Damage { get; set; }
    IEnumerator IEHit(HealthHandler targetHealth, float waiting);
}
public interface ICanHoldMultipleObjects<T> where T : Component
{
    public int HowMany { get; set; }
    public List<T> Objects {  get; set; } 
}
public interface ITrapMovable
{
    public float Speed { get; set; }
    public Vector3 Direction { get; set; } // Ex: Vector3.forward;
}
public interface IHaveVisualEffect
{
    public Material Material { get; set; }
    public TrapEffectType EffectOnTarget { get; set; }
    public float ChangeTime { get; set; }
}