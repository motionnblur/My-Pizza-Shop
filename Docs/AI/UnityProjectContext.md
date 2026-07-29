# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- **Project root:** repository root
- **Last analyzed:** 2026-07-28
- **Last analyzed commit:** `78ac204`
- **Summary:** Early-stage casual 3D game named *My Pizza Shop*. The current gameplay slice includes movement, wallet and ground-money collection, timed area purchases, autonomous pizza production and collection, player pizza stacks, a separate `CameraManager` with ScriptableObject-backed per-axis damping and player follow, pizza serving station with customer queue and money reward, trash station with DoTween fly-and-shrink animation, UI counters, pooled DOTween money-transfer effects, customer bot NavMesh movement, timed customer spawner, and ScriptableObject event channels for decoupled gameplay feedback.

## Confirmed Environment

- **Unity version:** `6000.3.20f1` (Unity 6.3).
- **Render pipeline:** Universal Render Pipeline (URP), confirmed by the direct package dependency and configured custom render pipeline asset.
- **Input system:** Unity Input System, confirmed (`activeInputHandler: 1`, Input System package, and `InputManager` usage).
- **Target platforms:** Android is explicitly configured; iOS and Standalone identifiers are also present. The intended release platform is likely Android/mobile, but that is not confirmed by build artifacts.

## Important Packages And Frameworks

| Area | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Rendering | URP 17.3.0 | Confirmed | `Packages/manifest.json`, `ProjectSettings/GraphicsSettings.asset` |
| Input | Input System 1.19.0; `InputManager` uses an asset-backed `Player` action map | Confirmed | `Packages/manifest.json`, `ProjectSettings/ProjectSettings.asset`, `Assets/Engineering/Scripts/Mono/Managers/InputManager.cs` |
| Camera | MainScene has one URP `Main Camera`, orthographic with size 5; a separate `CameraManager` child of `Managers` follows the Player while controlling the camera through an explicit reference, preserving the initial position offset and rotation, and applying per-axis `SmoothDamp` damping (`xDamping`, `yDamping`, `zDamping`) with an optional `maxFollowSpeed` limit. Damping values are stored in the assigned `SCameraSettings` ScriptableObject asset. Cinemachine is not present in the manifest or first-party code | Confirmed | `Assets/Scenes/MainScene.unity`, `Assets/Engineering/Scripts/Mono/Managers/CameraManager.cs`, `Assets/Engineering/ScriptableObjects/SCameraSettings.cs`, `Assets/Engineering/ScriptableObjects/SCameraSettings.asset`, `Packages/manifest.json` |
| Navigation | AI Navigation 2.0.13 is installed and used by `CustomerBot` for NavMesh movement; MainScene has a baked NavMeshSurface covering SpawnPoint, 2 waypoints, and 10 queue slots | Confirmed | `Packages/manifest.json`, `CustomerBot.cs`, `Assets/Scenes/MainScene_NavMeshData.asset` |
| UI | UGUI 2.0.0 is installed; project UI usage not inspected | Confirmed / unknown usage | `Packages/manifest.json` |
| Tests | Unity Test Framework 1.6.0 is installed; current source declares 294 EditMode `[Test]` methods and 174 PlayMode `[UnityTest]` methods covering core gameplay, camera follow and `SCameraSettings` behavior, UI, economy, money-animation pooling, pizza inventory, grill production, serving station with customer queue, customer queue domain models, purchase progress domain models, table model, table waste model, table flow, and trash station including pooled visual reuse and cleanup | Confirmed declaration counts; execution not performed in this audit | `Packages/manifest.json`, `Assets/Engineering/Tests/` |
| Tweening | DOTween is included as a vendor plugin and actively used for money-transfer animation | Confirmed | `Assets/Plugins/Demigiant/DOTween/`, `AnimationManager.cs` |
| Gameplay events | `SVoidEventChannel` decouples parameterless gameplay feedback; `SIntEventChannel` publishes pizza inventory counts to UI | Confirmed | Event-channel sources and assets, `EconomyManager.cs`, `SoundManager.cs`, `PlayerPizzaInventory.cs`, `UIManager.cs` |
| Other tooling | Timeline, Visual Scripting, Rider and Visual Studio integrations are installed; first-party usage is unverified | Confirmed / unverified usage | `Packages/manifest.json` |

## Directory Structure

| Path | Purpose | Confidence | Evidence |
| --- | --- | --- | --- |
| `Assets/Engineering/Scripts/Mono/` | First-party runtime MonoBehaviours: managers, player, areas, and collectible items | Confirmed | Folder and source inventory |
| `Assets/Engineering/Scripts/Mono/Actors/GrillStation/` | Autonomous pizza production and its collection trigger | Confirmed | `GrillStation.cs`, `GrillPlate.cs` |
| `Assets/Engineering/Scripts/Domain/ServeStation/` | Pure C# domain model: `ServeStationModel` (authoritative owner of stored-pizza state, deposit/serve/reward calculations) and `ServeResult` (immutable result struct). No `UnityEngine` dependency. | Confirmed | `ServeStationModel.cs`, `ServeResult.cs` |
| `Assets/Engineering/Scripts/Domain/Purchase/` | Pure C# domain model: `PurchaseProgressModel` (authoritative owner of unlock price, paid amount, remaining amount, and purchase completion state) and `PurchaseProgressResult` (immutable result struct). No `UnityEngine` dependency. | Confirmed | `PurchaseProgressModel.cs`, `PurchaseProgressResult.cs` |
| `Assets/Engineering/Scripts/Domain/CustomerQueue/` | Pure C# domain models: `CustomerOrderModel` (authoritative owner of per-customer order pizza count and remaining/received-amount rules) and `CustomerQueueModel` (authoritative owner of queue capacity, enqueue, remove-front, and remove-at-index rules). `EnqueueResult` and `RemoveFrontResult` are immutable result structs. No `UnityEngine` dependency. | Confirmed | `CustomerOrderModel.cs`, `CustomerQueueModel.cs`, `EnqueueResult.cs`, `RemoveFrontResult.cs` |
| `Assets/Engineering/Scripts/Domain/Table/` | Pure C# domain models: `TableModel` (seat reservation/release rules), `TableWasteModel` (leftover-waste count and accumulation rules), and `ReserveSeatResult`/`ReleaseSeatResult`/`AddLeftoversResult` immutable result structs. No `UnityEngine` dependency. | Confirmed | `TableModel.cs`, `TableWasteModel.cs`, `ReserveSeatResult.cs`, `ReleaseSeatResult.cs`, `AddLeftoversResult.cs` |
| `Assets/Engineering/Scripts/Mono/Actors/ServeStation/` | Player-to-station pizza storage/serving with customer queue, money reward, and separate deposit/serve trigger relays. `ServeStation` coordinates `CustomerQueueController` (queue ownership, `RemoveCustomer` for arbitrary-customer removal) and `ServeStationVisuals` (pizza visual pool). | Confirmed | `ServeStation.cs`, `CustomerQueueController.cs`, `ServeStationVisuals.cs`, `PlateTrigger.cs`, `ServeTrigger.cs` |
| `Assets/Engineering/Scripts/Mono/Actors/CustomerQueue/` | Customer bot NavMesh movement and timed customer spawner | Confirmed | `CustomerBot.cs`, `CustomerSpawner.cs` |
| `Assets/Engineering/Scripts/Mono/Actors/TrashStation/` | Player-to-station pizza disposal with DoTween animation | Confirmed | `TrashStation.cs`, `TrashPlate.cs` |
| `Assets/Engineering/Scripts/Mono/Actors/Table/` | `Table` — seat management and leftover-waste delegation to `TableWasteModel` and `TableWasteVisuals`. `TableManager` — cross-table reservation, release, and `AddLeftoversToTable` API. `TableWasteVisuals` — pooled leftover visual stack with configurable anchor and spacing. | Confirmed | `Table.cs`, `TableManager.cs`, `TableWasteVisuals.cs` |
| `Assets/Engineering/ScriptableObjects/` | First-party ScriptableObject definitions for economy, station, camera, animation, and event-channel tuning | Confirmed | Folder and source inventory |
| `Assets/Engineering/Prefabs/` | Player, purchase-area, animated-money, ground-money, pizza-maker, serving-station, customer-bot, trash-station, placeholder-pizza, and leftover prefabs | Confirmed | Prefab inventory and serialized script-reference inspection |
| `Assets/Scenes/` | Authored scene assets; contains `MainScene.unity` | Confirmed | File inventory |
| `Assets/Settings/` | Project visual/render-pipeline configuration assets | Likely | Folder name plus URP project configuration |
| `Assets/Art/` | Art assets | Likely | Folder name; contents not inspected |
| `Assets/TutorialInfo/` | Unity template tutorial/readme content | Confirmed | `Readme.cs`, `ReadmeEditor.cs` |

## Assembly Boundaries

`Engineering.Domain.asmdef` compiles pure C# domain models with no Engine references (`noEngineReferences: true`). `Engineering.ScriptableObjects.asmdef` compiles ScriptableObject assets and references Domain. `Engineering.Runtime.asmdef` compiles all MonoBehaviours and references Domain + ScriptableObjects + `Unity.InputSystem`. `Engineering.Tests.Editor.asmdef` and `Engineering.Tests.PlayMode.asmdef` reference all three runtime assemblies plus Unity Test Framework test assemblies, keeping tests separated from player code.

## Scenes And Startup Flow

- **Enabled build scene:** `Assets/Scenes/MainScene.unity` is the single enabled build scene in `ProjectSettings/EditorBuildSettings.asset`. The stale `SampleScene.unity` entry has been removed.
- **Scene asset on disk:** `Assets/Scenes/MainScene.unity`. The scene contains a baked NavMeshSurface (`Assets/Scenes/MainScene_NavMeshData.asset`) covering SpawnPoint, two approach waypoints, and CustomerSlot_0–9. Floor plane scaled to (1,1,4) so the 10 queue slots (Z=-3.74 to Z=-17.24) rest on walkable ground.
- **Startup scene:** `Assets/Scenes/MainScene.unity`.
- **Scene loading flow:** Unknown; no first-party scene-loading code was found in the inspected scripts.

## Architecture

| Pattern | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Runtime style | MonoBehaviour-centric | Confirmed | First-party gameplay sources |
| Input flow | Central input adapter publishes C# events to consumers | Confirmed | `InputManager.cs`, `PlayerMovement.cs` |
| Global state | `MainSceneInstaller` is the composition root for `MainScene`. Scene-object dependencies are injected through public `Initialize` methods during `Awake`. No `DontDestroyOnLoad` or static singletons. | Confirmed | `MainSceneInstaller.cs`, component sources |
| Player movement | Rigidbody velocity set in `FixedUpdate`, camera-relative | Confirmed | `PlayerMovement.cs` |
| Camera/input boundary | `InputManager` publishes `LookChanged` from `Player/Look`, with bindings for gamepad right stick, pointer delta, and joystick hat switch. No first-party consumer currently subscribes to that event. A separate `CameraManager` child of `Managers` follows the Player in `LateUpdate` through explicit `target` and `cameraTransform` references, applying per-axis `SmoothDamp` damping while preserving the initial camera rotation and position offset. Damping tuning values (xDamping, yDamping, zDamping, maxFollowSpeed) are stored in a `SCameraSettings` ScriptableObject asset (`Assets/Engineering/ScriptableObjects/SCameraSettings.asset`) and validated via `OnValidate` — damping values are clamped to >= 0.001 and `maxFollowSpeed <= 0` means unlimited speed. `PlayerMovement` reads an Inspector-assigned `cameraTransform`; MainScene overrides it with `Main Camera`. | Confirmed | `InputManager.cs`, `PlayerMovement.cs`, `CameraManager.cs`, `SCameraSettings.cs`, `SCameraSettings.asset`, `Assets/InputSystem_Actions.inputactions`, `Assets/Scenes/MainScene.unity`, `Assets/Engineering/Prefabs/Player.prefab` |
| Economy | ScriptableObject-configured payment rate, coroutine-based purchase areas, trigger-based ground-money collection, pizza-serving money rewards, and the scene-authored `CurrencyService` bridge that owns the live `PlayerWallet` reference. All cross-scene dependencies are injected through public `Initialize` methods called by `MainSceneInstaller`. | Confirmed | `SEconomy.cs`, `SAnimation.cs`, `CurrencyService.cs`, `EconomyManager.cs`, `BuyingArea.cs`, `MoneyToCollect.cs`, `ServeStation.cs`, `MainSceneInstaller.cs` |
| Pizza production | Each `GrillStation` produces independently up to a ScriptableObject-configured capacity; `GrillPlate` collects ready pizzas into the player inventory | Confirmed | `SGrillStation.cs`, `GrillStation.cs`, `GrillPlate.cs`, `PlayerPizzaInventory.cs` |
| Pizza serving | `ServeStationModel` is the authoritative owner of stored-pizza state, deposit, serve, completion, and reward calculations. `ServeStation` is the Unity adapter that delegates to `ServeStationModel` and coordinates `CustomerQueueController` (queue ownership, registration, slot assignment, front-customer removal, arbitrary-customer `RemoveCustomer`) and `ServeStationVisuals` (pizza visual pool creation and positioning). `CustomerQueueController` owns `CustomerQueueModel`, the ordered `List<CustomerBot>`, queue-slot assignment, and enforces queue capacity. `ServeStationVisuals` owns the pizza visual pool and stack positioning based on the plate collider. `TryRegisterCustomer` delegates to `CustomerQueueController.TryRegister`. `DepositFrom` transfers player pizzas into station storage (no money, no event), then refreshes visuals. `ServeFrontCustomer` (no-arg) delegates to `ServeStationModel.TryServe`, awards money, raises `PizzaServed`, removes completed front customer via `CustomerQueueController.RemoveFrontCustomer`, and refreshes visuals. `ServeStation` receives its `TableManager` dependency at runtime through `Initialize` (not serialized) and subscribes to both `SeatReleased` and `LeftoversRemoved` in `OnEnable`/`OnDisable`. On either event it retries the first waiting customer via `OnRetryWaitingCustomer`. `PlateTrigger` deposits on enter; `ServeTrigger` serves on enter/stay. | Confirmed | `ServeStationModel.cs`, `ServeResult.cs`, `SServeStation.cs`, `ServeStation.cs`, `PlateTrigger.cs`, `ServeTrigger.cs`, `CustomerQueueController.cs`, `ServeStationVisuals.cs`, `PlayerPizzaInventory.cs`, `EconomyManager.cs`, `CustomerBot.cs` |
| Purchase progress | `PurchaseProgressModel` is the authoritative owner of unlock price, paid amount, remaining amount, and purchase completion state. `BuyingArea` is the Unity trigger adapter that delegates state to `PurchaseProgressModel`; `BuyingArea._unlockPrice` is Inspector-configured with `[SerializeField] private` and defaults to 100. `EconomyManager` remains the coroutine, currency, animation-request, destruction, and purchased-event orchestrator. Final-payment capping is not part of the current behavior-preserving design. | Confirmed | `PurchaseProgressModel.cs`, `PurchaseProgressResult.cs`, `BuyingArea.cs`, `EconomyManager.cs` |
| Customer queue | `CustomerBot` is the NavMesh/Transform adapter: traverses approach waypoints then moves to its assigned queue slot via `NavMeshAgent`. `CustomerSpawner` runs a timed coroutine, randomizing orders within configured min/max and respecting station capacity. | Confirmed | `CustomerBot.cs`, `CustomerSpawner.cs`, `SServeStation.cs` |
| Pizza trash | `TrashStation` removes all pizzas from the player, retrieves visuals from an internal `ObjectPool<GameObject>`, positions them at the player's pizza stack world positions, animates them to `TrashTarget` with staggered DoTween (`DOMove` + `DOScale(0)`), then releases them back to the pool; raises `PizzaTrashed` event for SFX | Confirmed | `STrashStation.cs`, `TrashStation.cs`, `TrashPlate.cs`, `PlayerPizzaInventory.cs` |
| Leftover waste | `TableWasteModel` is the authoritative owner of leftover count. `Table` delegates leftover state to `TableWasteModel` and optional `TableWasteVisuals`; raises `LeftoverStateChanged` on `TryRemoveLeftovers`/`ClearLeftovers`. `TableManager` exposes `AddLeftoversToTable(tableIndex, pizzaCount)`; raises `SeatReleased` on seat release and `LeftoversRemoved` on `Table.LeftoverStateChanged`; subscribes to `Table.LeftoverStateChanged` in `Awake`. When `CustomerBot` transitions from Eating to Leaving, it calls `AddLeftoversToTable` with `_orderModel.InitialPizzaCount` before releasing the seat. `TableWasteVisuals` manages a pooled leftover visual stack from an anchor with configurable spacing. No leftovers are created if the customer is destroyed or fails to reach the table. | Confirmed | `TableWasteModel.cs`, `AddLeftoversResult.cs`, `TableWasteVisuals.cs`, `Table.cs`, `TableManager.cs`, `CustomerBot.cs` |
| Pizza presentation | The player inventory maintains a pooled overhead pizza stack; the oven stack starts at the plate collider's world-space top surface; a typed inventory-count Event Channel updates UI | Confirmed | `GrillStation.cs`, `GrillPlate.cs`, `PlayerPizzaInventory.cs`, `SIntEventChannel.cs`, `UIManager.cs` |
| Gameplay feedback | `EconomyManager` raises `GroundMoneyCollected` and `BuyingAreaPurchased`; `ServeStation` raises `PizzaServed`; `TrashStation` raises `PizzaTrashed`; `SoundManager` subscribes and maps them to `SSound` clips through `SVoidEventChannel` assets; `AnimationManager` and `EconomyManager` share the `MoneyAnimationRequested.asset` channel in MainScene | Confirmed | `EconomyManager.cs`, `ServeStation.cs`, `TrashStation.cs`, `SoundManager.cs`, `SMoneyAnimationEventChannel.cs`, `MoneyAnimationRequest.cs` |
| Presentation | UI money text is updated by `UIManager`; `AnimationManager` pools money objects and animates them with DOTween | Confirmed | `UIManager.cs`, `AnimationManager.cs` |
| Networking | No first-party networking usage found | Unknown | Package inventory and inspected gameplay sources |
| Persistence/save | No save system found in inspected sources | Unknown | Inspected gameplay source set |

## Coding Conventions

- **Namespace style:** `Engineering.*`, aligned with folder roles. All namespaces follow `Engineering.Scripts.Mono.<Folder>` matching directory structure (e.g., `Engineering.Scripts.Mono.Actors.GrillStation`, `Engineering.Scripts.Mono.Items`). No `Engineering.Engineering` duplication; legacy `Actors.PizzaMaker` namespace has been replaced with `Actors.GrillStation`.
- **Serialized fields:** Mostly `[SerializeField] private`.
- **Private fields:** Predominantly `_camelCase`; retain nearby-file style where it differs.
- **Async:** Coroutine-based delayed behavior (`IEnumerator` / `WaitForSeconds`); no async/await found in inspected sources.
- **Events:** C# events named after state/action (`MoveChanged`, `SprintStarted`); subscriptions are paired with unsubscriptions in `OnEnable`/`OnDisable`.
- **Comments/docs:** No established XML documentation or extensive comments convention found.

## Testing And Validation

- **EditMode tests:** 294 `[Test]` declarations in `Assets/Engineering/Tests/Editor/`; they cover wallet, trigger relays, movement, camera follow and `SCameraSettings` tuning, ground-money prefab configuration, Event Channel listener registration, pizza inventory capacity (TryAdd/TryRemove), the plate-collider stack origin, build scene configuration, SoundManager pizzaServedEvent serialized reference in MainScene, production MainScene economy wiring, ServeStationModel deposit/serve/reward/completion calculations, PurchaseProgressModel constructor/payment/completion rules, CustomerQueueModel enqueue/remove-front/capacity rules, CustomerOrderModel receive/completion rules, PizzaInventoryModel domain rules, GrillStationModel domain rules, PaymentSessionModel domain rules, WalletModel domain rules, TableModel rules, TableWasteModel leftover-count rules, an architecture guard verifying every domain `.cs` file has no UnityEngine dependency, and a Player prefab contract test (`PlayerPrefabContractTests`) validating the Scripts/Mesh hierarchy, inventory placement on Scripts, CapsuleCollider on Mesh, Player tag, and PlayerTriggerRelay wiring.
- **PlayMode tests:** 174 `[UnityTest]` declarations in `Assets/Engineering/Tests/PlayMode/`; they cover payment, purchase-area removal, purchase progress persistence, partial payment state, zero/negative payment safety, disable/enable retention, re-entry after cancellation, pickup collection/UI updates, player-only collection, duplicate-trigger protection, moving-player animation targeting, economy Event Channel publication, pizza production/partial collection, pizza serving with customer queue (front-customer delivery, partial delivery, completed-order removal, slot guard, capacity, money, events, trigger flow, player/non-player tag filtering), customer spawner (order range, capacity enforcement, disabled cleanup), table flow (leftover accumulation, eating→leftovers integration, no-leftovers-on-destroy), pizza trashing (removal, events, animation, guard conditions, trigger flow, pooled visual reuse, release, reset, and destruction cleanup), UI scene-object wiring, money-animation pool reuse/cleanup, disable/enable storage retention, runtime price-change integration, and Player trigger regression tests (`PlayerTriggerRegressionTests`) with contract-level and real-physics regression for GrillPlate, PlateTrigger, TrashPlate, and TableWasteTrigger against an instantiated Player.prefab clone.
- **CI/build validation:** GitHub Actions workflow at `.github/workflows/unity-tests.yml` runs on pushes to `dev`/`main` and PRs targeting `main`. Separate `editmode-tests` and `playmode-tests` jobs use Unity `6000.3.20f1` via GameCI (`game-ci/unity-test-runner@v4`). Test result XML files are uploaded as artifacts. The workflow requires `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` repository secrets; it fails early with a clear message if any is missing.
- **Recommended minimum validation:** Run both test suites, then manually exercise the scene physical trigger, camera, and input wiring in Play Mode. After build-scene or SoundManager changes, also run `SceneConfigurationTests` EditMode suite.

## Available Unity Tooling

| Capability | Status | Evidence |
| --- | --- | --- |
| Unity MCP/editor inspection | unavailable | No Unity MCP capability was available to this analysis session; no project MCP provider was found in package/configuration inspection. |
| Rider integration | available in project | `com.unity.ide.rider` in `Packages/manifest.json` |
| Visual Studio integration | available in project | `com.unity.ide.visualstudio` in `Packages/manifest.json` |
| Unity Test Framework | available in project | `com.unity.test-framework` in `Packages/manifest.json` |

## Important Constraints

- Do not modify generated directories (`Library/`, `Temp/`, `Logs/`, `obj/`, build output, `UserSettings/`).
- Preserve Input System action names and the `Player` tag unless all consumers and serialized references are migrated deliberately. The `Player` map also currently includes `Crouch` and `Jump`, though no inspected runtime consumer uses them.
- Verify the Build Settings scene list in Unity before relying on it or changing it.
- Do not infer that installed packages are actively used without source/asset evidence.
- All managers are plain scene objects with `[SerializeField]` references wired through MainScene. Additive or replacement scene loading has not been validated.

## Unknowns And Confidence

- The startup scene is `MainScene.unity` as the single enabled build scene.
- The current camera is an orthographic scene camera with per-axis SmoothDamp Player follow; look input is already relayed but camera rotation and zoom behavior are not implemented.
- `MainScene` now assigns `pizzaServedEvent` on both `ServingStation` (via prefab reference) and `SoundManager` (scene override); the configured serve SFX is received and played.
- No Unity MCP provider or Editor-console capability was available to this audit. Full current test execution, Console inspection, and Play Mode verification remain unrecorded.
- The project is likely Android-focused, based on explicit Android settings, but release targets are not confirmed.
- Camera follow and damping have been manually verified by the user; independent automated test execution is not recorded in this context. Occlusion handling, touch-camera UX, and zoom limits remain design/implementation decisions.

## Source Files Inspected

- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `Assets/Engineering/Scripts/Mono/Managers/InputManager.cs`
- `Assets/Engineering/Scripts/Mono/Managers/CameraManager.cs`
- `Assets/Engineering/ScriptableObjects/SCameraSettings.cs`
- `Assets/Engineering/ScriptableObjects/SCameraSettings.asset`
- `Assets/Engineering/Scripts/Mono/Player/PlayerMovement.cs`
- `Assets/Engineering/Prefabs/Player.prefab`
- `Assets/InputSystem_Actions.inputactions`
- `Assets/Scenes/MainScene.unity`
- `Assets/Engineering/Scripts/Mono/Managers/EconomyManager.cs`
- `Assets/Engineering/Scripts/Mono/Managers/UIManager.cs`
- `Assets/Engineering/Scripts/Mono/Managers/AnimationManager.cs`
- `Assets/Engineering/Scripts/Mono/Player/PlayerWallet.cs`
- `Assets/Engineering/Scripts/Mono/Player/PlayerPizzaInventory.cs`
- `Assets/Engineering/Scripts/Mono/Player/PlayerTrigger.cs`
- `Assets/Engineering/Scripts/Mono/Player/PlayerTriggerRelay.cs`
- `Assets/Engineering/Scripts/Mono/Areas/BuyingArea.cs`
- `Assets/Engineering/Scripts/Mono/Items/MoneyToCollect.cs`
- `Assets/Engineering/Scripts/Class/ETriggerAreas.cs`
- `Assets/Engineering/ScriptableObjects/SEconomy.cs`
- `Assets/Engineering/ScriptableObjects/SAnimation.cs`
- `Assets/Engineering/ScriptableObjects/SGrillStation.cs`
- `Assets/Engineering/ScriptableObjects/SServeStation.cs`
- `Assets/Engineering/ScriptableObjects/STrashStation.cs`
- `Assets/Engineering/ScriptableObjects/SIntEventChannel.cs`
- `Assets/Engineering/Scripts/Domain/ServeStation/ServeResult.cs`
- `Assets/Engineering/Scripts/Domain/ServeStation/ServeStationModel.cs`
- `Assets/Engineering/Scripts/Mono/Actors/GrillStation/GrillStation.cs`
- `Assets/Engineering/Scripts/Mono/Actors/GrillStation/GrillPlate.cs`
- `Assets/Engineering/Scripts/Mono/Actors/ServeStation/ServeStation.cs`
- `Assets/Engineering/Scripts/Mono/Actors/ServeStation/CustomerQueueController.cs`
- `Assets/Engineering/Scripts/Mono/Actors/ServeStation/ServeStationVisuals.cs`
- `Assets/Engineering/Scripts/Mono/Actors/ServeStation/PlateTrigger.cs`
- `Assets/Engineering/Scripts/Mono/Actors/ServeStation/ServeTrigger.cs`
- `Assets/Engineering/Scripts/Mono/Actors/CustomerQueue/CustomerBot.cs`
- `Assets/Engineering/Scripts/Mono/Actors/CustomerQueue/CustomerSpawner.cs`
- `Assets/Engineering/Scripts/Mono/Actors/TrashStation/TrashStation.cs`
- `Assets/Engineering/Scripts/Mono/Actors/TrashStation/TrashPlate.cs`
- `Assets/Engineering/ScriptableObjects/SVoidEventChannel.cs`
- `Assets/Engineering/ScriptableObjects/Events/PizzaInventoryChanged.asset`
- `Assets/Engineering/ScriptableObjects/SGrillStation.asset`
- `Assets/Engineering/ScriptableObjects/Events/GroundMoneyCollected.asset`
- `Assets/Engineering/ScriptableObjects/Events/BuyingAreaPurchased.asset`
- `Assets/Engineering/ScriptableObjects/Events/PizzaServed.asset`
- `Assets/InputSystem_Actions.inputactions`
- `Assets/Engineering/Prefabs/Player.prefab`
- `Assets/Engineering/Prefabs/PizzaMaker.prefab`
- `Assets/Engineering/Prefabs/PizzaVisual.prefab`
- `Assets/Engineering/Prefabs/ServingStation.prefab`
- `Assets/Engineering/Prefabs/TrashStation.prefab`
- `Assets/Engineering/Prefabs/MoneyArea.prefab`
- `Assets/Engineering/Prefabs/MoneyToCollect.prefab`
- `Assets/Engineering/Scripts/Domain/Engineering.Domain.asmdef`
- `Assets/Engineering/ScriptableObjects/Engineering.ScriptableObjects.asmdef`
- `Assets/Engineering/Scripts/Mono/Engineering.Runtime.asmdef`
- `Assets/Engineering/Tests/Editor/CoreGameplayTests.cs`
- `Assets/Engineering/Tests/Editor/CameraManagerEditModeTests.cs`
- `Assets/Engineering/Tests/Editor/SCameraSettingsEditModeTests.cs`
- `Assets/Engineering/Tests/Editor/ServeStationModelTests.cs`
- `Assets/Engineering/Tests/Editor/PlayerPizzaInventoryTests.cs`
- `Assets/Engineering/Tests/PlayMode/EconomyPaymentPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/UIManagerPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/MoneyAnimationPoolPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/GrillStationPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/ServeStationPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/TrashStationPlayModeTests.cs`
- `Assets/Engineering/Scripts/Domain/Table/TableWasteModel.cs`
- `Assets/Engineering/Scripts/Domain/Table/AddLeftoversResult.cs`
- `Assets/Engineering/Scripts/Mono/Actors/Table/TableWasteVisuals.cs`
- `Assets/Engineering/Tests/Editor/TableWasteModelTests.cs`
- `Assets/Engineering/Tests/Editor/PlayerPrefabContractTests.cs`
- `Assets/Engineering/Tests/PlayMode/PlayerPrefabPrebuildSetup.cs`
- `Assets/Engineering/Tests/PlayMode/PlayerPrefabTestFixture.cs`
- `Assets/Engineering/Tests/PlayMode/PlayerTriggerRegressionTests.cs`
- `.github/workflows/unity-tests.yml`

<!-- unity-onboarding:generated:end -->
