using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData _itemData;
    public int _quantity;
    private int _baseValue;

    // Optional data (animal flattening)
    public int _flatIndex = -1;      // -1 means not applicable / normal item

    public InventoryItem(ItemData data, int quantity = 1, int flatIndex = -1)
    {
        _itemData = data;
        _quantity = quantity;
        _flatIndex = flatIndex;
    }

    public int GetBaseValue()
    {
        return _baseValue;
    }

    // Helper to calculate value if it's an animal item
    public int GetCalculatedValue()
    {
        if (_itemData is AnimalItemData animalData)
        {
            float multiplier = 1f;
            if (animalData.flatValueMultipliers != null && _flatIndex >= 0 && _flatIndex < animalData.flatValueMultipliers.Length)
            {
                multiplier = animalData.flatValueMultipliers[_flatIndex];
            }
            return Mathf.RoundToInt(_baseValue * multiplier);
        }
        return 0;
    }
}