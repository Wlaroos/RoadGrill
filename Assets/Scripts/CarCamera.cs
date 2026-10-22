using UnityEngine;
using UnityEngine.UI;

public class CarCamera : MonoBehaviour
{
    public enum CameraMode { FirstPerson, ThirdPerson }
    public enum TargetMode { Car, Player }

    [Header("Mode & Keybinds")]
    [SerializeField] private CameraMode currentMode = CameraMode.ThirdPerson;
    [SerializeField] private float transitionSpeed = 10f;

    [Header("Target & Tracking (Car)")]
    [SerializeField] private Transform carTransform;
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Transform driverEyePoint;
    [SerializeField] private Transform driverLeanPoint;

    [Header("Lean Settings")]
    [SerializeField] private bool holdToLean = true; 
    [SerializeField] private float leanTransitionSpeed = 15f;

    [Header("Target & Tracking (Player)")]
    [SerializeField] private Transform playerEyePoint; // Fallback / Inspector assigned reference

    [Header("1st Person Settings")]
    [SerializeField] private float fpFov = 75f;
    [SerializeField] private float fpYawRange = 80f;
    [SerializeField] private float fpPitchMin = -30f;
    [SerializeField] private float fpPitchMax = 50f;

    [Header("3rd Person Settings")]
    [SerializeField] private float tpFov = 60f;
    [SerializeField] private float tpDistance = 6f;
    [SerializeField] private float tpHeight = 2.2f;
    [SerializeField] private float tpLookAtHeight = 1.2f;
    [SerializeField] private LayerMask obstacleLayers; 

    private Camera cam;
    private float fpHeadYaw = 0f;
    private float fpHeadPitch = 0f;
    private float currentFov;
    private TargetMode currentTargetMode = TargetMode.Car;

    // Runtime tracking for player override if passed dynamically
    private Transform dynamicPlayerEyePoint;
    private bool isLeaning = false;
    private float leanProgress = 0f;

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
            dynamicPlayerEyePoint = newPlayerEye;
        }
        
        fpHeadYaw = 0f;
        fpHeadPitch = 0f;
        isLeaning = false;
        leanProgress = 0f;
    }

    private void OnEnable()
    {
        PlayerInputs.OnToggleCameraPressed += ToggleCamera;
        PlayerInputs.OnLeanPressed += HandleLeanPressed;
    }

    private void OnDisable()
    {
        PlayerInputs.OnToggleCameraPressed -= ToggleCamera;
        PlayerInputs.OnLeanPressed -= HandleLeanPressed;
    }

    private void HandleLeanPressed()
    {
        if (!holdToLean && currentTargetMode == TargetMode.Car)
        {
            isLeaning = !isLeaning;
        }
    }

    private void Update()
    {
        if (currentTargetMode == TargetMode.Car)
        {
            if (holdToLean)
            {
                isLeaning = PlayerInputs.LeanHeld;
            }

            if (currentMode == CameraMode.FirstPerson)
            {
                Vector2 lookVal = PlayerInputs.LookValue;
                fpHeadYaw = Mathf.Clamp(fpHeadYaw + lookVal.x, -fpYawRange, fpYawRange);
                fpHeadPitch = Mathf.Clamp(fpHeadPitch - lookVal.y, fpPitchMin, fpPitchMax);
            }
        }
    }

    private void ToggleCamera()
    {
        if (currentTargetMode == TargetMode.Car)
        {
            currentMode = (currentMode == CameraMode.ThirdPerson) ? CameraMode.FirstPerson : CameraMode.ThirdPerson;
            if (currentMode == CameraMode.FirstPerson)
            {
                fpHeadYaw = 0f;
                fpHeadPitch = 0f;
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 targetPosition;
        Quaternion targetRotation;
        float targetFov;

        if (currentTargetMode == TargetMode.Player)
        {
            Transform activePlayerEye = dynamicPlayerEyePoint != null ? dynamicPlayerEyePoint : playerEyePoint;
            if (activePlayerEye == null) return;
            
            targetPosition = activePlayerEye.position;
            
            Vector3 flatForward = activePlayerEye.forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude > 0.001f)
            {
                flatForward.Normalize();
                Quaternion baseRotation = Quaternion.LookRotation(flatForward, Vector3.up);
                targetRotation = Quaternion.Euler(activePlayerEye.eulerAngles.x, baseRotation.eulerAngles.y, 0f);
            }
            else
            {
                targetRotation = Quaternion.Euler(activePlayerEye.eulerAngles.x, activePlayerEye.eulerAngles.y, 0f);
            }

            targetFov = fpFov;
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
        Quaternion baseRotation;

        if (driverLeanPoint != null)
        {
            leanProgress = Mathf.MoveTowards(leanProgress, isLeaning ? 1f : 0f, Time.deltaTime * leanTransitionSpeed);
            position = Vector3.Lerp(driverEyePoint.position, driverLeanPoint.position, leanProgress);
            baseRotation = Quaternion.Slerp(driverEyePoint.rotation, driverLeanPoint.rotation, leanProgress);
        }
        else
        {
            position = driverEyePoint.position;
            baseRotation = driverEyePoint.rotation;
        }

        Quaternion headLook = Quaternion.Euler(fpHeadPitch, fpHeadYaw, 0f);
        rotation = baseRotation * headLook;
    }

    private void GetThirdPersonTransform(out Vector3 position, out Quaternion rotation)
    {
        Vector3 idealPosition = carTransform.position 
            - (carTransform.forward * tpDistance) 
            + (carTransform.up * tpHeight);

        Vector3 rayOrigin = carTransform.position + carTransform.up * tpLookAtHeight;
        Vector3 rayDirection = idealPosition - rayOrigin;
        float rayDistance = rayDirection.magnitude;

        if (Physics.Raycast(rayOrigin, rayDirection.normalized, out RaycastHit hit, rayDistance, obstacleLayers))
        {
            position = hit.point + hit.normal * 0.2f;
        }
        else
        {
            position = idealPosition;
        }

        Vector3 lookAtTarget = carTransform.position + (carTransform.up * tpLookAtHeight);
        rotation = Quaternion.LookRotation(lookAtTarget - position, carTransform.up);
    }
}