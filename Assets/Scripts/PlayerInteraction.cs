using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float _interactionRange = 3.5f;
    [SerializeField] private LayerMask _interactionLayer;

    [Header("UI References")]
    [SerializeField] private Crosshair _crosshair;

    private Camera _mainCamera;
    private Interactable _currentInteractable;
    private bool _isInteractCheckActive = true;

    private void Start()
    {
        _mainCamera = Camera.main;
        
        if (_crosshair != null)
        {
            _crosshair.ApplyDefaultCrosshair();
        }
    }

    private void OnEnable()
    {
        PlayerInputs.OnInteractPressed += HandleInteractionInputEvent;
        PlayerInventory.OnInventoryStateChanged += HandleInventoryStateChanged;
        PlayerVehicleManager.OnDrivingStateChanged += HandleDrivingStateChanged;
    }

    private void OnDisable()
    {
        PlayerInputs.OnInteractPressed -= HandleInteractionInputEvent;
        PlayerInventory.OnInventoryStateChanged -= HandleInventoryStateChanged;
        PlayerVehicleManager.OnDrivingStateChanged -= HandleDrivingStateChanged;
    }

    private void Update()
    {
        // Only run the raycast check when actively on foot and inventory is closed
        if (_isInteractCheckActive)
        {
            CheckForInteractable();
        }
    }

    private void HandleInventoryStateChanged(bool isOpen)
    {
        if (isOpen)
        {
            _isInteractCheckActive = false;
            SetCrosshairVisible(true);
            ClearCurrentInteractable();
            ApplyCrosshairForType(InteractionType.DefaultUI);
        }
        else
        {
            // Resume interaction checks only if not currently driving
            bool isDriving = PlayerVehicleManager.Instance != null && PlayerVehicleManager.Instance.IsDriving;
            if (!isDriving)
            {
                _isInteractCheckActive = true;
                SetCrosshairVisible(true);
            }
        }
    }

    private void HandleDrivingStateChanged(bool isDriving)
    {
        if (isDriving)
        {
            _isInteractCheckActive = false;
            SetCrosshairVisible(false);
            ClearCurrentInteractable();
        }
        else
        {
            // Resume interaction checks only if inventory is closed
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            bool isInventoryOpen = inventory != null && inventory.IsInventoryOpen;
            
            if (!isInventoryOpen)
            {
                _isInteractCheckActive = true;
                SetCrosshairVisible(true);
            }
        }
    }

    private void HandleInteractionInputEvent()
    {
        bool isDriving = PlayerVehicleManager.Instance != null && PlayerVehicleManager.Instance.IsDriving;
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        bool isInventoryOpen = inventory != null && inventory.IsInventoryOpen;

        if (isInventoryOpen) return;

        if (isDriving)
        {
            PlayerVehicleManager.Instance?.ExitCar();
        }
        else if (_currentInteractable != null)
        {
            _currentInteractable.Interact();
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