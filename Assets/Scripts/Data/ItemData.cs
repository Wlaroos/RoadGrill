using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Base Item Info")]
    public string itemName;
    public string itemID;
    public Sprite icon;
    public int maxStackSize = 99;
    public bool isStackable = true;
    public float value = 10;
}