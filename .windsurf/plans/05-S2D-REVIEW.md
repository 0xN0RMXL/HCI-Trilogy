# Project Structure Review — Signal-2D

## Folder Hierarchy

```
Signal-2D/
├── Assets/
│   ├── Resources/               (empty placeholder)
│   ├── _Project/
│   │   ├── Scripts/
│   │   │   ├── Core/           (6 files)
│   │   │   ├── Conductor/      (4 files — Chart, Conductor, NoteData, SongData)
│   │   │   ├── Gameplay/       (9 files — Judge, Lane, Note, etc.)
│   │   │   ├── Editor/         (1 file — SignalSetupMenu.cs)
│   │   │   └── UI/             (8 files — menus, HUD, calibration)
│   │   ├── Scenes/             (5 scenes — ALL EXIST)
│   │   │   ├── Boot.unity
│   │   │   ├── MainMenu.unity
│   │   │   ├── Calibration.unity
│   │   │   ├── Game.unity
│   │   │   └── Results.unity
│   │   ├── Settings/
│   │   │   ├── PlayerInput.inputactions
│   │   │   └── Song1.asset
│   │   ├── Prefabs/
│   │   │   └── Note.prefab
│   │   └── Charts/
│   │       └── Song1.json      (116 notes, 120 BPM)
│   ├── _Recovery/              (0.unity — auto-recovery)
│   ├── DefaultVolumeProfile.asset
│   └── UniversalRenderPipelineGlobalSettings.asset
├── Packages/
│   ├── manifest.json           (references com.hcitrilogy.core + 2D + Cinemachine)
│   └── packages-lock.json
└── ProjectSettings/
```

## Missing Assets

| Asset | Status | Impact |
|---|---|---|
| Audio clip for Song1 | **MISSING** (`track: {fileID: 0}`) | Conductor won't play; no notes spawn |
| AudioMixer asset | **MISSING** | Volume sliders have no effect |
| Audio/SFX/ clips | **MISSING** | No hit/miss/click sounds |
| Audio/Music/ clips | **MISSING** | No background music |
| Settings Panel | **MISSING** | Settings button does nothing |
| Credits Panel | **MISSING** | Credits button does nothing |

## Script Inventory (28 scripts, ~1,525 lines)

| # | Script | Lines | Issues |
|---|---|---|---|
| 1 | `Core/Bootstrapper.cs` | 22 | Duplicate |
| 2 | `Core/LocalizationStrings.cs` | 39 | Duplicate; Signal-specific |
| 3 | `Core/PauseController.cs` | 63 | Does NOT set timeScale (correct for dsp-time) |
| 4 | `Core/SceneFlow.cs` | 84 | Duplicate; builds fader Canvas at runtime |
| 5 | `Core/ServiceLocator.cs` | 44 | Extended version with TryGet + Unregister + Clear |
| 6 | `Core/SettingsManager.cs` | 93 | Extended; adds audio offset, colorblind, note speed |
| 7 | `Conductor/Chart.cs` | 16 | Clean |
| 8 | `Conductor/Conductor.cs` | 121 | Guard: `audioSource.clip == null` → early return (BLOCKS GAME) |
| 9 | `Conductor/NoteData.cs` | 16 | Clean |
| 10 | `Conductor/SongData.cs` | 25 | Clean |
| 11 | `Gameplay/CameraShaker.cs` | 57 | Clean |
| 12 | `Gameplay/ComboMeter.cs` | 33 | Clean |
| 13 | `Gameplay/FeedbackBus.cs` | 23 | Clean |
| 14 | `Gameplay/HealthMeter.cs` | 41 | Clean |
| 15 | `Gameplay/Judge.cs` | 71 | Clean |
| 16 | `Gameplay/Judgment.cs` | 5 | Clean |
| 17 | `Gameplay/Lane.cs` | 137 | Clean |
| 18 | `Gameplay/Note.cs` | 57 | Clean |
| 19 | `Gameplay/NoteSpawner.cs` | 74 | Clean |
| 20 | `Gameplay/ScoreManager.cs` | 38 | Clean |
| 21 | `Editor/SignalSetupMenu.cs` | 567 | Missing settings/credits panels |
| 22 | `UI/CalibrationController.cs` | 136 | Uses PlayClipAtPoint (imprecise scheduling) |
| 23 | `UI/GameSceneController.cs` | 52 | Clean |
| 24 | `UI/HUD.cs` | 70 | Clean |
| 25 | `UI/JudgmentText.cs` | 51 | Clean |
| 26 | `UI/MainMenuController.cs` | 52 | Missing settings/credits panel refs |
| 27 | `UI/PauseMenu.cs` | 43 | Clean |
| 28 | `UI/ResultsScreen.cs` | 50 | Clean |
| 29 | `UI/SettingsPanel.cs` | 53 | Exists but never instantiated in MainMenu scene |

## Detailed Code Review

### Conductor.cs — CRITICAL (Silent Mode Block)

```csharp
// Line 70: This guard prevents the entire game from running without an audio clip
public void Play()
{
    if (Song == null || audioSource.clip == null) return;  // ← BLOCKS GAME
    ...
}
```

The `Song1.asset` has `track: {fileID: 0}` (null clip). Without modification, the game is completely non-functional.

**Fix:** Add silent mode that drives `SongPositionSeconds` from `Time.unscaledTime` when no clip is present. Estimate song length from chart data.

### CalibrationController.cs — Imprecise Audio Scheduling

```csharp
// Uses PlayClipAtPoint which creates a new temporary AudioSource per click
AudioSource.PlayClipAtPoint(clickClip, Vector3.zero);
```

This cannot be precisely scheduled and creates garbage. Should use a single AudioSource with `PlayScheduled()` for sample-accurate timing.

### PauseController.cs — Correct for dsp-time

Unlike Lockdown-3D, Signal-2D's PauseController does NOT set `Time.timeScale = 0`. This is correct because the Conductor uses `AudioSettings.dspTime` which is unaffected by timeScale. Instead, it pauses the Conductor directly via `HandlePause()`.

### SettingsPanel.cs — Exists But Not Used

`SettingsPanel.cs` is a fully functional settings UI component with volume sliders, audio offset, colorblind mode, and note speed controls. However, it's never added to the MainMenu scene by the setup menu. This is a missed opportunity — the component already exists, it just needs to be instantiated.

### ServiceLocator.cs — Most Complete Version

Signal-2D's `ServiceLocator` adds `TryGet<T>()`, `Unregister<T>()`, and `Clear()` methods. This should become the canonical version in the shared core package.
