using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private GameObject _inventoryUIPanel;
    [SerializeField] private GameObject _itemUIPrefab;
    [Space]
    [SerializeField] private PlayerInputs _playerInputs;
    [Space]
    [SerializeField] private Crosshair _crosshair;
    [Space]
    [SerializeField] private List<InventoryItem> _inventoryItems = new List<InventoryItem>();
    public IReadOnlyList<InventoryItem> InventoryItems => _inventoryItems;
    private bool _invOpen = false;
    public bool IsInventoryOpen => _invOpen;
    private List<GameObject> _uiPool = new List<GameObject>();
    private CanvasGroup _inventoryUICanvasGroup;

    public static event Action<bool> OnInventoryStateChanged;

    private void OnEnable()
    {
        PlayerInputs.OnInventoryPressed += ToggleInventory;
    }

    private void OnDisable()
    {
        PlayerInputs.OnInventoryPressed -= ToggleInventory;
    }

    private void Awake()
    {
        if (_inventoryUIPanel != null)
        {
            _inventoryUICanvasGroup = _inventoryUIPanel.GetComponent<CanvasGroup>();
            if (_inventoryUICanvasGroup != null)
            {
                _inventoryUICanvasGroup.alpha = 0;
                _inventoryUICanvasGroup.interactable = false;
                _inventoryUICanvasGroup.blocksRaycasts = false;
            }
        }
        _invOpen = false;

        // Auto-assign PlayerInputs if left empty in the Inspector
        if (_playerInputs == null)
        {
            _playerInputs = GetComponent<PlayerInputs>() ?? FindFirstObjectByType<PlayerInputs>();
        }
    }

    private void ToggleInventory()
    {
        // Use this if inventory is not supposed to be opened while driving
        // PlayerVehicleManager vehicleMgr = PlayerVehicleManager.Instance;
        // if (vehicleMgr != null && vehicleMgr.IsDriving) return;

        if(!_invOpen)
        {
            if (_inventoryUICanvasGroup != null)
            {
                _inventoryUICanvasGroup.alpha = 1;
                _inventoryUICanvasGroup.interactable = true;
                _inventoryUICanvasGroup.blocksRaycasts = true;
            }

            UpdateInventoryUI();
            _invOpen = true;
            Cursor.lockState = CursorLockMode.Confined;
            _playerInputs?.DisablePlayerInputs();

            // Broadcast that inventory opened
            OnInventoryStateChanged?.Invoke(true);
        }
        else
        {
            if (_inventoryUICanvasGroup != null)
            {
                _inventoryUICanvasGroup.alpha = 0;
                _inventoryUICanvasGroup.interactable = false;
                _inventoryUICanvasGroup.blocksRaycasts = false;
            }

            _invOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            _playerInputs?.EnablePlayerInputs();

            // Broadcast that inventory closed
            OnInventoryStateChanged?.Invoke(false);
        }
    }

    private void UpdateInventoryUI()
    {
        // Deactivate all pooled UI elements first
        foreach (GameObject uiObj in _uiPool)
        {
            if (uiObj != null) uiObj.SetActive(false);
        }

        // Populate UI for each inventory item
        for (int i = 0; i < _inventoryItems.Count; i++)
        {
            InventoryItem item = _inventoryItems[i];
            GameObject itemUI;

            // Reuse existing pooled object or instantiate a new one
            if (i < _uiPool.Count)
            {
                itemUI = _uiPool[i];
                itemUI.SetActive(true);
            }
            else
            {
                itemUI = Instantiate(_itemUIPrefab, _inventoryUIPanel.transform.GetChild(0));
                _uiPool.Add(itemUI);
            }

            // Populate the UI components
            SetupItemUI(itemUI, item);
        }
    }

    private void SetupItemUI(GameObject itemUI, InventoryItem item)
    {
        Transform layoutGroupTransform = itemUI.transform.GetChild(0);

        Image iconImage = layoutGroupTransform.GetChild(0).GetComponent<Image>();
        TextMeshProUGUI flatIndexText = layoutGroupTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = layoutGroupTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI amountText = layoutGroupTransform.GetChild(3).GetComponent<TextMeshProUGUI>();

        InventoryItemUI itemUIComponent = itemUI.GetComponent<InventoryItemUI>();
        if (itemUIComponent == null)
        {
            itemUIComponent = itemUI.AddComponent<InventoryItemUI>();
        }
        itemUIComponent.Initialize(item._itemData, _crosshair);

        // Set Icon
        if (iconImage != null)
        {
            if (item._itemData != null && item._itemData.icon != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = item._itemData.icon;
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        // Set Name
        if (nameText != null)
        {
            nameText.text = item._itemData != null ? item._itemData.itemName : "Unknown";
        }

        // Set FlatIndex word
        if (flatIndexText != null)
        {
            string flatWord = GetFlatIndexWord(item._flatIndex);
            flatIndexText.text = flatWord;
            flatIndexText.gameObject.SetActive(!string.IsNullOrEmpty(flatWord));
        }

        // Set Amount (only show if greater than 1)
        if (amountText != null)
        {
            amountText.text = item._quantity > 1 ? "x" + item._quantity.ToString() : "";
        }

        // Force immediate layout update so text boxes and parent groups resize correctly
        Canvas.ForceUpdateCanvases();
        if (layoutGroupTransform is RectTransform rectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }

    private string GetFlatIndexWord(int flatIndex)
    {
        // 0 or lower shows nothing
        if (flatIndex <= 0) return string.Empty;

        return flatIndex switch
        {
            1 => "Flat",
            2 => "Flatter",
            3 => "Flattest",
            _ => "Pancake" // Fallback for index higher than 3
        };
    }

    public void AddItem(ItemData data, int amount = 1, int flatIndex = -1)
    {
        // Check if stackable and if an identical item (same data + same state/flatIndex) already exists
        InventoryItem existingItem = _inventoryItems.Find(item => item._itemData == data && item._flatIndex == flatIndex);

        if (existingItem != null && data.isStackable)
        {
            existingItem._quantity += amount;
            Debug.Log($"Stacked {amount}x {data.itemName}. Total: {existingItem._quantity}");
        }
        else
        {
            InventoryItem newItem = new InventoryItem(data, amount, flatIndex);
            _inventoryItems.Add(newItem);
            Debug.Log($"Added new item: {data.itemName} (Qty: {amount})");
        }
    }
}