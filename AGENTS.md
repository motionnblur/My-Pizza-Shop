# My Pizza Shop — Agent Guide

## Purpose

This file is the fast entry point for AI agents and contributors. Read it before exploring the repository. For verified project details and evidence, see [Docs/AI/UnityProjectContext.md](Docs/AI/UnityProjectContext.md).

## Project Snapshot

- **Engine:** Unity 6.3 (`6000.3.20f1`), Universal Render Pipeline (URP).
- **Game:** `My Pizza Shop`, an early-stage casual/mobile-oriented 3D game.
- **Gameplay code:** `Assets/Engineering/`.
- **Current gameplay slice:** player movement and wallet, Input System event relay, timed payments, purchasable trigger areas, autonomous pizza production, player pizza stacks, and ScriptableObject event channels for gameplay feedback.
- **Primary authored scene on disk:** `Assets/Scenes/MainScene.unity`.

## Start Here

1. Read this file, then inspect only the relevant files under `Assets/Engineering/`.
2. Check the Unity Console before and after a change when the Editor is available.
3. For scene/prefab work, inspect the target asset and its serialized script references before editing code assumptions.
4. Keep changes scoped. Do not refactor unrelated systems while implementing a feature or bug fix.

## Code Map

| Path | Responsibility |
| --- | --- |
| `Assets/Engineering/Scripts/Mono/Managers/InputManager.cs` | Wraps the Input System's `Player` action map and publishes input events. |
| `Assets/Engineering/Scripts/Mono/Managers/EconomyManager.cs` | Persistent singleton; transfers wallet money to a purchase area over time. |
| `Assets/Engineering/Scripts/Mono/Player/` | Player movement, wallet, and trigger helpers. |
| `Assets/Engineering/Scripts/Mono/Areas/BuyingArea.cs` | Trigger-driven unlock/purchase zone. |
| `Assets/Engineering/Scripts/Mono/Actors/GrillStation/` | Autonomous pizza production station and its player-collection trigger. |
| `Assets/Engineering/Scripts/Mono/Player/PlayerPizzaInventory.cs` | Player pizza capacity, count, and overhead visual stack. |
| `Assets/Engineering/ScriptableObjects/SEconomy.cs` | Economy tuning asset definition. |
| `Assets/Engineering/ScriptableObjects/SGrillStation.cs` | Pizza production-rate and station-capacity tuning asset definition. |
| `Assets/Engineering/ScriptableObjects/VoidEventChannel.cs` | Decoupled, parameterless gameplay-event channel. |
| `Assets/Scenes/` | Authored scenes. |
| `Assets/Settings/` | Render-pipeline assets and project visual settings. |

## Architecture And Conventions

- Runtime behavior is **MonoBehaviour-centric**; there are no first-party assembly definitions or test assemblies.
- Use namespaces rooted at `Engineering` and preserve the existing folder-to-namespace pattern.
- Use `[SerializeField] private` for Inspector-assigned dependencies and tuning values.
- Private runtime fields use `_camelCase`; serialized fields in existing code may use either `_camelCase` or `camelCase`. Follow the nearest file's convention.
- Input is event-driven: subscribe in `OnEnable` and unsubscribe in `OnDisable`. Extend `InputManager` rather than polling duplicate input actions in consumers.
- Cross-system gameplay feedback uses `VoidEventChannel` assets. Publishers raise an intent event; consumers subscribe through Inspector-assigned channel references rather than calling each other directly.
- Pizza inventory count is published through the typed `SIntEventChannel`; UI listens to the channel instead of depending on the player inventory component.
- `EconomyManager` is currently the sole persistent singleton. Do not add another global manager unless the feature genuinely needs it.
- The player is located by the `Player` tag in `EconomyManager`; retain or deliberately migrate this contract together with scene/prefab changes.
- Use physics movement in `FixedUpdate`, as `PlayerMovement` does.

## Important Contracts

- The Input System action asset must include a `Player` map with `Move`, `Look`, `Attack`, `Interact`, `Previous`, `Next`, and `Sprint` actions.
- `EconomyManager` requires an assigned `SEconomy` and a scene object tagged `Player` with `PlayerWallet`.
- `BuyingArea` expects collider trigger callbacks and calls `EconomyManager.Instance`.
- `EconomyManager` raises `GroundMoneyCollected` after a successful ground-money transaction and `BuyingAreaPurchased` after an area is purchased. `SoundManager` listens to these channels and owns clip selection/playback.
- `GrillStation` owns ready-pizza state and production; `GrillPlate` only forwards player trigger collection. `PlayerPizzaInventory.TryAdd` enforces player capacity and returns the accepted amount.
- Do not rename Input action maps/actions, tags, or serialized fields without updating their scene/prefab and code consumers.

## Scene And Build Caution

- `Assets/Scenes/MainScene.unity` exists, but `ProjectSettings/EditorBuildSettings.asset` currently enables `Assets/Scenes/SampleScene.unity`, which is not present in the repository.
- Treat the actual startup/build scene as **unverified** until it is checked in Unity's Build Profiles/Build Settings. Do not silently change it during unrelated work.

## Packages

- **URP 17.3.0**, **Input System 1.19.0**, **AI Navigation 2.0.13**, **UGUI 2.0.0**, **Timeline 1.8.12**, and **Unity Test Framework 1.6.0** are direct dependencies.
- The presence of Multiplayer Center, Visual Scripting, and the Test Framework does not prove they are used by gameplay code. Verify usage before integrating with them.

## Validation

- No first-party EditMode or PlayMode tests were found.
- For script changes, compile in Unity and check Console errors. For gameplay changes, exercise the affected flow in Play Mode when the Editor is available.
- Do not claim a successful build or scene validation without actually performing it.

## Do Not Touch By Default

- Generated/cached directories: `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`, `Builds/`, and `UserSettings/`.
- Package versions or Project Settings unless the task explicitly requires them.
- `.unity`, `.prefab`, `.asset`, materials, animation assets, or render settings for code-only tasks.
- `Assets/TutorialInfo/` unless working on the Unity template/tutorial content.

## Documentation Maintenance

Update this guide and `Docs/AI/UnityProjectContext.md` when changing the scene flow, a cross-cutting contract, major package, core system location, or validation process. Keep both documents concise and factual.
