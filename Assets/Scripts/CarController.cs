using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Raycast Suspension Settings")]
    [SerializeField] private Transform[] _wheelRaycastPoints;
    [SerializeField] private float _suspensionRestLength = 0.45f;
    [Tooltip("Radius of the wheels, used for suspension calculations.")]
    [SerializeField] private float _wheelRadius = 0.35f;
    [Tooltip("Strength of the suspension springs, how stiff the suspension is.")]
    [SerializeField] private float _springStrength = 10000f;
    [Tooltip("Damping of the suspension springs, how quickly they return to rest length.")]
    [SerializeField] private float _springDamping = 2500f;
    [Tooltip("Anti-roll force to prevent the car from flipping during sharp turns.")]
    [SerializeField] private float _antiRollForce = 5000f;
    [Tooltip("Layer mask defining what objects count as the ground for suspension and wheels.")]
    [SerializeField] private LayerMask _groundLayer = ~0;

    [Header("Engine & Handling Settings")]
    [Tooltip("Force applied to the car for acceleration.")]
    [SerializeField] private float _accelerationForce = 2500f;
    [Tooltip("Maximum speed the car can reach.")]
    [SerializeField] private float _maxSpeed = 500f;
    [Tooltip("Force applied to the car when braking.")]
    [SerializeField] private float _brakeForce = 4000f;
    [Tooltip("Force applied to the car when using the handbrake.")]
    [SerializeField] private float _handbrakeForce = 8000f;
    [Tooltip("Resistance force applied when no acceleration or braking input is given, causing the car to coast to a stop.")]
    [SerializeField] private float _coastingDragForce = 500f;
    [Tooltip("Maximum steering angle for the front wheels in degrees.")]
    [SerializeField] private float _maxSteerAngle = 30f;
    [Tooltip("How fast the steering wheel turns (emulates steering wheel lock time). Higher = faster.")]
    [SerializeField] private float _steeringSpeed = 5f;
    [Tooltip("Lower values result in more sliding.")]
    [SerializeField, Range(0f, 1f)] private float _tireGripFactor = 0.6f;
    [Tooltip("Lower values result in more sliding during handbrake.")]
    [SerializeField, Range(0f, 1f)] private float _handbrakeDriftFactor = 0.15f;
    [Tooltip("Pushes the car down at high speeds to keep it grounded.")]
    [SerializeField] private float _downforceCoefficient = 50f;

    [Header("Visual Wheel Settings")]
    [SerializeField] private Transform[] _wheelVisuals;
    [SerializeField, Range(0.1f, 1f)] private float _visualWheelSpinSpeed = 0.5f;

    private Rigidbody _rb;
    private float[] _wheelSpinAngles;

    // Inputs
    private float _rawMoveInput;
    private float _rawSteerInput;
    private float _smoothedSteerInput;
    private bool _isHandbraking;
    
    // Control state (Suspension and physics remain active even when player is out)
    private bool _isControlled = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb.mass < 100f) _rb.mass = 1200f;
        _rb.centerOfMass = new Vector3(0f, -0.4f, -0.7f);

        if (_wheelVisuals != null)
        {
            _wheelSpinAngles = new float[_wheelVisuals.Length];
        }
    }

    // Enables or disables driver input while keeping suspension and physics active.
    public void SetDriverControl(bool controlled)
    {
        _isControlled = controlled;
        if (!controlled)
        {
            _rawMoveInput = 0f;
            _rawSteerInput = 0f;
            _isHandbraking = true;
        }
    }

    private void Update()
    {
        if (_isControlled)
        {
            Vector2 moveVal = PlayerInputs.MovementValue;
            _rawMoveInput = moveVal.y;
            _rawSteerInput = moveVal.x;
            _isHandbraking = PlayerInputs.HandbrakeHeld;
        }
        else
        {
            _rawMoveInput = 0f;
            _rawSteerInput = 0f;
            _isHandbraking = true; // Keep handbrake engaged when player is out of the car
        }

        // Smooth steering input to emulate a physical steering wheel turning
        _smoothedSteerInput = Mathf.MoveTowards(_smoothedSteerInput, _rawSteerInput, _steeringSpeed * Time.deltaTime);

        UpdateWheelVisuals();
    }

    private void FixedUpdate()
    {
        if (_wheelRaycastPoints == null || _wheelRaycastPoints.Length == 0) return;

        float rayDistance = _suspensionRestLength + _wheelRadius;
        float wheelMass = _rb.mass / _wheelRaycastPoints.Length;

        // Apply downforce proportional to speed squared
        _rb.AddForce(-transform.up * (_downforceCoefficient * _rb.linearVelocity.sqrMagnitude));

        // Anti-roll bar calculation (Assumes 4 wheels: 0,1 = Front, 2,3 = Rear)
        if (_wheelRaycastPoints.Length >= 4)
        {
            ApplyAntiRoll(_wheelRaycastPoints[0], _wheelRaycastPoints[1], rayDistance); // Front Axle
            ApplyAntiRoll(_wheelRaycastPoints[2], _wheelRaycastPoints[3], rayDistance); // Rear Axle
        }

        for (int i = 0; i < _wheelRaycastPoints.Length; i++)
        {
            Transform rayPoint = _wheelRaycastPoints[i];
            if (rayPoint == null) continue;

            bool isFrontWheel = (i < 2);

            // Local steering frame using smoothed steer input
            Quaternion steerRotation = isFrontWheel ? Quaternion.Euler(0, _smoothedSteerInput * _maxSteerAngle, 0) : Quaternion.identity;

            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out RaycastHit hit, rayDistance, _groundLayer))
            {
                Vector3 wheelWorldVel = _rb.GetPointVelocity(rayPoint.position);

                // Keep wheel vectors flat along the ground plane
                Vector3 wheelForward = Vector3.ProjectOnPlane(rayPoint.rotation * steerRotation * Vector3.forward, hit.normal).normalized;
                Vector3 wheelRight = Vector3.ProjectOnPlane(rayPoint.rotation * steerRotation * Vector3.right, hit.normal).normalized;

                // Suspension force using ground normal to prevent sagging during body roll
                Vector3 rayDir = hit.normal;
                float currentLength = hit.distance - _wheelRadius;
                float offset = _suspensionRestLength - currentLength;

                float springVel = Vector3.Dot(rayDir, wheelWorldVel);
                float springForce = (offset * _springStrength) - (springVel * _springDamping);

                _rb.AddForceAtPosition(rayDir * Mathf.Max(0, springForce), rayPoint.position);

                // Lateral friction (grip)
                float steeringVel = Vector3.Dot(wheelRight, wheelWorldVel);

                // Rear wheels lose lateral grip during handbrake to allow drifting
                float effectiveGrip = (_isHandbraking && !isFrontWheel)
                    ? _tireGripFactor * _handbrakeDriftFactor
                    : _tireGripFactor;

                float desiredVelChange = -steeringVel * effectiveGrip;
                float accel = desiredVelChange / Time.fixedDeltaTime;
                _rb.AddForceAtPosition(wheelRight * (wheelMass * accel), rayPoint.position);

                // Acceleration, braking & handbrake
                float forwardVel = Vector3.Dot(wheelForward, wheelWorldVel);

                if (_isHandbraking && !isFrontWheel)
                {
                    float requiredBrakeAccel = -forwardVel / Time.fixedDeltaTime;
                    float maxBrakeAccel = _handbrakeForce / wheelMass;
                    float finalBrakeAccel = Mathf.Clamp(requiredBrakeAccel, -maxBrakeAccel, maxBrakeAccel);

                    _rb.AddForceAtPosition(wheelForward * (wheelMass * finalBrakeAccel), rayPoint.position);
                }
                else if (_rawMoveInput != 0)
                {
                    bool isBraking = (_rawMoveInput > 0f && forwardVel < -0.1f) || (_rawMoveInput < 0f && forwardVel > 0.1f);

                    if (isBraking)
                    {
                        float requiredBrakeAccel = -forwardVel / Time.fixedDeltaTime;
                        float maxBrakeAccel = _brakeForce / wheelMass;
                        float finalBrakeAccel = Mathf.Clamp(requiredBrakeAccel, -maxBrakeAccel, maxBrakeAccel);

                        _rb.AddForceAtPosition(wheelForward * (wheelMass * finalBrakeAccel), rayPoint.position);
                    }
                    else
                    {
                        // Only apply acceleration if we haven't reached max speed in the direction of input
                        bool canAccelerate = (_rawMoveInput > 0f && forwardVel < _maxSpeed) || (_rawMoveInput < 0f && forwardVel > -_maxSpeed);

                        if (canAccelerate)
                        {
                            _rb.AddForceAtPosition(wheelForward * (_rawMoveInput * _accelerationForce), rayPoint.position);
                        }
                    }
                }
                else
                {
                    // Apply coasting resistance when there are no inputs
                    if (Mathf.Abs(forwardVel) > 0.1f)
                    {
                        float requiredBrakeAccel = -forwardVel / Time.fixedDeltaTime;
                        float maxBrakeAccel = _coastingDragForce / wheelMass;
                        float finalBrakeAccel = Mathf.Clamp(requiredBrakeAccel, -maxBrakeAccel, maxBrakeAccel);

                        _rb.AddForceAtPosition(wheelForward * (wheelMass * finalBrakeAccel), rayPoint.position);
                    }
                    else
                    {
                        // When speed drops near zero, stop the car completely
                        Vector3 currentVel = _rb.linearVelocity;
                        float currentForwardSpeed = Vector3.Dot(currentVel, transform.forward);
                        if (Mathf.Abs(currentForwardSpeed) < 0.2f)
                        {
                            _rb.linearVelocity = Vector3.zero;
                        }
                    }
                }
            }
        }
    }

    private void ApplyAntiRoll(Transform wheelL, Transform wheelR, float rayDistance)
    {
        bool groundedL = Physics.Raycast(wheelL.position, -wheelL.up, out RaycastHit hitL, rayDistance, _groundLayer);
        bool groundedR = Physics.Raycast(wheelR.position, -wheelR.up, out RaycastHit hitR, rayDistance, _groundLayer);

        float travelL = 1.0f;
        float travelR = 1.0f;

        if (groundedL)
        {
            travelL = (-wheelL.InverseTransformPoint(hitL.point).y - _wheelRadius) / _suspensionRestLength;
        }

        if (groundedR)
        {
            travelR = (-wheelR.InverseTransformPoint(hitR.point).y - _wheelRadius) / _suspensionRestLength;
        }

        float antiRollForceFactor = (travelL - travelR) * _antiRollForce;

        if (groundedL)
        {
            _rb.AddForceAtPosition(hitL.normal * -antiRollForceFactor, wheelL.position);
        }
        if (groundedR)
        {
            _rb.AddForceAtPosition(hitR.normal * antiRollForceFactor, wheelR.position);
        }
    }

    private void UpdateWheelVisuals()
    {
        if (_wheelVisuals == null || _wheelRaycastPoints == null) return;

        float rayDistance = _suspensionRestLength + _wheelRadius;

        for (int i = 0; i < _wheelVisuals.Length; i++)
        {
            Transform rayPoint = _wheelRaycastPoints[i];
            Transform visual = _wheelVisuals[i];

            if (rayPoint == null || visual == null) continue;

            bool isFrontWheel = (i < 2);

            Vector3 targetWorldPos;
            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out RaycastHit hit, rayDistance, _groundLayer))
            {
                targetWorldPos = hit.point + (rayPoint.up * _wheelRadius);
            }
            else
            {
                targetWorldPos = rayPoint.position - (rayPoint.up * _suspensionRestLength);
            }

            visual.position = targetWorldPos;

            if (!(_isHandbraking && !isFrontWheel))
            {
                Vector3 wheelVel = _rb.GetPointVelocity(rayPoint.position);
                float forwardSpeed = Vector3.Dot(wheelVel, rayPoint.forward);
                float spinDelta = (forwardSpeed / _wheelRadius) * Mathf.Rad2Deg * _visualWheelSpinSpeed * Time.deltaTime;

                _wheelSpinAngles[i] = (_wheelSpinAngles[i] + spinDelta) % 360f;
            }

            float steerAngle = isFrontWheel ? (_smoothedSteerInput * _maxSteerAngle) : 0f;

            Quaternion steeringRot = Quaternion.Euler(0f, steerAngle, 0f);
            Quaternion spinRot = Quaternion.Euler(_wheelSpinAngles[i], 0f, 0f);

            visual.rotation = rayPoint.rotation * steeringRot * spinRot;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Animal animal = collision.gameObject.GetComponent<Animal>();
        if (animal != null)
        {
            float impactVelocity = collision.relativeVelocity.magnitude;
            animal.HandleCarHit(impactVelocity);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Animal animal = other.GetComponent<Animal>() ?? other.GetComponentInParent<Animal>();
        if (animal != null)
        {
            float impactVelocity = _rb.linearVelocity.magnitude;
            animal.HandleCarHit(impactVelocity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_wheelRaycastPoints == null) return;

        Gizmos.color = Color.green;
        float rayDistance = _suspensionRestLength + _wheelRadius;
        foreach (Transform rayPoint in _wheelRaycastPoints)
        {
            if (rayPoint != null)
            {
                Gizmos.DrawRay(rayPoint.position, -rayPoint.up * rayDistance);
            }
        }
    }
}