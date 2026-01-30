# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

For project governance principles, reference [.specify/memory/constitution.md](.specify/memory/constitution.md).

## Project Overview

MWorldsTech is a Unity 6 game project with integrated MCP (Model Context Protocol) support, enabling AI assistants to directly interact with Unity Editor. The architecture follows a two-tier design:

**AI Assistant** ⇄ (stdio) ⇄ **Node.js MCP Server** ⇄ (WebSocket) ⇄ **Unity Editor** ⇄ **Game Objects/Scenes**

## Setup Requirements

### Asset Store Packages (Required)
Install these packages before running the project:
1. **Mirror** - Free networking framework from Asset Store
2. **PlayFlow** - Free cloud services from Asset Store
3. **TextMesh Pro** - Free text rendering (Unity package)

After installation:
- **Window > Render Pipeline > URP Converter**: Run "Built-in to URP" → "Material Upgrade"

### Folder Structure for Assets
```
Assets/
└── Asset Packs/
    └── (Asset Store content goes here)
```

## Development Commands

### Unity Operations
- Open Unity Editor and work with the project directly
- **Tools > MCP Unity > Server Window**: Start/stop MCP server and configure settings
- **Window > General > Test Runner**: Run Unity Test Framework tests (EditMode/PlayMode)
- Default scenes: [ClientScene.unity](Assets/Scenes/ClientScene.unity) or [ServerScene.unity](Assets/Scenes/ServerScene.unity)

### MCP Node.js Server (Server~/Library/PackageCache/com.gamelovers.mcp-unity@*/Server~)
```bash
cd Library/PackageCache/com.gamelovers.mcp-unity@*/Server~
npm install          # Install dependencies
npm run build        # Compile TypeScript to build/
npm start            # Run MCP server
npm test             # Run tests
```

### MCP Tools Available
Use these via natural language prompts with AI assistants:
- **Menu & Selection**: `execute_menu_item`, `select_gameobject`
- **GameObject Query/Modify**: `get_gameobject`, `update_gameobject` (name, tag, layer, active state)
- **GameObject Hierarchy**: `duplicate_gameobject`, `delete_gameobject`, `reparent_gameobject`
- **Transform Operations**: `move_gameobject`, `rotate_gameobject`, `scale_gameobject`, `set_transform`
- **Components**: `update_component` (add/modify components on GameObjects)
- **Assets**: `create_prefab`, `add_asset_to_scene`, `add_package`
- **Scenes**: `create_scene`, `load_scene`, `save_scene`, `delete_scene`, `unload_scene`, `get_scene_info`
- **Materials**: `create_material`, `assign_material`, `modify_material`, `get_material_info`
- **Testing**: `run_tests` (EditMode/PlayMode via Unity Test Runner)
- **Scripts**: `recompile_scripts`
- **Logging**: `send_console_log`, `get_console_logs`
- **Batch Operations**: `batch_execute` (atomic multi-step operations with rollback)

### MCP Resources (read-only queries)
- `unity://menu-items`: List all available menu items
- `unity://scenes-hierarchy`: Get current scene hierarchy
- `unity://gameobject/{id}`: Get GameObject details with components
- `unity://logs`: Retrieve Unity console logs
- `unity://packages`: List installed packages
- `unity://assets`: Query AssetDatabase

## Architecture Principles (from Constitution)

### I. Scripts Over Inspector Magic
Behavior lives in C# code, not inspector values. No logic encoded via serialized booleans, UnityEvents, or event chains. If behavior matters, it should be readable in code.

### II. Data Is Separate From Behavior
ScriptableObjects are data containers only. No logic beyond validation. Runtime state does not live in assets.

### III. Explicit Wiring Beats Implicit Discovery
No `FindObjectOfType` in production code. No scene hierarchy order dependencies. Dependencies are passed explicitly or assigned at startup by a composition root.

### IV. Composition Over Inheritance
Prefer small components with single responsibility. Shallow inheritance trees. Interfaces over base classes.

### V. Scenes Are Configuration, Not Logic
Scenes assemble systems but don't define behavior. No gameplay logic in scene callbacks beyond delegation. Scene load order must not encode rules.

### VI. Lifecycle Is Explicit
Don't casually spread logic across Unity lifecycle methods (`Awake`, `Start`, `OnEnable`, `Update`). Prefer explicit `Initialize`, `Tick`, `Shutdown` methods. Centralize update loops.

### VII. Single Source of Truth
State lives in one place. No mirrored state across components. UI reflects state but doesn't own it.

### VIII. Names Should Explain the System
Optimize for reading, not typing. No abbreviations. Folder structure mirrors architectural boundaries (domain and feature first, Unity glue last).

### IX. Failure Should Be Loud
Silent failure is worse than a crash. Assert invariants early. Throw when assumptions violated. Log with intent, not spam.

### X. Unity Is the Host, Not the Architecture
Core logic in plain C# assemblies with minimal Unity dependencies. Unity-facing code is adapters and glue. Systems must be testable without Play Mode.

### XI. Modularity Over Convenience
No god objects. No cross-cutting singletons without justification. Systems communicate through narrow contracts.

### XII. Optimize for Change, Not Perfection
Make deletion easy. Prefer clarity over cleverness. Refactor early and often.

## Layered Architecture Constraints

Structure code in layers:
- **Core**: Plain C# domain logic (no `MonoBehaviour`, no scene knowledge)
- **Game**: Gameplay orchestration and state (minimal Unity dependencies)
- **UnityAdapters**: Input, physics, audio, rendering, scene/prefab glue

New gameplay logic starts in Core, then wires into Unity via adapters.

Avoid runtime reflection and stringly-typed lookups. No global service locators without written rationale.

## Project Structure

```
Assets/
├── Scenes/              # Game scenes
│   ├── ClientScene.unity    # Client-side scene
│   └── ServerScene.unity    # Server/host scene
├── Scripts/
│   ├── GameNet/         # Custom networking implementation
│   │   ├── Client/      # Client-side networking logic
│   │   ├── Server/      # Server-side networking logic
│   │   ├── Shared/      # Shared networking code
│   │   └── Prefabs/     # Network prefabs registry
│   └── SimpleNet/       # Alternative networking implementation
├── Settings/            # URP render pipeline configs (PC_RPAsset, Mobile_RPAsset)
│   └── Build Profiles/  # Build configuration profiles
├── Prefabs/             # Reusable game object prefabs
├── Resources/           # Runtime-loaded assets
├── Mirror/              # Mirror networking framework (Asset Store)
├── PlayFlowCloud/       # PlayFlow cloud services (Asset Store)
├── TextMesh Pro/        # TextMesh Pro text rendering
├── TutorialInfo/        # Tutorial/readme system with custom inspector
└── InputSystem_Actions.inputactions  # Input bindings (Move, Look, Attack, Interact, Crouch)

Library/PackageCache/
└── com.gamelovers.mcp-unity@*/  # MCP Unity integration package

ProjectSettings/         # Unity engine configuration
└── Packages/com.unity.dedicated-server/  # Dedicated server configuration

.specify/                # Spec Kit project planning system
├── memory/constitution.md  # Project governance principles
└── templates/           # Markdown templates (spec, plan, tasks, checklist)
```

## Tech Stack

- **Engine**: Unity 6.0.3+ (URP 17.3.0)
- **Language**: C# 9.0 (.NET 4.7.1)
- **Testing**: Unity Test Framework 1.6.0
- **Input**: Unity Input System 1.17.0
- **AI Navigation**: Unity AI Navigation 2.0.9
- **Networking**:
  - Mirror (Asset Store package)
  - PlayFlow Cloud (Asset Store package)
  - Custom GameNet implementation
  - Dedicated Server 2.0.1
  - Multiplayer Center 1.0.1
- **UI/Text**: TextMesh Pro
- **Utilities**:
  - Newtonsoft Json 3.2.2
  - Timeline 1.8.10
- **Cross-compilation**: Linux x86_64 toolchain (1.0.2)
- **MCP Integration**: com.gamelovers.mcp-unity (GitHub package)
- **Platforms**: Windows Standalone (primary), Linux Dedicated Server, Android/Mobile support

## Input System Configuration

Pre-configured Player action map with:
- **Move**: Vector2 (WASD/Analog stick)
- **Look**: Vector2 (Mouse/Right analog stick)
- **Attack**: Button (Left mouse/Gamepad button)
- **Interact**: Button with Hold interaction
- **Crouch**: Toggle button

## Definition of Done for Features

- State flow is explicit
- Dependencies wired explicitly
- No inspector-driven behavior
- No hidden coupling to scene order or hierarchy
- Test coverage (unit tests in Core, Play Mode tests only when necessary)
- Clear PR description with intent
- No unrelated refactors bundled with feature work

## MCP Server Configuration

- **Endpoint**: `ws://localhost:8090/McpUnity` (port configurable in Unity MCP Server Window)
- **Config file**: `ProjectSettings/McpUnitySettings.json` (auto-managed)
- **Timeout**: 10 seconds default (configurable)
- **Remote connections**: Disabled by default (enable in Server Window to bind to 0.0.0.0)

## Networking Architecture

The project implements a client-server multiplayer architecture with multiple networking backends:

- **ClientScene.unity**: Client-side scene for player connections
- **ServerScene.unity**: Dedicated server/host scene
- **GameNet**: Custom networking implementation with separation of concerns
  - Client logic in `Assets/Scripts/GameNet/Client/`
  - Server logic in `Assets/Scripts/GameNet/Server/`
  - Shared protocol in `Assets/Scripts/GameNet/Shared/`
  - Prefab registry for network object spawning
- **Mirror**: Alternative networking framework (Asset Store)
- **PlayFlow Cloud**: Cloud services integration

Follow Constitution Principle X: Keep core networking logic in plain C# (Shared/), with Unity adapters in Client/Server layers.

## Common MCP Workflows

1. **Start MCP Server**: Unity Editor → Tools > MCP Unity > Server Window → "Start Server"
2. **Execute Unity Commands**: Use natural language with AI assistant (e.g., "Create 10 empty GameObjects named Enemy_1 through Enemy_10")
3. **Debug MCP**: `npm run inspector` in Server~ directory or check Unity console logs
4. **Batch Operations**: Use `batch_execute` for atomic multi-step operations with rollback support

## Governance

- No commits to main (PR required, CI must pass)
- Claude Code is authorized AI tool
- Spec Kit is planning source of truth
- Constitution supersedes local preferences
- Exceptions require written rationale in PR with minimal scope

## Adding New Systems

When adding a system:
- Define clear state owner type
- Define public contract (interfaces, events, commands)
- Make it easy to delete
- Start with Core layer logic, add Unity adapters last
- No implicit discovery or inspector magic
