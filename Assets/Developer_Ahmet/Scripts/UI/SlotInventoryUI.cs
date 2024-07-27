using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotInventoryUI : MonoBehaviour
{
    [SerializeField] GameObject SlotPrefab;
    [SerializeField] Transform slotsContent;

    public void SetInventory(List<ICollectInventory> _items)
    {
        if (_items.Count <= 0) return;
        ClearSlotContent();
        int length = _items.Count;
        for (int i = 0; i < length; i++)
        {
            if (_items[i] is not null && _items[i] is IEnterAnySlotable s)
            {
                GameObject slotObj = Instantiate(SlotPrefab, slotsContent);
                if (slotObj.TryGetComponent(out SlotUI slot))
                {
                    if (!slot.IsFull)
                    {
                        slot.SetItem(_items[i]);
                        continue;
                    }
                }                
            }
        }
    }
    private void ClearSlotContent()
    {
        int length = slotsContent.childCount;
        for (int i = 0; i < length; i++)
        {
            GameObject slot = slotsContent.GetChild(i).gameObject;
            DestroyImmediate(slot);
        }
    }
}
