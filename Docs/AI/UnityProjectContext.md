# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- **Project root:** repository root
- **Last analyzed:** 2026-07-25
- **Last analyzed commit:** `db5d41d`
- **Summary:** Early-stage casual 3D game named *My Pizza Shop*. The current gameplay slice includes movement, wallet and ground-money collection, timed area purchases, autonomous pizza production and collection, player pizza stacks, pizza serving station with customer queue and money reward, trash station with DoTween fly-and-shrink animation, UI counters, pooled DOTween money-transfer effects, customer bot NavMesh movement, timed customer spawner, and ScriptableObject event channels for decoupled gameplay feedback.

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
| Navigation | AI Navigation 2.0.13 is installed and used by `CustomerBot` for NavMesh movement; MainScene has a baked NavMeshSurface covering SpawnPoint, 2 waypoints, and 10 queue slots | Confirmed | `Packages/manifest.json`, `CustomerBot.cs`, `Assets/Scenes/MainScene_NavMeshData.asset` |
| UI | UGUI 2.0.0 is installed; project UI usage not inspected | Confirmed / unknown usage | `Packages/manifest.json` |
| Tests | Unity Test Framework 1.6.0 is installed; source declares 109 EditMode and 69 PlayMode test cases covering core gameplay, UI, economy, money-animation pooling, pizza inventory, grill production, serving station with customer queue, and trash station | Confirmed; execution verified: EditMode 89 passed, 0 failed, 0 skipped; PlayMode 65 passed, 0 failed, 0 skipped | `Packages/manifest.json`, `Assets/Engineering/Tests/` |
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
| `Assets/Engineering/Scripts/Mono/Actors/ServeStation/` | Player-to-station pizza storage/serving with customer queue, money reward, and separate deposit/serve trigger relays. `ServeStation` delegates storage and calculations to `ServeStationModel`. | Confirmed | `ServeStation.cs`, `PlateTrigger.cs`, `ServeTrigger.cs` |
| `Assets/Engineering/Scripts/Mono/Actors/CustomerQueue/` | Customer bot NavMesh movement and timed customer spawner | Confirmed | `CustomerBot.cs`, `CustomerSpawner.cs` |
| `Assets/Engineering/Scripts/Mono/Actors/TrashStation/` | Player-to-station pizza disposal with DoTween animation | Confirmed | `TrashStation.cs`, `TrashPlate.cs` |
| `Assets/Engineering/ScriptableObjects/` | First-party ScriptableObject definitions for economy and animation tuning | Confirmed | `SEconomy.cs`, `SAnimation.cs` |
| `Assets/Engineering/Prefabs/` | Player, purchase-area, animated-money, ground-money, pizza-maker, serving-station, customer-bot, trash-station, and placeholder-pizza prefabs | Confirmed | Prefab inventory and serialized script-reference inspection |
| `Assets/Scenes/` | Authored scene assets; contains `MainScene.unity` | Confirmed | File inventory |
| `Assets/Settings/` | Project visual/render-pipeline configuration assets | Likely | Folder name plus URP project configuration |
| `Assets/Art/` | Art assets | Likely | Folder name; contents not inspected |
| `Assets/TutorialInfo/` | Unity template tutorial/readme content | Confirmed | `Readme.cs`, `ReadmeEditor.cs` |

## Assembly Boundaries

`Engineering.asmdef` compiles first-party runtime code into the `Engineering` assembly and explicitly references `Unity.InputSystem`. `Engineering.Tests.Editor.asmdef` and `Engineering.Tests.PlayMode.asmdef` reference that runtime assembly and Unity Test Framework test assemblies, keeping tests separated from player code.

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
| Economy | ScriptableObject-configured payment rate, coroutine-based purchase areas, trigger-based ground-money collection, pizza-serving money rewards, and the scene-authored `CurrencyService` bridge that owns the live `PlayerWallet` reference. All cross-scene dependencies are injected through public `Initialize` methods called by `MainSceneInstaller`. | Confirmed | `SEconomy.cs`, `SAnimation.cs`, `CurrencyService.cs`, `EconomyManager.cs`, `BuyingArea.cs`, `MoneyToCollect.cs`, `ServeStation.cs`, `MainSceneInstaller.cs` |
| Pizza production | Each `GrillStation` produces independently up to a ScriptableObject-configured capacity; `GrillPlate` collects ready pizzas into the player inventory | Confirmed | `SGrillStation.cs`, `GrillStation.cs`, `GrillPlate.cs`, `PlayerPizzaInventory.cs` |
| Pizza serving | `ServeStationModel` is the authoritative owner of stored-pizza state, deposit, serve, completion, and reward calculations. `ServeStation` is the Unity adapter that delegates to `ServeStationModel` and owns the ordered `List<CustomerBot>`, queue-slot assignment, visual GameObjects, event publication, currency calls, and customer destruction. `RegisterCustomer` assigns queue-slot transforms immediately. `TryDepositPizzas` transfers player pizzas into station storage (no money, no event). `TryServeFrontCustomer` (no-arg) delegates to `ServeStationModel.TryServe`, awards money, raises `PizzaServed`, and removes/reassigns slots on completion. `PlateTrigger` deposits on enter; `ServeTrigger` serves on enter/stay. Station has a pizza visual pool created in `OnEnable` from `pizzaVisualPrefab` and `pizzaStackSpacing`. | Confirmed | `ServeStationModel.cs`, `ServeResult.cs`, `SServeStation.cs`, `ServeStation.cs`, `PlateTrigger.cs`, `ServeTrigger.cs`, `PlayerPizzaInventory.cs`, `EconomyManager.cs`, `CustomerBot.cs` |
| Purchase progress | `PurchaseProgressModel` is the authoritative owner of unlock price, paid amount, remaining amount, and purchase completion state. `BuyingArea` is the Unity trigger adapter that delegates state to `PurchaseProgressModel`. `EconomyManager` remains the coroutine, currency, animation-request, destruction, and purchased-event orchestrator. Final-payment capping is not part of the current behavior-preserving design. | Confirmed | `PurchaseProgressModel.cs`, `PurchaseProgressResult.cs`, `BuyingArea.cs`, `EconomyManager.cs` |
| Customer queue | `CustomerBot` traverses approach waypoints then moves to its assigned queue slot via `NavMeshAgent`. `CustomerSpawner` runs a timed coroutine, randomizing orders within configured min/max and respecting station capacity. | Confirmed | `CustomerBot.cs`, `CustomerSpawner.cs`, `SServeStation.cs` |
| Pizza trash | `TrashStation` removes all pizzas from the player, spawns temp visuals at the player's pizza stack world positions, animates them to `TrashTarget` with staggered DoTween (`DOMove` + `DOScale(0)`), then destroys them; raises `PizzaTrashed` event for SFX | Confirmed | `STrashStation.cs`, `TrashStation.cs`, `TrashPlate.cs`, `PlayerPizzaInventory.cs` |
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

- **EditMode tests:** 109 `[Test]` declarations in `Assets/Engineering/Tests/Editor/`; they cover wallet, trigger relays, movement, ground-money prefab configuration, Event Channel listener registration, pizza inventory capacity (TryAdd/TryRemove), the plate-collider stack origin, build scene configuration, SoundManager pizzaServedEvent serialized reference in MainScene, production MainScene economy wiring, ServeStationModel deposit/serve/reward/completion calculations, PurchaseProgressModel constructor/payment/completion rules, and an architecture guard verifying domain files have no UnityEngine dependency.
- **PlayMode tests:** 69 `[UnityTest]` declarations in `Assets/Engineering/Tests/PlayMode/`; they cover payment, purchase-area removal, purchase progress persistence, partial payment state, zero/negative payment safety, disable/enable retention, re-entry after cancellation, pickup collection/UI updates, player-only collection, duplicate-trigger protection, moving-player animation targeting, economy Event Channel publication, pizza production/partial collection, pizza serving with customer queue (front-customer delivery, partial delivery, completed-order removal, slot guard, capacity, money, events, trigger flow, player/non-player tag filtering), customer spawner (order range, capacity enforcement, disabled cleanup), pizza trashing (removal, events, animation, guard conditions, trigger flow), UI scene-object wiring, money-animation pool reuse/cleanup, disable/enable storage retention, and runtime price-change integration.
- **CI/build validation:** None found.
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
- `MainScene` now assigns `pizzaServedEvent` on both `ServingStation` (via prefab reference) and `SoundManager` (scene override); the configured serve SFX is received and played.
- No Unity MCP provider or Editor-console capability was available to this audit. Full current test execution, Console inspection, and Play Mode verification remain unrecorded.
- The project is likely Android-focused, based on explicit Android settings, but release targets are not confirmed.

## Source Files Inspected

- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `Assets/Engineering/Scripts/Mono/Managers/InputManager.cs`
- `Assets/Engineering/Scripts/Mono/Managers/EconomyManager.cs`
- `Assets/Engineering/Scripts/Mono/Managers/UIManager.cs`
- `Assets/Engineering/Scripts/Mono/Managers/AnimationManager.cs`
- `Assets/Engineering/Scripts/Mono/Player/PlayerMovement.cs`
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
- `Assets/Engineering/Engineering.asmdef`
- `Assets/Engineering/Tests/Editor/CoreGameplayTests.cs`
- `Assets/Engineering/Tests/Editor/ServeStationModelTests.cs`
- `Assets/Engineering/Tests/Editor/PlayerPizzaInventoryTests.cs`
- `Assets/Engineering/Tests/PlayMode/EconomyPaymentPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/UIManagerPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/MoneyAnimationPoolPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/GrillStationPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/ServeStationPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/TrashStationPlayModeTests.cs`

<!-- unity-onboarding:generated:end -->
