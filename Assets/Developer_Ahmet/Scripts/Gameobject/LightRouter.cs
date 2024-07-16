using System.Collections.Generic;
using UnityEngine;

public class LightRouter : MonoBehaviour, ICollectable, IInteractable, ICollectHand, IRotateAnObject
{
    [SerializeField] RotateHandler RotateObject;
    public CollectType CollectType { get; set; }
    public GameObject HandObject { get; set; }
    public bool IsCollected { get; set; }
    public IRotatable RotatableObject { get { return RotateObject; } set { } }

    public List<InteractType> InteractTypes { get; set; } = new List<InteractType>();

    CharacterBehaviour player;
    private void Awake()
    {
        player = GameObject.FindWithTag("Animal").GetComponent<CharacterBehaviour>();
        HandObject = gameObject;
        InteractTypes = new List<InteractType> { InteractType.Pickable, InteractType.Dropable, InteractType.Rotatable };
    }

    public void Collect()
    {
        player.CollectObject(this,HandObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Animal"))
        {
            //if (player.HandsFull()) return;
            //player.playerUI.SetInteract(this);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        
    }

    public void Interact(InteractType _interactType)
    {
        if (!InteractTypes.Contains(_interactType))
        {
            Debug.LogError(_interactType + " etkilesimi " + gameObject.name + " adli bu objede bulunmamaktadir.");
            return;
        }
        if (_interactType == InteractType.Pickable)
        {
            player.CollectObject(this, HandObject);
            int layer = LayerMask.NameToLayer("PlayerCarry");
            Transform[] ts = transform.GetComponentsInChildren<Transform>();
            List<Transform> all = new();
            all.Add(transform);
            SetObjectOutlined(true, all);
        }
        else if (_interactType == InteractType.Dropable)
        {
            //birakme islemleri...
            transform.SetParent(LightPuzzleHandler.instance.GetDirectorsParent());
            IsCollected = false;
            player.playerUI.CloseInteractUIS();
            Debug.Log(name + " adli obje birakildi..");
            int layer = LayerMask.NameToLayer("PlayerIgnore");
            Transform[] ts = transform.GetComponentsInChildren<Transform>();
            List<Transform> all = new();
            all.Add(transform);
            SetObjectOutlined(false, all);
        }
    }
    
    public void SetObjectOutlined(bool _outlineActive, List<Transform> _gameObjects)
    {
        int newLayer = _outlineActive ? LayerMask.NameToLayer("PlayerCarry") : LayerMask.NameToLayer("PlayerIgnore");
        foreach (var item in _gameObjects)
            item.gameObject.layer = newLayer;
    }
}
public interface ICollectable
{
    public CollectType CollectType { get; set; }
    public bool IsCollected { get; set; }
    void Collect();
}
public interface ICollectInventory
{

}
public interface ICollectHand
{
    public GameObject HandObject { get; set; }
}

public interface IInteractable
{
    public List<InteractType> InteractTypes { get; set; }
    void Interact(InteractType _interactType);
}
public enum CollectType
{
    LightRouter
}
public enum InteractType
{
    Pickable,
    Dropable,
    Rotatable
}
