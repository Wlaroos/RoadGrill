using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [System.Serializable]
    public struct CrosshairConfig
    {
        public InteractionType interactionType;
        public Color color;
        public Sprite sprite;
    }

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode _interactKey = KeyCode.E;

    [Header("Raycast Settings")]
    [SerializeField] private float _interactionRange = 3.5f;
    [SerializeField] private LayerMask _interactionLayer;

    [Header("UI References")]
    [SerializeField] private Image _crosshairImage; // Crosshair image at screen center
    
    [Header("Default Crosshair")]
    [SerializeField] private Color _defaultCrosshairColor = Color.white;
    [SerializeField] private Sprite _defaultCrosshairSprite;

    [Header("Target Specific Crosshairs")]
    [SerializeField] private List<CrosshairConfig> _crosshairConfigs = new List<CrosshairConfig>();

    private Camera _mainCamera;
    private Interactable _currentInteractable;
    private Dictionary<InteractionType, CrosshairConfig> _configLookup;

    private void Awake()
    {
        BuildLookupDictionary();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        ApplyDefaultCrosshair();
    }

    private void BuildLookupDictionary()
    {
        _configLookup = new Dictionary<InteractionType, CrosshairConfig>();
        foreach (var config in _crosshairConfigs)
        {
            if (!_configLookup.ContainsKey(config.interactionType))
            {
                _configLookup.Add(config.interactionType, config);
            }
        }
    }

    private void Update()
    {
        bool isDriving = PlayerVehicleManager.Instance != null && PlayerVehicleManager.Instance.IsDriving;

        // Handle crosshair visibility and inputs based on driving state
        if (isDriving)
        {
            SetCrosshairVisible(false);
            ClearCurrentInteractable();
            HandleDrivingExitInput();
        }
        else
        {
            SetCrosshairVisible(true);
            CheckForInteractable();
            HandleInteractionInput();
        }
    }

    private void SetCrosshairVisible(bool visible)
    {
        if (_crosshairImage != null)
        {
            _crosshairImage.gameObject.SetActive(visible);
        }
    }

    private void CheckForInteractable()
    {
        // Raycast from the exact center of the screen
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

        // If nothing hit or out of range
        ClearCurrentInteractable();
        ApplyDefaultCrosshair();
    }

    private void ApplyCrosshairForType(InteractionType type)
    {
        if (_crosshairImage == null) return;

        if (_configLookup != null && _configLookup.TryGetValue(type, out CrosshairConfig config))
        {
            _crosshairImage.color = config.color;
            if (config.sprite != null)
            {
                _crosshairImage.sprite = config.sprite;
            }
        }
        else
        {
            ApplyDefaultCrosshair();
        }
    }

    private void ApplyDefaultCrosshair()
    {
        if (_crosshairImage == null) return;

        _crosshairImage.color = _defaultCrosshairColor;
        if (_defaultCrosshairSprite != null)
        {
            _crosshairImage.sprite = _defaultCrosshairSprite;
        }
    }

    private void ClearCurrentInteractable()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable = null;
        }
    }

    private void HandleInteractionInput()
    {
        if (_currentInteractable != null && Input.GetKeyDown(_interactKey))
        {
            _currentInteractable.Interact();
        }
    }

    private void HandleDrivingExitInput()
    {
        // Pressing the interact key while driving exits the vehicle
        if (Input.GetKeyDown(_interactKey))
        {
            if (PlayerVehicleManager.Instance != null)
            {
                PlayerVehicleManager.Instance.ExitCar();
            }
        }
    }
}