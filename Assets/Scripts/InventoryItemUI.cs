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
    private bool _hovered;

    public void Initialize(ItemData itemData, Crosshair crosshair)
    {
        _bg = GetComponent<Image>();
        _itemData = itemData;
        _crosshair = crosshair;
    }

    private void OnEnable()
    {
        PlayerInputs.OnClicked += Clicked;
    }

    private void OnDisable()
    {
        PlayerInputs.OnClicked -= Clicked;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Hovered over: {_itemData?.itemName}");

        if (_crosshair != null)
        {
            _hovered = true;
            _bg.color = _hoverColor;
            _crosshair.ApplyCrosshairForType(InteractionType.InventoryInteract); 
        }


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_crosshair != null)
        {
            _hovered = false;
            _bg.color = _defaultColor;
            _crosshair.ApplyCrosshairForType(InteractionType.DefaultUI); 
        }
    }
    
    private void Clicked()
    {
        if(_hovered)
        {
            Debug.Log("Clicked on " + _itemData.itemName + " in the Inventory.");
        }
    }
}