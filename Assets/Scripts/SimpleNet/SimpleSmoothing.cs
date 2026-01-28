using Mirror;
using UnityEngine;

/// <summary>
/// Client-side interpolation for remote players.
/// Smoothly interpolates transform towards target values from NetworkTransformSync.
/// Does NOT run for locally owned players.
/// </summary>
public class SimpleSmoothing : NetworkBehaviour
{
    [Header("Smoothing Settings")]
    public float positionLerpSpeed = 12f;
    public float rotationLerpSpeed = 12f;

    private NetworkTransformSync transformSync;

    private void Start()
    {
        transformSync = GetComponent<NetworkTransformSync>();
        if (transformSync == null)
        {
            Debug.LogError("SimpleSmoothing: NetworkTransformSync component not found!");
        }
    }

    private void Update()
    {
        // Only smooth non-owned remote players
        if (isLocalPlayer)
        {
            return;
        }

        if (transformSync == null)
        {
            return;
        }

        // Interpolate towards target position/rotation from NetworkTransformSync
        transform.position = Vector3.Lerp(
            transform.position,
            transformSync.TargetPosition,
            positionLerpSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            transformSync.TargetRotation,
            rotationLerpSpeed * Time.deltaTime
        );
    }
}
