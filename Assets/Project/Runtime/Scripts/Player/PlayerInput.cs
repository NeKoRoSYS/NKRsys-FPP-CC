using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public VariableJoystick joystick;
    public static PlayerInput Instance;
    PlayerInputActions playerInputActions;
    public InputAction MoveAction { get; set; }
    public InputAction LookAction { get; set; }
    public InputAction JumpAction { get; set; }
    public InputAction CrouchAction { get; set; }
    public InputAction SprintAction { get; set; }
    
    private void Awake() {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        MoveAction = playerInputActions.Default.Move;
        LookAction = playerInputActions.Default.Look;
        JumpAction = playerInputActions.Default.Jump;
        CrouchAction = playerInputActions.Default.Crouch;
        SprintAction = playerInputActions.Default.Sprint;
    }

    private void OnEnable() => playerInputActions.Default.Enable();

    private void OnDisable() => playerInputActions.Default.Disable();
}