using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
        
        if (_bg != null) _bg.color = _defaultColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Clicked on " + _itemData.itemName + " in the Inventory.");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right clicked on " + _itemData.itemName + " in the Inventory.");
        }
    }
}