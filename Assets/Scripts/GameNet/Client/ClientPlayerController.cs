using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

namespace GameNet.Client
{
    /// <summary>
    /// Simple WASD movement controller for the local player.
    /// Uses Unity's new Input System with Input Actions.
    /// Client-authoritative: movement happens locally and syncs via ClientAuthorityTransformSync.
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class ClientPlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        private float moveSpeed = 5f;

        [Header("Mouse Look Settings")]
        [SerializeField]
        private Transform cameraTransform;

        [SerializeField]
        private float mouseSensitivity = 0.04f;

        [SerializeField]
        private float verticalLookClamp = 80f;

        [SerializeField]
        private bool lockCursor = true;

        private Vector2 moveInput;
        private Vector2 lookInput;
        private float cameraPitch = 0f;

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!isOwned)
            {
                return;
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
                    Debug.LogWarning("[ClientPlayerController] No camera assigned or found in children. Mouse look will not work.", this);
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
            // Only accept input from the owner
            if (!isOwned)
            {
                return;
            }

            HandleMovement();
            HandleMouseLook();
        }

        // Called by Unity Input System (Player Input component or manually)
        public void OnMove(InputValue value)
        {
            if (isOwned)
            {
                moveInput = value.Get<Vector2>();
            }
        }

        // Called by Unity Input System (Player Input component or manually)
        public void OnLook(InputValue value)
        {
            if (isOwned)
            {
                lookInput = value.Get<Vector2>();
            }
        }

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
                transform.position += moveDirection;
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

                // Rotate player body left/right (Y axis)
                transform.Rotate(0, mouseX, 0);

                // Rotate camera up/down (X axis) with clamping
                cameraPitch -= mouseY;
                cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookClamp, verticalLookClamp);
                cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
            }

            // Reset look input for next frame (if using mouse delta polling)
            if (Mouse.current != null)
            {
                lookInput = Vector2.zero;
            }
        }
    }
}
