# SimpleNet - Minimal 2-Player Listen-Server Multiplayer

SimpleNet is a minimal, direct-IP multiplayer system for Unity using Mirror networking. It supports exactly 2 players in a listen-server architecture where the host is authoritative.

## What This Is

- **Listen-server**: The host player runs both server and client
- **Direct IP connection**: No relay, no lobby, just IP:port
- **2 players maximum**: Host + 1 client
- **Host authoritative**: All gameplay state lives on the host
- **Session ends if host leaves**: No host migration

## Limitations

This is intentionally minimal and casual:

- No host migration
- No anti-cheat
- No lobby/matchmaking
- No late-join reconciliation (players appear when they connect)
- No lag compensation or prediction
- Session ends when host disconnects

## Required Packages

### Mirror Networking (Required)

Mirror is the networking framework used by SimpleNet. Install it via:

### Option 1: Unity Asset Store

1. Open Unity Asset Store in browser
2. Search for "Mirror Networking"
3. Import into your project

### Option 2: Git URL (Package Manager)

1. Open Window > Package Manager
2. Click the "+" button
3. Select "Add package from git URL"
4. Enter: `https://github.com/MirrorNetworking/Mirror.git?path=/Assets/Mirror`

### Option 3: Unity Registry

1. Window > Package Manager
2. Search for "Mirror" in Unity Registry (if available)

### Unity Input System (Required)

SimpleNet uses the new Unity Input System for player input. To set it up:

1. Open **Window > Package Manager**
2. Search for **"Input System"** and install it (if not already installed)
3. When prompted, allow Unity to restart and switch to the new Input System
4. Alternatively, manually set it in **Edit > Project Settings > Player > Active Input Handling** to **"Input System Package (New)"** or **"Both"**

**Note**: SimpleNet reads input directly from `Keyboard.current` and `Mouse.current` - no Input Actions asset setup is required.

## Scene Setup

**Before you begin**: Make sure Unity has compiled all six SimpleNet scripts (check that there are no errors in the Console). The scripts must compile successfully before you can add them as components.

### Step 1: Create the Player Prefab

1. Create a new GameObject in your scene (GameObject > Create Empty)
2. Name it "Player"
3. Add a visual representation:
   - Add a Cube child (GameObject > 3D Object > Cube)
   - Scale it to (0.5, 1.8, 0.5) to represent a character
4. Add the following components to the Player root (click "Add Component" in Inspector and search for each):
   - **NetworkIdentity** (from Mirror - must be added first)
   - **PlayerInput** (SimpleNet script)
   - **PlayerMotor** (SimpleNet script)
   - **NetworkTransformSync** (SimpleNet script)
   - **SimpleSmoothing** (SimpleNet script)
5. Drag the Player GameObject from Hierarchy into Assets to create a prefab
6. Delete the Player GameObject from the scene (it will be spawned by NetworkWorld)

### Step 2: Create the Bootstrap GameObject

1. Create an empty GameObject in your scene (GameObject > Create Empty)
2. Name it "NetworkBootstrap"
3. Add the following components (use "Add Component" button in Inspector and search for each):
   - **NetworkRunner** - This is the custom script from SimpleNet (extends Mirror's NetworkManager)
   - **NetworkWorld** - This is the custom script from SimpleNet (manages player spawning)
   - **TelepathyTransport** or **Kcp Transport** - Choose one from Mirror's transport options

### Step 3: Configure NetworkRunner

On the NetworkRunner component:

1. Set **Default Address** to `127.0.0.1` (for local testing)
2. Drag the **NetworkWorld** component into the **World** reference field
3. Under NetworkManager settings (base class):
   - Set **Network Address** to `localhost`
   - Set **Max Connections** to `2`
   - Leave **Auto Create Player** unchecked (NetworkWorld handles this)

**Note**: The `port` field on NetworkRunner is for reference only. The actual port is configured on the Transport component (see Step 6).

### Step 4: Configure NetworkWorld

On the NetworkWorld component:

1. Drag your **Player prefab** into the **Player Prefab** field
2. (Optional) Create spawn points:
   - Create 2 empty GameObjects in the scene
   - Position them where you want players to spawn (e.g., (0,0,0) and (5,0,0))
   - Drag them into the **Spawn Points** array
3. Set **Max Players** to `2`

### Step 5: Register the Player Prefab with Mirror

1. Select the NetworkBootstrap GameObject
2. In the NetworkRunner component, find the **Spawnable Prefabs** list (from NetworkManager)
3. Expand the list and add your Player prefab to it
   - This is required for Mirror to spawn the prefab over the network

### Step 6: Configure Transport

On the TelepathyTransport component (or KCP Transport if you chose that instead):

1. Set **Port** to `7777` (or any port you prefer)
2. Leave other settings at default

**Important**: This is where the actual network port is configured. Make sure this matches the port you tell clients to connect to.

## Running the Game

### Starting as Host

**Option 1: Using Context Menu (Quick Testing)**

1. Enter Play mode in Unity
2. In the Hierarchy, select the NetworkBootstrap GameObject
3. In the Inspector, find the NetworkRunner component
4. Right-click anywhere on the NetworkRunner component
5. From the context menu, select **"Start Host"**
6. You should see "NetworkRunner: Started host" in the Console

**Option 2: Using UI Button (Production)**

Create a UI button that calls:

```csharp
FindObjectOfType<NetworkRunner>().StartHostServer();
```

### Starting as Client

**Option 1: Using Context Menu (Quick Testing)**

1. Enter Play mode in Unity
2. In the Hierarchy, select the NetworkBootstrap GameObject
3. In the Inspector, find the NetworkRunner component
4. Make sure **Default Address** is set to the host's IP (or `127.0.0.1` for local testing)
5. Right-click anywhere on the NetworkRunner component
6. From the context menu, select **"Start Client (Default Address)"**
7. You should see "NetworkRunner: Connecting to..." in the Console

**Option 2: Using UI Buttons (Production)**

Create UI buttons that call:

```csharp
// Connect to default address
FindObjectOfType<NetworkRunner>().StartClientDefault();

// Connect to specific address
FindObjectOfType<NetworkRunner>().StartClientConnection("192.168.1.100");
```

### Stopping the Game

**Option 1: Using Context Menu**

1. While in Play mode, select the NetworkBootstrap GameObject
2. Right-click the NetworkRunner component
3. Select **"Shutdown"** from the context menu

**Option 2: Using UI Button**

```csharp
FindObjectOfType<NetworkRunner>().Shutdown();
```

**Option 3: Stop Play Mode**

Simply stop Play mode in Unity - this will clean up automatically.

## Local Testing (Same Machine)

To test with two players on the same computer:

### Method 1: Editor + Build (Recommended)

**IMPORTANT: Always start the HOST first, then the CLIENT**

1. Build your game (File > Build Settings > Build)
2. Start the built executable
3. In the build: Right-click NetworkRunner → **"Start Host"**
4. Wait for "NetworkRunner: Started host" to appear in the build's console/logs
5. Return to Unity Editor and enter Play mode
6. In the Editor: Set Default Address to `127.0.0.1`
7. In the Editor: Right-click NetworkRunner → **"Start Client (Default Address)"**
8. You should see both players spawn and be able to move around

**If you get "Connection Refused"**: Make sure step 3 completed successfully before starting the client.

### Method 2: Two Builds

**Note**: This method requires UI buttons (see "Quick UI Setup" below) since context menus don't work in built games.

1. Build your game twice to different folders (or just copy the build folder)
2. Start the first build and click "Start Host" button
3. Start the second build and click "Start Client" button
4. Both instances should connect and spawn players

**Note**: You cannot run two instances in the same Editor due to port conflicts.

### Quick UI Setup for Testing Builds

If you want to test with builds, add simple UI buttons:

1. In your scene: **GameObject > UI > Canvas** (creates Canvas + EventSystem)
2. Right-click Canvas > **UI > Button** - name it "HostButton"
3. Right-click Canvas > **UI > Button** - name it "ClientButton"
4. Position them on screen (e.g., top-left and top-right)
5. Change button text to "Start Host" and "Start Client"

6. Create a simple UI script `NetworkUI.cs`:

```csharp
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    public NetworkRunner networkRunner;

    public void OnHostButtonClicked()
    {
        networkRunner.StartHostServer();
    }

    public void OnClientButtonClicked()
    {
        networkRunner.StartClientDefault();
    }
}
```

7. Add `NetworkUI` component to Canvas
8. Drag NetworkRunner into the `networkRunner` field
9. For each button:
   - Select the button
   - In the Button component, find **OnClick()**
   - Click the "+" to add an event
   - Drag the Canvas (with NetworkUI) into the object field
   - Select `NetworkUI.OnHostButtonClicked()` or `OnClientButtonClicked()`

Now your builds will have clickable buttons to start host/client!

## Network LAN/Internet Testing

### LAN (Same Network)

**Step-by-step process:**

1. **Host Setup**:
   - Host finds their local IP address:
     - Windows: Open Command Prompt, type `ipconfig`, look for "IPv4 Address" (e.g., 192.168.1.100)
     - Mac: Open Terminal, type `ifconfig`, look for "inet" address
     - Linux: Open Terminal, type `ip addr` or `ifconfig`
   - Host starts their game and uses **"Start Host"** from context menu
   - Host verifies "NetworkRunner: Started host" appears in console
   - Host shares their IP address with the client player

2. **Client Setup**:
   - Client enters the host's IP in NetworkRunner's **Default Address** field (e.g., `192.168.1.100`)
   - Client starts their game and uses **"Start Client (Default Address)"** from context menu
   - Client should see "NetworkRunner: Connecting to..." in console

3. **Firewall**:
   - If connection fails, temporarily disable firewall on the host machine to test
   - If that works, re-enable firewall and allow port 7777 (UDP/TCP) through it

### Internet (Different Networks)

1. Host must port-forward their router (port 7777 UDP/TCP) to their PC's local IP
2. Host shares their public IP (search "what is my ip" on Google)
3. Client connects to the public IP
4. **Warning**: Exposing ports to the internet has security risks. Use at your own risk.

## Troubleshooting

### "Already running" Warning

- You called Start while already connected
- Call `Shutdown()` first, wait a frame, then start again

### Client Can't Connect: "Connection Refused" or "Target Machine Actively Refused It"

This error means the client cannot reach the host. Check these in order:

1. **Host must be running FIRST**
   - Start the host before starting the client
   - Verify "NetworkRunner: Started host" appears in the host's console
   - The host must be in Play mode and actively running

2. **Check the IP address**
   - For same machine: Use `127.0.0.1` (localhost)
   - For LAN: Host runs `ipconfig` (Windows) or `ifconfig` (Mac/Linux) to get their IP
   - Client uses the host's actual IP address (e.g., `192.168.1.100`)

3. **Check the port**
   - Both host and client must use the same port (default: 7777)
   - Verify the port in the Transport component on both machines

4. **Firewall**
   - Windows: Allow the Unity Editor and your game through Windows Defender Firewall
   - Mac: System Preferences > Security & Privacy > Firewall > Allow Unity
   - Temporarily disable antivirus to test if it's blocking the connection

5. **Router** (for internet play only)
   - Host must port-forward UDP/TCP port 7777 to their local IP
   - Use the host's public IP (search "what is my ip")

### Player Prefab Not Spawning

- Check that Player prefab has a **NetworkIdentity** component
- Verify **playerPrefab** is assigned in NetworkWorld
- Ensure Player prefab is in NetworkRunner's **Spawnable Prefabs** list
- Check Unity Console for errors

### Players Not Moving

- Ensure PlayerInput and PlayerMotor are both on the prefab
- Verify PlayerInput is sending commands (check console logs if you add debug output)
- Check that the host/server is receiving input and simulating movement in PlayerMotor

### Jittery Movement on Client

- Increase **positionLerpSpeed** and **rotationLerpSpeed** in SimpleSmoothing
- Or decrease **Sync Interval** in NetworkTransformSync (inherited from NetworkBehaviour) for more frequent updates (uses more bandwidth)
  - Default: 0.05s (20Hz)
  - Lower values = smoother but more bandwidth

### Multiple NetworkRunner Errors

- Only one NetworkRunner should exist in the scene
- Check you haven't duplicated the NetworkBootstrap GameObject

### "NetworkWorld reference is missing" Error

- Ensure the NetworkWorld component is assigned in NetworkRunner's **World** field

### Port Configuration Issues

- The network port is configured on the **Transport component** (TelepathyTransport or KCP), not on NetworkRunner
- The `port` field on NetworkRunner is for reference/documentation only
- Make sure the Transport port matches what clients use to connect

### Input System Error: "You are trying to read Input using the UnityEngine.Input class"

This means your project isn't configured for the new Input System:

1. Open **Edit > Project Settings > Player**
2. Scroll down to **Active Input Handling**
3. Change it to **"Input System Package (New)"** or **"Both"**
4. Allow Unity to restart when prompted
5. If the Input System package isn't installed, install it via Window > Package Manager first

## Architecture Overview

The six scripts work together as follows:

1. **NetworkRunner**: Entry point. Extends Mirror's NetworkManager. Handles starting/stopping host/client and delegates spawning to NetworkWorld.

2. **NetworkWorld**: Manages player spawning and despawning. Maintains spawn points and player limits.

3. **PlayerInput**: Runs on the local player only. Captures WASD and mouse input, sends it to the server via Mirror Commands.

4. **PlayerMotor**: Runs on the server only. Receives input from PlayerInput and simulates movement authoritatively.

5. **NetworkTransformSync**: Syncs the authoritative transform from server to all clients using Mirror SyncVars.

6. **SimpleSmoothing**: Runs on clients for non-owned players. Interpolates position/rotation for smooth remote player movement.

## Controls

Default keyboard and mouse controls (no configuration needed):

- **W/A/S/D**: Move forward/left/back/right
- **Mouse Movement**: Look around (first-person camera)
- **Left Shift** (hold): Sprint

Mouse sensitivity can be adjusted in the **mouseSensitivity** field on PlayerMotor (default: 0.1)

## Extending the System

This is a minimal foundation. You can extend it by:

- Adding shooting/interaction systems (use Commands for client→server, ClientRpc or SyncVars for server→client)
- Adding UI for IP input, player lists, etc.
- Adding a simple chat system
- Adding game rules and win conditions
- Spawning additional networked objects (ensure they have NetworkIdentity and are in Spawnable Prefabs)

Remember: always send player actions to the server, simulate on server, and replicate results to clients.

## Technical Notes

### Input Flow

1. Local player presses WASD/moves mouse
2. PlayerInput captures input and sends Command to server (20Hz)
3. Server receives Command and calls PlayerMotor.ProcessInput()
4. PlayerMotor simulates movement on server (Update loop)
5. NetworkTransformSync reads transform and updates SyncVars (20Hz)
6. SyncVars replicate to all clients
7. Local player: SyncVar hooks apply position directly (has network lag)
8. Remote players: SimpleSmoothing interpolates towards target (smooth)

### Server Authority

All gameplay logic runs on the server:

- Movement simulation happens on the host
- Position/rotation are replicated to clients
- Clients render the replicated state
- Local players see their own state with ~RTT latency
- Remote players see smoothed interpolated state

This prevents cheating and keeps logic simple, at the cost of input latency for the local player.
