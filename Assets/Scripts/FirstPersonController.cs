using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _sprintMultiplier = 1.5f;
    [SerializeField] private float _jumpHeight = 1.5f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Look")]
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private float _maxPitch = 80f;
    [SerializeField] private float _minPitch = -80f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch = 0f;
    private bool jumpRequested = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        if (_playerCamera == null)
        {
            Transform eyePoint = transform.Find("PlayerEyePoint");
            if (eyePoint != null)
            {
                _playerCamera = eyePoint;
            }
            else
            {
                Debug.LogWarning("PlayerEyePoint transform not assigned or found on FirstPersonController!");
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        PlayerInputs.OnJumpPressed += HandleJumpPressed;
    }

    private void OnDisable()
    {
        PlayerInputs.OnJumpPressed -= HandleJumpPressed;
    }

    private void HandleJumpPressed()
    {
        jumpRequested = true;
    }

    private void Update()
    {
        // Mouse Look via PlayerInputs
        Vector2 lookVal = PlayerInputs.LookValue;
        float mouseX = lookVal.x;
        float mouseY = lookVal.y;

        transform.Rotate(Vector3.up * mouseX);

        if (_playerCamera != null)
        {
            cameraPitch = Mathf.Clamp(cameraPitch - mouseY, _minPitch, _maxPitch);
            _playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        // Movement Input via PlayerInputs
        Vector2 moveVal = PlayerInputs.MovementValue;
        float moveX = moveVal.x;
        float moveZ = moveVal.y;

        // Sprinting
        bool isSprinting = PlayerInputs.SprintHeld;
        float currentSpeed = isSprinting ? _moveSpeed * _sprintMultiplier : _moveSpeed;

        Vector3 moveDir = transform.right * moveX + transform.forward * moveZ;
        
        // Ground check & Gravity
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += _gravity * Time.deltaTime;
        }

        // Jumping
        if (jumpRequested && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }
        jumpRequested = false;

        Vector3 velocity = moveDir * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}