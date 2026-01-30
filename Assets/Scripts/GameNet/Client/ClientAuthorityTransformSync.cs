using UnityEngine;
using Mirror;
using GameNet.Shared;

namespace GameNet.Client
{
    /// <summary>
    /// Custom client-authoritative transform synchronization.
    /// Owner sends position/rotation to server, which relays to other clients.
    /// Non-owners interpolate toward received target.
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class ClientAuthorityTransformSync : NetworkBehaviour
    {
        [Header("Sync Settings")]
        [SerializeField]
        private float sendRate = 10f; // Hz

        [SerializeField]
        private float interpolationSpeed = 10f;

        private float nextSendTime;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private void Start()
        {
            if (!isOwned)
            {
                // Initialize target to current values for non-owners
                targetPosition = transform.position;
                targetRotation = transform.rotation;
            }
        }

        private void Update()
        {
            if (isOwned)
            {
                // Owner: Send updates at regular intervals
                if (Time.time >= nextSendTime)
                {
                    nextSendTime = Time.time + (1f / sendRate);
                    CmdSendTransform(transform.position, transform.rotation);
                }
            }
            else
            {
                // Non-owner: Interpolate toward target
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * interpolationSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * interpolationSpeed);
            }
        }

        [Command]
        private void CmdSendTransform(Vector3 position, Quaternion rotation)
        {
            // Server receives transform from owner and relays to all clients
            RpcReceiveTransform(position, rotation);
        }

        [ClientRpc]
        private void RpcReceiveTransform(Vector3 position, Quaternion rotation)
        {
            // Skip if we're the owner (we already have the correct transform)
            if (isOwned)
            {
                return;
            }

            targetPosition = position;
            targetRotation = rotation;
        }

        /// <summary>
        /// Manually set the transform for snapshot synchronization.
        /// Called when receiving snapshot data.
        /// </summary>
        public void SetTransformFromSnapshot(Vector3 position, Quaternion rotation)
        {
            if (isOwned)
            {
                // Owner: Apply directly
                transform.position = position;
                transform.rotation = rotation;
            }
            else
            {
                // Non-owner: Set target for interpolation
                targetPosition = position;
                targetRotation = rotation;
            }
        }
    }
}
