using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemUI : BaseInteractiveUI
{
    private ItemData _itemData;

    public void Initialize(ItemData itemData)
    {
        base.Initialize();
        _itemData = itemData;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (_itemData == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Clicked on " + _itemData.itemName + " in the Inventory.");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right clicked on " + _itemData.itemName + " in the Inventory.");
        }
    }
}