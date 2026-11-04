using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]

public abstract class BaseInteractiveUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] protected Color32 _defaultColor;
    [SerializeField] protected Color32 _hoverColor;
    
    protected Crosshair _crosshair;
    protected Image _bg;

    protected virtual void Awake()
    {
        _bg = GetComponent<Image>();
        _bg.color = _defaultColor;

        _crosshair = Crosshair.Instance;
    }

    public virtual void Initialize()
    {
        _bg.color = _defaultColor;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (_crosshair != null)
        {
            _bg.color = _hoverColor;
            _crosshair.ApplyCrosshairForType(InteractionType.InventoryInteract); 
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (_crosshair != null)
        {
            _bg.color = _defaultColor;
            _crosshair.ApplyCrosshairForType(InteractionType.DefaultUI); 
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // Override in child classes
    }
}