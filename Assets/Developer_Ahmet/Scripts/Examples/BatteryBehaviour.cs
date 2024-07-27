using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BatteryBehaviour : MonoBehaviour, IInteractable, ICollectable, ICollectInventory, IEnterAnySlotable
{
    [SerializeField] Sprite mySprite;
    public List<InteractType> InteractTypes { get; set; } = new List<InteractType>() { InteractType.Pickable, InteractType.Dropable};
    public CollectHandType CollectType { get; set; } = CollectHandType.Battery;
    public bool IsCollected { get; set; }
    public InteractableBarHandler barHandler { get; set; }
    public CollectInventoryType InventoryType { get; set; } = CollectInventoryType.Battery;
    public Sprite MySprite { get { return mySprite; } }

    CharacterBehaviour player;
    private void Awake()
    {
        player = GameObject.FindWithTag("Animal").GetComponent<CharacterBehaviour>();
        barHandler = GetComponentInChildren<InteractableBarHandler>();
        barHandler.gameObject.SetActive(false);
    }
    public void Collect()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(InteractType _interactType)
    {
        if (!InteractTypes.Contains(_interactType))
        {
            Debug.LogError(_interactType + " etkilesimi " + gameObject.name + " adli bu objede bulunmamaktadir.");
            return;
        }
        Debug.Log($"This object interacted({_interactType}). Object name is {name}");
        if (_interactType == InteractType.Pickable)
        {
            //barHandler.gameObject.SetActive(false);
            player.CollectObject(this,gameObject);
            IsCollected = true;
        }
        else if (_interactType == InteractType.Dropable)
        {
            int _currentLv = 1; //Diger objeler icin onlarin classlarindan leveline erisebilirsin sonrasinda. su an tek tasinabilir obje Director.
            IsCollected = false;    

        }
    }

    public void AddInventory(InventoryHandler inventoryHandler)
    {
        throw new System.NotImplementedException();
    }

    public void EnterSlot(SlotHandler _slotHandler)
    {
        _slotHandler.AddMyContent(gameObject);
    }
}
