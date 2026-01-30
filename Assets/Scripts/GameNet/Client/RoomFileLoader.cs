using UnityEngine;
using GameNet.Shared;
using GameNet.Utils;

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
                Debug.Log($"[RoomFileLoader] Room file not found at: {filePath}. Creating default room with welcome message...", this);
                CreateDefaultRoomWithTextCubes();
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
            CreateDefaultRoomWithTextCubes();
        }

        /// <summary>
        /// Creates a default room with text cubes displaying a welcome message.
        /// Called automatically when no room file exists.
        /// </summary>
        private void CreateDefaultRoomWithTextCubes()
        {
            var cubes = TextCubes.GetTextCubes(
                "Welcome to\nMWorldsTech Demo",
                startX: -40f,
                startY: 1f,
                startZ: 15f,
                pixelSize: 1f,
                charSpacing: 1f,
                lineSpacing: 2f,
                prefabId: "Cube",
                stateJson: "",
                stableIdPrefix: "welcome-cube"
            );

            RoomFile defaultRoom = new RoomFile
            {
                roomName = "Default Room",
                objects = new System.Collections.Generic.List<RoomObjectFileEntry>()
            };

            foreach (var cubeData in cubes)
            {
                var entry = new RoomObjectFileEntry
                {
                    stableId = cubeData["stableId"] as string,
                    prefabId = cubeData["prefabId"] as string,
                    posX = (float)cubeData["posX"],
                    posY = (float)cubeData["posY"],
                    posZ = (float)cubeData["posZ"],
                    rotX = (float)cubeData["rotX"],
                    rotY = (float)cubeData["rotY"],
                    rotZ = (float)cubeData["rotZ"],
                    stateJson = cubeData["stateJson"] as string
                };
                defaultRoom.objects.Add(entry);
            }

            SaveRoomFile(defaultRoom);
            loadedRoom = defaultRoom;
            Debug.Log($"[RoomFileLoader] Created default room with {defaultRoom.objects.Count} text cubes.");
        }
    }
}
