using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Host-authoritative world and player spawn management.
/// Handles spawning/despawning players and enforces max player count.
/// </summary>
public class NetworkWorld : MonoBehaviour
{
    [Header("Player Setup")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;
    public int maxPlayers = 2;

    private Dictionary<int, GameObject> playerInstances = new Dictionary<int, GameObject>();
    private int nextSpawnIndex = 0;

    /// <summary>
    /// Called once when the server starts. Hook for initializing world objects.
    /// </summary>
    public void InitializeWorld()
    {
        if (!NetworkServer.active)
        {
            return;
        }

        Debug.Log("NetworkWorld: World initialized on server");
        // Future: spawn static world objects here if needed
    }

    /// <summary>
    /// Called when a client connects and needs a player spawned.
    /// </summary>
    public void OnClientConnected(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active)
        {
            return;
        }

        // Enforce max player limit
        if (playerInstances.Count >= maxPlayers)
        {
            Debug.LogWarning($"NetworkWorld: Max players ({maxPlayers}) reached. Disconnecting new connection.");
            conn.Disconnect();
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("NetworkWorld: playerPrefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        // Add player for this connection and assign ownership
        NetworkServer.AddPlayerForConnection(conn, playerInstance);
        playerInstances[conn.connectionId] = playerInstance;

        Debug.Log($"NetworkWorld: Spawned player for connection {conn.connectionId} at {spawnPosition}");
    }

    /// <summary>
    /// Called when a client disconnects. Cleans up their player object.
    /// </summary>
    public void OnClientDisconnected(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active)
        {
            return;
        }

        if (playerInstances.TryGetValue(conn.connectionId, out GameObject playerInstance))
        {
            playerInstances.Remove(conn.connectionId);

            if (playerInstance != null)
            {
                NetworkServer.Destroy(playerInstance);
                Debug.Log($"NetworkWorld: Despawned player for connection {conn.connectionId}");
            }
        }
    }

    private Vector3 GetSpawnPosition()
    {
        // Use configured spawn points if available
        if (spawnPoints != null && spawnPoints.Length > 0 && nextSpawnIndex < spawnPoints.Length)
        {
            Vector3 position = spawnPoints[nextSpawnIndex].position;
            nextSpawnIndex++;
            return position;
        }

        // Fallback: spawn at origin with small offset per player
        Vector3 fallbackPosition = new Vector3(nextSpawnIndex * 2f, 0, 0);
        nextSpawnIndex++;
        return fallbackPosition;
    }

    /// <summary>
    /// Helper to find the local player GameObject. Useful for UI/camera attachment.
    /// </summary>
    public GameObject GetLocalPlayerInstance()
    {
        foreach (var playerInstance in playerInstances.Values)
        {
            if (playerInstance != null)
            {
                NetworkIdentity identity = playerInstance.GetComponent<NetworkIdentity>();
                if (identity != null && identity.isLocalPlayer)
                {
                    return playerInstance;
                }
            }
        }

        return null;
    }
}
