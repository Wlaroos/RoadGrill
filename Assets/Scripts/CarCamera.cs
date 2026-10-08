using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public enum CameraMode { FirstPerson, ThirdPerson }

    [Header("Mode & Keybinds")]
    [SerializeField] private CameraMode currentMode = CameraMode.ThirdPerson;
    [SerializeField] private KeyCode toggleKey = KeyCode.V;
    [SerializeField] private float transitionSpeed = 10f; // Fast lerp for smooth mode switching

    [Header("Target & Tracking")]
    [SerializeField] private Transform carTransform;
    [SerializeField] private Transform driverEyePoint; // Empty GameObject at driver's head
    [SerializeField] private Rigidbody carRigidbody;

    [Header("1st Person Settings")]
    [SerializeField] private float fpFov = 75f;
    [SerializeField] private float fpMouseSensitivity = 2f;
    [SerializeField] private float fpYawRange = 80f;
    [SerializeField] private float fpPitchMin = -30f;
    [SerializeField] private float fpPitchMax = 50f;

    [Header("3rd Person Settings")]
    [SerializeField] private float tpFov = 60f;
    [SerializeField] private float tpDistance = 6f;
    [SerializeField] private float tpHeight = 2.2f;
    [SerializeField] private float tpLookAtHeight = 1.2f;
    [SerializeField] private LayerMask obstacleLayers; // Layers to check for camera collision clipping

    // Camera component reference
    private Camera cam;

    // First person head rotation state
    private float fpHeadYaw = 0f;
    private float fpHeadPitch = 0f;

    // Smooth transition tracking
    private float currentFov;

    private void Start()
    {
        cam = GetComponent<Camera>();
        currentFov = (currentMode == CameraMode.FirstPerson) ? fpFov : tpFov;
        cam.fieldOfView = currentFov;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Toggle mode on keypress
        if (Input.GetKeyDown(toggleKey))
        {
            currentMode = (currentMode == CameraMode.ThirdPerson) ? CameraMode.FirstPerson : CameraMode.ThirdPerson;
            
            // Reset head look when switching back to 1st person
            if (currentMode == CameraMode.FirstPerson)
            {
                fpHeadYaw = 0f;
                fpHeadPitch = 0f;
            }
        }

        // Mouse look inputs (primarily used in 1st person mode)
        if (currentMode == CameraMode.FirstPerson)
        {
            float mouseX = Input.GetAxis("Mouse X") * fpMouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * fpMouseSensitivity;

            fpHeadYaw = Mathf.Clamp(fpHeadYaw + mouseX, -fpYawRange, fpYawRange);
            fpHeadPitch = Mathf.Clamp(fpHeadPitch - mouseY, fpPitchMin, fpPitchMax);
        }
    }

    private void LateUpdate()
    {
        if (carTransform == null || driverEyePoint == null) return;

        // Calculate target transform and FOV based on mode
        Vector3 targetPosition;
        Quaternion targetRotation;
        float targetFov;

        if (currentMode == CameraMode.FirstPerson)
        {
            GetFirstPersonTransform(out targetPosition, out targetRotation);
            targetFov = fpFov;

            // Instantly snap during 1st person driving so there is zero camera lag/jitter
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
        else
        {
            GetThirdPersonTransform(out targetPosition, out targetRotation);
            targetFov = tpFov;

            // Lock position directly to target (or tight lerp if transitioning modes)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
        }

        currentFov = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * transitionSpeed);
        cam.fieldOfView = currentFov;
    }

    private void GetFirstPersonTransform(out Vector3 position, out Quaternion rotation)
    {
        position = driverEyePoint.position;
        
        // Combine vehicle orientation with driver look offset
        Quaternion carRotation = driverEyePoint.rotation;
        Quaternion headLook = Quaternion.Euler(fpHeadPitch, fpHeadYaw, 0f);
        rotation = carRotation * headLook;
    }

    private void GetThirdPersonTransform(out Vector3 position, out Quaternion rotation)
    {
        // Position camera directly behind and above relative to the car's orientation
        Vector3 idealPosition = carTransform.position 
            - (carTransform.forward * tpDistance) 
            + (carTransform.up * tpHeight);

        // Obstacle Collision Detection (Prevents clipping through terrain/walls)
        Vector3 rayOrigin = carTransform.position + carTransform.up * tpLookAtHeight;
        Vector3 rayDirection = idealPosition - rayOrigin;
        float rayDistance = rayDirection.magnitude;

        if (Physics.Raycast(rayOrigin, rayDirection.normalized, out RaycastHit hit, rayDistance, obstacleLayers))
        {
            position = hit.point + hit.normal * 0.2f; // Push slightly off hit surface
        }
        else
        {
            position = idealPosition;
        }

        // Aim camera at car direction
        Vector3 lookAtTarget = carTransform.position + (carTransform.up * tpLookAtHeight);
        rotation = Quaternion.LookRotation(lookAtTarget - position, carTransform.up);
    }
}