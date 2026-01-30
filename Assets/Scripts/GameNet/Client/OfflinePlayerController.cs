using UnityEngine;
using UnityEngine.InputSystem;

namespace GameNet.Client
{
    /// <summary>
    /// Simple WASD movement controller for offline single-player mode.
    /// Uses Unity's new Input System with Input Actions.
    /// Does NOT require NetworkIdentity or any Mirror components.
    /// For networked player movement, use ClientPlayerController instead.
    /// </summary>
    public class OfflinePlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        private float moveSpeed = 5f;

        [SerializeField]
        private bool useCharacterController = false;

        [Header("Mouse Look Settings")]
        [SerializeField]
        private Transform cameraTransform;

        [SerializeField]
        private float mouseSensitivity = 0.04f;

        [SerializeField]
        private float verticalLookClamp = 80f;

        [SerializeField]
        private bool lockCursor = true;

        private CharacterController characterController;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float cameraPitch = 0f;
        private float playerYaw = 0f;

        private void Start()
        {
            if (useCharacterController)
            {
                characterController = GetComponent<CharacterController>();
                if (characterController == null)
                {
                    Debug.LogWarning("[OfflinePlayerController] Use Character Controller enabled but no CharacterController component found. Add CharacterController or disable this option.", this);
                }
            }

            // Find camera if not assigned
            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    cameraTransform = cam.transform;
                }
                else
                {
                    Debug.LogWarning("[OfflinePlayerController] No camera assigned or found in children. Mouse look will not work.", this);
                }
            }

            // Lock cursor if enabled
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            HandleMovement();
            HandleMouseLook();
        }

        // Called by Unity Input System (Player Input component or manually)
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        // Called by Unity Input System (Player Input component or manually)
        public void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

        // Fallback for direct keyboard polling if no Input Actions setup
        private void HandleMovement()
        {
            // If no input from Input Actions, try direct keyboard polling
            if (moveInput == Vector2.zero && Keyboard.current != null)
            {
                float h = 0f;
                float v = 0f;

                if (Keyboard.current.wKey.isPressed) { v += 1f; }
                if (Keyboard.current.sKey.isPressed) { v -= 1f; }
                if (Keyboard.current.aKey.isPressed) { h -= 1f; }
                if (Keyboard.current.dKey.isPressed) { h += 1f; }

                moveInput = new Vector2(h, v);
            }

            float horizontal = moveInput.x;
            float vertical = moveInput.y;

            // Calculate movement direction (forward/backward and strafe left/right)
            if (Mathf.Abs(vertical) > 0.01f || Mathf.Abs(horizontal) > 0.01f)
            {
                Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal) * moveSpeed * Time.deltaTime;

                if (useCharacterController && characterController != null)
                {
                    // Apply gravity if using CharacterController
                    moveDirection.y = -9.81f * Time.deltaTime;
                    characterController.Move(moveDirection);
                }
                else
                {
                    // Simple transform movement
                    transform.position += moveDirection;
                }
            }

            // Reset input for next frame (if using polling fallback)
            if (Keyboard.current != null && !Keyboard.current.anyKey.isPressed)
            {
                moveInput = Vector2.zero;
            }
        }

        private void HandleMouseLook()
        {
            if (cameraTransform == null)
            {
                return;
            }

            // Fallback to direct mouse polling if no Input Actions
            if (lookInput == Vector2.zero && Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                lookInput = mouseDelta;
            }

            // Apply mouse look
            if (lookInput.sqrMagnitude > 0.01f)
            {
                float mouseX = lookInput.x * mouseSensitivity;
                float mouseY = lookInput.y * mouseSensitivity;

                // Accumulate horizontal rotation
                playerYaw += mouseX;

                // Accumulate vertical rotation with clamping
                cameraPitch -= mouseY;
                cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookClamp, verticalLookClamp);

                // Check if camera is on a child object or the same object
                if (cameraTransform == transform)
                {
                    // Camera is on the same GameObject - apply combined rotation
                    transform.rotation = Quaternion.Euler(cameraPitch, playerYaw, 0);
                }
                else
                {
                    // Camera is on a child - rotate body and camera separately
                    transform.rotation = Quaternion.Euler(0, playerYaw, 0);
                    cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
                }
            }

            // Reset look input for next frame (if using mouse delta polling)
            if (Mouse.current != null)
            {
                lookInput = Vector2.zero;
            }
        }
    }
}
