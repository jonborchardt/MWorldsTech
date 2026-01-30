using Mirror;
using GameNet.Shared;
using System.Reflection;
using UnityEngine;

namespace GameNet.Client
{
    /// <summary>
    /// Main client entry point for GameNet.
    /// Handles connection to server and initial setup.
    /// Place this on a Bootstrap GameObject in the client scene.
    /// </summary>
    public class ClientBootstrap : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField]
        private string serverAddress = "127.0.0.1";

        [SerializeField]
        private ushort serverPort = 7777;

        [SerializeField]
        private float connectionTimeout = 10f;

        [Header("References")]
        [SerializeField]
        private NetPrefabRegistry prefabRegistry;

        [SerializeField]
        private RoomFileLoader roomLoader;

        [SerializeField]
        private NetworkManager networkManager;

        private bool isConnected = false;
        private float connectionStartTime;
        private bool isConnecting = false;

        private void Awake()
        {
            if (prefabRegistry == null)
            {
                Debug.LogError("[ClientBootstrap] NetPrefabRegistry not assigned. Assign it in the Inspector.", this);
                return;
            }

            if (roomLoader == null)
            {
                Debug.LogError("[ClientBootstrap] RoomFileLoader not assigned. Assign it in the Inspector.", this);
                return;
            }

            if (networkManager == null)
            {
                Debug.LogError("[ClientBootstrap] NetworkManager not assigned. Assign it in the Inspector.", this);
                return;
            }

            prefabRegistry.Initialize();

            // Hook up Mirror callbacks
            NetworkClient.OnConnectedEvent += OnClientConnected;
            NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
        }

        private void Start()
        {
            // Load room file on start (for offline mode)
            if (roomLoader != null)
            {
                roomLoader.LoadRoomFile();
            }
        }

        private void Update()
        {
            // Check for connection timeout
            if (isConnecting && !NetworkClient.isConnected)
            {
                if (Time.time - connectionStartTime > connectionTimeout)
                {
                    Debug.LogError($"[ClientBootstrap] Connection timeout after {connectionTimeout} seconds. Server not responding at {serverAddress}:{serverPort}");
                    isConnecting = false;
                    isConnected = false;
                    networkManager.StopClient();
                }
            }
        }

        [ContextMenu("Connect to Server")]
        public void Connect()
        {
            if (isConnected || NetworkClient.isConnected || isConnecting)
            {
                Debug.LogWarning("[ClientBootstrap] Already connected or connecting.", this);
                return;
            }

            if (roomLoader != null && !roomLoader.IsLoaded)
            {
                Debug.LogWarning("[ClientBootstrap] Room not loaded yet. Loading now...", this);
                roomLoader.LoadRoomFile();
            }

            // Cleanup offline mode (transitioning from offline to online)
            OfflinePlayerSpawner offlineSpawner = FindFirstObjectByType<OfflinePlayerSpawner>();
            if (offlineSpawner != null)
            {
                offlineSpawner.DestroyOfflinePlayer();
            }

            OfflineRoomManager offlineRoomManager = FindFirstObjectByType<OfflineRoomManager>();
            if (offlineRoomManager != null && offlineRoomManager.HasLoadedRoom)
            {
                offlineRoomManager.CleanupOfflineRoom();
            }

            // Set network address
            networkManager.networkAddress = serverAddress;

            // Set the port on the Transport component via reflection
            Transport transport = networkManager.GetComponent<Transport>();
            if (transport != null)
            {
                // Use reflection to set the port property (works for most transports)
                var portField = transport.GetType().GetField("port", BindingFlags.Public | BindingFlags.Instance);
                if (portField != null)
                {
                    portField.SetValue(transport, serverPort);
                    Debug.Log($"[ClientBootstrap] Transport port set to {serverPort}");
                }
                else
                {
                    Debug.LogWarning($"[ClientBootstrap] Could not find 'port' field on {transport.GetType().Name}. Using inspector-configured port.", this);
                }
            }
            else
            {
                Debug.LogWarning("[ClientBootstrap] Transport component not found on NetworkManager. Using default port.", this);
            }

            // Start connection attempt
            isConnecting = true;
            connectionStartTime = Time.time;
            Debug.Log($"[ClientBootstrap] Connecting to {serverAddress}:{serverPort}...");
            Debug.Log($"[ClientBootstrap] NetworkClient.isConnected before StartClient: {NetworkClient.isConnected}");

            networkManager.StartClient();

            Debug.Log($"[ClientBootstrap] NetworkClient.isConnected after StartClient: {NetworkClient.isConnected}");

            // Room will be published automatically when the player spawns
            // via ClientRoomPublisher.OnStartClient()
        }

        [ContextMenu("Disconnect from Server")]
        public void Disconnect()
        {
            if (!isConnected && !isConnecting)
            {
                Debug.LogWarning("[ClientBootstrap] Not connected.", this);
                return;
            }

            if (NetworkClient.isConnected)
            {
                networkManager.StopClient();
            }

            isConnecting = false;
            isConnected = false;
            Debug.Log("[ClientBootstrap] Disconnected from server.");
        }

        private void OnDestroy()
        {
            // Unregister callbacks
            NetworkClient.OnConnectedEvent -= OnClientConnected;
            NetworkClient.OnDisconnectedEvent -= OnClientDisconnected;

            if (isConnected && NetworkClient.isConnected)
            {
                Disconnect();
            }
        }

        // Called by Mirror events when connection succeeds
        private void OnClientConnected()
        {
            Debug.LogWarning($"[ClientBootstrap] *** OnClientConnected callback fired! ***");
            isConnecting = false;
            isConnected = true;
            Debug.Log($"[ClientBootstrap] Successfully connected to server at {serverAddress}:{serverPort}");
        }

        // Called by Mirror events when disconnected
        private void OnClientDisconnected()
        {
            isConnecting = false;
            isConnected = false;
            Debug.Log("[ClientBootstrap] Disconnected from server.");
        }
    }
}
