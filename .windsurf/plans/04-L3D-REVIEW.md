# Project Structure Review — Lockdown-3D

## Folder Hierarchy

```
Lockdown-3D/
├── Assets/
│   ├── _Project/
│   │   ├── Scripts/
│   │   │   ├── Core/          (6 files — duplicated + PauseController)
│   │   │   ├── Editor/        (1 file — LockdownSetupMenu.cs)
│   │   │   ├── Player/        (1 file — FirstPersonController.cs)
│   │   │   ├── Audio/         (1 file — FootstepSystem.cs)
│   │   │   ├── Interaction/   (4 files — IInteractable, Interactor, CrosshairPrompt, Highlightable)
│   │   │   ├── Puzzles/       (9 files — 3D versions of puzzles)
│   │   │   └── UI/            (6 files — menus, HUD, results)
│   │   └── Settings/
│   │       └── PlayerInput.inputactions
│   ├── DefaultVolumeProfile.asset
│   └── UniversalRenderPipelineGlobalSettings.asset
├── Packages/
│   ├── manifest.json          (references com.hcitrilogy.core + Cinemachine + ProBuilder)
│   └── packages-lock.json
└── ProjectSettings/
```

## Missing Assets

| Asset | Status | Impact |
|---|---|---|
| `Assets/_Project/Scenes/` | **MISSING** | No scenes to play |
| `Assets/_Project/Audio/` | **MISSING** | No sound effects or music |
| `Assets/_Project/Art/` | **MISSING** | No materials (setup menu creates them) |
| `Assets/_Project/Prefabs/` | **MISSING** | No prefabs |
| AudioMixer asset | **MISSING** | Volume sliders have no effect |
| Settings Panel | **MISSING** | Settings button does nothing |
| Credits Panel | **MISSING** | Credits button does nothing |

## Script Inventory (22 scripts, ~1,613 lines)

| # | Script | Lines | Issues |
|---|---|---|---|
| 1 | `Core/Bootstrapper.cs` | 15 | Duplicate |
| 2 | `Core/LocalizationStrings.cs` | 22 | Duplicate; Lockdown-specific |
| 3 | `Core/PauseController.cs` | 59 | Sets Time.timeScale=0 (works but fragile) |
| 4 | `Core/SceneFlow.cs` | 70 | Duplicate; uses UI Image fade |
| 5 | `Core/ServiceLocator.cs` | 24 | Exact duplicate |
| 6 | `Core/SettingsManager.cs` | 68 | Duplicate; FPS-specific settings |
| 7 | `Editor/LockdownSetupMenu.cs` | 775 | Missing settings/credits panels |
| 8 | `Player/FirstPersonController.cs` | 127 | Clean; well-structured |
| 9 | `Audio/FootstepSystem.cs` | 40 | Clean |
| 10 | `Interaction/IInteractable.cs` | 17 | Clean interface |
| 11 | `Interaction/Interactor.cs` | 78 | Clean |
| 12 | `Interaction/CrosshairPrompt.cs` | 27 | Clean |
| 13 | `Interaction/Highlightable.cs` | 54 | Clean; uses MaterialPropertyBlock |
| 14 | `Puzzles/Cable.cs` | 17 | Clean |
| 15 | `Puzzles/CableSocket.cs` | 58 | Clean |
| 16 | `Puzzles/Dial.cs` | 93 | Clean |
| 17 | `Puzzles/DoorLock.cs` | 64 | Clean |
| 18 | `Puzzles/Drawer.cs` | 61 | Clean |
| 19 | `Puzzles/Keypad.cs` | 75 | Clean |
| 20 | `Puzzles/KeypadButton.cs` | 28 | Clean |
| 21 | `Puzzles/NoteItem.cs` | 26 | Clean |
| 22 | `Puzzles/Pickupable.cs` | 74 | Clean |
| 23 | `Puzzles/PuzzleStateMachine.cs` | 51 | Clean |
| 24 | `UI/MainMenuController.cs` | 39 | Missing settings/credits panel refs |
| 25 | `UI/LabSceneController.cs` | 52 | Clean |
| 26 | `UI/NoteReader.cs` | 89 | Clean |
| 27 | `UI/OxygenTimer.cs` | 43 | Clean |
| 28 | `UI/PauseMenu.cs` | 33 | Clean |
| 29 | `UI/ResultsScreen.cs` | 29 | Clean |

## Detailed Code Review

### PauseController vs OxygenTimer

`PauseController` sets `Time.timeScale = 0` when paused. `OxygenTimer` uses `Time.deltaTime` for countdown. When paused, the timer correctly stops (desired). However, this is fragile — if any system switches to `Time.unscaledDeltaTime`, it would continue running during pause.

### MainMenuController.cs — Missing Panel References

Same issue as Containment-VR: `settingsPanel` and `creditsPanel` are null at runtime.

### LockdownSetupMenu.cs — Missing Panels

The 775-line setup menu creates Boot, MainMenu, Lab, and Results scenes with full puzzle wiring, but does NOT create:
- Settings panel (with volume/sensitivity sliders)
- Credits panel (with attribution text)
- AudioMixer asset

### FirstPersonController.cs — Well Structured

Uses `CharacterController`, integrates with `PauseController` and `SettingsManager`, handles movement/look/jump/headbob. No issues found.

### Interaction System — Clean Design

`IInteractable` interface with `Prompt`, `IsAvailable`, `Interact()`, `Hover()` is well-designed. `Interactor` raycasts forward from camera, `Highlightable` uses `MaterialPropertyBlock` for efficient highlighting. `CrosshairPrompt` provides visual feedback.
