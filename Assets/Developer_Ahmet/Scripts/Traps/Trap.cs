using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trap : IHaveVisualEffect
{
    public int ID;
    public bool IsActive { get; private set; }
    public int MinActiveLevel { get; set; }
    public int MaxActiveLevel { get; set; }
    public TrapType TrapType { get; set; }
    public Material Material { get; set; }
    public float ChangeTime { get; set; }
    public EffectType EffectType { get; set; }
    public GameObject ParticleObject { get; set; }
    public float EffectDamage { get; set; }
    public float EffectChangeTime { get; set; }

    protected Trap(int _id, TrapType _trapType, EffectType _effectType,float _changeTime,int _minActiveLevel,int _maxActiveLevel, GameObject particleObject = null, float effectDamage = 0, float effectChangeTime = 0)
    {
        ID = _id;
        TrapType = _trapType;
        EffectType = _effectType;
        Material = LightPuzzleHandler.instance.GetMaterialInEffectOnTarget(TrapType, EffectType);
        ChangeTime = _changeTime;
        MinActiveLevel = _minActiveLevel;
        MaxActiveLevel = _maxActiveLevel;
        ParticleObject = particleObject;
        EffectDamage = effectDamage;
        EffectChangeTime = effectChangeTime;
    }
    public void TrapCheck(int _currentLevel)
    {
        TrapActivation(_currentLevel < MaxActiveLevel ||  _currentLevel > MinActiveLevel);
    }
    private void TrapActivation(bool _active) => IsActive = _active;

}
public class TrapBehaviour : MonoBehaviour
{
    [SerializeField] protected int ID;
    [SerializeField] protected int Level;
    [SerializeField] protected int MinActiveLevel;
    [SerializeField] protected int MaxActiveLevel;
    [SerializeField] protected TrapType TrapType;
    [SerializeField] protected EffectType EffectType;
    [SerializeField,Range(0, 5)] protected float ChangeTime;
    [SerializeField] protected AudioSourceHelper AudioSourceHelper;
    [SerializeField] protected GameObject ParticleObject;
    [SerializeField] protected float EffectDamage;
    [SerializeField] protected float EffectChangeTime;
    protected Trap trap;
    private void Awake()
    {
        AudioSourceHelper.Position = transform.position;
    }
    public void SetTrap(Trap _trap) => trap = _trap;
    public Trap GetTrap() => trap;
    public void SetLevel(int _level)
    {
        Level = _level;
        trap.MinActiveLevel = _level - MinActiveLevel > 0 ? _level - MinActiveLevel : 0;
        trap.MaxActiveLevel = _level + MaxActiveLevel < LightPuzzleHandler.instance.PuzzleRoomCount ? _level + MaxActiveLevel : LightPuzzleHandler.instance.PuzzleRoomCount;
    }

    
    //[SerializeField] protected AudioSource AudioSource;
}
public class MouseTrap : Trap, ICanDamage
{
    public float Damage { get; set; }
    public MouseTrap(int _id, float _damage, TrapType _trapType, EffectType _effectType, float _changeTime, int _minActiveLevel, int _maxActiveLevel) : base(_id, _trapType,_effectType, _changeTime,_minActiveLevel,_maxActiveLevel)
    {
        Damage = _damage;
    }

    public void IEHit(HealthHandler _targetHealth)
    {
        //yield return new WaitForSeconds(_waiting);
        _targetHealth.TakeDamage((ICanDamage)this);
    }
}
public class ArrowDispenser : Trap, ICanHoldMultipleObjects<ArrowBehaviour>
{
    public ArrowDispenser(int _id,int _howManyArrows, TrapType _trapType,EffectType _effectType, float _changeTime, int _minActiveLevel, int _maxActiveLevel, Transform arrowShootingContent, float arrowsDamage, float arrowsSpeed, Quaternion defaultRotate, float extinctionValue) : base(_id, _trapType,_effectType, _changeTime,_minActiveLevel,_maxActiveLevel)
    {
        HowMany = _howManyArrows;
        ArrowShootingContent = arrowShootingContent;
        ArrowsDamage = arrowsDamage;
        ArrowsSpeed = arrowsSpeed;
        DefaultRotate = defaultRotate;
        ExtinctionValue = extinctionValue;
    }
    public bool AllObjectShoot()
    {
        return Objects.Count <= 0 ? true : false;
    }

    public Quaternion DefaultRotate { get; set; }
    public Transform ArrowShootingContent;
    public float ArrowsSpeed{ get; set; }
    public float ArrowsDamage{ get; set; }
    public float ExtinctionValue { get; set; }
    public int HowMany { get; set; }
    public List<ArrowBehaviour> Objects { get; set; } = new List<ArrowBehaviour>();
}

[System.Serializable]
public struct TrapEffectHelper
{
    public Material Material;
    public TrapType TrapType;
    public EffectType EffectType;

}
[System.Serializable]
public struct TrapSoundHelper
{
    public AudioClip Clip;
    public TrapType TrapType;
}
public enum TrapType
{
    None,
    MouseTrap,
    ArrowDispenser,
    Arrow,
    Breathing,
    PosionWater,
}
public enum EffectType
{
    Normal,
    Fiery,
    Freezer,
    Posion,
    Slowdown,
}
public interface ICanDamage
{
    public float Damage { get; set; }
    void IEHit(HealthHandler targetHealth);
}
public interface ICanHoldMultipleObjects<T> where T : Component
{
    public int HowMany { get; set; }
    public List<T> Objects {  get; set; }
    bool AllObjectShoot();
}
public interface ITrapMovable
{
    public Vector3 DefaultPos { get; set; }
    public float Speed { get; set; }
    public Vector3 Direction { get; set; } // Ex: Vector3.forward;
}
public interface IHaveVisualEffect
{
    public GameObject ParticleObject { get; set; }
    public float EffectDamage{ get; set; }
    public Material Material { get; set; }
    public TrapType TrapType { get; set; }
    public EffectType EffectType { get; set; }
    public float ChangeTime { get; set; }
    public float EffectChangeTime { get; set; }
}