using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class AnimalInteractable : Interactable
{
    [Header("Inventory Integration")]
    [SerializeField] private AnimalItemData _animalItemData;
    public AnimalItemData AnimalItemData => _animalItemData;
    
    [Header("Animal Stats")]
    [SerializeField] private float _velocityDamageMultiplier = 5f;
    [SerializeField] private float _hitCooldown = 0.75f; // Debounce to prevent multiple damage/flatten triggers in a single pass

    [Header("Wandering Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _changeDirectionInterval = 3f;
    [SerializeField] private float _idleChance = 0.3f; // Chance to stand still instead of moving

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

    // Wandering state variables
    private Vector3 _moveDirection;
    private float _wanderTimer;
    private bool _isIdle;

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

        ChooseNewDirection();
    }

    private void Update()
    {
        if (_isDead) return;

        // Handle wandering timer
        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            ChooseNewDirection();
        }
    }

    private void FixedUpdate()
    {
        if (_isDead) return;

        if (!_isIdle && _rb != null)
        {
            // Make sure the movement is on the XZ plane
            _moveDirection.y = 0f;
            _moveDirection.Normalize();

            Vector3 targetVelocity = _moveDirection * _moveSpeed;
            targetVelocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = targetVelocity;

            // Rotate only around the Y-axis
            if (_moveDirection != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
            }
        }
        else if (_isIdle && _rb != null)
        {
            // Halt horizontal movement while idling
            Vector3 vel = _rb.linearVelocity;
            vel.x = 0f;
            vel.z = 0f;
            _rb.linearVelocity = vel;
        }
    }

    private void ChooseNewDirection()
    {
        _wanderTimer = Random.Range(2f, _changeDirectionInterval + 2f);
        _isIdle = Random.value < _idleChance;

        if (!_isIdle)
        {
            float randomAngle = Random.Range(0f, 360f);
            // Keep Y at 0
            _moveDirection = new Vector3(Mathf.Sin(randomAngle * Mathf.Deg2Rad), 0f, Mathf.Cos(randomAngle * Mathf.Deg2Rad)).normalized;
        }
        else
        {
            _moveDirection = Vector3.zero;
        }
    }

    // Returns true if the hit was processed, false if it was blocked by cooldown
    public bool HandleCarHit(float impactVelocity)
    {
        // Check cooldown first
        if (Time.time - _lastHitTime < _hitCooldown) return false;

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

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // Stop wandering movement immediately
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.isKinematic = false;
            _rb.mass *= 5f; // Increase mass to make dead body harder to push around
        }
    }

    private void IncrementFlatten()
    {
        _flatIndex = Mathf.Clamp(_flatIndex + 1, 0, _maxFlatIndex);

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

    // Override Interact from Interactable base class
    public override void Interact()
    {
        base.Interact();
        if (_isDead)
        {
            if (PlayerInventory != null && _animalItemData != null)
            {
                PlayerInventory.AddItem(_animalItemData, 1, _flatIndex, _animalItemData.isChoppable, _animalItemData.isGrillable);
                Destroy(gameObject);
            }
        }
    }
}