using UnityEngine;
using Mirror;
using GameNet.Shared;
using GameNet.UnityAdapters;
using System.Collections.Generic;

namespace GameNet.Client
{
    /// <summary>
    /// Publishes local room data to the server after connection.
    /// Also supports offline mode where room objects are spawned locally without network.
    /// Sends spawn requests for all objects in the loaded room file.
    /// Handles snapshot requests from late-joining clients.
    /// </summary>
    public class ClientRoomPublisher : NetworkBehaviour
    {
        [SerializeField]
        private RoomFileLoader roomLoader;

        [SerializeField]
        private NetPrefabRegistry prefabRegistry;

        private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
        private bool hasPublishedRoom = false;

        public override void OnStartClient()
        {
            base.OnStartClient();

            Debug.Log($"[ClientRoomPublisher] OnStartClient called. isOwned={isOwned}, isClient={isClient}, isServer={isServer}, NetworkClient.isConnected={NetworkClient.isConnected}");

            if (!isOwned)
            {
                return;
            }

            // Auto-wire RoomFileLoader from scene if not assigned (set at runtime from scene)
            if (roomLoader == null)
            {
                roomLoader = FindFirstObjectByType<RoomFileLoader>();
                if (roomLoader != null)
                {
                    Debug.Log("[ClientRoomPublisher] Auto-wired RoomFileLoader from scene.");
                }
                else
                {
                    Debug.LogError("[ClientRoomPublisher] RoomFileLoader not found in scene. Cannot publish room.", this);
                    return;
                }
            }

            // When we spawn as a networked player, publish our room automatically
            Debug.Log("[ClientRoomPublisher] Client started. Publishing room...");
            PublishRoom();

            // Request snapshots from all existing clients when we join
            RequestSnapshotsFromAll();
        }

        /// <summary>
        /// Publishes the room to the server (online mode).
        /// If objects were already spawned offline, they will be registered with the network.
        /// </summary>
        [ContextMenu("Publish Room Online")]
        public void PublishRoom()
        {
            if (!NetworkClient.isConnected)
            {
                Debug.LogError("[ClientRoomPublisher] Cannot publish room: not connected to server. Load offline instead or connect first.", this);
                return;
            }

            if (!isOwned)
            {
                Debug.LogError("[ClientRoomPublisher] Cannot publish room on non-owned object.", this);
                return;
            }

            if (hasPublishedRoom)
            {
                Debug.LogWarning("[ClientRoomPublisher] Room already published.", this);
                return;
            }

            if (roomLoader == null)
            {
                Debug.LogError("[ClientRoomPublisher] RoomFileLoader not assigned.", this);
                return;
            }

            if (!roomLoader.IsLoaded)
            {
                Debug.LogError("[ClientRoomPublisher] No room loaded. Call RoomFileLoader.LoadRoomFile() first.", this);
                return;
            }

            if (prefabRegistry == null)
            {
                Debug.LogError("[ClientRoomPublisher] NetPrefabRegistry not assigned.", this);
                return;
            }

            RoomFile room = roomLoader.LoadedRoom;

            // Get RelayServerBehaviour to send Commands through
            var relayBehaviour = GetComponent<NetworkBehaviour>();
            NetworkBehaviour serverBehaviour = null;

            foreach (var behaviour in GetComponents<NetworkBehaviour>())
            {
                if (behaviour.GetType().Name == "RelayServerBehaviour")
                {
                    serverBehaviour = behaviour;
                    break;
                }
            }

            if (serverBehaviour == null)
            {
                Debug.LogError("[ClientRoomPublisher] RelayServerBehaviour not found on player object! Cannot send Commands.", this);
                return;
            }

            // Announce room using reflection to call Command on RelayServerBehaviour
            RoomAnnounceMessage announce = new RoomAnnounceMessage
            {
                roomName = room.roomName
            };
            Debug.Log($"[ClientRoomPublisher] Calling CmdAnnounceRoom for room '{room.roomName}'");

            var announceMethod = serverBehaviour.GetType().GetMethod("CmdAnnounceRoom");
            if (announceMethod != null)
            {
                announceMethod.Invoke(serverBehaviour, new object[] { announce });
            }

            // Send spawn requests for all objects
            Debug.Log($"[ClientRoomPublisher] Sending spawn requests for {room.objects.Count} object(s)...");
            var spawnMethod = serverBehaviour.GetType().GetMethod("CmdRequestSpawnRoomObject");

            foreach (var obj in room.objects)
            {
                SpawnRoomObjectRequest request = new SpawnRoomObjectRequest
                {
                    stableId = obj.stableId,
                    prefabId = obj.prefabId,
                    stateJson = obj.stateJson
                };
                request.SetPosition(obj.GetPosition());
                request.SetRotation(obj.GetRotation());

                Debug.Log($"[ClientRoomPublisher] Calling CmdRequestSpawnRoomObject: stableId='{obj.stableId}', prefabId='{obj.prefabId}'");

                if (spawnMethod != null)
                {
                    spawnMethod.Invoke(serverBehaviour, new object[] { request });
                }
            }

            hasPublishedRoom = true;
            Debug.Log($"[ClientRoomPublisher] Published room '{room.roomName}' with {room.objects.Count} objects.");
        }


        private void RequestSnapshotsFromAll()
        {
            // Get RelayServerBehaviour to send Command through
            NetworkBehaviour serverBehaviour = null;

            foreach (var behaviour in GetComponents<NetworkBehaviour>())
            {
                if (behaviour.GetType().Name == "RelayServerBehaviour")
                {
                    serverBehaviour = behaviour;
                    break;
                }
            }

            if (serverBehaviour == null)
            {
                Debug.LogWarning("[ClientRoomPublisher] RelayServerBehaviour not found, cannot request snapshots.");
                return;
            }

            var snapshotMethod = serverBehaviour.GetType().GetMethod("CmdRequestSnapshotFromAll");
            if (snapshotMethod != null)
            {
                snapshotMethod.Invoke(serverBehaviour, null);
                Debug.Log("[ClientRoomPublisher] Requested snapshots from all connected clients.");
            }
        }

        [TargetRpc]
        public void TargetReceiveSnapshotRequest(NetworkConnection target)
        {
            // Another client is requesting our snapshot
            SendSnapshotResponse();
        }

        private void SendSnapshotResponse()
        {
            if (!hasPublishedRoom || spawnedObjects.Count == 0)
            {
                // No objects to share yet
                return;
            }

            List<SnapshotObjectEntry> entries = new List<SnapshotObjectEntry>();

            foreach (var kvp in spawnedObjects)
            {
                GameObject obj = kvp.Value;
                StableId stableId = obj.GetComponent<StableId>();

                if (stableId == null)
                {
                    continue;
                }

                SnapshotObjectEntry entry = new SnapshotObjectEntry
                {
                    stableId = stableId.Id,
                    prefabId = GetPrefabIdForObject(obj),
                    stateJson = ""
                };
                entry.SetPosition(obj.transform.position);
                entry.SetRotation(obj.transform.rotation);

                entries.Add(entry);
            }

            SnapshotResponse response = new SnapshotResponse
            {
                objects = entries
            };

            CmdSendSnapshotResponse(response);
            Debug.Log($"[ClientRoomPublisher] Sent snapshot with {entries.Count} objects.");
        }

        [Command]
        private void CmdSendSnapshotResponse(SnapshotResponse response)
        {
            // Server will relay this to the requesting client
        }

        [ClientRpc]
        public void RpcReceiveSnapshotResponse(SnapshotResponse response)
        {
            if (isOwned)
            {
                // Don't process our own snapshot
                return;
            }

            // Apply remote snapshot
            foreach (var entry in response.objects)
            {
                ApplyRemoteObject(entry);
            }

            Debug.Log($"[ClientRoomPublisher] Received and applied snapshot with {response.objects.Count} objects.");
        }

        private void ApplyRemoteObject(SnapshotObjectEntry entry)
        {
            if (spawnedObjects.ContainsKey(entry.stableId))
            {
                // Already have this object
                return;
            }

            GameObject prefab = prefabRegistry.GetPrefab(entry.prefabId);
            if (prefab == null)
            {
                Debug.LogError($"[ClientRoomPublisher] Cannot apply remote object: prefabId '{entry.prefabId}' not found in registry.");
                return;
            }

            // Instantiate locally (not networked, just visual representation)
            GameObject obj = Instantiate(prefab, entry.GetPosition(), entry.GetRotation());

            StableId stableId = obj.GetComponent<StableId>();
            if (stableId != null)
            {
                stableId.SetId(entry.stableId);
            }

            spawnedObjects[entry.stableId] = obj;
        }

        public void RegisterSpawnedObject(string stableId, GameObject obj)
        {
            if (string.IsNullOrEmpty(stableId))
            {
                Debug.LogError("[ClientRoomPublisher] Cannot register object with empty stableId.", this);
                return;
            }

            spawnedObjects[stableId] = obj;
        }

        private string GetPrefabIdForObject(GameObject obj)
        {
            // Simple heuristic: use prefab name
            // In a real implementation, you'd want to track this more robustly
            return obj.name.Replace("(Clone)", "").Trim();
        }
    }
}
