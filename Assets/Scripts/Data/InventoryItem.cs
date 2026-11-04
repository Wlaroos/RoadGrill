using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData _itemData;
    public int _quantity;
    public int _flatIndex;
    private int _baseValue;

    public InventoryItem(ItemData data, int quantity, int flatIndex, bool choppable, bool grillable)
    {
        _itemData = data;
        _quantity = quantity;
        _flatIndex = flatIndex;
        _itemData.isChoppable = choppable;
        _itemData.isGrillable = grillable;
    }

    public int GetBaseValue()
    {
        return _baseValue;
    }

    // Helper to calculate flat value
    public int GetCalculatedValue()
    {
        float multiplier = 1f;

        if (_itemData.flatValueMultipliers != null && _flatIndex >= 0 && _flatIndex < _itemData.flatValueMultipliers.Length)
        {
            multiplier = _itemData.flatValueMultipliers[_flatIndex];
        }
            return Mathf.RoundToInt(_baseValue * multiplier);
    }
}