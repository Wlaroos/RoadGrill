using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Animal : Interactable
{
    [Header("Inventory Integration")]
    [SerializeField] private AnimalItemData _animalItemData;
    public AnimalItemData AnimalItemData => _animalItemData;
    
    [Header("Animal Stats")]
    [SerializeField] private float _velocityDamageMultiplier = 5f;
    [SerializeField] private float _hitCooldown = 0.75f; // Debounce to prevent multiple damage/flatten triggers in a single pass

    [Header("Flattening Settings (Dead Body)")]
    private int _flatIndex = 0;
    [SerializeField] private int _maxFlatIndex = 3;
    [SerializeField] private float _flattenAmountY = 0.15f; // Decrease Y scale per hit
    [SerializeField] private float _stretchAmountXZ = 0.1f; // Increase X/Z scale per hit

    [Header("Mesh Swapping (Optional Future Upgrade)")]
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Mesh[] _flatMeshes; // Array of meshes indexed by flattening stage

    public float LastHitTime => _lastHitTime;

    private float _currentHealth;
    private bool _isDead = false;
    private float _lastHitTime = -999f;
    private Rigidbody _rb;
    private Vector3 _originalScale;

    public override void Awake()
    {
        base.Awake();
        _currentHealth = _animalItemData.maxHealth;
        _rb = GetComponent<Rigidbody>();
        _originalScale = transform.localScale;

        if (_meshFilter == null)
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
        }
    }

    // Returns true if the hit was processed, false if it was blocked by cooldown
    public bool HandleCarHit(float impactVelocity)
    {
        // Check cooldown first
        if (Time.time - _lastHitTime < _hitCooldown) return false;

        Debug.Log($"{gameObject.name} was hit at {impactVelocity:F1} m/s.");

        _lastHitTime = Time.time;

        if (!_isDead)
        {
            float damage = impactVelocity * _velocityDamageMultiplier;
            TakeDamage(damage);
        }
        else
        {
            // Dead body flattening logic
            if (_flatIndex < _maxFlatIndex)
            {
                IncrementFlatten();
            }
        }

        return true;
    }

    private void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount:F1} damage. Health: {_currentHealth:F1} / {_animalItemData.maxHealth}");

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;

        gameObject.layer = LayerMask.NameToLayer("Interactable");

        Debug.Log($"{gameObject.name} has died!");

        // Disable AI / movement scripts here

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.mass *= 5f; // Increase mass to make it harder to move
        }
    }

    private void IncrementFlatten()
    {
        _flatIndex = Mathf.Clamp(_flatIndex + 1, 0, _maxFlatIndex);
        Debug.Log($"{gameObject.name} flattened {_flatIndex} / {_maxFlatIndex}");

        // Scale Modification (Flattened Y, stretched X/Z)
        float newY = Mathf.Max(0.05f, _originalScale.y - (_flatIndex * _flattenAmountY));
        float newXZ = _originalScale.x + (_flatIndex * _stretchAmountXZ);
        transform.localScale = new Vector3(newXZ, newY, newXZ);

        // Mesh Swapping (Swaps to custom meshes if assigned)
        if (_flatMeshes != null && _flatMeshes.Length > 0 && _meshFilter != null)
        {
            int meshIndex = Mathf.Clamp(_flatIndex - 1, 0, _flatMeshes.Length - 1);
            if (_flatMeshes[meshIndex] != null)
            {
                _meshFilter.sharedMesh = _flatMeshes[meshIndex];
            }
        }
    }

    // Override Interact from Interactable base class (integrates with PlayerInteraction)
    public override void Interact()
    {
        base.Interact();
        if (_isDead)
        {
            if (PlayerInventory != null && _animalItemData != null)
            {
                PlayerInventory.AddItem(_animalItemData, 1, _flatIndex);
                Destroy(gameObject);
            }
        }
    }
}