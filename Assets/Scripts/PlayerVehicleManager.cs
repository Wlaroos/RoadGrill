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

    private bool isDriving = true;
    public bool IsDriving => isDriving;

    public static PlayerVehicleManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }
    }

    private void Start()
    {
        SetDrivingState(true);
    }

    public void ExitCar()
    {
        isDriving = false;

        // Disable car driving input (suspension physics remains active)
        _carController.SetDriverControl(false);

        // Determine spawn position and rotation outside the car
        Vector3 spawnPos = _exitSpawnPoint != null ? _exitSpawnPoint.position : _car.transform.position - _car.transform.right * 2f;
        Quaternion spawnRot = _car.transform.rotation;

        // Temporarily disable CharacterController to avoid position glitches when teleporting
        CharacterController cc = _player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        _player.transform.position = spawnPos;
        Vector3 playerEuler = _player.transform.eulerAngles;
        _player.transform.rotation = Quaternion.Euler(playerEuler.x, spawnRot.eulerAngles.y, playerEuler.z);

        if (cc != null) cc.enabled = true;

        // Re-enable player controller scripts and physics components
        SetPlayerComponentsActive(true);

        // Switch the single CarCamera to track the player's camera eye point
        _carCamera.SetTargetMode(CarCamera.TargetMode.Player, _playerCameraPoint);
    }

    private void EnterCar()
    {
        isDriving = true;

        // Disable player controller/physics components, keeping PlayerInteraction active
        SetPlayerComponentsActive(false);

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
            SetPlayerComponentsActive(!driving);
        }
    }

    private void SetPlayerComponentsActive(bool active)
    {
        if (_player == null) return;

        // Enable or disable all MonoBehaviour scripts on the player except PlayerInteraction
        MonoBehaviour[] scripts = _player.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != null && script is not PlayerInteraction && script != this)
            {
                script.enabled = active;
            }
        }

        // Also handle physics components so the player doesn't fall or collide while inside the car
        CharacterController cc = _player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = active;

        Rigidbody rb = _player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = !active;
    }

    public void InteractWithCar()
    {
        if (isDriving)
        {
            ExitCar();
        }
        else
        {
            EnterCar();
        }
    }
}