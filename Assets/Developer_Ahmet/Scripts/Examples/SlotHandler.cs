using System.Collections.Generic;
using UnityEngine;

public class SlotHandler : MonoBehaviour
{
    [SerializeField] List<Transform> MySlots = new List<Transform>();
    [SerializeField] int SlotCapasity = 1;
    [SerializeField] Transform target;
    InventoryHandler inventoryHandler;
    private void OnValidate()
    {
        if (MySlots.Count == 0) return;
        List<Transform> slots = new List<Transform>();
        int length = SlotCapasity;
        for (int i = 0; i < length; i++)
        {
            if (i < MySlots.Count)
            slots.Add(MySlots[i]);
        }
        if (MySlots.Count > SlotCapasity)
        {
            MySlots.Clear();
            int length1 = slots.Count;
            for (int i = 0; i < length1; i++)
            {
                MySlots.Add(slots[i]);
            }
        }
    }
    private void Awake()
    {
        inventoryHandler = GameObject.FindWithTag("Animal").GetComponent<InventoryHandler>();
    }
    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && inventoryHandler.IsHaveAnyItemInInventory())
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, LightPuzzleHandler.LayerMaskHelper.CarryLayer, QueryTriggerInteraction.Ignore))
            {
                GameObject hitObject = hit.collider.gameObject;
                if(hitObject.GetInstanceID() == gameObject.GetInstanceID())
                {
                    if (target is IInteractable interact)
                    {
                        interact.Interact(InteractType.Dropable);
                        if (interact is IEnterAnySlotable slotable)
                        {
                            slotable.EnterSlot(this);
                        }
                    }
                }                
            }
        }
    }
    public void AddMyContent(GameObject _object)
    {
        if (SlotCapasity > 0)
        {
            _object.transform.SetPositionAndRotation(MySlots[SlotCapasity].transform.position, MySlots[SlotCapasity].rotation);
            SlotCapasity--;
            if (SlotCapasity < 0)
            {
                SlotCapasity = 0;
                Debug.Log("Tum slotlar dolu.");
                return;
            }
        }
        else
        {
            Debug.Log("Slot kapasitesi 0 veya 0 dan kucuk => " + SlotCapasity);
        }
    }
}
