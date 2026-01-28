using Mirror;
using UnityEngine;

/// <summary>
/// Entry point for starting and stopping networking.
/// Extends Mirror's NetworkManager to handle host/client lifecycle.
/// </summary>
public class NetworkRunner : NetworkManager
{
    [Header("Network Configuration")]
    public int port = 7777;
    public string defaultAddress = "127.0.0.1";

    [Header("World Reference")]
    public NetworkWorld world;

    private void Awake()
    {
        // Disable auto player creation - NetworkWorld handles spawning manually
        autoCreatePlayer = false;
    }

    /// <summary>
    /// Starts the host (server + client). Call this from UI or inspector context menu.
    /// </summary>
    [ContextMenu("Start Host")]
    public void StartHostServer()
    {
        if (NetworkServer.active || NetworkClient.active)
        {
            Debug.LogWarning("NetworkRunner: Already running. Call Shutdown() first.");
            return;
        }

        if (world == null)
        {
            Debug.LogError("NetworkRunner: NetworkWorld reference is missing!");
            return;
        }

        // Note: Port is configured directly on the Transport component in the inspector
        // The 'port' field here is for reference/documentation only

        StartHost();
        Debug.Log($"NetworkRunner: Started host (port configured on Transport component)");
    }

    /// <summary>
    /// Starts a client connecting to the specified address.
    /// </summary>
    public void StartClientConnection(string address)
    {
        if (NetworkServer.active || NetworkClient.active)
        {
            Debug.LogWarning("NetworkRunner: Already running. Call Shutdown() first.");
            return;
        }

        networkAddress = address;

        // Note: Port is configured directly on the Transport component in the inspector

        StartClient();
        Debug.Log($"NetworkRunner: Connecting to {address} (port configured on Transport component)");
    }

    /// <summary>
    /// Starts a client using the default address.
    /// </summary>
    [ContextMenu("Start Client (Default Address)")]
    public void StartClientDefault()
    {
        StartClientConnection(defaultAddress);
    }

    /// <summary>
    /// Stops host or client cleanly.
    /// </summary>
    [ContextMenu("Shutdown")]
    public void Shutdown()
    {
        if (NetworkServer.active)
        {
            StopHost();
            Debug.Log("NetworkRunner: Stopped host");
        }
        else if (NetworkClient.active)
        {
            StopClient();
            Debug.Log("NetworkRunner: Stopped client");
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("NetworkRunner: Server started");

        if (world != null)
        {
            world.InitializeWorld();
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Delegate player spawning to NetworkWorld
        if (world != null)
        {
            world.OnClientConnected(conn);
        }
        else
        {
            Debug.LogError("NetworkRunner: NetworkWorld reference missing during player spawn!");
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (world != null)
        {
            world.OnClientDisconnected(conn);
        }

        base.OnServerDisconnect(conn);
        Debug.Log($"NetworkRunner: Client disconnected - connectionId: {conn.connectionId}");
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("NetworkRunner: Connected to server");

        // Ready up and request to be added as a player
        if (!NetworkClient.ready)
        {
            NetworkClient.Ready();
        }

        if (NetworkClient.localPlayer == null)
        {
            NetworkClient.AddPlayer();
        }
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("NetworkRunner: Disconnected from server");
    }
}
