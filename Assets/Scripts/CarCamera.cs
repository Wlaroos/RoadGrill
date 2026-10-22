using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public enum CameraMode { FirstPerson, ThirdPerson }
    public enum TargetMode { Car, Player }

    [Header("Mode & Keybinds")]
    [SerializeField] private CameraMode currentMode = CameraMode.ThirdPerson;
    [SerializeField] private float transitionSpeed = 10f;

    [Header("Target & Tracking (Car)")]
    [SerializeField] private Transform carTransform;
    [SerializeField] private Transform driverEyePoint;
    [SerializeField] private Rigidbody carRigidbody;

    [Header("Target & Tracking (Player)")]
    [SerializeField] private Transform playerEyePoint;

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

    private TargetMode currentTargetMode = TargetMode.Car;

    private void Start()
    {
        cam = GetComponent<Camera>();
        currentFov = (currentMode == CameraMode.FirstPerson) ? fpFov : tpFov;
        cam.fieldOfView = currentFov;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetTargetMode(TargetMode mode, Transform newPlayerEye = null)
    {
        currentTargetMode = mode;
        if (mode == TargetMode.Player && newPlayerEye != null)
        {
            playerEyePoint = newPlayerEye;
        }
        
        // Reset look angles on switch
        fpHeadYaw = 0f;
        fpHeadPitch = 0f;
    }

    private void Update()
    {
        if (currentTargetMode == TargetMode.Car && PlayerInputs.ToggleCameraPressed)
        {
            currentMode = (currentMode == CameraMode.ThirdPerson) ? CameraMode.FirstPerson : CameraMode.ThirdPerson;
            
            if (currentMode == CameraMode.FirstPerson)
            {
                fpHeadYaw = 0f;
                fpHeadPitch = 0f;
            }
        }

        if (currentTargetMode == TargetMode.Car && currentMode == CameraMode.FirstPerson)
        {
            Vector2 lookVal = PlayerInputs.LookValue;
            float mouseX = lookVal.x * fpMouseSensitivity;
            float mouseY = lookVal.y * fpMouseSensitivity;

            fpHeadYaw = Mathf.Clamp(fpHeadYaw + mouseX, -fpYawRange, fpYawRange);
            fpHeadPitch = Mathf.Clamp(fpHeadPitch - mouseY, fpPitchMin, fpPitchMax);
        }
    }

    private void LateUpdate()
    {
        Vector3 targetPosition;
        Quaternion targetRotation;
        float targetFov;

        if (currentTargetMode == TargetMode.Player)
        {
            if (playerEyePoint == null) return;
            
            // Follow player's head point directly
            targetPosition = playerEyePoint.position;
            
            // Construct rotation using the player's forward vector projected onto the horizontal plane,
            // combined with world up vector. This prevents camera roll/tilt artifacts.
            Vector3 flatForward = playerEyePoint.forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude > 0.001f)
            {
                flatForward.Normalize();
                Quaternion baseRotation = Quaternion.LookRotation(flatForward, Vector3.up);
                // Retain the player's pitch from their actual rotation while stripping roll
                targetRotation = Quaternion.Euler(playerEyePoint.eulerAngles.x, baseRotation.eulerAngles.y, 0f);
            }
            else
            {
                targetRotation = Quaternion.Euler(playerEyePoint.eulerAngles.x, playerEyePoint.eulerAngles.y, 0f);
            }

            targetFov = fpFov;

            // Instantly snap during 1st person driving so there is zero camera lag/jitter
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
        else
        {
            if (carTransform == null || driverEyePoint == null) return;

            if (currentMode == CameraMode.FirstPerson)
            {
                GetFirstPersonTransform(out targetPosition, out targetRotation);
                targetFov = fpFov;

                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
            else
            {
                GetThirdPersonTransform(out targetPosition, out targetRotation);
                targetFov = tpFov;

                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
            }
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