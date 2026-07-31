# My Pizza Shop — Agent Guide

## Purpose

This file is the fast entry point for AI agents and contributors. Read it before exploring the repository. For verified project details and evidence, see [Docs/AI/UnityProjectContext.md](Docs/AI/UnityProjectContext.md).

## Project Snapshot

- **Engine:** Unity 6.3 (`6000.3.20f1`), Universal Render Pipeline (URP).
- **Game:** `My Pizza Shop`, an early-stage casual/mobile-oriented 3D game.
- **Gameplay code:** `Assets/Engineering/`.
- **Current gameplay slice:** player movement and wallet, Input System event relay, a separate `CameraManager` with per-axis `SmoothDamp` player follow, ScriptableObject-backed camera tuning, timed payments, purchasable trigger areas, autonomous pizza production, player pizza stacks, pizza serving station with customer queue and money reward, trash station with DoTween animation, customer bot NavMesh movement, timed customer spawner, and ScriptableObject event channels for gameplay feedback.
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
| `Assets/Engineering/Scripts/Mono/Managers/CameraManager.cs` | Scene-local camera follow manager; follows the player in `LateUpdate` with per-axis `Mathf.SmoothDamp`. On startup it positions the camera at `target.position + settings.FollowOffset` and preserves the initial camera rotation. Reads tuning from `SCameraSettings`. |
| `Assets/Engineering/Scripts/Mono/Managers/EconomyManager.cs` | Plain scene object; transfers wallet money to a purchase area over time. |
| `Assets/Engineering/Scripts/Mono/Bootstrap/MainSceneInstaller.cs` | Composition root for MainScene; validates and initializes all cross-scene dependencies in `Awake`. |
| `Assets/Engineering/Scripts/Mono/Player/` | Player movement, wallet, and trigger helpers. |
| `Assets/Engineering/Scripts/Mono/Areas/BuyingArea.cs` | Trigger-driven unlock/purchase zone. |
| `Assets/Engineering/Scripts/Mono/Actors/GrillStation/` | Autonomous pizza production station and its player-collection trigger. |
| `Assets/Engineering/Scripts/Domain/ServeStation/` | Pure C# domain model: `ServeStationModel` (authoritative owner of stored-pizza state and deposit/serve/reward calculations) and `ServeResult` (immutable result struct). No `UnityEngine` dependency. |
| `Assets/Engineering/Scripts/Domain/Purchase/` | Pure C# domain model: `PurchaseProgressModel` (authoritative owner of unlock price, paid amount, remaining amount, and purchase completion state) and `PurchaseProgressResult` (immutable result struct). No `UnityEngine` dependency. |
| `Assets/Engineering/Scripts/Domain/Payment/` | Pure C# domain model: `PaymentSessionModel` (authoritative owner of per-tick payment amount and active/cancelled session state). `EconomyManager` delegates to it. No `UnityEngine` dependency. |
| `Assets/Engineering/Scripts/Domain/GrillStation/` | Pure C# domain model: `GrillStationModel` (authoritative owner of ready-pizza count and production capacity rules). `GrillStation` delegates to it. No `UnityEngine` dependency. |
| `Assets/Engineering/Scripts/Domain/Economy/` | Pure C# domain model: `WalletModel` (authoritative owner of money balance and spend/credit rules). `PlayerWallet` delegates to it. No `UnityEngine` dependency. |
| `Assets/Engineering/Scripts/Domain/Inventory/` | Pure C# domain models: `PizzaInventoryModel` (player pizza count/capacity rules) and `WasteInventoryModel` (player waste count/capacity rules). `PlayerPizzaInventory` and `PlayerWasteInventory` delegate to them. No `UnityEngine` dependency. |
| `Assets/Engineering/Scripts/Domain/Table/` | Pure C# domain models: `TableModel` (seat reservation/release rules), `TableWasteModel` (leftover-waste count and accumulation rules), and `ReserveSeatResult`/`ReleaseSeatResult`/`AddLeftoversResult` immutable result structs. No `UnityEngine` dependency. |
| `Assets/Engineering/Scripts/Mono/Actors/Table/` | `Table` — seat management and leftover-waste delegation to `TableWasteModel` and `TableWasteVisuals`; raises `LeftoverStateChanged` on removal/clear. `TableManager` — cross-table reservation, release, `AddLeftoversToTable` API; raises `SeatReleased` and `LeftoversRemoved`; lazily subscribes to `Table.LeftoverStateChanged` before table operations. `TableWasteVisuals` — pooled leftover visual stack with configurable anchor and spacing. |
| `Assets/Engineering/Scripts/Domain/CustomerQueue/` | Pure C# domain models: `CustomerOrderModel` (authoritative owner of per-customer order pizza count and remaining/received-amount rules) and `CustomerQueueModel` (authoritative owner of queue capacity, enqueue, remove-front, and remove-at-index rules). `EnqueueResult` and `RemoveFrontResult` are immutable result structs. No `UnityEngine` dependency. |
| `Assets/Engineering/Scripts/Mono/Actors/ServeStation/` | Player deposits pizzas into station storage through `PlateTrigger`; `ServeTrigger` sells storage to the front customer; money is awarded per pizza served and `PizzaServed` is raised. `ServeStation` is the Unity facade/orchestrator that delegates storage and calculations to `ServeStationModel` and coordinates `CustomerQueueController` (queue ownership, registration, slot assignment, front-customer removal, arbitrary-customer `RemoveCustomer`) and `ServeStationVisuals` (pizza visual pool creation and positioning). `ServeStation` receives its `TableManager` dependency at runtime through `Initialize` (not serialized) and subscribes to both `SeatReleased` and `LeftoversRemoved` in `OnEnable`/`OnDisable` for safe lifecycle management. On either event it retries the first waiting customer via `OnRetryWaitingCustomer`. |
| `Assets/Engineering/Scripts/Mono/Actors/TrashStation/` | Trash disposal station; separately removes pizza and waste stacks with pooled DoTween fly-and-shrink animations, raising `PizzaTrashed` and `WasteDisposed` events. |
| `Assets/Engineering/Scripts/Mono/Actors/CustomerQueue/` | `CustomerBot` — NavMeshAgent-driven NavMesh/Transform adapter with approach-waypoint traversal and queue-slot movement. `CustomerSpawner` — configurable timed coroutine spawning customers with random orders. |
| `Assets/Engineering/Scripts/Mono/Player/PlayerPizzaInventory.cs` | Player pizza capacity, count (`TryAdd`/`TryRemove`), and overhead visual stack. `PlayerWasteInventory` manages a separate leftover-waste stack and accepts waste only when the player carries no pizzas. |
| `Assets/Engineering/ScriptableObjects/SEconomy.cs` | Economy tuning asset definition. |
| `Assets/Engineering/ScriptableObjects/SGrillStation.cs` | Pizza production-rate and station-capacity tuning asset definition. |
| `Assets/Engineering/ScriptableObjects/SServeStation.cs` | Pizza serving-station tuning: min/max pizzas per order, max queue customers (1–10), customer spawn interval, price-per-pizza, and max stored pizzas. `maxPizzas` was renamed to `maxPizzasPerOrder` with `[FormerlySerializedAs]` for asset data preservation. |
| `Assets/Engineering/ScriptableObjects/STrashStation.cs` | Trash station animation tuning asset definition. |
| `Assets/Engineering/ScriptableObjects/SCameraSettings.cs` | Camera follow tuning asset definition: X/Y/Z damping, optional maximum follow speed, and FollowOffset (default `-7, 10, -7` for isometric framing). |
| `Assets/Engineering/ScriptableObjects/SVoidEventChannel.cs` | Decoupled, parameterless gameplay-event channel. |
| `Assets/Engineering/ScriptableObjects/SIntEventChannel.cs` | Decoupled integer-value event channel used by the pizza inventory UI. |
| `Assets/Engineering/Prefabs/PizzaVisual.prefab` | Placeholder pizza visual used by the oven and player stacks. |
| `Assets/Engineering/Prefabs/CustomerBot.prefab` | Customer-bot prefab — capsule visual, NavMeshAgent, CapsuleCollider, CustomerBot component. |
| `Assets/Engineering/Prefabs/ServingStation.prefab` | Serving-station prefab — `ServeStation` on root, separate `plateTrigger`/`serveTrigger` children, and `CustomerQueue` with 10 `CustomerSlot_0–9` queue-slot transforms. |
| `Assets/Engineering/Prefabs/TrashStation.prefab` | Trash-station prefab — `TrashStation` on root, `TrashPlate` on `triggerArea`, `TrashTarget` child. |
| `Assets/Engineering/Prefabs/leftover.prefab` | Placeholder leftover visual (cube mesh with material) used by `TableWasteVisuals`. |
| `Assets/Scenes/` | Authored scenes. |
| `Assets/Settings/` | Render-pipeline assets and project visual settings. |

## Architecture And Conventions

- Runtime behavior is **MonoBehaviour-centric**. Gameplay code is split into three layered assemblies:
  - `Engineering.Domain.asmdef` — pure C# domain models, no UnityEngine dependency (`noEngineReferences: true`).
  - `Engineering.ScriptableObjects.asmdef` — ScriptableObject assets (tuning data, event channels).
  - `Engineering.Runtime.asmdef` — all MonoBehaviours, references Domain + ScriptableObjects + `Unity.InputSystem`.
  Separate EditMode and PlayMode test assemblies reference all three runtime assemblies.
- Use namespaces rooted at `Engineering` and preserve the existing folder-to-namespace pattern. All namespaces follow the pattern `Engineering.Scripts.Mono.<Folder>` matching their directory structure (e.g., `Engineering.Scripts.Mono.Actors.GrillStation`, `Engineering.Scripts.Mono.Items`). There is no `Engineering.Engineering` duplication and no legacy `PizzaMaker` namespace.
- Use `[SerializeField] private` for Inspector-assigned dependencies and tuning values.
- Private runtime fields use `_camelCase`; serialized fields in existing code may use either `_camelCase` or `camelCase`. Follow the nearest file's convention.
- Input is event-driven: subscribe in `OnEnable` and unsubscribe in `OnDisable`. Extend `InputManager` rather than polling duplicate input actions in consumers.
- Cross-system gameplay feedback uses `SVoidEventChannel` and `SIntEventChannel` assets. Publishers raise an intent event; consumers subscribe through Inspector-assigned channel references rather than calling each other directly.
- Pizza inventory count is published through the typed `SIntEventChannel`; UI listens to the channel instead of depending on the player inventory component.
- There are **no static singletons or DontDestroyOnLoad managers**. All cross-component dependencies use `[SerializeField]` references wired through MainScene. Managers are plain scene objects; they are not singletons.
- `MainSceneInstaller` is the composition root for `MainScene`. Scene-object dependencies are injected through public `Initialize` methods called during `Awake`. Prefab-local and ScriptableObject references remain Inspector-assigned.
- `EconomyManager` has a runtime `CurrencyService` reference set via `Initialize`. `BuyingArea` has a runtime `EconomyManager` reference set via `Initialize`. `ServeStation` and `MoneyToCollect` have runtime `CurrencyService` references set via `Initialize`. `ServeStation` also receives a runtime `TableManager` reference via `Initialize`. `PlayerMovement` has a runtime `InputManager` reference set via `Initialize`. `UIManager` has a runtime `PlayerWallet` reference set via `Initialize`.
- Use physics movement in `FixedUpdate`, as `PlayerMovement` does.
- `CameraManager` is a separate child of the scene `Managers` object. Its `target` and `cameraTransform` references remain scene-specific; damping values and follow offset come from the Inspector-assigned `SCameraSettings` asset. On startup the camera snaps to `target.position + settings.FollowOffset`; camera rotation remains at its initial scene-authored rotation.

## Important Contracts

- The Input System action asset must include a `Player` map with `Move`, `Look`, `Attack`, `Interact`, `Previous`, `Next`, and `Sprint` actions.
- `EconomyManager` requires an assigned `SEconomy` and a runtime `CurrencyService` reference via `Initialize`.
- `BuyingArea` expects collider trigger callbacks and calls its runtime `EconomyManager` reference set via `Initialize`.
- `EconomyManager` raises `GroundMoneyCollected` after a successful ground-money transaction and `BuyingAreaPurchased` after an area is purchased. `SoundManager` listens to these channels and owns clip selection/playback. `SoundManager` also subscribes to `PizzaServed` (raised by `ServeStation`) and `PizzaTrashed` (raised by `TrashStation`) via the same `SVoidEventChannel` pattern. `EconomyManager` also subscribes to `moneyAnimationRequested` to request money fly animations; `AnimationManager` listens to the same channel.
- `GrillStation` owns ready-pizza state and production; `GrillPlate` only forwards player trigger collection. `PlayerPizzaInventory.TryAdd` enforces player capacity and returns the accepted amount.
- `GrillStation` positions its ready-pizza stack from `GrillPlate.PizzaStackBasePosition`, which uses the plate collider's `bounds.max.y`; do not replace this with a hard-coded pivot offset.
- `ServeStationModel` is the authoritative owner of stored-pizza state, deposit, serve, completion, and reward calculations. `ServeStation` is the Unity facade/orchestrator that delegates storage and calculations to `ServeStationModel`, coordinates `CustomerQueueController` and `ServeStationVisuals`, owns event publication and currency calls, and exposes `TryRegisterCustomer`, `DepositFrom`, and `ServeFrontCustomer`. `CustomerQueueController` owns the ordered `List<CustomerBot>`, `CustomerQueueModel`, queue capacity, queue-slot assignment, front-customer removal, arbitrary-customer removal (`RemoveCustomer`), customer destruction, and reassignment of remaining customers to preceding queue slots via `AssignQueueSlot`. `ServeStationVisuals` owns the pizza visual pool, creates it from `pizzaVisualPrefab`, and positions it from the assigned plate collider's top surface using `pizzaStackSpacing`. `TryRegisterCustomer` reserves a slot immediately upon spawn. `DepositFrom` transfers player pizzas into station storage (up to `maxStoredPizzas`), does not award money or raise events. `ServeFrontCustomer` (no-arg) delegates to `ServeStationModel.TryServe`, awards money for delivered pizzas, raises `PizzaServed` once, refreshes the station visual stack, and removes completed front customers through `CustomerQueueController`. `ServeStation` receives its `TableManager` dependency at runtime through `Initialize` (not serialized) and subscribes to both `SeatReleased` and `LeftoversRemoved` in `OnEnable`/`OnDisable` for safe lifecycle management. On either event it retries the first waiting customer via `OnRetryWaitingCustomer`.
- `PurchaseProgressModel` is the authoritative owner of unlock price, paid amount, remaining amount, and purchase completion state. `BuyingArea` is the Unity trigger adapter that delegates state to `PurchaseProgressModel` and owns trigger callbacks and `EconomyManager` interaction. `BuyingArea._unlockPrice` is Inspector-configured with `[SerializeField] private` and defaults to 100. `EconomyManager` remains the coroutine, currency, animation-request, destruction, and purchased-event orchestrator. Final-payment capping is not part of the current behavior-preserving design.
- `PlateTrigger` and `ServeTrigger` are separate child trigger relays on the serving-station prefab. `PlateTrigger` deposits pizzas on enter; `ServeTrigger` serves the front customer on enter/stay. The station stack anchor comes from the assigned non-trigger plate collider's `bounds.max.y`.
- `TrashStation` removes all pizzas from the player via `PlayerPizzaInventory.TryRemove`, retrieves visuals from an internal `UnityEngine.Pool.ObjectPool<GameObject>`, positions them at the player's pizza stack world positions, animates them to `TrashTarget` with DoTween (`DOMove` + `DOScale(0)`), then releases them back to the pool. Raises `PizzaTrashed` event for SFX. `TrashPlate` on `triggerArea` forwards player detection to the station.
- `Table` delegates leftover-waste state to `TableWasteModel`. `TableManager` exposes `AddLeftoversToTable(tableIndex, pizzaCount)`. When `CustomerBot` transitions from `Eating` to `Leaving`, it calls `AddLeftoversToTable` with `_orderModel.InitialPizzaCount` before releasing the seat. `TableWasteVisuals` manages a pooled leftover visual stack positioned from its anchor with `leftoverStackSpacing`. No leftovers are created if the customer is destroyed or fails to reach the table. `TableWasteTrigger` transfers waste to `PlayerWasteInventory` only while the player has no pizzas. `Table` raises `LeftoverStateChanged` on `TryRemoveLeftovers` and `ClearLeftovers`; `TableManager` lazily subscribes before table operations and re-raises it as `LeftoversRemoved`.
- Do not rename Input action maps/actions, tags, or serialized fields without updating their scene/prefab and code consumers.
- **Player prefab contract:** `Player.prefab` must maintain the `Player[Transform] → Scripts[PlayerPizzaInventory, PlayerWasteInventory] / Mesh[CapsuleCollider, Rigidbody, PlayerTriggerRelay]` hierarchy. All interaction triggers (`GrillPlate`, `PlateTrigger`, `TrashPlate`, `TableWasteTrigger`) resolve inventories via `other.transform.root.GetComponentInChildren<T>()` after checking the `Player` tag, so the Scripts/Mesh separation and rooted hierarchy must survive any prefab changes.

## Scene And Build

- **Startup scene:** `Assets/Scenes/MainScene.unity` is the single enabled build scene.
- The stale `Assets/Scenes/SampleScene.unity` entry has been removed from `ProjectSettings/EditorBuildSettings.asset`.

## Packages

- **URP 17.3.0**, **Input System 1.19.0**, **AI Navigation 2.0.13**, **UGUI 2.0.0**, **Timeline 1.8.12**, and **Unity Test Framework 1.6.0** are direct dependencies.
- The presence of Multiplayer Center, Visual Scripting, and the Test Framework does not prove they are used by gameplay code. Verify usage before integrating with them.

## Validation

- The project source currently declares 296 EditMode and 176 PlayMode tests, including camera-follow and `SCameraSettings` coverage. Run the affected suite after gameplay changes; test counts alone do not prove they passed.
- For script changes, compile in Unity and check Console errors. For gameplay changes, exercise the affected flow in Play Mode when the Editor is available.
- Do not claim a successful build or scene validation without actually performing it.
- **Player prefab contract:** `PlayerPrefabContractTests` (EditMode) validates the Player prefab hierarchy required by all trigger interactions — Scripts/Mesh children, inventory placement, collider, tag, and PlayerTriggerRelay wiring.
- **Player trigger regression:** `PlayerTriggerRegressionTests` (PlayMode) runs contract-level and real-physics regression tests for GrillPlate, PlateTrigger, TrashPlate, and TableWasteTrigger interactions with an instantiated Player.prefab clone.

## CI

- **Workflow:** `.github/workflows/unity-tests.yml` runs on push to `dev`/`main` and PRs targeting `main`.
- **Jobs:** separate `editmode-tests` and `playmode-tests` using Unity `6000.3.20f1` via GameCI (`game-ci/unity-test-runner@v4`).
- **Secrets:** `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` repository secrets must be configured. The workflow fails early with a clear message if any required secret is missing.
- **Artifacts:** test result XML files are uploaded as `editmode-test-results` and `playmode-test-results` on every run.
- **Local equivalent:** run EditMode and PlayMode suites through Unity Test Runner; CI mirrors this with GameCI.

## Do Not Touch By Default

- Generated/cached directories: `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`, `Builds/`, and `UserSettings/`.
- Package versions or Project Settings unless the task explicitly requires them.
- `.unity`, `.prefab`, `.asset`, materials, animation assets, or render settings for code-only tasks.
- `Assets/TutorialInfo/` unless working on the Unity template/tutorial content.

## Documentation Maintenance

Update this guide and `Docs/AI/UnityProjectContext.md` when changing the scene flow, a cross-cutting contract, major package, core system location, or validation process. Keep both documents concise and factual.
