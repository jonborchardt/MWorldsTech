using UnityEngine;
using GameNet.Shared;
using GameNet.Core;
using GameNet.UnityAdapters;

namespace GameNet.Client
{
    /// <summary>
    /// Unity adapter for loading room files.
    /// Delegates to RoomFileService (Core layer) for actual file I/O.
    /// </summary>
    public class RoomFileLoader : MonoBehaviour
    {
        private RoomFileService _service;
        private RoomFile _loadedRoom;
        private string _roomFileName;

        public RoomFile LoadedRoom => _loadedRoom;
        public bool IsLoaded => _loadedRoom != null;

        /// <summary>
        /// Initializes the loader with explicit dependencies.
        /// Call this before using LoadRoomFile or SaveRoomFile.
        /// </summary>
        public void Initialize(string roomFileName = "MyRoom.json")
        {
            if (string.IsNullOrEmpty(roomFileName))
            {
                throw new System.ArgumentException("Room file name cannot be null or empty.", nameof(roomFileName));
            }

            _roomFileName = roomFileName;
            _service = new RoomFileService(
                Application.persistentDataPath,
                new UnityJsonSerializer(),
                new UnityLogger(this)
            );
        }

        [ContextMenu("Load Room File")]
        public void LoadRoomFile()
        {
            if (_service == null)
            {
                Debug.LogError("[RoomFileLoader] Service not initialized. Call Initialize() first.", this);
                return;
            }

            _loadedRoom = _service.Load(_roomFileName);

            if (_loadedRoom == null)
            {
                Debug.Log("[RoomFileLoader] Room file not found. Creating default room with welcome message...", this);
                CreateDefaultRoomWithTextCubes();
            }
        }

        public void SaveRoomFile(RoomFile room)
        {
            if (_service == null)
            {
                Debug.LogError("[RoomFileLoader] Service not initialized. Call Initialize() first.", this);
                return;
            }

            if (room == null)
            {
                Debug.LogError("[RoomFileLoader] Cannot save null room.", this);
                return;
            }

            try
            {
                _service.Save(room, _roomFileName);
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
            _loadedRoom = defaultRoom;
            Debug.Log($"[RoomFileLoader] Created default room with {defaultRoom.objects.Count} text cubes.", this);
        }

        private void Awake()
        {
            // Initialize with default filename on Awake
            Initialize();
        }
    }
}
