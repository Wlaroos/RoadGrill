using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalItemData", menuName = "Inventory/Animal Item Data")]
public class AnimalItemData : ItemData
{
    [Header("Animal Specific Properties")]
    public int maxHealth = 100;
}