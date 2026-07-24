# My Pizza Shop — Agent Guide

## Purpose

This file is the fast entry point for AI agents and contributors. Read it before exploring the repository. For verified project details and evidence, see [Docs/AI/UnityProjectContext.md](Docs/AI/UnityProjectContext.md).

## Project Snapshot

- **Engine:** Unity 6.3 (`6000.3.20f1`), Universal Render Pipeline (URP).
- **Game:** `My Pizza Shop`, an early-stage casual/mobile-oriented 3D game.
- **Gameplay code:** `Assets/Engineering/`.
- **Current gameplay slice:** player movement and wallet, Input System event relay, timed payments, purchasable trigger areas, autonomous pizza production, player pizza stacks, pizza serving station with customer queue and money reward, trash station with DoTween animation, customer bot NavMesh movement, timed customer spawner, and ScriptableObject event channels for gameplay feedback.
- **Primary authored scene on disk:** `Assets/Scenes/MainScene.unity`. The scene's NavMeshSurface is baked and covers SpawnPoint, 2 approach waypoints, and CustomerSlot_0–9. Baked NavMesh data is stored in `Assets/Scenes/MainScene_NavMeshData.asset`.

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
| `Assets/Engineering/Scripts/Mono/Actors/ServeStation/` | Player deposits pizzas into station storage through `PlateTrigger`; `ServeTrigger` sells storage to the front customer; money is awarded per pizza served and `PizzaServed` is raised. |
| `Assets/Engineering/Scripts/Mono/Actors/TrashStation/` | Trash disposal station; removes all pizzas from player with DoTween fly-and-shrink animation, raises `PizzaTrashed` event. |
| `Assets/Engineering/Scripts/Mono/Actors/CustomerQueue/` | `CustomerBot` — NavMeshAgent-driven bot with approach-waypoint traversal and queue-slot movement. `CustomerSpawner` — configurable timed coroutine spawning customers with random orders. |
| `Assets/Engineering/Scripts/Mono/Player/PlayerPizzaInventory.cs` | Player pizza capacity, count (`TryAdd`/`TryRemove`), and overhead visual stack. |
| `Assets/Engineering/ScriptableObjects/SEconomy.cs` | Economy tuning asset definition. |
| `Assets/Engineering/ScriptableObjects/SGrillStation.cs` | Pizza production-rate and station-capacity tuning asset definition. |
| `Assets/Engineering/ScriptableObjects/SServeStation.cs` | Pizza serving-station tuning: min/max pizzas per order, max queue customers (1–10), customer spawn interval, price-per-pizza, and max stored pizzas. `maxPizzas` was renamed to `maxPizzasPerOrder` with `[FormerlySerializedAs]` for asset data preservation. |
| `Assets/Engineering/ScriptableObjects/STrashStation.cs` | Trash station animation tuning asset definition. |
| `Assets/Engineering/ScriptableObjects/SVoidEventChannel.cs` | Decoupled, parameterless gameplay-event channel. |
| `Assets/Engineering/ScriptableObjects/SIntEventChannel.cs` | Decoupled integer-value event channel used by the pizza inventory UI. |
| `Assets/Engineering/Prefabs/PizzaVisual.prefab` | Placeholder pizza visual used by the oven and player stacks. |
| `Assets/Engineering/Prefabs/CustomerBot.prefab` | Customer-bot prefab — capsule visual, NavMeshAgent, CapsuleCollider, CustomerBot component. |
| `Assets/Engineering/Prefabs/ServingStation.prefab` | Serving-station prefab — `ServeStation` on root, separate `plateTrigger`/`serveTrigger` children, and `CustomerQueue` with 10 `CustomerSlot_0–9` queue-slot transforms. |
| `Assets/Engineering/Prefabs/TrashStation.prefab` | Trash-station prefab — `TrashStation` on root, `TrashPlate` on `triggerArea`, `TrashTarget` child. |
| `Assets/Scenes/` | Authored scenes. |
| `Assets/Settings/` | Render-pipeline assets and project visual settings. |

## Architecture And Conventions

- Runtime behavior is **MonoBehaviour-centric**. `Engineering.asmdef` owns gameplay code, while separate EditMode and PlayMode test assemblies reference it.
- Use namespaces rooted at `Engineering` and preserve the existing folder-to-namespace pattern. All namespaces follow the pattern `Engineering.Scripts.Mono.<Folder>` matching their directory structure (e.g., `Engineering.Scripts.Mono.Actors.GrillStation`, `Engineering.Scripts.Mono.Items`). There is no `Engineering.Engineering` duplication and no legacy `PizzaMaker` namespace.
- Use `[SerializeField] private` for Inspector-assigned dependencies and tuning values.
- Private runtime fields use `_camelCase`; serialized fields in existing code may use either `_camelCase` or `camelCase`. Follow the nearest file's convention.
- Input is event-driven: subscribe in `OnEnable` and unsubscribe in `OnDisable`. Extend `InputManager` rather than polling duplicate input actions in consumers.
- Cross-system gameplay feedback uses `SVoidEventChannel` and `SIntEventChannel` assets. Publishers raise an intent event; consumers subscribe through Inspector-assigned channel references rather than calling each other directly.
- Pizza inventory count is published through the typed `SIntEventChannel`; UI listens to the channel instead of depending on the player inventory component.
- `EconomyManager` is the primary persistent singleton for money operations. `UIManager`, `AnimationManager`, and `SoundManager` are also `DontDestroyOnLoad` singletons.
- The player is located by the `Player` tag in `EconomyManager`; retain or deliberately migrate this contract together with scene/prefab changes.
- Use physics movement in `FixedUpdate`, as `PlayerMovement` does.

## Important Contracts

- The Input System action asset must include a `Player` map with `Move`, `Look`, `Attack`, `Interact`, `Previous`, `Next`, and `Sprint` actions.
- `EconomyManager` requires an assigned `SEconomy` and a scene object tagged `Player` with `PlayerWallet`.
- `BuyingArea` expects collider trigger callbacks and calls `EconomyManager.Instance`.
- `EconomyManager` raises `GroundMoneyCollected` after a successful ground-money transaction and `BuyingAreaPurchased` after an area is purchased. `SoundManager` listens to these channels and owns clip selection/playback.
- `GrillStation` owns ready-pizza state and production; `GrillPlate` only forwards player trigger collection. `PlayerPizzaInventory.TryAdd` enforces player capacity and returns the accepted amount.
- `GrillStation` positions its ready-pizza stack from `GrillPlate.PizzaStackBasePosition`, which uses the plate collider's `bounds.max.y`; do not replace this with a hard-coded pivot offset.
- `ServeStation` manages an ordered customer queue via `List<CustomerBot>`. `RegisterCustomer` reserves a slot immediately upon spawn. `TryDepositPizzas` transfers player pizzas into station storage (up to `maxStoredPizzas`), does not award money or raise events. `TryServeFrontCustomer` (no-arg) transfers `min(storedPizzaCount, frontCustomerRemainingOrder)` from station storage to the front customer, awards money for delivered pizzas, raises `PizzaServed` once, and updates the station's pizza visual stack. A completed front customer is removed and destroyed, and every remaining customer is reassigned to the preceding queue slot via `AssignQueueSlot`. The station visual pool is created in `OnEnable` from `pizzaVisualPrefab` and `pizzaStackSpacing`, anchored at the assigned plate collider's top surface.
- `PlateTrigger` and `ServeTrigger` are separate child trigger relays on the serving-station prefab. `PlateTrigger` deposits pizzas on enter; `ServeTrigger` serves the front customer on enter/stay. The station stack anchor comes from the assigned non-trigger plate collider's `bounds.max.y`.
- `TrashStation` removes all pizzas from the player via `PlayerPizzaInventory.TryRemove`, spawns temp visuals at the player's pizza stack world positions, animates them to `TrashTarget` with DoTween (`DOMove` + `DOScale(0)`), then destroys them. Raises `PizzaTrashed` event for SFX. `TrashPlate` on `triggerArea` forwards player detection to the station.
- Do not rename Input action maps/actions, tags, or serialized fields without updating their scene/prefab and code consumers.

## Scene And Build Caution

- `Assets/Scenes/MainScene.unity` exists, but `ProjectSettings/EditorBuildSettings.asset` currently enables `Assets/Scenes/SampleScene.unity`, which is not present in the repository.
- Treat the actual startup/build scene as **unverified** until it is checked in Unity's Build Profiles/Build Settings. Do not silently change it during unrelated work.

## Packages

- **URP 17.3.0**, **Input System 1.19.0**, **AI Navigation 2.0.13**, **UGUI 2.0.0**, **Timeline 1.8.12**, and **Unity Test Framework 1.6.0** are direct dependencies.
- The presence of Multiplayer Center, Visual Scripting, and the Test Framework does not prove they are used by gameplay code. Verify usage before integrating with them.

## Validation

- The project source currently declares 15 EditMode and 58 PlayMode tests. Run the affected suite after gameplay changes; test counts alone do not prove they passed.
- For script changes, compile in Unity and check Console errors. For gameplay changes, exercise the affected flow in Play Mode when the Editor is available.
- Do not claim a successful build or scene validation without actually performing it.

## Do Not Touch By Default

- Generated/cached directories: `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`, `Builds/`, and `UserSettings/`.
- Package versions or Project Settings unless the task explicitly requires them.
- `.unity`, `.prefab`, `.asset`, materials, animation assets, or render settings for code-only tasks.
- `Assets/TutorialInfo/` unless working on the Unity template/tutorial content.

## Documentation Maintenance

Update this guide and `Docs/AI/UnityProjectContext.md` when changing the scene flow, a cross-cutting contract, major package, core system location, or validation process. Keep both documents concise and factual.
