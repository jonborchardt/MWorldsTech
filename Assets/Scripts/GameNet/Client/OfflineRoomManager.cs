using UnityEngine;
using GameNet.Shared;
using System.Collections.Generic;

namespace GameNet.Client
{
    /// <summary>
    /// Manages room loading in offline mode (when not connected to server).
    /// This component lives on a scene GameObject and works without NetworkBehaviour.
    /// For online mode, ClientRoomPublisher (on the player) handles room publishing.
    /// </summary>
    public class OfflineRoomManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private RoomFileLoader roomLoader;

        [SerializeField]
        private NetPrefabRegistry prefabRegistry;

        [Header("Settings")]
        [SerializeField]
        private bool loadRoomOnStart = true;

        private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
        private bool hasLoadedRoom = false;

        public bool HasLoadedRoom => hasLoadedRoom;
        public Dictionary<string, GameObject> SpawnedObjects => spawnedObjects;

        private void Start()
        {
            // Only load in offline mode
            if (loadRoomOnStart && !Mirror.NetworkClient.isConnected)
            {
                LoadRoomOffline();
            }
        }

        [ContextMenu("Load Room Offline")]
        public void LoadRoomOffline()
        {
            if (hasLoadedRoom)
            {
                Debug.LogWarning("[OfflineRoomManager] Room already loaded.", this);
                return;
            }

            if (roomLoader == null)
            {
                Debug.LogError("[OfflineRoomManager] RoomFileLoader not assigned.", this);
                return;
            }

            if (prefabRegistry == null)
            {
                Debug.LogError("[OfflineRoomManager] NetPrefabRegistry not assigned.", this);
                return;
            }

            // Load room file if not already loaded
            if (!roomLoader.IsLoaded)
            {
                roomLoader.LoadRoomFile();
            }

            if (!roomLoader.IsLoaded)
            {
                Debug.LogError("[OfflineRoomManager] Failed to load room file.", this);
                return;
            }

            RoomFile room = roomLoader.LoadedRoom;

            // Spawn objects locally (no network)
            foreach (var objData in room.objects)
            {
                GameObject prefab = prefabRegistry.GetPrefab(objData.prefabId);
                if (prefab == null)
                {
                    Debug.LogError($"[OfflineRoomManager] Cannot spawn offline object: prefabId '{objData.prefabId}' not found in registry.");
                    continue;
                }

                GameObject obj = Instantiate(prefab, objData.GetPosition(), objData.GetRotation());

                StableId stableId = obj.GetComponent<StableId>();
                if (stableId != null)
                {
                    stableId.SetId(objData.stableId);
                }

                spawnedObjects[objData.stableId] = obj;
            }

            hasLoadedRoom = true;
            Debug.Log($"[OfflineRoomManager] Loaded room '{room.roomName}' offline with {spawnedObjects.Count} objects.");
        }

        /// <summary>
        /// Cleans up offline room objects when transitioning to online mode.
        /// </summary>
        public void CleanupOfflineRoom()
        {
            foreach (var obj in spawnedObjects.Values)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            spawnedObjects.Clear();
            hasLoadedRoom = false;
            Debug.Log("[OfflineRoomManager] Cleaned up offline room objects.");
        }
    }
}
