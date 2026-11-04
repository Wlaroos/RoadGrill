using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Base Item Info")]
    public string itemName;
    public string itemID;
    [Header("Icon")]
    public Sprite icon;
    [Header("World Prefab")]
    public GameObject worldPrefab;
    [Header("Conditionals")]
    public bool isChoppable;
    public ItemData choppedItem;
    public int choppedItemAmount = 1;
    public bool isGrillable;
    public ItemData grilledItem;
    public float cookTime = 5f;
    public bool isStackable = true;
    public int maxStackSize = 1;
    public bool isSellable = true;
    public float value = 10;
    [Header("Flatness")]
    public bool isFlattenable = false;
    public float[] flatValueMultipliers = { 1.0f, 0.8f, 0.5f, 0.2f };
}