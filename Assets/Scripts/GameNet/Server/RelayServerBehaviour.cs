using UnityEngine;
using Mirror;
using GameNet.Shared;
using System.Collections.Generic;

namespace GameNet.Server
{
    /// <summary>
    /// Server-side NetworkBehaviour that receives Commands from clients and relays messages.
    /// Attached to each player object on the server.
    /// Validates authority and relays spawn/state messages between clients.
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class RelayServerBehaviour : NetworkBehaviour
    {
        private RelayServerNetworkManager serverManager;

        private Dictionary<string, NetworkIdentity> spawnedObjectsByStableId = new Dictionary<string, NetworkIdentity>();
        private string clientRoomName = "Unknown";

        public override void OnStartServer()
        {
            base.OnStartServer();

            serverManager = FindFirstObjectByType<RelayServerNetworkManager>();
            if (serverManager == null)
            {
                Debug.LogError("[RelayServerBehaviour] RelayServerNetworkManager not found in scene.", this);
            }
            else
            {
                Debug.Log($"[RelayServerBehaviour] Player {connectionToClient.connectionId} connected to server (netId: {netId})");
            }
        }

        #region Commands from Client

        [Command]
        public void CmdAnnounceRoom(RoomAnnounceMessage announce)
        {
            if (!isServer)
            {
                return;
            }

            clientRoomName = announce.roomName;
            Debug.Log($"[RelayServerBehaviour] Client {connectionToClient.connectionId} announced room: '{clientRoomName}'");
        }

        [Command]
        public void CmdRequestSpawnRoomObject(SpawnRoomObjectRequest request)
        {
            if (!isServer)
            {
                return;
            }

            if (string.IsNullOrEmpty(request.stableId))
            {
                Debug.LogError($"[RelayServerBehaviour] Client {connectionToClient.connectionId} sent spawn request with empty stableId. Ignoring.");
                return;
            }

            if (spawnedObjectsByStableId.ContainsKey(request.stableId))
            {
                Debug.LogWarning($"[RelayServerBehaviour] Object with stableId '{request.stableId}' already spawned. Ignoring duplicate request.");
                return;
            }

            if (serverManager == null)
            {
                Debug.LogError("[RelayServerBehaviour] ServerManager is null. Cannot spawn object.");
                return;
            }

            // Spawn the object with this client as authority
            GameObject spawnedObj = serverManager.SpawnRoomObject(
                request.prefabId,
                request.stableId,
                request.GetPosition(),
                request.GetRotation(),
                connectionToClient
            );

            if (spawnedObj != null)
            {
                NetworkIdentity netId = spawnedObj.GetComponent<NetworkIdentity>();
                if (netId != null)
                {
                    spawnedObjectsByStableId[request.stableId] = netId;
                }

                // Notify all clients about the new object (Mirror handles this via spawn)
                Debug.Log($"[RelayServerBehaviour] Spawned room object '{request.prefabId}' for client {connectionToClient.connectionId}");
            }
        }

        [Command]
        public void CmdSendRoomObjectDelta(RoomObjectStateDelta delta)
        {
            if (!isServer)
            {
                return;
            }

            // Validate authority: only allow updates for objects owned by this connection
            if (!spawnedObjectsByStableId.TryGetValue(delta.stableId, out NetworkIdentity netId))
            {
                Debug.LogWarning($"[RelayServerBehaviour] Client {connectionToClient.connectionId} sent delta for unknown stableId '{delta.stableId}'. Ignoring.");
                return;
            }

            if (netId.connectionToClient != connectionToClient)
            {
                Debug.LogError($"[RelayServerBehaviour] Client {connectionToClient.connectionId} attempted to update object '{delta.stableId}' owned by another client. Ignoring.");
                return;
            }

            // Relay to all other clients
            RpcReceiveRoomObjectDelta(delta);
        }

        [Command]
        public void CmdRequestSnapshotFromAll()
        {
            if (!isServer)
            {
                return;
            }

            Debug.Log($"[RelayServerBehaviour] Client {connectionToClient.connectionId} requested snapshots from all clients.");

            // Send snapshot request to all other clients
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn == connectionToClient)
                {
                    continue; // Don't send to requesting client
                }

                // Find the RelayServerBehaviour for this connection
                if (conn.identity != null)
                {
                    RelayServerBehaviour behaviour = conn.identity.GetComponent<RelayServerBehaviour>();
                    if (behaviour != null)
                    {
                        behaviour.TargetRequestSnapshot(conn);
                    }
                }
            }
        }

        [Command]
        public void CmdSendSnapshotResponse(SnapshotResponse response)
        {
            if (!isServer)
            {
                return;
            }

            Debug.Log($"[RelayServerBehaviour] Client {connectionToClient.connectionId} sent snapshot with {response.objects.Count} objects.");

            // Relay snapshot to all other clients
            RpcReceiveSnapshotResponse(response);
        }

        #endregion

        #region RPCs to Clients

        [ClientRpc]
        private void RpcReceiveRoomObjectDelta(RoomObjectStateDelta delta)
        {
            // All clients receive this, they'll filter by authority in their own code
        }

        [ClientRpc]
        private void RpcReceiveSnapshotResponse(SnapshotResponse response)
        {
            // All clients receive this, they'll process if relevant
        }

        [TargetRpc]
        private void TargetRequestSnapshot(NetworkConnection target)
        {
            // This client should send a snapshot response
            // The client's ClientRoomPublisher will handle this
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up spawned objects when player disconnects
            if (isServer)
            {
                foreach (var netId in spawnedObjectsByStableId.Values)
                {
                    if (netId != null && netId.gameObject != null)
                    {
                        NetworkServer.Destroy(netId.gameObject);
                    }
                }
                spawnedObjectsByStableId.Clear();
            }
        }
    }
}
