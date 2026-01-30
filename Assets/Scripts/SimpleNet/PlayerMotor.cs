using Mirror;
using UnityEngine;

/// <summary>
/// Host-authoritative movement simulation.
/// Receives input from PlayerInput and simulates movement on the server only.
/// </summary>
public class PlayerMotor : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float sprintMultiplier = 1.5f;
    public float mouseSensitivity = 0.1f; // Tuned for new Input System mouse delta (raw pixels)

    [Header("Camera Settings")]
    public Transform cameraTransform; // Assign camera child in inspector

    private Vector3 currentMoveInput;
    private bool isSprinting;
    private float yaw;
    private float pitch;

    private void Start()
    {
        // Find camera if not assigned
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }
    }

    /// <summary>
    /// Called by PlayerInput (via Command) to update the current input state.
    /// </summary>
    public void ProcessInput(Vector3 moveInput, bool sprint, float mouseX, float mouseY)
    {
        currentMoveInput = moveInput;
        isSprinting = sprint;

        // Update rotation based on mouse input
        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }

    private void Update()
    {
        // Only simulate movement on server (authoritative)
        if (!isServer)
        {
            return;
        }

        // Apply movement
        if (currentMoveInput.sqrMagnitude > 0.01f)
        {
            // Calculate movement direction based on player's yaw
            Vector3 direction = Quaternion.Euler(0, yaw, 0) * currentMoveInput.normalized;

            float speed = moveSpeed;
            if (isSprinting)
            {
                speed *= sprintMultiplier;
            }

            transform.position += direction * speed * Time.deltaTime;
        }

        // Apply rotation - body horizontal, camera vertical
        transform.rotation = Quaternion.Euler(0, yaw, 0);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }
}
