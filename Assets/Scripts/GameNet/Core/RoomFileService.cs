using System;
using System.IO;
using GameNet.Shared;

namespace GameNet.Core
{
    /// <summary>
    /// Core service for loading and saving room files.
    /// Pure C# with no Unity dependencies - testable without Play Mode.
    /// </summary>
    public class RoomFileService
    {
        private readonly string _persistentDataPath;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IGameLogger _logger;

        public RoomFileService(string persistentDataPath, IJsonSerializer jsonSerializer, IGameLogger logger)
        {
            if (string.IsNullOrEmpty(persistentDataPath))
            {
                throw new ArgumentException("Persistent data path cannot be null or empty.", nameof(persistentDataPath));
            }

            _persistentDataPath = persistentDataPath;
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads a room file from the persistent data path.
        /// Returns null if the file doesn't exist or cannot be loaded.
        /// </summary>
        public RoomFile Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            string filePath = Path.Combine(_persistentDataPath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.Log($"[RoomFileService] Room file not found at: {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                RoomFile room = _jsonSerializer.Deserialize<RoomFile>(json);

                if (room == null)
                {
                    _logger.LogError($"[RoomFileService] Failed to deserialize room file at: {filePath}");
                    return null;
                }

                if (room.objects == null)
                {
                    room.objects = new System.Collections.Generic.List<RoomObjectFileEntry>();
                }

                _logger.Log($"[RoomFileService] Loaded room '{room.roomName}' with {room.objects.Count} objects from: {filePath}");
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RoomFileService] Exception loading room file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves a room file to the persistent data path.
        /// </summary>
        public void Save(RoomFile room, string fileName)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room), "Cannot save null room.");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            string filePath = Path.Combine(_persistentDataPath, fileName);

            try
            {
                string json = _jsonSerializer.Serialize(room, prettyPrint: true);
                File.WriteAllText(filePath, json);
                _logger.Log($"[RoomFileService] Saved room '{room.roomName}' to: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RoomFileService] Exception saving room file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the full file path for a given file name.
        /// </summary>
        public string GetFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            return Path.Combine(_persistentDataPath, fileName);
        }

        /// <summary>
        /// Checks if a room file exists.
        /// </summary>
        public bool Exists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            string filePath = Path.Combine(_persistentDataPath, fileName);
            return File.Exists(filePath);
        }
    }

    /// <summary>
    /// Abstraction for JSON serialization - allows different implementations.
    /// </summary>
    public interface IJsonSerializer
    {
        T Deserialize<T>(string json);
        string Serialize<T>(T obj, bool prettyPrint = false);
    }

    /// <summary>
    /// Abstraction for logging - decouples from Unity Debug class.
    /// </summary>
    public interface IGameLogger
    {
        void Log(string message);
        void LogError(string message);
    }
}
