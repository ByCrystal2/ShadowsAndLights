using System.Collections.Generic;
using UnityEngine;

public class InventoryCanvasHandler : MonoBehaviour
{
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] Transform slotsContent;
    InventoryHandler inventoryHandler;

    private void Awake()
    {
        inventoryHandler = GameObject.FindWithTag("Animal").GetComponent<InventoryHandler>();
    }
    private void Start()
    {
        CalculateItems();
    }
    public void CalculateItems()
    {
        ClearContent();
        int length = slotsContent.childCount;
        List<ICollectInventory> list = inventoryHandler.GetCollectInventories();
        int slotCount = 0;
        for (int i = 0; i < length; i++)
        {
            if (slotsContent.GetChild(i).TryGetComponent(out SlotUI slotUI))
            {
                if (slotCount >= list.Count)
                    break;
                Debug.Log("slotUI.name => " + slotUI.name);
                slotUI.SetItem(list[slotCount]);
                slotCount++;
            }
        }
    }
    private void ClearContent()
    {
        int length = slotsContent.childCount;
        List<ICollectInventory> list = inventoryHandler.GetCollectInventories();
        for (int i = 0; i < length; i++)
        {
            if (slotsContent.GetChild(i).TryGetComponent(out SlotUI slotUI))
            {
                slotUI.FreeSlot();
            }
        }
    }
}
