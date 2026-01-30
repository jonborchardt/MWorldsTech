using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameNet.Shared
{
    /// <summary>
    /// Root data structure for a room save file.
    /// Serialized to/from JSON on client side only.
    /// </summary>
    [Serializable]
    public class RoomFile
    {
        public string roomName = "Untitled Room";
        public List<RoomObjectFileEntry> objects = new List<RoomObjectFileEntry>();
    }

    /// <summary>
    /// Individual room object entry in the save file.
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

        public Vector3 GetPosition()
        {
            return new Vector3(posX, posY, posZ);
        }

        public Quaternion GetRotation()
        {
            return Quaternion.Euler(rotX, rotY, rotZ);
        }

        public void SetPosition(Vector3 pos)
        {
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;
        }

        public void SetRotation(Quaternion rot)
        {
            Vector3 euler = rot.eulerAngles;
            rotX = euler.x;
            rotY = euler.y;
            rotZ = euler.z;
        }
    }
}
