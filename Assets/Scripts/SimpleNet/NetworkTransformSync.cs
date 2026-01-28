using Mirror;
using UnityEngine;

/// <summary>
/// Minimal transform replication without using Mirror's built-in NetworkTransform.
/// Syncs position and rotation from server to clients at a fixed interval.
/// Uses the inherited syncInterval from NetworkBehaviour (default 0.1s, configurable in inspector).
/// </summary>
public class NetworkTransformSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPositionChanged))]
    private Vector3 syncPosition;

    [SyncVar(hook = nameof(OnRotationChanged))]
    private Quaternion syncRotation;

    private float nextSyncTime;

    // Public getters for SimpleSmoothing to read target values
    public Vector3 TargetPosition => syncPosition;
    public Quaternion TargetRotation => syncRotation;

    private void Start()
    {
        // Set sync interval to 20Hz (0.05s) for smooth replication
        syncInterval = 0.05f;

        // Initialize sync vars with current transform
        if (isServer)
        {
            syncPosition = transform.position;
            syncRotation = transform.rotation;
        }
    }

    private void Update()
    {
        // Server: send transform updates at fixed interval
        if (isServer && Time.time >= nextSyncTime)
        {
            syncPosition = transform.position;
            syncRotation = transform.rotation;
            nextSyncTime = Time.time + syncInterval;
        }
    }

    /// <summary>
    /// SyncVar hook: called on clients when position changes.
    /// For local player: apply directly (no smoothing, but has network lag).
    /// For remote players: SimpleSmoothing handles interpolation.
    /// </summary>
    private void OnPositionChanged(Vector3 oldPos, Vector3 newPos)
    {
        if (isLocalPlayer)
        {
            transform.position = newPos;
        }
    }

    /// <summary>
    /// SyncVar hook: called on clients when rotation changes.
    /// For local player: apply directly (no smoothing, but has network lag).
    /// For remote players: SimpleSmoothing handles interpolation.
    /// </summary>
    private void OnRotationChanged(Quaternion oldRot, Quaternion newRot)
    {
        if (isLocalPlayer)
        {
            transform.rotation = newRot;
        }
    }
}
