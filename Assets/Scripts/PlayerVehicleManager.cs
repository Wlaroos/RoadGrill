using UnityEngine;

public class PlayerVehicleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _car;
    [SerializeField] private CarController _carController;
    [SerializeField] private CarCamera _carCamera;
    [SerializeField] private GameObject _player; // Assign your existing player GameObject from the scene here
    [SerializeField] private Transform _exitSpawnPoint; // Empty GameObject outside the driver door
    [SerializeField] private Transform _playerCameraPoint; // Empty GameObject representing the player's camera eye point

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode _interactKey = KeyCode.E;
    [SerializeField] private float _maxEntryDistance = 5.0f;
    [SerializeField] private LayerMask _carLayer;

    private Camera mainCamera;
    private bool isDriving = true;

    private void Start()
    {
        mainCamera = Camera.main;

        if (_player == null)
        {
            Debug.LogWarning("Player GameObject is not assigned in PlayerVehicleManager!", this);
        }

        SetDrivingState(true);
    }

    private void Update()
    {
        if (isDriving)
        {
            if (Input.GetKeyDown(_interactKey))
            {
                ExitCar();
            }
        }
        else
        {
            if (Input.GetKeyDown(_interactKey))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, _maxEntryDistance, _carLayer))
                {
                    if (hit.collider.gameObject.transform.root == _car.transform.root)
                    {
                        EnterCar();
                    }
                }
            }
        }
    }

    private void ExitCar()
    {
        if (_player == null)
        {
            Debug.LogWarning("Cannot exit car: Player reference is missing!", this);
            return;
        }

        isDriving = false;

        // Disable car driving input (suspension physics remains active)
        _carController.SetDriverControl(false);

        // Determine spawn position and rotation outside the car
        Vector3 spawnPos = _exitSpawnPoint != null ? _exitSpawnPoint.position : _car.transform.position - _car.transform.right * 2f;
        Quaternion spawnRot = _car.transform.rotation;

        // Reposition and activate the player
        CharacterController cc = _player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        _player.transform.position = spawnPos;
        _player.transform.rotation = spawnRot;

        if (cc != null) cc.enabled = true;
        _player.SetActive(true);

        // Switch the single CarCamera to track the player's camera eye point
        _carCamera.SetTargetMode(CarCamera.TargetMode.Player, _playerCameraPoint);

        // Enable first person controller script on player
        var fpController = _player.GetComponent<FirstPersonController>();
        if (fpController != null) fpController.enabled = true;
    }

    private void EnterCar()
    {
        isDriving = true;

        // Disable player components/object
        if (_player != null)
        {
            var fpController = _player.GetComponent<FirstPersonController>();
            if (fpController != null) fpController.enabled = false;
            _player.SetActive(false);
        }

        // Switch the single CarCamera back to tracking the car
        _carCamera.SetTargetMode(CarCamera.TargetMode.Car);

        // Re-enable car driver controls
        _carController.SetDriverControl(true);
    }

    private void SetDrivingState(bool driving)
    {
        isDriving = driving;
        _carController.SetDriverControl(driving);
        _carCamera.SetTargetMode(driving ? CarCamera.TargetMode.Car : CarCamera.TargetMode.Player);
        
        if (_player != null) 
        {
            _player.SetActive(!driving);
            var fpController = _player.GetComponent<FirstPersonController>();
            if (fpController != null) fpController.enabled = !driving;
        }
    }
}