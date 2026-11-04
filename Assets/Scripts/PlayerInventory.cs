using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum InvType
{
    Inventory,
    ChopTable,
    Grill
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

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

    public InvType CurrentInvType { get; private set; } = InvType.Inventory;
    private GrillInteractable _activeGrillInteractable;

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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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

        if (_playerInputs == null)
        {
            _playerInputs = GetComponent<PlayerInputs>() ?? FindFirstObjectByType<PlayerInputs>();
        }
    }

    public void OpenInventory(InvType invType, GrillInteractable grillInteractable = null)
    {
        CurrentInvType = invType;
        _activeGrillInteractable = grillInteractable;

        if (!_invOpen)
        {
            ToggleInventory();
        }
        else
        {
            UpdateInventoryUI();
        }
    }

    private void ToggleInventory()
    {
        if (!_invOpen)
        {
            if (_playerInputs != null && CurrentInvType != InvType.ChopTable && CurrentInvType != InvType.Grill)
            {
                CurrentInvType = InvType.Inventory;
            }

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

            CurrentInvType = InvType.Inventory;
            _activeGrillInteractable = null;

            OnInventoryStateChanged?.Invoke(false);
        }
    }

    private void UpdateInventoryUI()
    {
        _inventoryUIPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = CurrentInvType switch
        {
            InvType.Inventory => "Inventory",
            InvType.ChopTable => "Chop Table",
            InvType.Grill => "Grill",
            _ => "Inventory"
        };

        foreach (GameObject uiObj in _uiPool)
        {
            if (uiObj != null) uiObj.SetActive(false);
        }

        for (int i = 0; i < _inventoryItems.Count; i++)
        {
            InventoryItem item = _inventoryItems[i];
            GameObject itemUI;

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

        Button itemButton = itemUI.GetComponent<Button>();
        if (itemButton == null)
        {
            itemButton = itemUI.AddComponent<Button>();
        }
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() => HandleItemClicked(item));

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

        if (nameText != null)
        {
            nameText.text = item._itemData != null ? item._itemData.itemName : "Unknown";
        }

        if (flatIndexText != null)
        {
            string flatWord = GetFlatIndexWord(item._flatIndex);
            flatIndexText.text = flatWord;
            flatIndexText.gameObject.SetActive(!string.IsNullOrEmpty(flatWord));
        }

        if (amountText != null)
        {
            amountText.text = item._quantity > 1 ? "x" + item._quantity.ToString() : "";
        }

        Canvas.ForceUpdateCanvases();
        if (layoutGroupTransform is RectTransform rectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }

    public void HandleItemClicked(InventoryItem item)
    {
        if (item == null || item._itemData == null) return;

        switch (CurrentInvType)
        {
            case InvType.Inventory:
                Debug.Log($"[Inventory] Clicked item: {item._itemData.itemName} (Qty: {item._quantity})");
                break;

            case InvType.ChopTable:
                if (item._itemData.isChoppable && item._itemData.choppedItem != null)
                {
                    ItemData chopped = item._itemData.choppedItem;
                    item._quantity--;
                    if (item._quantity <= 0)
                    {
                        _inventoryItems.Remove(item);
                    }

                    AddItem(chopped, 1, item._flatIndex, chopped.isChoppable, chopped.isGrillable);
                    Debug.Log($"[ChopTable] Chopped {item._itemData.itemName} into {chopped.itemName}");
                    UpdateInventoryUI();
                }
                else
                {
                    Debug.Log($"[ChopTable] {item._itemData.itemName} cannot be chopped.");
                }
                break;

            case InvType.Grill:
                if (item._itemData.isGrillable)
                {
                    if (_activeGrillInteractable != null)
                    {
                        _inventoryItems.Remove(item);
                        _activeGrillInteractable.PlaceItemOnGrill(item._itemData);
                        Debug.Log($"[Grill] Placed {item._itemData.itemName} on grill.");
                        ToggleInventory();
                    }
                    else
                    {
                        Debug.LogWarning("[Grill] No active GrillInteractable found.");
                    }
                }
                else
                {
                    Debug.Log($"[Grill] {item._itemData.itemName} cannot be grilled.");
                }
                break;
        }
    }

    private string GetFlatIndexWord(int flatIndex)
    {
        if (flatIndex <= 0) return string.Empty;

        return flatIndex switch
        {
            1 => "Flat",
            2 => "Flatter",
            3 => "Flattest",
            _ => "Pancake"
        };
    }

    public void AddItem(ItemData data, int amount, int flatIndex, bool choppable, bool grillable)
    {
        InventoryItem existingItem = _inventoryItems.Find(item => item._itemData == data && item._flatIndex == flatIndex);

        if (existingItem != null && data.isStackable)
        {
            existingItem._quantity += amount;
            Debug.Log($"Stacked {amount}x {data.itemName}. Total: {existingItem._quantity}");
        }
        else
        {
            InventoryItem newItem = new InventoryItem(data, amount, flatIndex, choppable, grillable);
            _inventoryItems.Add(newItem);
            Debug.Log($"Added new item: {data.itemName} (Qty: {amount})");
        }
    }
}