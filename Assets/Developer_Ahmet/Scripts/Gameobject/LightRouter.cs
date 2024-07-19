using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightRouter : MonoBehaviour, ICollectable, IInteractable, ICollectHand, IRotateAnObject
{
    [SerializeField] RotateHandler RotateObject;
    public CollectType CollectType { get; set; }
    public GameObject HandObject { get; set; }
    public bool IsCollected { get; set; }
    public IRotatable RotatableObject { get { return RotateObject; } set { } }

    public List<InteractType> InteractTypes { get; set; } = new List<InteractType>();
    public DirectorBarHandler barHandler { get; set; }

    CharacterBehaviour player;
    private void Awake()
    {
        player = GameObject.FindWithTag("Animal").GetComponent<CharacterBehaviour>();
        HandObject = gameObject;
        InteractTypes = new List<InteractType> { InteractType.Pickable, InteractType.Dropable, InteractType.Rotatable };
        barHandler = GetComponentInChildren<DirectorBarHandler>();
        barHandler.gameObject.SetActive(false);
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
            barHandler.gameObject.SetActive(false);
            player.CollectObject(this, HandObject);
            int layer = LayerMask.NameToLayer("PlayerCarry");
            Transform[] ts = transform.GetComponentsInChildren<Transform>();
            List<Transform> all = new();
            //all.Add(transform);
            SetObjectOutlined(true, all);
        }
        else if (_interactType == InteractType.Dropable)
        {
            //birakme islemleri...
            int _currentLv = 1; //Diger objeler icin onlarin classlarindan leveline erisebilirsin sonrasinda. su an tek tasinabilir obje Director.
            if(transform.TryGetComponent(out DirectorBehaviour b))
                _currentLv = b.GetLevelID();

            transform.SetParent(LightPuzzleHandler.instance.GetDirectorsParent(_currentLv));
            barHandler.gameObject.SetActive(false);
            transform.localPosition = new Vector3(transform.localPosition.x, GetFloorHeight(), transform.localPosition.z);
            //transform.localPosition = new Vector3();
            IsCollected = false;
            player.playerUI.CloseInteractUIS();
            Debug.Log(name + " adli obje birakildi..");
            int layer = LayerMask.NameToLayer("PlayerIgnore");
            Transform[] ts = transform.GetComponentsInChildren<Transform>();
            List<Transform> all = new();
            //all.Add(transform);
            SetObjectOutlined(false, all);
        }
    }
    
    public float GetFloorHeight()
    {
        return 0;
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
    public DirectorBarHandler barHandler { get; set; }
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
