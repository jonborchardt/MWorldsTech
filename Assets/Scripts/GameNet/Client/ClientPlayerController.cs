using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using GameNet.Shared;

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

        private PlayerMovementController movementController;

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

            // Initialize movement controller
            movementController = new PlayerMovementController(transform, cameraTransform, null)
            {
                MoveSpeed = moveSpeed,
                MouseSensitivity = mouseSensitivity,
                VerticalLookClamp = verticalLookClamp,
                UseCharacterController = false
            };

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

            if (movementController != null)
            {
                movementController.UpdateMovement(Time.deltaTime);
                movementController.UpdateMouseLook();
            }
        }

        // Called by Unity Input System (Player Input component or manually)
        public void OnMove(InputValue value)
        {
            if (isOwned)
            {
                movementController?.OnMove(value.Get<Vector2>());
            }
        }

        // Called by Unity Input System (Player Input component or manually)
        public void OnLook(InputValue value)
        {
            if (isOwned)
            {
                movementController?.OnLook(value.Get<Vector2>());
            }
        }
    }
}
