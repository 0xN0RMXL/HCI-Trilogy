# AI Builder Task File

## Priority Definitions

```
P0 = Game won't run at all / compiler errors
P1 = Major feature broken
P2 = Code quality / polish
P3 = Future improvement
```

## Execution Order

Tasks must be executed in order within each priority level. P0 tasks first, then P1, etc.

---

## Containment-VR Tasks

### CVR-01 (P0) — Fix HapticBus.cs Deprecated XR API

**File:** `Containment-VR/Assets/_Project/Scripts/Player/HapticBus.cs`
**Action:** Replace entire file with XRI 3.0 compatible version
**Key changes:**
- `XRBaseInteractable` → `XRGrabInteractable`
- `XRBaseInputInteractor` → `XRBaseControllerInteractor`
- `onHoverEntered`/`onSelectEntered` → event-based with `HoverEnterEventArgs`/`SelectEnterEventArgs`
- Add `using UnityEngine.XR.Interaction.Toolkit.Interactables;`
- Add `using UnityEngine.XR.Interaction.Toolkit.Interactors;`
**Verification:** Compiles without errors in Unity 6 with XRI 3.x

### CVR-02 (P1) — Add EnsureEventSystem to ContainmentSetupMenu

**File:** `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`
**Action:** Add `EnsureEventSystem()` method and call it in `CreateMainMenuScene()` and `CreateResultsScene()`
**Verification:** MainMenu buttons respond to VR pointer input

### CVR-03 (P1) — Add Settings Panel to ContainmentSetupMenu

**File:** `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`
**Action:** In `CreateMainMenuScene()`, after MainMenuController wiring:
1. Create `SettingsPanel` GameObject (Image + inactive)
2. Add title "SETTINGS"
3. Add volume sliders (Master, SFX, Music) — use Slider UI component
4. Add VR-specific toggles (Locomotion, Turn Mode, Vignette, Dominant Hand)
5. Add close button that calls `Toggle(settingsPanel)`
6. Wire `settingsPanel` into MainMenuController serialized field
**Verification:** Settings button opens panel with functional controls

### CVR-04 (P1) — Add Credits Panel to ContainmentSetupMenu

**File:** `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`
**Action:** In `CreateMainMenuScene()`, after settings panel:
1. Create `CreditsPanel` GameObject (Image + inactive)
2. Add attribution text from CREDITS.md
3. Add close button
4. Wire `creditsPanel` into MainMenuController serialized field
**Verification:** Credits button opens panel

### CVR-05 (P1) — Add TrackedDeviceGraphicRaycaster to World-Space Canvases

**File:** `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`
**Action:** After creating each world-space Canvas in the setup menu, add:
```csharp
canvasGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
```
**Verification:** VR controllers can interact with world-space UI buttons

### CVR-06 (P2) — Remove Duplicated Core Scripts

**Files to delete:**
- `Containment-VR/Assets/_Project/Scripts/Core/Bootstrapper.cs`
- `Containment-VR/Assets/_Project/Scripts/Core/LocalizationStrings.cs` (keep — project-specific)
- `Containment-VR/Assets/_Project/Scripts/Core/SceneFlow.cs` (keep — VR-specific fade)
- `Containment-VR/Assets/_Project/Scripts/Core/ServiceLocator.cs`
- `Containment-VR/Assets/_Project/Scripts/Core/SettingsManager.cs` (keep — VR-specific settings)

**Action:** Remove exact duplicates that match `com.hcitrilogy.core`. Update references to use the shared package namespace.
**Verification:** Project compiles without errors

### CVR-07 (P2) — Add AudioMixer Creation to Setup Menu

**File:** `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`
**Action:** Create AudioMixer with Master/SFX/Music groups. Expose parameters. Wire into SettingsManager.
**Note:** Unity's programmatic AudioMixer creation is limited. May need to create a `.mixer` asset manually and reference it.
**Verification:** Volume sliders in settings panel affect audio output

---

## Lockdown-3D Tasks

### L3D-01 (P1) — Add Settings Panel to LockdownSetupMenu

**File:** `Lockdown-3D/Assets/_Project/Scripts/Editor/LockdownSetupMenu.cs`
**Action:** In `CreateMainMenuScene()`, after MainMenuController wiring:
1. Create `SettingsPanel` GameObject (Image + inactive)
2. Add title "SETTINGS"
3. Add volume sliders (Master, SFX, Music)
4. Add sensitivity slider
5. Add invert Y toggle
6. Add head bob toggle
7. Add close button
8. Wire `settingsPanel` into MainMenuController serialized field
**Verification:** Settings button opens panel with functional controls

### L3D-02 (P1) — Add Credits Panel to LockdownSetupMenu

**File:** `Lockdown-3D/Assets/_Project/Scripts/Editor/LockdownSetupMenu.cs`
**Action:** In `CreateMainMenuScene()`, after settings panel:
1. Create `CreditsPanel` GameObject (Image + inactive)
2. Add attribution text
3. Add close button
4. Wire `creditsPanel` into MainMenuController serialized field
**Verification:** Credits button opens panel

### L3D-03 (P1) — Wire Settings Panel Sliders to SettingsManager

**File:** `Lockdown-3D/Assets/_Project/Scripts/Editor/LockdownSetupMenu.cs`
**Action:** Create a `SettingsPanelController` component (or inline the wiring) that:
- On slider change → calls `SettingsManager.SetMasterVolume(float)` etc.
- On toggle change → calls `SettingsManager.SetMouseSensitivity(float)` etc.
- On enable → reads current values from SettingsManager
**Verification:** Settings persist across scene loads

### L3D-04 (P2) — Remove Duplicated Core Scripts

**Files to delete:**
- `Lockdown-3D/Assets/_Project/Scripts/Core/Bootstrapper.cs`
- `Lockdown-3D/Assets/_Project/Scripts/Core/LocalizationStrings.cs` (keep — project-specific)
- `Lockdown-3D/Assets/_Project/Scripts/Core/SceneFlow.cs` (keep — UI Image fade)
- `Lockdown-3D/Assets/_Project/Scripts/Core/ServiceLocator.cs`
- `Lockdown-3D/Assets/_Project/Scripts/Core/SettingsManager.cs` (keep — FPS-specific settings)
**Verification:** Project compiles without errors

### L3D-05 (P2) — Add AudioMixer Creation to Setup Menu

**File:** `Lockdown-3D/Assets/_Project/Scripts/Editor/LockdownSetupMenu.cs`
**Verification:** Volume sliders work

---

## Signal-2D Tasks

### S2D-01 (P0) — Add Silent Mode to Conductor

**File:** `Signal-2D/Assets/_Project/Scripts/Conductor/Conductor.cs`
**Action:**
1. Add new fields: `_usingSilentMode`, `_playStartTime`, `_pauseAccumTime`, `_pauseStartedAtTime`
2. Modify `Play()`: remove `audioSource.clip == null` guard; add `_usingSilentMode` flag; set `_playStartTime`
3. Modify `Update()`: branch on `_usingSilentMode` — use `Time.unscaledTime` for silent mode, `AudioSettings.dspTime` for audio mode
4. Add `EstimateSongLength()` helper
5. Modify `HandlePause()`: track `_pauseAccumTime` for silent mode
**Verification:** Game plays with notes scrolling even without audio clip; song ends after last note + 2 seconds

### S2D-02 (P1) — Add Settings Panel to SignalSetupMenu

**File:** `Signal-2D/Assets/_Project/Scripts/Editor/SignalSetupMenu.cs`
**Action:** In `CreateMainMenuScene()`:
1. Create `SettingsPanel` GameObject (Image + inactive)
2. Add `SettingsPanel.cs` component (already exists!)
3. Wire its sliders to SettingsManager
4. Add close button
5. Wire `settingsPanel` into MainMenuController serialized field
**Verification:** Settings button opens panel with functional controls

### S2D-03 (P1) — Add Credits Panel to SignalSetupMenu

**File:** `Signal-2D/Assets/_Project/Scripts/Editor/SignalSetupMenu.cs`
**Action:** In `CreateMainMenuScene()`:
1. Create `CreditsPanel` GameObject (Image + inactive)
2. Add attribution text
3. Add close button
4. Wire `creditsPanel` into MainMenuController serialized field
**Verification:** Credits button opens panel

### S2D-04 (P2) — Fix CalibrationController Audio Scheduling

**File:** `Signal-2D/Assets/_Project/Scripts/UI/CalibrationController.cs`
**Action:** Replace `AudioSource.PlayClipAtPoint` with `PlayScheduled` on the existing `clickSource`
**Verification:** Metronome clicks are sample-accurate; no temporary GameObjects created

### S2D-05 (P2) — Remove Duplicated Core Scripts

**Files to delete:**
- `Signal-2D/Assets/_Project/Scripts/Core/Bootstrapper.cs`
- `Signal-2D/Assets/_Project/Scripts/Core/LocalizationStrings.cs` (keep — project-specific)
- `Signal-2D/Assets/_Project/Scripts/Core/SceneFlow.cs` (keep — runtime Canvas fade)
- `Signal-2D/Assets/_Project/Scripts/Core/ServiceLocator.cs` (move to core pkg first)
- `Signal-2D/Assets/_Project/Scripts/Core/SettingsManager.cs` (keep — rhythm-specific settings)
**Verification:** Project compiles without errors

### S2D-06 (P2) — Add AudioMixer Creation to Setup Menu

**File:** `Signal-2D/Assets/_Project/Scripts/Editor/SignalSetupMenu.cs`
**Verification:** Volume sliders work

---

## Cross-Cutting Tasks

### XC-01 (P2) — Consolidate ServiceLocator in Core Package

**File:** `Packages/com.hcitrilogy.core/Runtime/ServiceLocator.cs`
**Action:** Copy Signal-2D's extended version (with TryGet, Unregister, Clear) into the shared package. Update namespace to `HCITrilogy.Core`.
**Verification:** All three projects can use the shared ServiceLocator

### XC-02 (P2) — Create Base SettingsManager in Core Package

**File:** `Packages/com.hcitrilogy.core/Runtime/SettingsManager.cs`
**Action:** Extract common volume/mixer/PlayerPrefs logic into base class. Each project extends with game-specific settings.
**Verification:** Common settings logic is shared; game-specific settings preserved

### XC-03 (P2) — Create Base SceneFlow in Core Package

**File:** `Packages/com.hcitrilogy.core/Runtime/SceneFlow.cs`
**Action:** Extract common singleton/async loading logic into base class with virtual `FadeOut()`/`FadeIn()` methods.
**Verification:** Common scene flow logic is shared; fade implementations preserved per project

### XC-04 (P2) — Create Base Bootstrapper in Core Package

**File:** `Packages/com.hcitrilogy.core/Runtime/Bootstrapper.cs`
**Action:** Single implementation that loads "MainMenu" via SceneFlow.
**Verification:** All projects use shared Bootstrapper

### XC-05 (P3) — Add Assembly Definition Files

**Files to create:**
- `Packages/com.hcitrilogy.core/Runtime/HCITrilogy.Core.asmdef`
- `Containment-VR/Assets/_Project/Scripts/HCITrilogy.Containment.asmdef`
- `Lockdown-3D/Assets/_Project/Scripts/HCITrilogy.Lockdown.asmdef`
- `Signal-2D/Assets/_Project/Scripts/HCITrilogy.Signal.asmdef`
**Verification:** No cross-namespace accidental references; compilation is faster

---

## Execution Checklist

Run these steps in order to make all three games playable:

1. [ ] Fix CVR-01 (HapticBus deprecated API)
2. [ ] Fix S2D-01 (Conductor silent mode)
3. [ ] Fix CVR-02 (Containment EventSystem)
4. [ ] Fix CVR-03 (Containment settings panel)
5. [ ] Fix CVR-04 (Containment credits panel)
6. [ ] Fix CVR-05 (Containment TrackedDeviceGraphicRaycaster)
7. [ ] Fix L3D-01 (Lockdown settings panel)
8. [ ] Fix L3D-02 (Lockdown credits panel)
9. [ ] Fix L3D-03 (Lockdown settings wiring)
10. [ ] Fix S2D-02 (Signal settings panel)
11. [ ] Fix S2D-03 (Signal credits panel)
12. [ ] Run each project's setup menu in Unity Editor to generate scenes
13. [ ] Verify all three games are playable end-to-end
