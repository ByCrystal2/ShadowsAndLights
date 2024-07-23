using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public partial class ArrowDispenserBehaviour : TrapBehaviour
{
   ArrowDispenser m_ArrowDispenser;
    private void Start()
    {
        m_ArrowDispenser = new ArrowDispenser(ID, HowMuchArrow, TrapType, EffectType, ChangeTime, MinActiveLevel, MaxActiveLevel, ArrowShootingContent, ArrowsDamage, ArrowsSpeed, ArrowDefaultRotate, ExtinctionValue);
        for (int i = 0; i < HowMuchArrow; i++)
        {
            GameObject arrow = Instantiate(LightPuzzleHandler.instance.GetArrow(ArrowType), m_ArrowDispenser.ArrowShootingContent);
            ArrowBehaviour arrowBehaviour = arrow.GetComponent<ArrowBehaviour>();
            arrowBehaviour.SetDispenser(m_ArrowDispenser);
            arrow.transform.localRotation = ArrowDefaultRotate;
            arrow.SetActive(false);
            m_ArrowDispenser.Objects.Add(arrowBehaviour);
        }
        SetTrap(m_ArrowDispenser);
        //InvokeRepeating(nameof(ShootNewArrow), 2,4);
    }    
    IEnumerator ShootArrows()
    {
        for (int i = 0;i < HowMuchArrow; i++)
        {
            yield return new WaitForSeconds(ChangeTime);
            ShotArrow();
        }        
    }
    void ShotArrow()
    {
        m_ArrowDispenser.Objects[0].gameObject.SetActive(true);
        m_ArrowDispenser.Objects.RemoveAt(0);
        //AudioSource.PlayOneShot(LightPuzzleHandler.instance.GetSoundInEffectOnTarget(TrapType));
        GameAudioManager.instance.PlayTrapSound(AudioSourceHelper);
        Debug.Log($"This ArrowDispenser ({name}) Fired! Currnet Arrows Count:{m_ArrowDispenser.Objects.Count}");
    }
    Coroutine shotArrowsCoroutine;
    private void OnTriggerEnter(Collider other)
    {
        if (m_ArrowDispenser.AllObjectShoot()) return;
        if (other.CompareTag("Animal"))
        {
            Debug.Log($"This ArrowDispenser ({name}) trigged the target ({other.name}).");
            if (IsConsecutiveShots)
            {
                if (shotArrowsCoroutine == null) { shotArrowsCoroutine = StartCoroutine(ShootArrows()); }
            }
            else
                ShotArrow();
        }
                     
    }
}
public partial class ArrowDispenserBehaviour : TrapBehaviour
{
    public float ArrowsDamage;
    public float ArrowsSpeed;
    public int HowMuchArrow;
    public Transform ArrowShootingContent;
    public Quaternion ArrowDefaultRotate;
    public bool IsConsecutiveShots;
    public ArrowType ArrowType;
    [Range(0, 50)] public float ExtinctionValue;
}//SerializeFieds...
[System.Serializable]
public struct ArrowHelper
{
    public GameObject Arrow;
    public ArrowType ArrowType;
}
public enum ArrowType
{
    Normal,
    FieryArrow,
    FreezerArrow,
    PoisonArrow,
    SlowdownArrow
}