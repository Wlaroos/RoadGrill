using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float _interactionRange = 3.5f;
    [SerializeField] private LayerMask _interactionLayer;

    [Header("UI References")]
    [SerializeField] private Crosshair _crosshair; // Reference to the new Crosshair component

    private Camera _mainCamera;
    private Interactable _currentInteractable;
    private PlayerInventory _playerInventory;

    private void Start()
    {
        _mainCamera = Camera.main;
        _playerInventory = FindFirstObjectByType<PlayerInventory>();
        
        if (_crosshair != null)
        {
            _crosshair.ApplyDefaultCrosshair();
        }
    }

    private void OnEnable()
    {
        PlayerInputs.OnInteractPressed += HandleInteractionInputEvent;
    }

    private void OnDisable()
    {
        PlayerInputs.OnInteractPressed -= HandleInteractionInputEvent;
    }

    private void Update()
    {
        bool isDriving = PlayerVehicleManager.Instance != null && PlayerVehicleManager.Instance.IsDriving;
        bool isInventoryOpen = _playerInventory != null && _playerInventory.IsInventoryOpen;

        // Handle crosshair visibility and styling based on game state
        if (isInventoryOpen)
        {
            SetCrosshairVisible(true);
            ClearCurrentInteractable();
            ApplyCrosshairForType(InteractionType.DefaultUI);
        }
        else
        {
            if (isDriving)
            {
                SetCrosshairVisible(false);
                ClearCurrentInteractable();
            }
            else
            {
                SetCrosshairVisible(true);
                CheckForInteractable();
            }
        }
    }

    private void HandleInteractionInputEvent()
    {
        bool isDriving = PlayerVehicleManager.Instance != null && PlayerVehicleManager.Instance.IsDriving;
        bool isInventoryOpen = _playerInventory != null && _playerInventory.IsInventoryOpen;

        // Prevent world interactions if inventory is currently open
        if (isInventoryOpen) return;

        if (isDriving)
        {
            if (PlayerVehicleManager.Instance != null)
            {
                PlayerVehicleManager.Instance.ExitCar();
            }
        }
        else
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.Interact();
            }
        }
    }

    private void SetCrosshairVisible(bool visible)
    {
        if (_crosshair != null)
        {
            _crosshair.SetVisible(visible);
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionLayer))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();

            if (interactable != null)
            {
                if (_currentInteractable != interactable)
                {
                    ClearCurrentInteractable();
                    _currentInteractable = interactable;
                }

                ApplyCrosshairForType(interactable.InteractionType);
                return;
            }
        }

        ClearCurrentInteractable();
        ApplyDefaultCrosshair();
    }

    private void ApplyCrosshairForType(InteractionType type)
    {
        if (_crosshair != null)
        {
            _crosshair.ApplyCrosshairForType(type);
        }
    }

    private void ApplyDefaultCrosshair()
    {
        if (_crosshair != null)
        {
            _crosshair.ApplyDefaultCrosshair();
        }
    }

    private void ClearCurrentInteractable()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable = null;
        }
    }
}