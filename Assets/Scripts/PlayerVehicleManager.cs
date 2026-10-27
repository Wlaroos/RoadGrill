using UnityEngine;
using System;

public class PlayerVehicleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _car;
    [SerializeField] private CarController _carController;
    [SerializeField] private CarCamera _carCamera;
    [SerializeField] private GameObject _player;
    [SerializeField] private FirstPersonController _firstPersonController; // Added direct reference
    [SerializeField] private Transform _exitSpawnPoint;
    [SerializeField] private Transform _playerCameraPoint; 

    private bool isDriving = true;
    public bool IsDriving => isDriving;
    public static event Action<bool> OnDrivingStateChanged;

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

        // Auto-find FirstPersonController if left unassigned
        if (_firstPersonController == null && _player != null)
        {
            _firstPersonController = _player.GetComponent<FirstPersonController>();
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

        SetDrivingState(false);

        _carCamera.SetTargetMode(CarCamera.TargetMode.Player, _playerCameraPoint);

        // Explicitly enable player inputs and movement controller
        PlayerInputs playerInputs = _player.GetComponent<PlayerInputs>();
        playerInputs?.EnablePlayerInputs();

        if (_firstPersonController != null)
        {
            _firstPersonController.enabled = true;
        }
    }

    private void EnterCar()
    {
        isDriving = true;
        SetDrivingState(true);

        _carCamera.SetTargetMode(CarCamera.TargetMode.Car);
        _carController.SetDriverControl(true);

        if (_firstPersonController != null)
        {
            _firstPersonController.enabled = false;
        }
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
        
        // Broadcast event so scripts turn themselves off/on
        OnDrivingStateChanged?.Invoke(driving);

        if (_player != null) 
        {
            // Toggle physical player body components
            CharacterController cc = _player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = !driving;

            Rigidbody rb = _player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true; // MUST remain kinematic so it doesn't conflict with CharacterController

            // Can hide/show the player model if they get inside the car
            // .SetActive(!driving); 
        }

        if (_firstPersonController != null)
        {
            _firstPersonController.enabled = !driving;
        }
    }

    public void InteractWithCar()
    {
        if (isDriving) ExitCar();
        else EnterCar();
    }
}