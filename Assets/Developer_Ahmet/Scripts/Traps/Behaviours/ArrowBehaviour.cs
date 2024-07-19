using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class ArrowBehaviour : MonoBehaviour, ICanDamage, ITrapMovable, IHaveVisualEffect
{
    [SerializeField] protected int ID;
    [SerializeField] protected TrapType trapType;
    [SerializeField] protected EffectType effectType;
    [SerializeField, Range(0, 5)] protected float changeTime;
    [SerializeField] protected AudioSourceHelper AudioSourceHelper;
    [SerializeField] protected GameObject particleEffectObject;
    [SerializeField,Range(0, 50)] protected float effectDamage;
    [SerializeField,Range(0, 50)] protected float effectChangeTime;
    ArrowDispenser m_Dispenser;
    Arrow myArrow;
    public void SetDispenser( ArrowDispenser dispenser)
    {
        m_Dispenser = dispenser;
        ParticleObject = particleEffectObject;
        Damage = m_Dispenser.ArrowsDamage;
        Speed = m_Dispenser.ArrowsSpeed;
        TrapType = trapType;
        EffectType = effectType;
        EffectDamage = effectDamage;
        ChangeTime = changeTime;
        EffectChangeTime = effectChangeTime;
        SetArrow();
        Renderer myRenderer = GetComponentInChildren<Renderer>();
        Material[] oldMaterials =  myRenderer.materials;
        Material[] yeniDizi = new Material[oldMaterials.Length + 1];
        for (int i = 0; i < oldMaterials.Length; i++)
        {
            yeniDizi[i] = oldMaterials[i];
        }
        yeniDizi[yeniDizi.Length - 1] = LightPuzzleHandler.instance.GetMaterialInEffectOnTarget(m_Dispenser.TrapType,m_Dispenser.EffectType);
        myRenderer.materials = yeniDizi;
        Material = LightPuzzleHandler.instance.GetMaterialInEffectOnTarget(TrapType,EffectType);
        followWay = true;
    }
    #region ArrowTypeControl
    public void SetArrow()
    {
        switch (effectType)
        {
            case EffectType.Normal:
                myArrow = null;
                break;
            case EffectType.Fiery:
                myArrow = new FieryArrow();
                break;
            case EffectType.Freezer:
                myArrow = new FreezerArrow();
                break;
            case EffectType.Posion:
                myArrow = new PoisonArrow();
                break;
            case EffectType.Slowdown:
                myArrow = new SlowdownArrow();
                break;
            default:
                break;
        }
    }
    public Arrow GetArrow()
    {
        return myArrow;
    }
    #endregion
    #region Props
    public float Damage { get; set; }
    public float Speed { get; set; }
    public Vector3 Direction { get; set; } = Vector3.forward;
    public Material Material { get; set; }
    public TrapType TrapType { get ; set; }
    public float ChangeTime { get; set; }
    public Vector3 DefaultPos { get; set; }
    public EffectType EffectType { get; set; }
    public GameObject ParticleObject { get; set; }
    public float EffectDamage { get; set; }
    public float EffectChangeTime { get; set; }
    #endregion
    public void IEHit(HealthHandler targetHealth)
    {
        targetHealth.TakeDamage((ICanDamage)this);
        if (myArrow != null)
        {
            targetHealth.GetComponent<VisualHandler>().StartEffect(this);
        }
    }

    private void OnEnable()
    {
        hasCollided = false;
        AudioSourceHelper.Position = transform.position;
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
        if (Vector3.Distance(transform.localPosition, DefaultPos) > m_Dispenser.ExtinctionValue)
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
    bool hasCollided;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthHandler healthHandler))
        {
            if (hasCollided) return;
            Debug.Log($"This Arrow ({name}) trigged the target ({other.name}).");
            IEHit(healthHandler);
            //AudioSource.PlayOneShot(LightPuzzleHandler.instance.GetSoundInEffectOnTarget(TrapType));
            GameAudioManager.instance.PlayTrapSound(AudioSourceHelper);
            DeactivateArrow();
            hasCollided = true;
        }
        else
            followWay = false;
    }
    public abstract class Arrow
    {
        public abstract GameObject EffectTheTarget(GameObject _arrow, Transform _targetTransform);
    }
    public class FieryArrow : Arrow
    {
        public override GameObject EffectTheTarget(GameObject _arrow, Transform _targetTransform)
        {
            GameObject particleEffect = Instantiate(_arrow, _targetTransform);
            return particleEffect;
        }
    }
    public class FreezerArrow : Arrow
    {
        public override GameObject EffectTheTarget(GameObject _arrow, Transform _targetTransform)
        {
            GameObject particleEffect = Instantiate(_arrow, _targetTransform);
            return particleEffect;
        }
    }
    public class PoisonArrow : Arrow
    {
        public override GameObject EffectTheTarget(GameObject _arrow, Transform _targetTransform)
        {
            GameObject particleEffect = Instantiate(_arrow, _targetTransform);
            return particleEffect;
        }
    }
    public class SlowdownArrow : Arrow
    {
        public override GameObject EffectTheTarget(GameObject _arrow, Transform _targetTransform)
        {
            GameObject particleEffect = Instantiate(_arrow, _targetTransform);
            return particleEffect;
        }
    }
}