using UnityEngine;

public class GameInput : MonoBehaviour {
    // Singleton instance
    public static GameInput Instance { get; private set; }

    // Reference to the generated InputActions class
    private InputActions inputActions;

    // Initialize the singleton instance and enable input actions
    private void Awake() {
        Instance = this;
        inputActions = new InputActions();
        inputActions.Enable(); // Important to enable the input actions
    }

    // Disable input actions when the object is destroyed
    private void OnDestroy() {
        inputActions.Disable();
    }

    // Method to check if the right action is being pressed
    // use isPressed() to check if the action is currently being pressed
    public bool IsRightActionPressed() {
        return inputActions.Player.MoveRight.IsPressed();
    }

    // Method to check if the left action is being pressed
    public bool IsLeftActionPressed() {
        return inputActions.Player.MoveLeft.IsPressed();
    }

    // Method to check if the jump action was pressed
    // use WasPerformedThisFrame() to check if the action was just pressed
    public bool IsJumpActionPressed() {
        return inputActions.Player.Jump.WasPerformedThisFrame();
    }

    // Method to check if the jump action was released
    // use WasReleasedThisFrame() to check if the action was just released
    public bool IsJumpActionReleased() {
        return inputActions.Player.Jump.WasReleasedThisFrame();
    }
}