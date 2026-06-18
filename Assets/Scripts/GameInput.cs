using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{

    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    public event EventHandler OnPlayerAttack;
    public event EventHandler OnPlayerDash;

    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        playerInputActions.Combat.Attack.started += PlayerAttack_started;
        playerInputActions.Player.Dash.performed += PlayerDash_performed;
    }

    private void PlayerDash_performed(InputAction.CallbackContext obj) {
        OnPlayerDash?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerAttack_started(InputAction.CallbackContext obj) {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovmentVector() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public Vector3 GetMousePosition() {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        return mousePos;
    }

    public void DisableMovement() {
        playerInputActions.Disable();
    }
}
