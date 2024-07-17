using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public partial class ArrowDispenserBehaviour : TrapBehaviour
{
   ArrowDispenser m_ArrowDispenser;
    private void Start()
    {
        m_ArrowDispenser = new ArrowDispenser(ID, HowMuchArrow, TrapEffectType.ArrowDispenser, ChangeTime, ArrowShootingContent, ArrowsDamage, ArrowsSpeed, ArrowDefaultRotate);
        for (int i = 0; i < HowMuchArrow; i++)
        {
            GameObject arrow = Instantiate(LightPuzzleHandler.instance.GetArrow(), m_ArrowDispenser.ArrowShootingContent);
            ArrowBehaviour arrowBehaviour = arrow.GetComponent<ArrowBehaviour>();
            arrowBehaviour.SetDispenser(m_ArrowDispenser);
            arrow.transform.localRotation = ArrowDefaultRotate;
            arrow.SetActive(false);
            m_ArrowDispenser.Objects.Add(arrowBehaviour);
        }
        //InvokeRepeating(nameof(ShootNewArrow), 2,4);
    }
    private void Update()
    {
        // burda kalindi en son. ok sohotlanmali.
    }
    public void ShootNewArrow()
    {
        if (m_ArrowDispenser.Objects.Count <= 0) return;
        m_ArrowDispenser.Objects[0].gameObject.SetActive(true);
        m_ArrowDispenser.Objects.RemoveAt(0);
    }
}
public partial class ArrowDispenserBehaviour : TrapBehaviour
{
    public float ArrowsDamage;
    public float ArrowsSpeed;
    public int HowMuchArrow;
    public Transform ArrowShootingContent;
    public Quaternion ArrowDefaultRotate;
}//SerializeFieds...
