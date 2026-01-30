using UnityEngine;

namespace GameNet.Shared
{
    /// <summary>
    /// Component that stores a stable string identifier for networked objects.
    /// This ID persists across network spawns and is used for matching saved room objects.
    /// </summary>
    public class StableId : MonoBehaviour
    {
        [SerializeField]
        private string id = string.Empty;

        public string Id
        {
            get => id;
            set => id = value;
        }

        private void Awake()
        {
            // Generate a new ID if empty (for runtime-created objects like players)
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
            }
        }

        public void SetId(string newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                Debug.LogError($"[StableId] Cannot set empty ID on {gameObject.name}", this);
                return;
            }
            id = newId;
        }
    }
}
