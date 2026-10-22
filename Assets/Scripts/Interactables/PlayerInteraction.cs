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

    [Header("Raycast Settings")]
    [SerializeField] private float _interactionRange = 3.5f;
    [SerializeField] private LayerMask _interactionLayer;

    [Header("UI References")]
    [SerializeField] private Image _crosshairImage; // Crosshair image at screen center
    [SerializeField] private float _crosshairScale = 3f;

    [Header("Default Crosshair")]
    [SerializeField] private Color _defaultCrosshairColor = Color.white;
    [SerializeField] private Sprite _defaultCrosshairSprite;

    [Header("Target Specific Crosshairs")]
    [SerializeField] private List<CrosshairConfig> _crosshairConfigs = new List<CrosshairConfig>();

    private RectTransform _rt;
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
        _rt = _crosshairImage.GetComponent<RectTransform>();
        ApplyDefaultCrosshair();
    }

    private void OnEnable()
    {
        PlayerInputs.OnInteractPressed += HandleInteractionInputEvent;
    }

    private void OnDisable()
    {
        PlayerInputs.OnInteractPressed -= HandleInteractionInputEvent;
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

        // Handle crosshair visibility based on driving state
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

    private void HandleInteractionInputEvent()
    {
        bool isDriving = PlayerVehicleManager.Instance != null && PlayerVehicleManager.Instance.IsDriving;

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
        if (_crosshairImage != null)
        {
            _crosshairImage.gameObject.SetActive(visible);
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
        if (_crosshairImage == null) return;

        if (_configLookup != null && _configLookup.TryGetValue(type, out CrosshairConfig config))
        {
            _crosshairImage.color = config.color;
            if (config.sprite != null)
            {
                _crosshairImage.sprite = config.sprite;
                float width = config.sprite.rect.width * _crosshairScale;
                float height = config.sprite.rect.height * _crosshairScale;
                SetImageSizeAndPivotOffset(config.sprite, width, height);
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
            float width = _defaultCrosshairSprite.rect.width * _crosshairScale;
            float height = _defaultCrosshairSprite.rect.height * _crosshairScale;
            SetImageSizeAndPivotOffset(_defaultCrosshairSprite, width, height);
        }
    }

    private void ClearCurrentInteractable()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable = null;
        }
    }

    private void SetImageSizeAndPivotOffset(Sprite sprite, float width, float height)
    {
        if (_rt != null)
        {
            _rt.sizeDelta = new Vector2(width, height);

            Vector2 pivot = sprite.pivot;
            Vector2 normalizedPivot = new Vector2(pivot.x / sprite.rect.width, pivot.y / sprite.rect.height);
            
            float offsetX = (0.5f - normalizedPivot.x) * width;
            float offsetY = (0.5f - normalizedPivot.y) * height;

            _rt.anchoredPosition = new Vector2(offsetX, offsetY);
        }
    }
}