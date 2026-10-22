using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData itemData;       // Works for any item (sword, potion, animal)
    public int quantity;
    private int baseValue;

    // Optional data for items that require extra state (like animal flattening)
    public int flatIndex = -1;      // -1 means not applicable / normal item

    public InventoryItem(ItemData data, int quantity = 1, int flatIndex = -1)
    {
        this.itemData = data;
        this.quantity = quantity;
        this.flatIndex = flatIndex;
    }

    public int GetBaseValue()
    {
        return baseValue;
    }

    // Helper to calculate value if it's an animal item
    public int GetCalculatedValue()
    {
        if (itemData is AnimalItemData animalData)
        {
            float multiplier = 1f;
            if (animalData.flatValueMultipliers != null && flatIndex >= 0 && flatIndex < animalData.flatValueMultipliers.Length)
            {
                multiplier = animalData.flatValueMultipliers[flatIndex];
            }
            return Mathf.RoundToInt(baseValue * multiplier);
        }
        return 0; // Default or standard item value logic here
    }
}