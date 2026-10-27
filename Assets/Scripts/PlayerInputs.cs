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
    public static bool LeanHeld { get; private set; }
    public static event Action OnLeanPressed;
    public static event Action OnInteractPressed;
    public static event Action OnRestartPressed;
    public static event Action OnEscapePressed;
    public static event Action OnCrouchPressed;
    public static event Action OnJumpPressed;
    public static event Action OnToggleCameraPressed;
    public static event Action OnInventoryPressed;
    private bool playerInputsDisabled;

    private void Awake()
    {
        InputActions = new InputSystem_Actions();
        InputActions.Player.SetCallbacks(this);
    }

    private void OnEnable()
    {
        EnablePlayerInputs();
    }

    private void OnDisable()
    {
        DisablePlayerInputs();
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
            if(playerInputsDisabled)
            {
                OnInventoryPressed?.Invoke();
            }
            else
            {
                OnEscapePressed?.Invoke();
            }
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

    public void OnLean(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnLeanPressed?.Invoke();
            LeanHeld = true;
        }
        else if (context.canceled)
        {
            LeanHeld = false;
        }
    }

    public void DisablePlayerInputs()
    {
        playerInputsDisabled = true;

        MovementValue = Vector2.zero;
        LookValue = Vector2.zero;
        SprintHeld = false;
        HandbrakeHeld = false;
        LeanHeld = false;
        
        // Selectively disable movement/gameplay inputs
        InputActions.Player.Move.Disable();
        InputActions.Player.Look.Disable();
        InputActions.Player.Crouch.Disable();
        InputActions.Player.Sprint.Disable();
        InputActions.Player.Interact.Disable();
        InputActions.Player.Jump.Disable();
        InputActions.Player.Lean.Disable();
        InputActions.Player.ToggleCamera.Disable();
        InputActions.Player.Handbrake.Disable();

        InputActions.Player.Inventory.Enable();
        InputActions.Player.Restart.Enable();
        InputActions.Player.Escape.Enable();
    }

    public void EnablePlayerInputs()
    {
        playerInputsDisabled = false;
        
        // Re-enable the inputs
        InputActions.Player.Move.Enable();
        InputActions.Player.Look.Enable();
        InputActions.Player.Jump.Enable();
        InputActions.Player.Crouch.Enable();
        InputActions.Player.Sprint.Enable();
        InputActions.Player.Lean.Enable();
        InputActions.Player.Interact.Enable();
        InputActions.Player.ToggleCamera.Enable();
        InputActions.Player.Handbrake.Enable();

        InputActions.Player.Inventory.Enable();
        InputActions.Player.Restart.Enable();
        InputActions.Player.Escape.Enable();
    }
}