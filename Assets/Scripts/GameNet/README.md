# GameNet

GameNet is a lightweight multiplayer library for Unity that provides client-authoritative room hosting over a dumb relay server. Designed for casual, friends-only multiplayer where each client owns their room and visitors can join to see a mirrored view.

## Architecture 

**Core Principles:**
- **Always a dedicated server**: No host mode or listen server. The server is a separate process.
- **Dumb relay server**: The server does NOT simulate gameplay, load room files, or interpret game objects beyond prefab IDs and stable object IDs.
- **Client-authoritative rooms**: Each client is authoritative for the objects in their own room. Other clients can visit and receive mirrored updates.
- **Server responsibilities**: Accept connections, track clients, relay messages, and server-spawn network objects while assigning authority to the requesting client.
- **Room persistence**: Clients load their room from a local JSON save file. The file is NOT transferred to the server. Clients send spawn requests and state updates derived from it.

**Network Flow:**
1. Server starts and listens for connections
2. Client connects and loads local room file
3. Client sends spawn requests to server for each room object
4. Server spawns objects and assigns authority to the client
5. Other clients receive spawn notifications and create mirrored objects locally
6. Late-joining clients request snapshots from existing clients to catch up

## Folder Structure

```
Assets/Scripts/GameNet/
├── Shared/
│   ├── GameNet.Shared.asmdef      (Assembly definition for shared code)
│   ├── StableId.cs                (Persistent object identifier)
│   ├── NetPrefabRegistry.cs       (Prefab ID to GameObject mapping)
│   ├── NetMessages.cs             (Network message structures)
│   └── SimpleJsonRoomFile.cs      (Room save file data structures)
├── Client/
│   ├── GameNet.Client.asmdef      (Assembly definition for client code)
│   ├── ClientBootstrap.cs         (Client entry point and connection)
│   ├── RoomFileLoader.cs          (Load/save room JSON files)
│   ├── OfflineRoomManager.cs      (Offline room loading, scene GameObject)
│   ├── ClientRoomPublisher.cs     (Online room publishing, on player)
│   ├── ClientPlayerController.cs  (Networked player movement, requires Mirror)
│   ├── OfflinePlayerController.cs (Offline player movement, no Mirror needed)
│   ├── ClientAuthorityTransformSync.cs (Custom transform sync)
│   ├── OfflinePlayerSpawner.cs    (Offline single-player mode)
│   └── GameNetDebugUI.cs          (Debug UI with Connect/Disconnect buttons)
├── Server/
│   ├── GameNet.Server.asmdef      (Assembly definition for server code)
│   ├── RelayServerNetworkManager.cs (NetworkManager subclass)
│   ├── RelayServerBehaviour.cs      (Handles Commands/RPCs)
│   └── ServerBootstrap.cs           (Auto-start server in headless mode)
├── Prefabs/                       (Networked prefabs - Cube, NetworkedPlayer, OfflinePlayer)
└── README.md (this file)
```

## Assembly Definitions

Three assemblies are defined:

1. **GameNet.Shared** (references: Mirror)
   - Shared data structures and messages
   - No dependencies on client or server code
   - File: [Shared/GameNet.Shared.asmdef](Shared/GameNet.Shared.asmdef)

2. **GameNet.Client** (references: GameNet.Shared, Mirror, Unity.InputSystem)
   - Client-specific bootstrap, room loading, and input handling
   - Requires Unity Input System package (com.unity.inputsystem)
   - Uses new Input System API with direct device polling fallback (`Keyboard.current`, `Mouse.current`)
   - Includes version define for ENABLE_INPUT_SYSTEM when Input System package is present
   - File: [Client/GameNet.Client.asmdef](Client/GameNet.Client.asmdef)

3. **GameNet.Server** (references: GameNet.Shared, Mirror)
   - Server-specific relay logic and object spawning
   - File: [Server/GameNet.Server.asmdef](Server/GameNet.Server.asmdef)

## Scene Setup

### Step 1: Create or Use Prefab Registry

**Important:** Wait for Unity to finish compiling the GameNet scripts before proceeding. Check the bottom-right corner of Unity Editor for "Compiling..." to complete.

A prefab registry already exists at [Assets/Scripts/GameNet/GameNetPrefabRegistry.asset](GameNetPrefabRegistry.asset) with these prefabs registered:
- "NetworkedPlayer" (NetworkedPlayer prefab)
- "Cube" (Cube prefab)

**To create a new registry or modify existing:**

1. In Unity, right-click in the Project window
2. Create > GameNet > Prefab Registry
3. Name it `GameNetPrefabRegistry`
4. Add your networked prefabs to the registry:
   - Set a unique `prefabId` string for each prefab (e.g., "Cube", "Player", "Tree")
   - Assign the prefab GameObject reference

### Step 2: Create Player Prefabs

**Existing Prefabs:**
The project already includes pre-configured prefabs in [Assets/Scripts/GameNet/Prefabs/](Prefabs/):
- **NetworkedPlayer.prefab** - Networked player with all required components
- **OfflinePlayer.prefab** - Offline player for single-player mode
- **Cube.prefab** - Example networked room object

You can use these as-is or create your own following the instructions below.

**Networked Player Prefab (for online mode):**

1. Create a GameObject in the scene (e.g., "NetworkedPlayer")
2. Add the following components:
   - NetworkIdentity (from Mirror)
   - StableId (from GameNet.Shared)
   - RelayServerBehaviour (from GameNet.Server) - **Server builds only**
   - ClientPlayerController (from GameNet.Client) - **Client builds only**
   - ClientAuthorityTransformSync (from GameNet.Client) - **Client builds only**
   - **ClientRoomPublisher** (from GameNet.Client) - **Client builds only**
3. Configure ClientRoomPublisher on the prefab:
   - Assign "Room Loader" reference (will be set at runtime from scene)
   - Assign "Prefab Registry" reference (will be set at runtime from scene)
4. Optionally add a visual representation (Capsule, Cube, etc.)
5. Save as a prefab in your Prefabs folder (e.g., "NetworkedPlayer")
6. Delete the instance from the scene

**Note:** For online mode, ClientRoomPublisher is on the player prefab. For offline mode, OfflineRoomManager (on the ClientBootstrap scene GameObject) handles room loading. This separation allows both modes to work correctly.

**Offline Player Prefab (for single-player mode, optional):**

1. Create a GameObject in the scene (e.g., "OfflinePlayer")
2. Add the following components:
   - **OfflinePlayerController** (for WASD movement) OR your own movement script
   - Camera (if first-person) or attach a Camera as child
   - Visual representation (Capsule, Cube, etc.)
   - Optional: CharacterController (if you want gravity/collisions)
   - **Do NOT add** NetworkIdentity, Mirror components, ClientPlayerController, or any GameNet.Server components
3. Save as a prefab (e.g., "OfflinePlayer")
4. Delete the instance from the scene
5. This prefab will be used by OfflinePlayerSpawner

**Important:** Use `OfflinePlayerController` for offline players, NOT `ClientPlayerController`. ClientPlayerController requires NetworkIdentity and is only for networked players.

### Step 3: Create Room Object Prefabs

For each type of object you want to spawn in rooms:

1. Create a GameObject (e.g., "RoomCube")
2. Add the following components:
   - NetworkIdentity
   - StableId
   - ClientAuthorityTransformSync
3. Add visual components (MeshRenderer, etc.)
4. Save as a prefab
5. Add to the GameNetPrefabRegistry with a unique prefabId

### Step 4: Setup Server Scene

1. Create a new scene (e.g., "ServerScene")
2. Create an empty GameObject named "NetworkManager"
3. Add the following components:
   - RelayServerNetworkManager (from GameNet.Server)
   - Transport component (Telepathy or KCP from Mirror)
4. Configure RelayServerNetworkManager:
   - Set "Player Prefab" to your NetworkedPlayer prefab
   - Assign the GameNetPrefabRegistry reference
5. Create an empty GameObject named "ServerBootstrap"
6. Add the ServerBootstrap component
7. Assign the NetworkManager reference
8. Configure ServerBootstrap:
   - Enable "Auto Start In Batch Mode" for dedicated server builds
   - Optionally enable "Auto Start In Editor" for testing

### Step 5: Setup Client Scene

1. Create a new scene (e.g., "ClientScene")
2. Create an empty GameObject named "NetworkManager"
3. Add the following components:
   - NetworkManager (standard Mirror NetworkManager, or use RelayServerNetworkManager)
   - Transport component (must match server transport)
4. Configure NetworkManager:
   - Set "Player Prefab" to your NetworkedPlayer prefab
5. Create an empty GameObject named "ClientBootstrap"
6. Add the following components:
   - ClientBootstrap
   - RoomFileLoader
   - **OfflineRoomManager** (for offline mode support)
7. Configure ClientBootstrap:
   - Set "Server Address" (e.g., "127.0.0.1" for local testing)
   - Set "Server Port" (must match transport port, default 7777)
   - Assign "Prefab Registry" reference
   - Assign "Room Loader" reference (drag the same GameObject)
   - Assign "Network Manager" reference
8. Configure RoomFileLoader:
   - Set "Room File Name" (e.g., "MyRoom.json")
9. Configure OfflineRoomManager:
   - Assign "Room Loader" reference (drag the same GameObject)
   - Assign "Prefab Registry" reference
   - Check "Load Room On Start" to auto-load room in offline mode

### Step 6: Configure Transport Port

On both server and client NetworkManager objects:

1. Select the Transport component (Telepathy or KCP)
2. Set the same port on both (default: 7777)
3. For Telepathy:
   - Server: Port = 7777
   - Client: Port = 7777
4. For KCP:
   - Port = 7777

### Optional: Debug UI Panel

Add a simple UI panel with Connect/Disconnect buttons for easy testing:

1. In your client scene, create a Canvas if you don't have one:
   - Right-click Hierarchy → UI → Canvas
   - Set Canvas Scaler to "Scale With Screen Size" (recommended)

2. Create a panel for the debug UI:
   - Right-click Canvas → UI → Panel
   - Name it "GameNetDebugPanel"
   - Position it where you want (e.g., top-right corner)

3. Add UI elements to the panel:
   - **Connect Button**: Right-click panel → UI → Button, name it "ConnectButton"
     - Change button text to "Connect to Server"
   - **Disconnect Button**: Right-click panel → UI → Button, name it "DisconnectButton"
     - Change button text to "Disconnect"
   - **Status Text**: Right-click panel → UI → Text, name it "StatusText"
     - Change text to "Status: Offline"

4. Add the GameNetDebugUI component to the panel:
   - Select "GameNetDebugPanel"
   - Add Component → GameNet.Client → GameNetDebugUI

5. Configure GameNetDebugUI:
   - Assign "Client Bootstrap" reference
   - Assign "Connect Button" reference
   - Assign "Disconnect Button" reference
   - Assign "Status Text" reference
   - Optional: Check "Hide On Connect" to hide panel when connected

6. Test it:
   - Press Play
   - Click "Connect to Server" button
   - Status should change to "Connected" (if server is running)

## Offline Mode (Single-Player)

GameNet supports offline single-player mode where you can load and interact with your room without a server.

### How Offline Mode Works

1. **Client starts without server connection**
2. **Room loads locally** from JSON file
3. **Objects spawn locally** (no network involved)
4. **Optional offline player** spawns for movement/interaction
5. **Later: Click a button to connect** to server and share with friends

### Setting Up Offline Mode

**Step 1: Configure OfflineRoomManager**

On your ClientBootstrap GameObject:
1. Check "Load Room On Start" in OfflineRoomManager component
2. This will automatically load and spawn your room offline when the client starts
3. Ensure Room Loader and Prefab Registry references are assigned

**Step 2: Add Offline Player (Optional)**

1. Create a new GameObject named "OfflinePlayerSpawner"
2. Add the `OfflinePlayerSpawner` component
3. Assign an offline player prefab (should have movement script, camera, etc.)
   - This prefab does NOT need NetworkIdentity or any Mirror components
   - Just use regular Unity components (CharacterController, Camera, etc.)
4. Set spawn position
5. Check "Spawn On Start"

**Step 3: Test Offline Mode**

1. Open your client scene
2. Press Play (do NOT start server)
3. Your room should load and spawn objects
4. If configured, your offline player spawns and you can move around

### Transitioning from Offline to Online

**To connect to server after starting offline:**

1. Select the ClientBootstrap GameObject in Hierarchy
2. Right-click on ClientBootstrap component
3. Click **"Connect to Server"**
4. The offline player is destroyed
5. Room objects are registered with the network
6. Networked player spawns
7. You're now sharing your room with connected friends

**Or programmatically:**
```csharp
FindFirstObjectByType<ClientBootstrap>().Connect();
```

**Or via UI button:**
Use the built-in GameNetDebugUI component (see "Optional: Debug UI Panel" below)

### What Happens During Transition

1. OfflineRoomManager cleans up offline room objects
2. Offline player is destroyed
3. Client connects to server
4. Server spawns networked player with ClientRoomPublisher component
5. ClientRoomPublisher (on player) sends spawn requests for your room objects
6. Server spawns room objects with you as authority
7. Other connected clients receive your room objects
8. You receive snapshots of other clients' rooms

## Running Locally

### Method 1: Offline Single-Player (No Server)

1. Open your client scene in Unity
2. Press Play
3. Your room loads automatically
4. Move around with offline player (if configured)
5. To connect later: Right-click ClientBootstrap → "Connect to Server"

### Method 2: Two Builds

1. Build your game twice to different folders:
   - First build: Server build with ServerScene as the only scene in Build Settings
   - Second build: Client build with ClientScene as the only scene in Build Settings

2. Start the server build:
   - Run the executable with `-batchmode -nographics` command line args
   - Or run normally and call `ServerBootstrap.StartServer()` via a UI button

3. Start the client build:
   - Run the executable normally
   - Call `ClientBootstrap.Connect()` via a UI button or automatically on Start

### Method 2: Editor Testing

1. Open ServerScene in the Unity Editor
2. Play the scene (server will auto-start if configured)
3. Build a client executable and run it, pointing to `127.0.0.1`
4. Or: Open a second Unity Editor instance with ClientScene and connect

### Method 3: Headless Server

Run the server build with Unity's headless mode:

```bash
# Windows
.\ServerBuild.exe -batchmode -nographics -logFile server.log

# Linux
./ServerBuild.x86_64 -batchmode -nographics -logFile server.log

# macOS
./ServerBuild.app/Contents/MacOS/ServerBuild -batchmode -nographics -logFile server.log
```

The server will auto-start if `ServerBootstrap.autoStartInBatchMode` is enabled.

## Room File Format

Room files are JSON files stored locally on each client at:
- `Application.persistentDataPath/[RoomFileName].json`
- On Windows: `%USERPROFILE%\AppData\LocalLow\[CompanyName]\[ProductName]\MyRoom.json`
- On Linux: `~/.config/unity3d/[CompanyName]/[ProductName]/MyRoom.json`
- On macOS: `~/Library/Application Support/[CompanyName]/[ProductName]/MyRoom.json`

### Example Room File

```json
{
  "roomName": "My Awesome Room",
  "objects": [
    {
      "stableId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "prefabId": "Cube",
      "posX": 0.0,
      "posY": 1.0,
      "posZ": 0.0,
      "rotX": 0.0,
      "rotY": 45.0,
      "rotZ": 0.0,
      "stateJson": ""
    },
    {
      "stableId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "prefabId": "Tree",
      "posX": 5.0,
      "posY": 0.0,
      "posZ": 3.0,
      "rotX": 0.0,
      "rotY": 0.0,
      "rotZ": 0.0,
      "stateJson": "{\"health\": 100}"
    }
  ]
}
```

### Creating a Room File

**Option 1: Use the built-in sample generator (easiest)**

1. In your client scene, select the GameObject with the RoomFileLoader component
2. In the Inspector, right-click on the RoomFileLoader component header
3. Select **"Create Sample Room File"** from the context menu
4. Check the Unity Console for the file path where it was saved
5. Edit the generated JSON file to add your own objects

**Option 2: Manually create the JSON file**

1. Navigate to your persistentDataPath folder (see "Room File Format" section above for location)
2. Create a new file matching your configured roomFileName (e.g., `MyRoom.json`)
3. Copy the example JSON format from below and customize it

**Option 3: Create a custom editor tool**

Create an editor script to export scene objects to room format (advanced)

### Room File Fields

- `roomName`: Display name for the room
- `objects`: Array of room objects
  - `stableId`: Unique GUID for this object (must be unique across all objects)
  - `prefabId`: Must match an entry in GameNetPrefabRegistry
  - `posX, posY, posZ`: World position
  - `rotX, rotY, rotZ`: Euler rotation angles
  - `stateJson`: Optional JSON blob for custom object state (can be empty string)

## PlayFlow Integration

PlayFlow is a cloud hosting service for Unity servers. GameNet is designed to work with PlayFlow's dedicated server model.

### Build Target for PlayFlow

1. Use Unity's **Linux Dedicated Server** build target (recommended)
2. Or use **Windows Server** if Linux is not available
3. Enable **Headless Mode** (no graphics)

### Build Settings

1. Open **File > Build Settings** (or press Ctrl+Shift+B / Cmd+Shift+B)
2. In the Build Settings window:
   - **For Unity 6+**: Switch platform to "Dedicated Server"
   - **For Unity 2021-2023**: Switch to "Linux" and check "Server Build" checkbox
   - **For Windows Server**: Switch to "Windows" and check "Server Build" checkbox
3. In "Scenes In Build" section:
   - Click "Add Open Scenes" to add your ServerScene
   - Remove any client-only scenes from the list
   - ServerScene should be index 0
4. Click the **"Build"** button (NOT "Build And Run")
5. Choose output folder in the file dialog (e.g., `Builds/Server/`)
6. Wait for build to complete

### PlayFlow Configuration

GameNet requires minimal PlayFlow configuration:

**Port Configuration:**
- GameNet uses the port configured on the Mirror Transport component (default: 7777)
- Expose this port in PlayFlow's container/network settings
- Use UDP for KCP transport or TCP for Telepathy

**Command Line Arguments:**
- PlayFlow should launch with: `-batchmode -nographics -logFile /path/to/server.log`
- GameNet's ServerBootstrap will auto-detect batch mode and start the server

**Environment Variables:**
- None required by default
- Optionally set custom variables and read them in ServerBootstrap if needed

**Health Check:**
- Monitor Unity's log output for `[ServerBootstrap] Server started successfully.`
- Or implement a simple HTTP health check endpoint if needed

### PlayFlow Deployment Checklist

1. Build server with Linux Dedicated Server target
2. Ensure ServerBootstrap.autoStartInBatchMode is enabled
3. Verify Transport port matches PlayFlow port mapping
4. Upload build to PlayFlow
5. Configure PlayFlow to expose the correct port (7777 by default)
6. Set command line args: `-batchmode -nographics`
7. Monitor logs for successful server start
8. Test client connection with PlayFlow's assigned IP/domain

## How It Works

### Client-Authoritative Spawning

1. Client loads room file via RoomFileLoader
2. Client connects to server via ClientBootstrap
3. ClientRoomPublisher sends SpawnRoomObjectRequest for each object
4. Server receives requests in RelayServerBehaviour
5. Server spawns objects with NetworkServer.Spawn(obj, ownerConnection)
6. All connected clients receive spawn notification from Mirror
7. Clients create local mirrored copies of remote objects

### Transform Synchronization

ClientAuthorityTransformSync provides custom transform sync:

- **Owner**: Sends position/rotation at fixed rate (10 Hz default) via Command
- **Server**: Relays via ClientRpc to all other clients
- **Non-owners**: Interpolate toward target position/rotation smoothly

This is more efficient than Mirror's NetworkTransform for client-authoritative objects.

### Late Join / Snapshots

When a new client connects after others:

1. New client sends SnapshotRequest to server via CmdRequestSnapshotFromAll
2. Server relays TargetRequestSnapshot to all existing clients
3. Existing clients respond with SnapshotResponse containing their room objects
4. Server relays responses via RpcReceiveSnapshotResponse
5. New client creates local copies of all received objects

### Player Movement

GameNet provides two player controllers for different scenarios:

**OfflinePlayerController** (for offline/single-player mode):
- Simple WASD movement without network dependencies
- Does NOT require NetworkIdentity or Mirror components
- Use this for your offline player prefab
- Optional CharacterController support for gravity/collisions
- Uses Unity's new Input System with fallback to direct device polling (`Keyboard.current`, `Mouse.current`)
- **Movement**: W/S (forward/backward), A/D (strafe left/right)
- **Look**: Mouse movement (horizontal: rotate body, vertical: pitch camera)
- Mouse cursor locked by default when active

**ClientPlayerController** (for online/networked mode):
- Networked WASD movement (requires NetworkIdentity)
- Movement is client-authoritative (happens locally first)
- ClientAuthorityTransformSync sends updates to server for relay
- Use this for your networked player prefab
- Uses Unity's new Input System with fallback to direct device polling (`Keyboard.current`, `Mouse.current`)
- **Movement**: W/S (forward/backward), A/D (strafe left/right)
- **Look**: Mouse movement (horizontal: rotate body, vertical: pitch camera)
- Mouse cursor locked by default when active

**Input System Details:**
- Both controllers use Unity's new Input System package (com.unity.inputsystem)
- Can receive input via Input Actions (OnMove, OnLook callback methods)
- Falls back to direct keyboard/mouse polling if no Input Actions are configured
- Does NOT support the legacy Input Manager (Input.GetAxis)

## Features

**Core Capabilities:**
- ✅ Offline single-player mode (play without server)
- ✅ Seamless transition from offline to online
- ✅ Client-authoritative room hosting
- ✅ Multiple clients can visit each other's rooms
- ✅ Late join support with snapshot synchronization
- ✅ JSON-based room persistence (local files)
- ✅ Debug UI with Connect/Disconnect buttons
- ✅ Context menu helpers for testing
- ✅ Uses Unity's new Input System with direct device polling fallback
- ✅ First-person mouse look with pitch clamping and cursor locking

## Limitations

**By Design:**
- No NAT traversal or matchmaking (use direct IP or PlayFlow for hosting)
- No host migration (if server dies, all clients disconnect)
- No anti-cheat beyond basic authority validation
- No advanced prediction or reconciliation
- No built-in persistence on server (rooms exist only in client memory/files)
- No automatic file transfer of room files to server
- No physics synchronization (add custom components if needed)
- No voice chat or advanced features

**Technical:**
- Room files must be created manually or via custom tooling
- All networked prefabs must be registered in NetPrefabRegistry
- All networked objects must have NetworkIdentity, StableId, and appropriate sync components
- Transport must be configured identically on client and server
- Late join depends on existing clients responding to snapshot requests (not guaranteed if clients are buggy)

**Best Practices:**
- Keep room files small (< 1000 objects recommended)
- Use meaningful prefabIds (avoid duplicates)
- Ensure stable IDs are truly unique (use System.Guid.NewGuid())
- Test with 2-4 clients initially before scaling up
- Monitor server logs for authority violations
- Use PlayFlow or similar hosting for public servers (don't expose home IP)

## Troubleshooting

### Client can't connect to server

- Verify server is running (check logs for "Server started successfully")
- Verify IP address and port are correct
- Verify transport type matches (both using Telepathy or both using KCP)
- Check firewall rules (allow port 7777 or your configured port)
- For PlayFlow: verify port mapping is correct

### Objects not spawning

- Check that prefabId exists in NetPrefabRegistry
- Verify all prefabs have NetworkIdentity component
- Check server logs for spawn errors
- Verify room file is loaded (check RoomFileLoader logs)

### Late join not working

- Verify existing clients have ClientRoomPublisher component
- Check that snapshot request/response is being sent (check logs)
- Verify prefabs match on all clients

### Transform sync not working

- Verify ClientAuthorityTransformSync is on the object
- Check that NetworkIdentity has authority assigned correctly
- Increase sendRate if updates seem too slow
- Check network latency (Mirror stats)

### Server build won't start

- Verify ServerBootstrap.autoStartInBatchMode is enabled
- Check that RelayServerNetworkManager is in the scene
- Run with `-logFile` to see Unity logs
- Verify all asmdefs compiled correctly

## Migration from SimpleNet

If migrating from the existing SimpleNet library:

1. GameNet uses a **dedicated server model** instead of host mode
2. Room files are JSON instead of binary (convert your existing saves)
3. Use GameNetPrefabRegistry instead of SimpleNet's prefab system
4. Replace SimpleNet components with GameNet equivalents:
   - SimpleNetManager → RelayServerNetworkManager (server) or NetworkManager (client)
   - SimpleNetObject → ClientAuthorityTransformSync + StableId
   - SimpleNetPlayer → ClientPlayerController + RelayServerBehaviour
5. Update network calls to use Mirror's Command/ClientRpc pattern

## License

GameNet is part of the MWorldsTech project. See repository license for details.

## Support

For issues or questions:
- Check Unity console logs (both client and server)
- Enable Mirror's Network Visibility debugging
- Review this README's Troubleshooting section
- Check project constitution and architecture docs in `.specify/memory/constitution.md`
