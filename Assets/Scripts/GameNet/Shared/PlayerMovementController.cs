using UnityEngine;
using UnityEngine.InputSystem;

namespace GameNet.Shared
{
    /// <summary>
    /// Core player movement and mouse look logic.
    /// Plain C# class that can be composed into any controller.
    /// Follows Constitution Principle X: Unity is the Host, Not the Architecture.
    /// </summary>
    public class PlayerMovementController
    {
        private readonly Transform bodyTransform;
        private readonly Transform cameraTransform;
        private readonly CharacterController characterController;

        private Vector2 moveInput;
        private Vector2 lookInput;
        private float cameraPitch;
        private float playerYaw;

        // Settings
        public float MoveSpeed { get; set; } = 5f;
        public float MouseSensitivity { get; set; } = 0.04f;
        public float VerticalLookClamp { get; set; } = 80f;
        public bool UseCharacterController { get; set; }

        public PlayerMovementController(
            Transform bodyTransform,
            Transform cameraTransform,
            CharacterController characterController = null)
        {
            this.bodyTransform = bodyTransform;
            this.cameraTransform = cameraTransform;
            this.characterController = characterController;
        }

        /// <summary>
        /// Called by Unity Input System action callback.
        /// </summary>
        public void OnMove(Vector2 input)
        {
            moveInput = input;
        }

        /// <summary>
        /// Called by Unity Input System action callback.
        /// </summary>
        public void OnLook(Vector2 input)
        {
            lookInput = input;
        }

        /// <summary>
        /// Update movement. Call this from Update or FixedUpdate.
        /// </summary>
        public void UpdateMovement(float deltaTime)
        {
            // Fallback: direct keyboard polling if no Input Actions
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
                Vector3 moveDirection = (bodyTransform.forward * vertical + bodyTransform.right * horizontal) * MoveSpeed * deltaTime;

                if (UseCharacterController && characterController != null)
                {
                    // Apply gravity if using CharacterController
                    moveDirection.y = -9.81f * deltaTime;
                    characterController.Move(moveDirection);
                }
                else
                {
                    // Simple transform movement
                    bodyTransform.position += moveDirection;
                }
            }

            // Reset input for next frame (if using polling fallback)
            if (Keyboard.current != null && !Keyboard.current.anyKey.isPressed)
            {
                moveInput = Vector2.zero;
            }
        }

        /// <summary>
        /// Update mouse look. Call this from Update.
        /// </summary>
        public void UpdateMouseLook()
        {
            if (cameraTransform == null)
            {
                return;
            }

            // Fallback: direct mouse polling if no Input Actions
            if (lookInput == Vector2.zero && Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                lookInput = mouseDelta;
            }

            // Apply mouse look
            if (lookInput.sqrMagnitude > 0.01f)
            {
                float mouseX = lookInput.x * MouseSensitivity;
                float mouseY = lookInput.y * MouseSensitivity;

                // Accumulate horizontal rotation
                playerYaw += mouseX;

                // Accumulate vertical rotation with clamping
                cameraPitch -= mouseY;
                cameraPitch = Mathf.Clamp(cameraPitch, -VerticalLookClamp, VerticalLookClamp);

                // Check if camera is on a child object or the same object
                if (cameraTransform == bodyTransform)
                {
                    // Camera is on the same GameObject - apply combined rotation
                    bodyTransform.rotation = Quaternion.Euler(cameraPitch, playerYaw, 0);
                }
                else
                {
                    // Camera is on a child - rotate body and camera separately
                    bodyTransform.rotation = Quaternion.Euler(0, playerYaw, 0);
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
