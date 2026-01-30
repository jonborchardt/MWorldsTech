Place your networked prefabs here.

Each prefab should have:
- NetworkIdentity component
- StableId component
- ClientAuthorityTransformSync component (for objects that need transform sync)

For player prefabs, also add:
- RelayServerBehaviour (server builds only)
- ClientPlayerController (client builds only)
- ClientRoomPublisher (client builds only)

Register all prefabs in the GameNetPrefabRegistry ScriptableObject with unique prefabIds.
