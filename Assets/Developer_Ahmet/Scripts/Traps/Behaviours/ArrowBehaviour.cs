using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class ArrowBehaviour : MonoBehaviour, ICanDamage, ITrapMovable, IHaveVisualEffect
{
    [SerializeField] protected int ID;
    [SerializeField] protected TrapType trapType;
    [SerializeField] protected EffectType effectType;
    [SerializeField, Range(0, 2)] protected float changeTime;
    [SerializeField] protected AudioSourceHelper AudioSourceHelper;
    ArrowDispenser m_Dispenser;
    public void SetDispenser( ArrowDispenser dispenser)
    {
        m_Dispenser = dispenser;
        Damage = m_Dispenser.ArrowsDamage;
        Speed = m_Dispenser.ArrowsSpeed;
        TrapType = trapType;
        EffectType = effectType;
        ChangeTime = changeTime;
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
    #region Props
    public float Damage { get; set; }
    public float Speed { get; set; }
    public Vector3 Direction { get; set; } = Vector3.forward;
    public Material Material { get; set; }
    public TrapType TrapType { get ; set; }
    public float ChangeTime { get; set; }
    public Vector3 DefaultPos { get; set; }
    public EffectType EffectType { get; set; }
    #endregion
    public void IEHit(HealthHandler targetHealth)
    {
        targetHealth.TakeDamage(this);
    }

    private void OnEnable()
    {
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthHandler healthHandler))
        {
            Debug.Log($"This Arrow ({name}) trigged the target ({other.name}).");
            IEHit(healthHandler);
            //AudioSource.PlayOneShot(LightPuzzleHandler.instance.GetSoundInEffectOnTarget(TrapType));
            GameAudioManager.instance.PlayTrapSound(AudioSourceHelper);
            DeactivateArrow();
        }
        else
            followWay = false;
    }
}
