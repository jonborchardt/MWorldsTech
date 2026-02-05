using UnityEngine;
using UnityEngine.InputSystem;
namespace Game.Player.DesktopFps
{
    [DisallowMultipleComponent]
    public sealed class CameraController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Player body transform that receives yaw rotation. If null, uses this transform.")]
        [SerializeField] private Transform playerBody;
        [Tooltip("Camera pivot transform that receives pitch rotation.")]
        [SerializeField] private Transform cameraPivot;
        [Tooltip("Optional Camera transform. Only used for position/forward if set; otherwise pivot is used.")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("InputReader handles player input")]
        [SerializeField] private InputSystem.InputReader inputReader;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.08f;
        [SerializeField] private float minPitch = -85.0f;
        [SerializeField] private float maxPitch = 85.0f;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnEnable = true;

        private float yaw;
        private float pitch;

        private void Awake()
        {
            if (playerBody == null)
            {
                playerBody = transform;
            }

            if (cameraPivot == null)
            {
                Debug.LogError($"{nameof(CameraController)}: cameraPivot is not set.", this);
                enabled = false;
                return;
            }

            if (cameraTransform == null)
            {
                cameraTransform = cameraPivot;
            }

            var euler = playerBody.rotation.eulerAngles;
            yaw = euler.y;

            pitch = cameraPivot.localRotation.eulerAngles.x;
            if (pitch > 180.0f)
            {
                pitch -= 360.0f;
            }
        }

        private void OnEnable()
        {
            ApplyCursorLock(lockCursorOnEnable);

            if (inputReader != null)
            {
                inputReader.onLook += OnLook;
            }
        }

        private void OnDisable()
        {
            ApplyCursorLock(false);

            if (inputReader != null)
            {
                inputReader.onLook -= OnLook;
            }
        }

        public void OnLook(Vector2 lookDelta)
        {
            yaw += lookDelta.x * mouseSensitivity;
            pitch -= lookDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            playerBody.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        }

        // Compatibility API (used by Synty sample logic)

        public Vector3 GetCameraForward()
        {
            return cameraTransform != null ? cameraTransform.forward : cameraPivot.forward;
        }

        public Vector3 GetCameraForwardZeroedYNormalised()
        {
            Vector3 f = GetCameraForward();
            f.y = 0f;

            float mag = f.magnitude;
            if (mag <= 0.0001f)
            {
                Vector3 fallback = playerBody != null ? playerBody.forward : Vector3.forward;
                fallback.y = 0f;
                return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.forward;
            }

            return f / mag;
        }

        public Vector3 GetCameraRightZeroedYNormalised()
        {
            Vector3 r = cameraTransform != null ? cameraTransform.right : cameraPivot.right;
            r.y = 0f;

            float mag = r.magnitude;
            if (mag <= 0.0001f)
            {
                return Vector3.right;
            }

            return r / mag;
        }

        public Vector3 GetCameraPosition()
        {
            return cameraTransform != null ? cameraTransform.position : cameraPivot.position;
        }

        public float GetCameraTiltX()
        {
            // Synty code expects 0..360 style Euler pitch
            return cameraPivot != null ? cameraPivot.eulerAngles.x : 0f;
        }

        public void LockOn(bool enable, Transform target)
        {
            // No-op for FPS. Retained for animation controller compatibility.
        }

        private static void ApplyCursorLock(bool shouldLock)
        {
            if (!shouldLock)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
