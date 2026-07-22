# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- **Project root:** repository root
- **Last analyzed:** 2026-07-22
- **Last analyzed commit:** `419fbca`
- **Summary:** Early-stage casual 3D game named *My Pizza Shop*. Confirmed gameplay code implements movement, player money, timed purchases, and purchase-area triggers.

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
| Tests | Unity Test Framework 1.6.0 is installed; no first-party test assemblies/files found | Confirmed | `Packages/manifest.json`, `Assets/` file inventory |
| Other tooling | Timeline, Visual Scripting, Rider and Visual Studio integrations are installed; first-party usage is unverified | Confirmed / unverified usage | `Packages/manifest.json` |

## Directory Structure

| Path | Purpose | Confidence | Evidence |
| --- | --- | --- | --- |
| `Assets/Engineering/Scripts/Mono/` | First-party runtime MonoBehaviours: managers, player, and areas | Confirmed | Folder and source inventory |
| `Assets/Engineering/ScriptableObjects/` | First-party ScriptableObject definitions, currently economy tuning | Confirmed | `SEconomy.cs` |
| `Assets/Scenes/` | Authored scene assets; contains `MainScene.unity` | Confirmed | File inventory |
| `Assets/Settings/` | Project visual/render-pipeline configuration assets | Likely | Folder name plus URP project configuration |
| `Assets/Art/` | Art assets | Likely | Folder name; contents not inspected |
| `Assets/TutorialInfo/` | Unity template tutorial/readme content | Confirmed | `Readme.cs`, `ReadmeEditor.cs` |

## Assembly Boundaries

No first-party `.asmdef` or `.asmref` files were found. First-party code therefore compiles into Unity's default assemblies. There are no recorded editor-only or test assembly boundaries.

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
| Global state | `EconomyManager` is a `DontDestroyOnLoad` singleton | Confirmed | `EconomyManager.cs` |
| Player movement | Rigidbody velocity set in `FixedUpdate`, camera-relative | Confirmed | `PlayerMovement.cs` |
| Economy | ScriptableObject-configured rate/speed, coroutine-based payment into trigger area | Confirmed | `SEconomy.cs`, `EconomyManager.cs`, `BuyingArea.cs` |
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

- **EditMode tests:** None found.
- **PlayMode tests:** None found.
- **CI/build validation:** None found.
- **Recommended minimum validation:** Let Unity compile, check Console output, then test the changed gameplay flow in Play Mode.

## Available Unity Tooling

| Capability | Status | Evidence |
| --- | --- | --- |
| Unity MCP/editor inspection | unavailable | No Unity MCP capability was available to this analysis session; no project MCP provider was found in package/configuration inspection. |
| Rider integration | available in project | `com.unity.ide.rider` in `Packages/manifest.json` |
| Visual Studio integration | available in project | `com.unity.ide.visualstudio` in `Packages/manifest.json` |
| Unity Test Framework | available in project | `com.unity.test-framework` in `Packages/manifest.json` |

## Important Constraints

- Do not modify generated directories (`Library/`, `Temp/`, `Logs/`, `obj/`, build output, `UserSettings/`).
- Preserve Input System action names and the `Player` tag unless all consumers and serialized references are migrated deliberately.
- Verify the Build Settings scene list in Unity before relying on it or changing it.
- Do not infer that installed packages are actively used without source/asset evidence.

## Unknowns And Confidence

- The intended startup scene is **unknown** due to the `SampleScene`/`MainScene` mismatch.
- Scene hierarchy, prefab references, Input Action asset bindings, and runtime Console state were not inspected because no Unity Editor/MCP connection was available.
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
- `Assets/Engineering/Scripts/Mono/Player/PlayerMovement.cs`
- `Assets/Engineering/Scripts/Mono/Player/PlayerWallet.cs`
- `Assets/Engineering/Scripts/Mono/Areas/BuyingArea.cs`
- `Assets/Engineering/Scripts/Class/ETriggerAreas.cs`
- `Assets/Engineering/ScriptableObjects/SEconomy.cs`

<!-- unity-onboarding:generated:end -->
