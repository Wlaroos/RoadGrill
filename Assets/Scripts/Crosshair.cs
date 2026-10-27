using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Crosshair : MonoBehaviour
{
    [System.Serializable]
    public struct CrosshairConfig
    {
        public InteractionType interactionType;
        public Color color;
        public Sprite sprite;
    }

    [Header("UI Scaling")]
    [SerializeField] private float _crosshairScale = 3f;

    [Header("Default Crosshair")]
    [SerializeField] private Color _defaultCrosshairColor = Color.white;
    [SerializeField] private Sprite _defaultCrosshairSprite;

    [Header("Target Specific Crosshairs")]
    [SerializeField] private List<CrosshairConfig> _crosshairConfigs = new List<CrosshairConfig>();

    private Image _crosshairImage;
    private RectTransform _rt;
    private Dictionary<InteractionType, CrosshairConfig> _configLookup;

    private void Awake()
    {
        _crosshairImage = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();

        BuildLookupDictionary();
    }

    private void Start()
    {
        ApplyDefaultCrosshair();
    }

    private void Update()
    {
        // Follow mouse position when the cursor is unlocked or confined
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Mouse.current != null)
            {
                _rt.position = Mouse.current.position.ReadValue();
            }
        }
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

    public void ApplyCrosshairForType(InteractionType type)
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

    public void ApplyDefaultCrosshair()
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

    public void SetVisible(bool visible)
    {
        if (_crosshairImage != null)
        {
            _crosshairImage.gameObject.SetActive(visible);
        }
    }

    private void SetImageSizeAndPivotOffset(Sprite sprite, float width, float height)
    {
        if (_rt != null)
        {
            _rt.sizeDelta = new Vector2(width, height);

            // Calculate pivot offsets if crosshair needs centered positioning when locked
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 pivot = sprite.pivot;
                Vector2 normalizedPivot = new Vector2(pivot.x / sprite.rect.width, pivot.y / sprite.rect.height);

                float offsetX = (0.5f - normalizedPivot.x) * width;
                float offsetY = (0.5f - normalizedPivot.y) * height;

                _rt.anchoredPosition = new Vector2(offsetX, offsetY);
            }
        }
    }
}