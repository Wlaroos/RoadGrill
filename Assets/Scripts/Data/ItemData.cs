using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Base Item Info")]
    public string itemName;
    public string itemID;
    [Header("Icon")]
    public Sprite icon;
    [Header("Conditionals")]
    public bool isChoppable;
    [ConditionalHide("isChoppable")]
    public ItemData choppedItem;
    public bool isGrillable;
    [ConditionalHide("isGrillable")]
    public ItemData grilledItem;
    public bool isStackable = true;
    [ConditionalHide("isStackable")]
    public int maxStackSize = 1;
    public bool isSellable = true;
    [ConditionalHide("isSellable")]
    public float value = 10;
}