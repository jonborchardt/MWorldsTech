# MWorldsTech

## What this is
A Unity 6 game project with MCP (Model Context Protocol) integration, enabling AI assistants to directly interact with Unity Editor for automated development workflows.

## What this is not
A standalone game engine or a general-purpose Unity plugin. This is a game project that uses MCP Unity for AI-assisted development.

## Tech
- Unity 6.3LTS
- C# 9.0 (.NET 4.7.1)
- Universal Render Pipeline (URP)
- Unity Input System
- Node.js 18+ (for MCP server)
- Target platforms: Windows Standalone, Android/Mobile

In order to run, add the following asset packs from unity store or local into the correct folder as:

Assets

- / Asset Packs
  - / Synt/AnimationBaseLocomotion

Packages

- / Mirror (free in asset store)
- / PlayFlow (free in asset store)
- / TextMesh Pro (free package)

After installing asset packs, run window/render/render pipeline converter -> 'built in to urp' -> 'material upgrade'

## How to run
1. Open project in Unity 6 Editor
2. Start MCP server: **Tools > MCP Unity > Server Window** → "Start Server"
3. Open scene: **Assets/Scenes/DesktopRig.unity**
4. Press Play in Unity Editor

## Current Scene Layout
- **DesktopRig.unity**: Desktop FPS player development/testing scene

## PlayFlow + Mirror Quickstart (Internal Notes)

This document supplements the official PlayFlow Mirror quickstart.
Read the original walkthrough first, then follow the notes below for the real world details and fixes.

Official guide:
https://docs.playflowcloud.com/quickstart/mirror

---

### Assumptions

- You already have a Unity project using Mirror
- You are deploying a dedicated server via PlayFlow
- You are comfortable with Unity build profiles and scenes

---

### SDK Installation Notes

- The official guide installs PlayFlow via Git URL
- I installed PlayFlow via the Unity Asset Store instead
- Either approach works

You will need:
- A PlayFlow account
- A PlayFlow project created in the dashboard

---

### Build Profile Setup (Important)

PlayFlow server uploads require a **Server build profile**, not your normal client build.

What to do:
1. Open Build Profiles in Unity
2. Create a **Linux Server** profile
3. Assign:
   - Platform: Linux
   - Type: Server
   - Scene: your server scene only
4. Make sure this profile is selected before uploading via PlayFlow

If this is wrong, the PlayFlow upload will fail or the server will not boot correctly.

---

### Network Manager Configuration

Follow the official guide for:
- NetworkManager setup
- Auto Start Server Build enabled

Key clarification:
- The NetworkManager **does not point at port 7777**
- The NetworkManager points at the **server IP provided by PlayFlow**

---

### Transport Configuration

Mirror transport behavior depends on what you are using.

#### Telepathy (TCP)
- Use **TCP**
- Do **not** enable TLS
- This is fine for most non twitch games
- *We use this*

#### KCP (UDP)
- Use **UDP**
- Faster, better for action games

Important:
- Mirror internally still uses port **7777**
- PlayFlow maps this to an external port automatically

---

### PlayFlow Network Port Setup

In the PlayFlow dashboard:
1. Go to Configuration → Network Ports
2. Add a new port

For Telepathy:
- Protocol: TCP
- TLS: unchecked

For KCP:
- Protocol: UDP

Port Number:
- 7777

---

### Uploading the Server

In Unity:
1. Open PlayFlow Cloud window
2. Paste API key
3. Verify:
   - Server build profile is active
   - Server scene is selected
4. Click Upload Server

This builds and uploads the headless Linux server.

---

### Creating and Connecting to a Server

After creating a server in PlayFlow:
1. Open the server details
2. Copy:
   - Host (IP)
   - External Port

---

### Client Configuration

In client scenes:

- Find the **ClientBootstrap** object
- In the ClientBootstrap script:
  - Set Server Address to PlayFlow Host
  - Set Port to PlayFlow External Port

This is where the client actually connects.
Do not hardcode 7777 here.

---

### Testing the Game (Current Workflow)

Temporary testing flow:
1. Launch the game
2. Select the ClientBootstrap object
3. Right click the ClientBootstrap script
4. Choose **Connect to Server**

This manually connects the client to PlayFlow.

Note:
- There will be proper UI for this in the near future

---

### Common Gotchas

- Wrong build profile selected
- Server scene included in client build
- TLS accidentally enabled for Telepathy
- Using 7777 directly instead of PlayFlow external port
- Forgetting to copy the server IP from PlayFlow

---

### Debugging

- Check PlayFlow dashboard logs for server output
- Confirm transport type matches PlayFlow port config
- Ensure firewall is not blocking the connection

---

### References

Official PlayFlow Mirror guide:
https://docs.playflowcloud.com/quickstart/mirror

---

## Repo rules
- No commits to main
- PR required
- CI must be green

## AI usage
- Claude Code is allowed
- Spec Kit is the planning source of truth
