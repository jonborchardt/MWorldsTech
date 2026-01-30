using UnityEngine;
using Mirror;

namespace GameNet.Client
{
    /// <summary>
    /// Spawns a local player prefab for offline single-player mode.
    /// When connecting online, this local player is destroyed and replaced by the networked player.
    /// </summary>
    public class OfflinePlayerSpawner : MonoBehaviour
    {
        [Header("Offline Player")]
        [SerializeField]
        private GameObject offlinePlayerPrefab;

        [SerializeField]
        private Vector3 spawnPosition = Vector3.zero;

        [SerializeField]
        private bool spawnOnStart = true;

        private GameObject spawnedPlayer;

        private void Start()
        {
            if (spawnOnStart && !NetworkClient.isConnected)
            {
                SpawnOfflinePlayer();
            }
        }

        [ContextMenu("Spawn Offline Player")]
        public void SpawnOfflinePlayer()
        {
            if (spawnedPlayer != null)
            {
                Debug.LogWarning("[OfflinePlayerSpawner] Offline player already spawned.", this);
                return;
            }

            if (NetworkClient.isConnected)
            {
                Debug.LogWarning("[OfflinePlayerSpawner] Cannot spawn offline player: already connected to server. Networked player will spawn instead.", this);
                return;
            }

            if (offlinePlayerPrefab == null)
            {
                Debug.LogError("[OfflinePlayerSpawner] Offline player prefab not assigned.", this);
                return;
            }

            spawnedPlayer = Instantiate(offlinePlayerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("[OfflinePlayerSpawner] Spawned offline player for single-player mode.");
        }

        /// <summary>
        /// Call this when transitioning from offline to online mode to clean up the offline player.
        /// </summary>
        public void DestroyOfflinePlayer()
        {
            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
                spawnedPlayer = null;
                Debug.Log("[OfflinePlayerSpawner] Destroyed offline player (transitioning to online mode).");
            }
        }

        private void OnDestroy()
        {
            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
            }
        }
    }
}
