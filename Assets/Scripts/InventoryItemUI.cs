using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color32 _defaultColor;
    [SerializeField] private Color32 _hoverColor;
    private ItemData _itemData;
    private Crosshair _crosshair;
    private Image _bg;

    public void Initialize(ItemData itemData, Crosshair crosshair)
    {
        _bg = GetComponent<Image>();
        _itemData = itemData;
        _crosshair = crosshair;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Hovered over: {_itemData?.itemName}");

        if (_crosshair != null)
        {
            _bg.color = _hoverColor;
            _crosshair.ApplyCrosshairForType(InteractionType.InventoryInteract); 
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_crosshair != null)
        {
            _bg.color = _defaultColor;
            _crosshair.ApplyCrosshairForType(InteractionType.DefaultUI); 
        }
    }
}