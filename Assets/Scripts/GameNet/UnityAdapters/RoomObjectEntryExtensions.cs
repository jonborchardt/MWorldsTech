using UnityEngine;
using GameNet.Shared;

namespace GameNet.UnityAdapters
{
    /// <summary>
    /// Extension methods for RoomObjectFileEntry to bridge pure C# data with Unity types.
    /// This keeps the data structure Unity-independent while providing convenient Unity interop.
    /// </summary>
    public static class RoomObjectEntryExtensions
    {
        public static Vector3 GetPosition(this RoomObjectFileEntry entry)
        {
            return new Vector3(entry.posX, entry.posY, entry.posZ);
        }

        public static Quaternion GetRotation(this RoomObjectFileEntry entry)
        {
            return Quaternion.Euler(entry.rotX, entry.rotY, entry.rotZ);
        }

        public static void SetPosition(this RoomObjectFileEntry entry, Vector3 pos)
        {
            entry.posX = pos.x;
            entry.posY = pos.y;
            entry.posZ = pos.z;
        }

        public static void SetRotation(this RoomObjectFileEntry entry, Quaternion rot)
        {
            Vector3 euler = rot.eulerAngles;
            entry.rotX = euler.x;
            entry.rotY = euler.y;
            entry.rotZ = euler.z;
        }
    }
}
