# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- **Project root:** repository root
- **Last analyzed:** 2026-07-23
- **Last analyzed commit:** `db88ad9`
- **Summary:** Early-stage casual 3D game named *My Pizza Shop*. The current gameplay slice includes movement, wallet and ground-money collection, timed area purchases, UI money display, pooled DOTween money-transfer effects, and ScriptableObject event channels for decoupled gameplay feedback.

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
| Navigation | AI Navigation 2.0.13 is installed; gameplay usage not found in inspected sources | Confirmed / unknown usage | `Packages/manifest.json` |
| UI | UGUI 2.0.0 is installed; project UI usage not inspected | Confirmed / unknown usage | `Packages/manifest.json` |
| Tests | Unity Test Framework 1.6.0 is installed; 10 EditMode and 17 PlayMode tests cover core gameplay, UI, economy, and money-animation pooling | Confirmed | `Packages/manifest.json`, `Assets/Engineering/Tests/` |
| Tweening | DOTween is included as a vendor plugin and actively used for money-transfer animation | Confirmed | `Assets/Plugins/Demigiant/DOTween/`, `AnimationManager.cs` |
| Gameplay events | Parameterless ScriptableObject event channels decouple economy feedback from audio playback | Confirmed | `VoidEventChannel.cs`, event assets, `EconomyManager.cs`, `SoundManager.cs` |
| Other tooling | Timeline, Visual Scripting, Rider and Visual Studio integrations are installed; first-party usage is unverified | Confirmed / unverified usage | `Packages/manifest.json` |

## Directory Structure

| Path | Purpose | Confidence | Evidence |
| --- | --- | --- | --- |
| `Assets/Engineering/Scripts/Mono/` | First-party runtime MonoBehaviours: managers, player, areas, and collectible items | Confirmed | Folder and source inventory |
| `Assets/Engineering/ScriptableObjects/` | First-party ScriptableObject definitions for economy and animation tuning | Confirmed | `SEconomy.cs`, `SAnimation.cs` |
| `Assets/Engineering/Prefabs/` | Player, purchase-area, animated-money, and ground-money prefabs | Confirmed | Prefab inventory and serialized script-reference inspection |
| `Assets/Scenes/` | Authored scene assets; contains `MainScene.unity` | Confirmed | File inventory |
| `Assets/Settings/` | Project visual/render-pipeline configuration assets | Likely | Folder name plus URP project configuration |
| `Assets/Art/` | Art assets | Likely | Folder name; contents not inspected |
| `Assets/TutorialInfo/` | Unity template tutorial/readme content | Confirmed | `Readme.cs`, `ReadmeEditor.cs` |

## Assembly Boundaries

`Engineering.asmdef` compiles first-party runtime code into the `Engineering` assembly and explicitly references `Unity.InputSystem`. `Engineering.Tests.Editor.asmdef` and `Engineering.Tests.PlayMode.asmdef` reference that runtime assembly and Unity Test Framework test assemblies, keeping tests separated from player code.

## Scenes And Startup Flow

- **Enabled build scene:** `Assets/Scenes/SampleScene.unity` in `ProjectSettings/EditorBuildSettings.asset`.
- **Scene asset found on disk:** `Assets/Scenes/MainScene.unity`.
- **Likely startup scene:** Unknown. The enabled build-scene path appears stale because `SampleScene.unity` was not found.
- **Scene loading flow:** Unknown; no first-party scene-loading code was found in the inspected scripts.

## Architecture

| Pattern | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Runtime style | MonoBehaviour-centric | Confirmed | First-party gameplay sources |
| Input flow | Central input adapter publishes C# events to consumers | Confirmed | `InputManager.cs`, `PlayerMovement.cs` |
| Global state | `EconomyManager`, `UIManager`, and `AnimationManager` are `DontDestroyOnLoad` singletons | Confirmed | Manager sources |
| Player movement | Rigidbody velocity set in `FixedUpdate`, camera-relative | Confirmed | `PlayerMovement.cs` |
| Economy | ScriptableObject-configured payment rate, coroutine-based purchase areas, and trigger-based ground-money collection | Confirmed | `SEconomy.cs`, `SAnimation.cs`, `EconomyManager.cs`, `BuyingArea.cs`, `MoneyToCollect.cs` |
| Gameplay feedback | `EconomyManager` raises `GroundMoneyCollected` and `BuyingAreaPurchased`; `SoundManager` subscribes and maps them to `SSound` clips | Confirmed | `EconomyManager.cs`, `SoundManager.cs`, `VoidEventChannel.cs` |
| Presentation | UI money text is updated by `UIManager`; `AnimationManager` pools money objects and animates them with DOTween | Confirmed | `UIManager.cs`, `AnimationManager.cs` |
| Networking | No first-party networking usage found | Unknown | Package inventory and inspected gameplay sources |
| Persistence/save | No save system found in inspected sources | Unknown | Inspected gameplay source set |

## Coding Conventions

- **Namespace style:** `Engineering.*`, aligned with folder roles.
- **Serialized fields:** Mostly `[SerializeField] private`.
- **Private fields:** Predominantly `_camelCase`; retain nearby-file style where it differs.
- **Async:** Coroutine-based delayed behavior (`IEnumerator` / `WaitForSeconds`); no async/await found in inspected sources.
- **Events:** C# events named after state/action (`MoveChanged`, `SprintStarted`); subscriptions are paired with unsubscriptions in `OnEnable`/`OnDisable`.
- **Comments/docs:** No established XML documentation or extensive comments convention found.

## Testing And Validation

- **EditMode tests:** 12 tests in `Assets/Engineering/Tests/Editor/`; they cover wallet, trigger relays, movement, ground-money prefab configuration, and Event Channel listener registration.
- **PlayMode tests:** 19 tests in `Assets/Engineering/Tests/PlayMode/`; they cover payment, purchase-area removal, pickup collection/UI updates, player-only collection, duplicate-trigger protection, moving-player animation targeting, economy Event Channel publication, UI singleton behavior, and money-animation pool reuse/cleanup.
- **CI/build validation:** None found.
- **Validated commands:** EditMode (`-testPlatform EditMode`) 10/10 passed; PlayMode (`-testPlatform PlayMode`) 17/17 passed on 2026-07-23.
- **Recommended minimum validation:** Run both test suites, then manually exercise the scene physical trigger, camera, and input wiring in Play Mode.

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
- `AGENTS.md` describes `EconomyManager` as the sole persistent singleton, but current code also makes `UIManager` and `AnimationManager` persistent singletons; treat the code as authoritative until that guide is reconciled.

## Unknowns And Confidence

- The intended startup scene is **unknown** due to the `SampleScene`/`MainScene` mismatch.
- Scene hierarchy and runtime Console state were not inspected because no Unity Editor/MCP connection was available. Serialized references for the gameplay prefabs and action-map definitions were inspected from disk.
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
- `Assets/Engineering/Scripts/Mono/Player/PlayerTrigger.cs`
- `Assets/Engineering/Scripts/Mono/Player/PlayerTriggerRelay.cs`
- `Assets/Engineering/Scripts/Mono/Areas/BuyingArea.cs`
- `Assets/Engineering/Scripts/Mono/Items/MoneyToCollect.cs`
- `Assets/Engineering/Scripts/Class/ETriggerAreas.cs`
- `Assets/Engineering/ScriptableObjects/SEconomy.cs`
- `Assets/Engineering/ScriptableObjects/SAnimation.cs`
- `Assets/Engineering/ScriptableObjects/VoidEventChannel.cs`
- `Assets/Engineering/ScriptableObjects/Events/GroundMoneyCollected.asset`
- `Assets/Engineering/ScriptableObjects/Events/BuyingAreaPurchased.asset`
- `Assets/InputSystem_Actions.inputactions`
- `Assets/Engineering/Prefabs/Player.prefab`
- `Assets/Engineering/Prefabs/MoneyArea.prefab`
- `Assets/Engineering/Prefabs/MoneyToCollect.prefab`
- `Assets/Engineering/Engineering.asmdef`
- `Assets/Engineering/Tests/Editor/CoreGameplayTests.cs`
- `Assets/Engineering/Tests/PlayMode/EconomyPaymentPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/UIManagerPlayModeTests.cs`
- `Assets/Engineering/Tests/PlayMode/MoneyAnimationPoolPlayModeTests.cs`

<!-- unity-onboarding:generated:end -->
