using System.Collections.Generic;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    [SerializeField] InventoryCanvasHandler canvasHandler;
    List<ICollectInventory> CollectedInventoryItems = new List<ICollectInventory>();

    public void AddInventory(ICollectInventory collectedObj)
    {
        CollectedInventoryItems.Add(collectedObj);
        if (collectedObj is MonoBehaviour behaviour)
        {
            Debug.Log($"{behaviour.name} adli obje envantere eklendi");
        }
        canvasHandler.CalculateItems();
    }
    public void RemoveInventory(ICollectInventory _item)
    {
        if (CollectedInventoryItems.Contains(_item))
        {
            CollectedInventoryItems.Remove(_item);
            canvasHandler.CalculateItems();
        }
        else
            Debug.Log($"{_item.InventoryType} envanter tipli oge envanter itemlerinde bulunmamaktadir.");
    }
    private void FixedUpdate()
    {
        Debug.Log("CollectedInventoryItems.Count => " + CollectedInventoryItems.Count);
    }
    public bool IsItemInInventory(ICollectInventory _item) => CollectedInventoryItems.Contains(_item);
    public bool IsHaveAnyItemInInventory() => CollectedInventoryItems.Count > 0;
    public List<ICollectInventory> GetCollectInventories() => CollectedInventoryItems;
}
