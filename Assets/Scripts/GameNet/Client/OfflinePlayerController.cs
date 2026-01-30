using UnityEngine;
using UnityEngine.InputSystem;
using GameNet.Shared;

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
        private PlayerMovementController movementController;

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

            // Initialize movement controller
            movementController = new PlayerMovementController(transform, cameraTransform, characterController)
            {
                MoveSpeed = moveSpeed,
                MouseSensitivity = mouseSensitivity,
                VerticalLookClamp = verticalLookClamp,
                UseCharacterController = useCharacterController
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
            if (movementController != null)
            {
                movementController.UpdateMovement(Time.deltaTime);
                movementController.UpdateMouseLook();
            }
        }

        // Called by Unity Input System (Player Input component or manually)
        public void OnMove(InputValue value)
        {
            movementController?.OnMove(value.Get<Vector2>());
        }

        // Called by Unity Input System (Player Input component or manually)
        public void OnLook(InputValue value)
        {
            movementController?.OnLook(value.Get<Vector2>());
        }
    }
}
