using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarInteractable : Interactable
{
    [SerializeField] private PlayerVehicleManager _vehicleManager;

    public override void Interact()
    {
        base.Interact();

        if (_vehicleManager != null)
        {
            _vehicleManager.InteractWithCar();
        }
    }
}