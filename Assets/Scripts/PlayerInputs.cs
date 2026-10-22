using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.VisualScripting;

public class PlayerInputs : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    [SerializeField] public float MouseSensitivity = 0.1f;
    public InputSystem_Actions InputActions { get; private set; }
    public static Vector2 MovementValue { get; private set; }
    public static Vector2 LookValue { get; private set; }
    public static bool SprintHeld { get; private set; }
    public static bool HandbrakeHeld { get; private set; }
    public static event Action OnInteractPressed;
    public static event Action OnRestartPressed;
    public static event Action OnEscapePressed;
    public static event Action OnCrouchPressed;
    public static event Action OnJumpPressed;
    public static event Action OnToggleCameraPressed;
    public static event Action OnInventoryPressed;

    private void Awake()
    {
        InputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        InputActions.Player.Enable();
        InputActions.Player.SetCallbacks(this);
    }

    private void OnDisable()
    {
        SprintHeld = false;
        HandbrakeHeld = false;
        InputActions.Player.Disable();
        InputActions.Player.RemoveCallbacks(this);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnInteractPressed?.Invoke();
        }
    }

    public void OnRestart(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnRestartPressed?.Invoke();
        }
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnEscapePressed?.Invoke();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookValue = context.ReadValue<Vector2>() * MouseSensitivity;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnCrouchPressed?.Invoke();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnJumpPressed?.Invoke();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SprintHeld = true;
        }
        else if (context.canceled)
        {
            SprintHeld = false;
        }
    }

    public void OnHandbrake(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            HandbrakeHeld = true;
        }
        else if (context.canceled)
        {
            HandbrakeHeld = false;
        }
    }

    public void OnToggleCamera(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnToggleCameraPressed?.Invoke();
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnInventoryPressed?.Invoke();
        }
    }
}