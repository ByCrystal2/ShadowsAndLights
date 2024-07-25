using System.Collections.Generic;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    List<ICollectInventory> CollectedInventoryItems = new List<ICollectInventory>();

    public void AddInventory(ICollectInventory collectedObj)
    {
        CollectedInventoryItems.Add(collectedObj);
        if (collectedObj is MonoBehaviour behaviour)
        {
            Debug.Log($"{behaviour.name} adli obje envantere eklendi");
        }
    }
    public bool IsItemInInventory(ICollectInventory _item) => CollectedInventoryItems.Contains(_item);
    public bool IsHaveAnyItemInInventory() => CollectedInventoryItems.Count > 0;
}
