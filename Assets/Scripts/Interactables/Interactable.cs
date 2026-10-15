using UnityEngine;
using System;

public enum InteractionType
{
    Default,
    Vehicle,
    Item,
    NPC
}

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractionType _interactionType = InteractionType.Default;
    public InteractionType InteractionType => _interactionType;

    public virtual void Interact()
    {
        // Base interaction event or override in child classes
        Debug.LogFormat("Interacted with {0}", gameObject.name);
    }
}