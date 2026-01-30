using System.Collections.Generic;
using UnityEngine;

namespace GameNet.Shared
{
    /// <summary>
    /// Registry mapping prefabId strings to GameObject prefabs.
    /// Must be configured in the Inspector before runtime.
    /// Both client and server need this registry to instantiate networked objects.
    /// </summary>
    [CreateAssetMenu(fileName = "NetPrefabRegistry", menuName = "GameNet/Prefab Registry")]
    public class NetPrefabRegistry : ScriptableObject
    {
        [System.Serializable]
        public class PrefabEntry
        {
            public string prefabId;
            public GameObject prefab;
        }

        [SerializeField]
        private List<PrefabEntry> prefabs = new List<PrefabEntry>();

        private Dictionary<string, GameObject> prefabLookup;

        public void Initialize()
        {
            prefabLookup = new Dictionary<string, GameObject>();

            foreach (var entry in prefabs)
            {
                if (string.IsNullOrEmpty(entry.prefabId))
                {
                    Debug.LogError("[NetPrefabRegistry] Found entry with empty prefabId. Skipping.", this);
                    continue;
                }

                if (entry.prefab == null)
                {
                    Debug.LogError($"[NetPrefabRegistry] PrefabId '{entry.prefabId}' has null prefab reference. Skipping.", this);
                    continue;
                }

                if (prefabLookup.ContainsKey(entry.prefabId))
                {
                    Debug.LogError($"[NetPrefabRegistry] Duplicate prefabId '{entry.prefabId}' found. Using first entry only.", this);
                    continue;
                }

                prefabLookup[entry.prefabId] = entry.prefab;
            }

            Debug.Log($"[NetPrefabRegistry] Initialized with {prefabLookup.Count} prefabs.");
        }

        public GameObject GetPrefab(string prefabId)
        {
            if (prefabLookup == null)
            {
                Debug.LogError("[NetPrefabRegistry] Registry not initialized. Call Initialize() first.", this);
                return null;
            }

            if (string.IsNullOrEmpty(prefabId))
            {
                Debug.LogError("[NetPrefabRegistry] Cannot get prefab for empty prefabId.", this);
                return null;
            }

            if (!prefabLookup.TryGetValue(prefabId, out GameObject prefab))
            {
                Debug.LogError($"[NetPrefabRegistry] PrefabId '{prefabId}' not found in registry. Available IDs: {string.Join(", ", prefabLookup.Keys)}", this);
                return null;
            }

            return prefab;
        }

        public bool HasPrefab(string prefabId)
        {
            if (prefabLookup == null)
            {
                return false;
            }

            return prefabLookup.ContainsKey(prefabId);
        }
    }
}
