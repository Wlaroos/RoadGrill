using UnityEngine;
using System;
using Unity.VisualScripting;

public enum InteractionType
{
    Default,
    DefaultUI,
    Vehicle,
    Item,
    NPC,
    InventoryInteract
}

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractionType _interactionType = InteractionType.Default;
    public InteractionType InteractionType => _interactionType;

    private PlayerInventory _playerInv;
    public PlayerInventory PlayerInventory => _playerInv;

    public virtual void Awake()
    {
        _playerInv = FindFirstObjectByType<PlayerInventory>();
    }
    public virtual void Interact()
    {
        // Base interaction event or override in child classes
        Debug.LogFormat("Interacted with {0}", gameObject.name);
    }
}