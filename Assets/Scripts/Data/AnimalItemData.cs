using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalItemData", menuName = "Inventory/Animal Item Data")]
public class AnimalItemData : ItemData
{
    [Header("Animal Specific Properties")]
    public int maxHealth = 100;
    [Tooltip("Value multiplier based on how flat/run-over it is")]
    public float[] flatValueMultipliers = { 1.0f, 0.8f, 0.5f, 0.2f };
}