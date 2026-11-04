using UnityEngine;
using System.Collections;

public class GrillInteractable : Interactable
{
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private Transform _spawnPoint;
    
    private Renderer _r;
    private InventoryItem _storedInventoryItem;
    private GameObject _spawnedObject;
    
    private float _cookTimer = 0f;
    private float _targetCookTime = 0f;
    private bool _isCooking = false;
    private bool _isCooked = false;

    public override void Awake()
    {
        base.Awake();
        _r = GetComponent<Renderer>();
    }

    public override void Interact()
    {
        base.Interact();

        if (!_isCooking && !_isCooked)
        {
            PlayerInventory.Instance?.OpenInventory(InvType.Grill, this);
        }
        else if (_isCooked)
        {
            if (_storedInventoryItem != null && _storedInventoryItem._itemData.grilledItem != null)
            {
                PlayerInventory.Instance?.AddItem(_storedInventoryItem._itemData.grilledItem, 1, _storedInventoryItem._flatIndex, _storedInventoryItem._itemData.grilledItem.isChoppable, _storedInventoryItem._itemData.grilledItem.isGrillable);
                Debug.Log($"Collected cooked item: {_storedInventoryItem._itemData.grilledItem.itemName}");
            }
            ResetGrill();
        }
        else if (_isCooking)
        {
            Debug.Log($"Grill is currently cooking {_storedInventoryItem._itemData.itemName}... ({_cookTimer:F1}/{_targetCookTime}s)");
        }
    }

    public void PlaceItemOnGrill(InventoryItem inventoryItem)
    {
        _storedInventoryItem = inventoryItem;
        _targetCookTime = inventoryItem._itemData.cookTime;
        _cookTimer = 0f;
        _isCooking = true;
        _isCooked = false;

        if (_spawnPoint == null) _spawnPoint = transform;

        if (inventoryItem._itemData.worldPrefab != null)
        {
            _spawnedObject = Instantiate(inventoryItem._itemData.worldPrefab, _spawnPoint.position, _spawnPoint.rotation, _spawnPoint);
        }
        else
        {
            _spawnedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _spawnedObject.transform.position = _spawnPoint.position;
            _spawnedObject.transform.localScale = Vector3.one * 0.1f;
            _spawnedObject.transform.SetParent(_spawnPoint);
        }

        StartCoroutine(CookRoutine(_spawnedObject));
    }

    private IEnumerator CookRoutine(GameObject cookedObject)
    {
        Material objectMaterial = cookedObject.GetComponent<Renderer>().material;
        Color defaultColor = objectMaterial.color;
        Color endColor = Color.black;

        while (_cookTimer < _targetCookTime)
        {
            float t = _cookTimer / _targetCookTime;
            objectMaterial.color = Color.Lerp(defaultColor, endColor, t);

            _cookTimer += Time.deltaTime;

            yield return null;
        }

        _isCooking = false;
        _isCooked = true;
        Debug.Log($"Item {_storedInventoryItem._itemData.itemName} is fully cooked on the grill! Interact to collect.");
    }

    private void ResetGrill()
    {
        _isCooking = false;
        _isCooked = false;
        _storedInventoryItem = null;
        if (_spawnedObject != null)
        {
            Destroy(_spawnedObject);
            _spawnedObject = null;
        }
        _cookTimer = 0f;
        _targetCookTime = 0f;
    }

    public override void RaycastHover()
    {
        base.RaycastHover();
        if (_r != null) _r.material = _hoverMat;
    }

    public override void RaycastUnhover()
    {
        base.RaycastUnhover();
        if (_r != null) _r.material = _defaultMat;
    }
}