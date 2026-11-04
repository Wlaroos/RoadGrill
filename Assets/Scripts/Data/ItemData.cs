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
    [ConditionalHide("isChoppable")]
    public ItemData choppedItem;
    [ConditionalHide("isChoppable")]
    public int choppedItemAmount = 1;
    public bool isGrillable;
    [ConditionalHide("isGrillable")]
    public ItemData grilledItem;
    [ConditionalHide("isGrillable")]
    public float cookTime = 5f;
    public bool isStackable = true;
    [ConditionalHide("isStackable")]
    public int maxStackSize = 1;
    public bool isSellable = true;
    [ConditionalHide("isSellable")]
    public float value = 10;

    // Hidden field to track the flat index of the item
    private int _flatIndex = 0;
    public int FlatIndex
    {
        get { return _flatIndex; }
        set { _flatIndex = value; }
    }
}