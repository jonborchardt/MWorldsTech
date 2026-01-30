using System;
using System.Collections.Generic;

namespace GameNet.Shared
{
    /// <summary>
    /// Root data structure for a room save file.
    /// Serialized to/from JSON on client side only.
    /// Pure C# data structure with no Unity dependencies.
    /// </summary>
    [Serializable]
    public class RoomFile
    {
        public string roomName = "Untitled Room";
        public List<RoomObjectFileEntry> objects = new List<RoomObjectFileEntry>();
    }

    /// <summary>
    /// Individual room object entry in the save file.
    /// Pure C# data structure with no Unity dependencies.
    /// Use RoomObjectEntryExtensions (in UnityAdapters) for Unity type conversions.
    /// </summary>
    [Serializable]
    public class RoomObjectFileEntry
    {
        public string stableId = string.Empty;
        public string prefabId = string.Empty;

        public float posX;
        public float posY;
        public float posZ;

        public float rotX;
        public float rotY;
        public float rotZ;

        /// <summary>
        /// Optional JSON blob for custom state.
        /// Can be empty or contain additional object-specific data.
        /// </summary>
        public string stateJson = string.Empty;
    }
}
