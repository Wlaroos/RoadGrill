using UnityEngine;

public class PlayerVehicleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _car;
    [SerializeField] private CarController _carController;
    [SerializeField] private CarCamera _carCamera;
    [SerializeField] private GameObject _player;
    [SerializeField] private Transform _exitSpawnPoint;
    [SerializeField] private Transform _playerCameraPoint; 

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

        _carController.SetDriverControl(false);

        Vector3 spawnPos = _exitSpawnPoint != null ? _exitSpawnPoint.position : _car.transform.position - _car.transform.right * 2f;
        Quaternion spawnRot = _car.transform.rotation;

        CharacterController cc = _player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        _player.transform.position = spawnPos;
        Vector3 playerEuler = _player.transform.eulerAngles;
        _player.transform.rotation = Quaternion.Euler(playerEuler.x, spawnRot.eulerAngles.y, playerEuler.z);

        if (cc != null) cc.enabled = true;

        SetPlayerComponentsActive(true);

        // Switch to player mode and pass the player's camera point
        _carCamera.SetTargetMode(CarCamera.TargetMode.Player, _playerCameraPoint);
    }

    private void EnterCar()
    {
        isDriving = true;

        SetPlayerComponentsActive(false);

        // Switch back to car mode (CarCamera manages its own driver/lean points)
        _carCamera.SetTargetMode(CarCamera.TargetMode.Car);

        _carController.SetDriverControl(true);
    }

    private void SetDrivingState(bool driving)
    {
        isDriving = driving;
        _carController.SetDriverControl(driving);
        
        if (driving)
        {
            _carCamera.SetTargetMode(CarCamera.TargetMode.Car);
        }
        else
        {
            _carCamera.SetTargetMode(CarCamera.TargetMode.Player, _playerCameraPoint);
        }
        
        if (_player != null) 
        {
            SetPlayerComponentsActive(!driving);
        }
    }

    private void SetPlayerComponentsActive(bool active)
    {
        if (_player == null) return;

        MonoBehaviour[] scripts = _player.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != null && script is not PlayerInteraction && script != this)
            {
                script.enabled = active;
            }
        }

        CharacterController cc = _player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = active;

        Rigidbody rb = _player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = !active;
    }

    public void InteractWithCar()
    {
        if (isDriving) ExitCar();
        else EnterCar();
    }
}