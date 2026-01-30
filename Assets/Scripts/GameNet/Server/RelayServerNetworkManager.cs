using UnityEngine;
using Mirror;
using GameNet.Shared;

namespace GameNet.Server
{
    /// <summary>
    /// Custom NetworkManager for the relay server.
    /// Handles player spawning and server lifecycle.
    /// </summary>
    public class RelayServerNetworkManager : NetworkManager
    {
        [Header("GameNet Settings")]
        [SerializeField]
        private NetPrefabRegistry prefabRegistry;

        [SerializeField]
        private GameObject relayBehaviourPrefab;

        public NetPrefabRegistry PrefabRegistry => prefabRegistry;

        public override void Awake()
        {
            base.Awake();

            if (prefabRegistry == null)
            {
                Debug.LogError("[RelayServerNetworkManager] NetPrefabRegistry not assigned. Server cannot spawn objects.", this);
            }
            else
            {
                prefabRegistry.Initialize();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("[RelayServerNetworkManager] Server started.");
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("[RelayServerNetworkManager] Server stopped.");
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log($"[RelayServerNetworkManager] Client connected: {conn.connectionId}");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log($"[RelayServerNetworkManager] Client disconnected: {conn.connectionId}");
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // Spawn player with RelayServerBehaviour attached
            GameObject player = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, player);

            Debug.Log($"[RelayServerNetworkManager] Player spawned for connection {conn.connectionId}");
        }

        /// <summary>
        /// Server-side helper to spawn a room object with authority assigned to a specific connection.
        /// </summary>
        public GameObject SpawnRoomObject(string prefabId, string stableId, Vector3 position, Quaternion rotation, NetworkConnectionToClient authority)
        {
            if (prefabRegistry == null)
            {
                Debug.LogError("[RelayServerNetworkManager] Cannot spawn object: PrefabRegistry is null.");
                return null;
            }

            GameObject prefab = prefabRegistry.GetPrefab(prefabId);
            if (prefab == null)
            {
                Debug.LogError($"[RelayServerNetworkManager] Cannot spawn object: prefabId '{prefabId}' not found in registry.");
                return null;
            }

            GameObject obj = Instantiate(prefab, position, rotation);

            // Set stable ID before spawning
            StableId stableIdComponent = obj.GetComponent<StableId>();
            if (stableIdComponent != null)
            {
                stableIdComponent.SetId(stableId);
            }
            else
            {
                Debug.LogWarning($"[RelayServerNetworkManager] Spawned object '{prefabId}' does not have StableId component.");
            }

            // Spawn on network with authority
            NetworkServer.Spawn(obj, authority);

            Debug.Log($"[RelayServerNetworkManager] Spawned room object '{prefabId}' (stableId: {stableId}) with authority to connection {authority.connectionId}");

            return obj;
        }
    }
}
