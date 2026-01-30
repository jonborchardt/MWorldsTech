using UnityEngine;
using GameNet.Shared;

namespace GameNet.Client
{
    /// <summary>
    /// Loads a room JSON file from persistent data path.
    /// Room files are stored locally and not transmitted to the server.
    /// </summary>
    public class RoomFileLoader : MonoBehaviour
    {
        [SerializeField]
        private string roomFileName = "MyRoom.json";

        private RoomFile loadedRoom;

        public RoomFile LoadedRoom => loadedRoom;
        public bool IsLoaded => loadedRoom != null;

        [ContextMenu("Load Room File")]
        public void LoadRoomFile()
        {
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, roomFileName);

            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"[RoomFileLoader] Room file not found at: {filePath}. Create a room file first.", this);
                loadedRoom = null;
                return;
            }

            try
            {
                string json = System.IO.File.ReadAllText(filePath);
                loadedRoom = JsonUtility.FromJson<RoomFile>(json);

                if (loadedRoom == null)
                {
                    Debug.LogError($"[RoomFileLoader] Failed to deserialize room file at: {filePath}", this);
                    return;
                }

                if (loadedRoom.objects == null)
                {
                    loadedRoom.objects = new System.Collections.Generic.List<RoomObjectFileEntry>();
                }

                Debug.Log($"[RoomFileLoader] Loaded room '{loadedRoom.roomName}' with {loadedRoom.objects.Count} objects from: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RoomFileLoader] Exception loading room file: {ex.Message}", this);
                loadedRoom = null;
            }
        }

        public void SaveRoomFile(RoomFile room)
        {
            if (room == null)
            {
                Debug.LogError("[RoomFileLoader] Cannot save null room.", this);
                return;
            }

            string filePath = System.IO.Path.Combine(Application.persistentDataPath, roomFileName);

            try
            {
                string json = JsonUtility.ToJson(room, true);
                System.IO.File.WriteAllText(filePath, json);
                Debug.Log($"[RoomFileLoader] Saved room '{room.roomName}' to: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RoomFileLoader] Exception saving room file: {ex.Message}", this);
            }
        }

        /// <summary>
        /// Creates a sample room file for testing.
        /// Right-click this component in Inspector and select "Create Sample Room File" to run.
        /// </summary>
        [ContextMenu("Create Sample Room File")]
        public void CreateSampleRoomFile()
        {
            RoomFile sampleRoom = new RoomFile
            {
                roomName = "Sample Room",
                objects = new System.Collections.Generic.List<RoomObjectFileEntry>
                {
                    new RoomObjectFileEntry
                    {
                        stableId = System.Guid.NewGuid().ToString(),
                        prefabId = "Cube",
                        posX = 0, posY = 1, posZ = 0,
                        rotX = 0, rotY = 0, rotZ = 0,
                        stateJson = ""
                    }
                }
            };

            SaveRoomFile(sampleRoom);
        }
    }
}
