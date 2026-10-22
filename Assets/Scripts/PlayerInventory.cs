// PlayerInventory.cs
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<InventoryItem> _inventoryItems = new List<InventoryItem>();
    public IReadOnlyList<InventoryItem> InventoryItems => _inventoryItems;

    public void AddItem(ItemData data, int amount = 1, int flatIndex = -1)
    {
        // Check if stackable and if an identical item (same data + same state/flatIndex) already exists
        InventoryItem existingItem = _inventoryItems.Find(item => item.itemData == data && item.flatIndex == flatIndex);

        if (existingItem != null && data.isStackable)
        {
            existingItem.quantity += amount;
            Debug.Log($"Stacked {amount}x {data.itemName}. Total: {existingItem.quantity}");
        }
        else
        {
            InventoryItem newItem = new InventoryItem(data, amount, flatIndex);
            _inventoryItems.Add(newItem);
            Debug.Log($"Added new item: {data.itemName} (Qty: {amount})");
        }

    }
}