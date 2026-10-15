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
    [SerializeField] private Transform _playerCamera; // Assign your "PlayerEyePoint" transform here in the Inspector
    [SerializeField] private float _mouseSensitivity = 2.0f;
    [SerializeField] private float _maxPitch = 80f;
    [SerializeField] private float _minPitch = -80f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Fallback if not assigned in Inspector: looks for child named "PlayerEyePoint"
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

        // Lock cursor to center of screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        if (_playerCamera != null)
        {
            cameraPitch = Mathf.Clamp(cameraPitch - mouseY, _minPitch, _maxPitch);
            _playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        // Movement Input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Sprinting
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? _moveSpeed * _sprintMultiplier : _moveSpeed;

        Vector3 moveDir = transform.right * moveX + transform.forward * moveZ;
        
        // Ground check & Gravity
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Small downward force to keep grounded state consistent
        }
        else
        {
            verticalVelocity += _gravity * Time.deltaTime;
        }

        // Jumping (Only jump if grounded)
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            // Physics formula: v = sqrt(h * -2 * g)
            verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        // Combine movement and apply via CharacterController
        Vector3 velocity = moveDir * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}