using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameNet.Shared
{
    /// <summary>
    /// Message sent by client to announce their room presence.
    /// </summary>
    [Serializable]
    public struct RoomAnnounceMessage
    {
        public string roomName;
    }

    /// <summary>
    /// Message sent by client to request server to spawn a room object with this client as authority.
    /// </summary>
    [Serializable]
    public struct SpawnRoomObjectRequest
    {
        public string stableId;
        public string prefabId;

        public float posX;
        public float posY;
        public float posZ;

        public float rotX;
        public float rotY;
        public float rotZ;

        public string stateJson;

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

    /// <summary>
    /// Delta update for an existing room object's transform and state.
    /// </summary>
    [Serializable]
    public struct RoomObjectStateDelta
    {
        public string stableId;

        public float posX;
        public float posY;
        public float posZ;

        public float rotX;
        public float rotY;
        public float rotZ;

        public string stateJson;

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

    /// <summary>
    /// Request sent by a newly joined client to existing clients asking for their room snapshots.
    /// </summary>
    [Serializable]
    public struct SnapshotRequest
    {
        // Empty for now, just a signal
        public int unused;
    }

    /// <summary>
    /// Response containing a client's current authoritative room objects.
    /// </summary>
    [Serializable]
    public struct SnapshotResponse
    {
        public List<SnapshotObjectEntry> objects;
    }

    /// <summary>
    /// Single object entry in a snapshot response.
    /// </summary>
    [Serializable]
    public struct SnapshotObjectEntry
    {
        public string stableId;
        public string prefabId;

        public float posX;
        public float posY;
        public float posZ;

        public float rotX;
        public float rotY;
        public float rotZ;

        public string stateJson;

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
