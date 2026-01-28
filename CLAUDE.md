# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MWorldsTech is a Unity 6 game project with integrated MCP (Model Context Protocol) support, enabling AI assistants to directly interact with Unity Editor. The architecture follows a two-tier design:

**AI Assistant** ⇄ (stdio) ⇄ **Node.js MCP Server** ⇄ (WebSocket) ⇄ **Unity Editor** ⇄ **Game Objects/Scenes**

## Development Commands

### Unity Operations
- Open Unity Editor and work with the project directly
- **Tools > MCP Unity > Server Window**: Start/stop MCP server and configure settings
- **Window > General > Test Runner**: Run Unity Test Framework tests (EditMode/PlayMode)

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
- `execute_menu_item`: Run Unity menu commands
- `select_gameobject`, `get_gameobject`: Query/select scene objects
- `update_gameobject`: Modify GameObject properties (name, tag, layer, active state)
- `update_component`: Add/modify components on GameObjects
- `create_prefab`, `add_asset_to_scene`: Asset management
- `create_scene`, `load_scene`, `save_scene`, `delete_scene`: Scene operations
- `run_tests`: Execute Unity tests via Test Runner
- `create_material`, `assign_material`, `modify_material`: Material workflow
- `batch_execute`: Run multiple operations atomically

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
├── Scenes/              # Game scenes (SampleScene.unity)
├── Settings/            # URP render pipeline configs (PC_RPAsset, Mobile_RPAsset)
├── TutorialInfo/        # Tutorial/readme system with custom inspector
└── InputSystem_Actions.inputactions  # Input bindings (Move, Look, Attack, Interact, Crouch)

Library/PackageCache/
└── com.gamelovers.mcp-unity@*/  # MCP Unity integration package

ProjectSettings/         # Unity engine configuration

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
- **MCP Integration**: com.gamelovers.mcp-unity (GitHub package)
- **Platforms**: Windows Standalone (primary), Android/Mobile support

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
