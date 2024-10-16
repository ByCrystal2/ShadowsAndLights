using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightRouter : MonoBehaviour, ICollectable, IInteractable, ICollectHand, IRotateAnObject
{
    [SerializeField] RotateHandler RotateObject;
    public CollectHandType CollectType { get; set; }
    public GameObject HandObject { get; set; }
    public bool IsCollected { get; set; }
    public IRotatable RotatableObject { get { return RotateObject; } set { } }

    public List<InteractType> InteractTypes { get; set; } = new List<InteractType>();
    public InteractableBarHandler barHandler { get; set; }

    CharacterBehaviour player;
    private void Awake()
    {
        player = GameObject.FindWithTag("Animal").GetComponent<CharacterBehaviour>();
        HandObject = gameObject;
        InteractTypes = new List<InteractType> { InteractType.Pickable, InteractType.Dropable, InteractType.Rotatable };
        barHandler = GetComponentInChildren<InteractableBarHandler>();
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
            IsCollected = true;
            //List<Collider> colliders = transform.GetComponentsInChildren<Collider>().ToList();
            //foreach (var item in colliders)
            //    item.enabled = false;
        }
        else if (_interactType == InteractType.Dropable)
        {
            int _currentLv = 1; //Diger objeler icin onlarin classlarindan leveline erisebilirsin sonrasinda. su an tek tasinabilir obje Director.
            if(transform.TryGetComponent(out DirectorBehaviour b))
                _currentLv = b.GetLevelID();

            transform.SetParent(LightPuzzleHandler.instance.GetDirectorsParent(_currentLv).transform);
            barHandler.gameObject.SetActive(false);
            IsCollected = false;
            player.playerUI.CloseInteractUIS();
            PlaceOnGround(false);

            //LightPuzzleHandler.instance.CurrentLevelHandler.OnLevelObjectChanged(); // => null hatas� geliyor diye yorum satiri yaptim. #Ahmet
            //List<Collider> colliders = transform.GetComponentsInChildren<Collider>().ToList();
            //foreach (var item in colliders)
            //    item.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        if (IsCollected)
        {
            PlaceOnGround(false);
        }
    }

    void PlaceOnGround(bool _useOffset)
    {
        Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + 5, transform.position.z), Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 30, LightPuzzleHandler.LayerMaskHelper.DirectorFloor))
            transform.position = new Vector3(transform.position.x, hit.point.y + (_useOffset ? 0.75f : 0), transform.position.z);
        
    }
}
public interface ICollectable
{
    public CollectHandType CollectType { get; set; }
    public bool IsCollected { get; set; }
    void Collect();
    public InteractableBarHandler barHandler { get; set; }
}
public interface ICollectHand
{
    public GameObject HandObject { get; set; }    
}
public interface ICollectInventory
{
    public Sprite MySprite { get; }
    CollectInventoryType InventoryType { get; set; }
    void AddInventory(InventoryHandler inventoryHandler);
}
public interface IInteractable
{
    public List<InteractType> InteractTypes { get; set; }
    void Interact(InteractType _interactType);
}
public interface IEnterAnySlotable{
    
    void EnterSlot(SlotHandler _slotHandler);
}
public enum CollectHandType
{
    LightRouter,
    Battery
}
public enum CollectInventoryType
{
    Battery
}
public enum InteractType
{
    Pickable,
    Dropable,
    Rotatable
}
