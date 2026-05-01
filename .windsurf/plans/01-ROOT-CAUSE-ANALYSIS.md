# Root Cause Analysis — Runtime Issues

## Bug 1: "Settings" and "Credits" buttons do nothing

**Root cause:** `MainMenuController` toggles `settingsPanel` / `creditsPanel` GameObjects. Both are `[SerializeField]` and null at runtime because **no setup menu creates these panels**. `Toggle(null)` is a no-op.

**Affected files:**
- `Lockdown-3D/Assets/_Project/Scripts/UI/MainMenuController.cs:20-21`
- `Signal-2D/Assets/_Project/Scripts/UI/MainMenuController.cs:21-22`
- `Containment-VR/Assets/_Project/Scripts/UI/MainMenuController.cs` (same pattern)
- `Lockdown-3D/Assets/_Project/Scripts/Editor/LockdownSetupMenu.cs` (missing panel creation)
- `Signal-2D/Assets/_Project/Scripts/Editor/SignalSetupMenu.cs` (missing panel creation)
- `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs` (missing panel creation)

**Fix:** Each setup menu must create Settings and Credits panel GameObjects as children of the MainMenu canvas, wire them into the MainMenuController's serialized fields, and start them inactive.

---

## Bug 2: "Play" leads to an empty game

### Containment-VR & Lockdown-3D

**Root cause:** No scene files exist. `Assets/_Project/Scenes/` folder is missing entirely. The editor setup menus generate scenes, but must be run manually from Unity Editor.

**Fix:** User must run the setup menu (`HCI [Project] → Setup → Initialize Project`) from inside Unity Editor. This is documented in `FINAL_STEPS_IN_EDITOR.md` but easily missed.

### Signal-2D

**Root cause:** `Song1.asset` has `track: {fileID: 0}` (no audio clip). The Conductor's `Play()` method exits early:

```csharp
// Conductor.cs:70
if (Song == null || audioSource.clip == null) return;
```

Without audio, `IsPlaying` never becomes true, `SongPositionSeconds` never advances, and `NoteSpawner` never spawns notes.

**Fix:** Modify `Conductor.cs` to support "silent mode" — drive `SongPositionSeconds` from `Time.unscaledTime` when no clip is assigned, and estimate song length from the chart data.

---

## Bug 3: "Quit" works

`OnQuit()` calls `Application.Quit()` directly. No dependencies on missing assets.

---

## Other Critical Issues

### No AudioMixer in any project
`SettingsManager` references `[SerializeField] private AudioMixer mixer` but no `.mixer` asset exists. Volume sliders have no effect. The code gracefully handles null mixer (`if (mixer == null) return;`), so it doesn't crash, but volume control is non-functional.

### Deprecated XR API in Containment-VR
`HapticBus.cs` uses `XRBaseInteractable` and `XRBaseInputInteractor`, removed in XR Interaction Toolkit 3.0. This causes compiler errors.

### Missing EventSystem in Containment-VR setup
The `ContainmentSetupMenu.cs` does not call `EnsureEventSystem()`. Without an EventSystem, UI buttons won't respond to input.
