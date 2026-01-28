using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Captures local player input and sends it to the server.
/// Only runs for the locally owned player.
/// Uses the new Unity Input System.
/// </summary>
public class PlayerInput : NetworkBehaviour
{
    [Header("Input Settings")]
    public float inputSendRate = 20f; // Hz

    private float nextInputSendTime;
    private PlayerMotor motor;

    private void Start()
    {
        motor = GetComponent<PlayerMotor>();
        if (motor == null)
        {
            Debug.LogError("PlayerInput: PlayerMotor component not found!");
        }
    }

    private void Update()
    {
        // Only process input for our own player
        if (!isLocalPlayer)
        {
            return;
        }

        // Check if keyboard and mouse are available
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        // Gather WASD input using new Input System
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontal += 1f;
        if (Keyboard.current.wKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed) vertical -= 1f;

        bool sprint = Keyboard.current.leftShiftKey.isPressed;

        Vector3 moveInput = new Vector3(horizontal, 0, vertical);

        // Gather mouse delta for camera
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x;
        float mouseY = mouseDelta.y;

        // Send input to server at fixed rate to reduce bandwidth
        if (Time.time >= nextInputSendTime)
        {
            CmdSendInput(moveInput, sprint, mouseX, mouseY);
            nextInputSendTime = Time.time + (1f / inputSendRate);
        }
    }

    /// <summary>
    /// Command: Client sends input to server for authoritative simulation.
    /// </summary>
    [Command]
    private void CmdSendInput(Vector3 moveInput, bool sprint, float mouseX, float mouseY)
    {
        // Forward input to motor on server
        if (motor != null)
        {
            motor.ProcessInput(moveInput, sprint, mouseX, mouseY);
        }
    }
}
