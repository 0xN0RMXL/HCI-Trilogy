# Detailed Fix & Refactoring Plan

## Phase 1: Critical Fixes (Make Games Playable)

### Task 1.1 — Fix HapticBus.cs (Containment-VR, P0)

**File:** `Containment-VR/Assets/_Project/Scripts/Player/HapticBus.cs`

**Before:**
```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace HCITrilogy.Containment.Player
{
    public class HapticBus : MonoBehaviour
    {
        [SerializeField] private float hoverAmplitude = 0.3f;
        [SerializeField] private float hoverDuration = 0.15f;
        [SerializeField] private float selectAmplitude = 0.6f;
        [SerializeField] private float selectDuration = 0.2f;

        private XRBaseInteractable _last;

        private void OnEnable()
        {
            var interactable = GetComponent<XRBaseInteractable>();
            if (interactable == null) return;
            interactable.onHoverEntered.AddListener(OnHover);
            interactable.onSelectEntered.AddListener(OnSelect);
        }

        private void OnDisable()
        {
            var interactable = GetComponent<XRBaseInteractable>();
            if (interactable == null) return;
            interactable.onHoverEntered.RemoveListener(OnHover);
            interactable.onSelectEntered.RemoveListener(OnSelect);
        }

        private void OnHover(XRBaseInteractor _) => SendHaptics(hoverAmplitude, hoverDuration);
        private void OnSelect(XRBaseInteractor _) => SendHaptics(selectAmplitude, selectDuration);

        private void SendHaptics(float amplitude, float duration)
        {
            var interactor = _last?.interactorSelecting as XRBaseInputInteractor;
            interactor?.SendHapticImpulse(amplitude, duration);
        }
    }
}
```

**After:**
```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace HCITrilogy.Containment.Player
{
    public class HapticBus : MonoBehaviour
    {
        [SerializeField] private float hoverAmplitude = 0.3f;
        [SerializeField] private float hoverDuration = 0.15f;
        [SerializeField] private float selectAmplitude = 0.6f;
        [SerializeField] private float selectDuration = 0.2f;

        private XRGrabInteractable _grabInteractable;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            if (_grabInteractable == null) return;
            _grabInteractable.hoverEntered.AddListener(OnHover);
            _grabInteractable.selectEntered.AddListener(OnSelect);
        }

        private void OnDisable()
        {
            if (_grabInteractable == null) return;
            _grabInteractable.hoverEntered.RemoveListener(OnHover);
            _grabInteractable.selectEntered.RemoveListener(OnSelect);
        }

        private void OnHover(HoverEnterEventArgs args) => SendHaptics(args.interactorObject, hoverAmplitude, hoverDuration);
        private void OnSelect(SelectEnterEventArgs args) => SendHaptics(args.interactorObject, selectAmplitude, selectDuration);

        private void SendHaptics(IXRInteractor interactor, float amplitude, float duration)
        {
            if (interactor is XRBaseControllerInteractor controllerInteractor)
                controllerInteractor.SendHapticImpulse(amplitude, duration);
        }
    }
}
```

### Task 1.2 — Fix Conductor Silent Mode (Signal-2D, P0)

**File:** `Signal-2D/Assets/_Project/Scripts/Conductor/Conductor.cs`

**New fields to add:**
```csharp
private bool _usingSilentMode;
private double _playStartTime;
private double _pauseAccumTime;
private double _pauseStartedAtTime;
```

**Modified Play():**
```csharp
public void Play()
{
    if (Song == null) return;
    _dspStartTime = AudioSettings.dspTime + leadInSeconds;
    _pauseAccum = 0;
    _pauseAccumTime = 0;
    _playStartTime = Time.unscaledTime + leadInSeconds;
    _usingSilentMode = audioSource.clip == null;
    if (!_usingSilentMode)
        audioSource.PlayScheduled(_dspStartTime);
    IsPlaying = true;
    IsPaused = false;
    OnSongStarted?.Invoke();
}
```

**Modified Update():**
```csharp
private void Update()
{
    if (!IsPlaying || IsPaused) return;

    if (_usingSilentMode)
    {
        double now = Time.unscaledTime - _pauseAccumTime;
        SongPositionSeconds = now - _playStartTime - _userOffsetSeconds - (Song != null ? Song.offsetSeconds : 0);
        double songLength = EstimateSongLength();
        if (SongPositionSeconds > songLength)
            Stop();
    }
    else
    {
        double now = AudioSettings.dspTime - _pauseAccum;
        SongPositionSeconds = now - _dspStartTime - _userOffsetSeconds - (Song != null ? Song.offsetSeconds : 0);
        if (!audioSource.isPlaying && audioSource.clip != null && SongPositionSeconds > audioSource.clip.length)
            Stop();
    }
}

private double EstimateSongLength()
{
    if (Song == null || Song.notes == null || Song.notes.Length == 0) return 60.0;
    double lastBeat = 0;
    foreach (var n in Song.notes)
        if (n.beat + n.holdBeats > lastBeat) lastBeat = n.beat + n.holdBeats;
    return lastBeat * SecondsPerBeat + 2.0;
}
```

**Modified HandlePause():**
```csharp
private void HandlePause(bool paused)
{
    if (!IsPlaying) return;
    if (paused && !IsPaused)
    {
        if (!_usingSilentMode) audioSource.Pause();
        _pauseStartedAtDsp = AudioSettings.dspTime;
        _pauseStartedAtTime = Time.unscaledTime;
        IsPaused = true;
    }
    else if (!paused && IsPaused)
    {
        if (_usingSilentMode)
            _pauseAccumTime += Time.unscaledTime - _pauseStartedAtTime;
        else
        {
            _pauseAccum += AudioSettings.dspTime - _pauseStartedAtDsp;
            audioSource.UnPause();
        }
        IsPaused = false;
    }
}
```

### Task 1.3 — Add Settings & Credits Panels to All Setup Menus (P1)

**Affected files:**
- `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`
- `Lockdown-3D/Assets/_Project/Scripts/Editor/LockdownSetupMenu.cs`
- `Signal-2D/Assets/_Project/Scripts/Editor/SignalSetupMenu.cs`

**Pattern for each setup menu's CreateMainMenuScene():**

After creating the MainMenuController and wiring the play/settings/credits/quit buttons, add:

```csharp
// --- Settings Panel ---
var settingsPanelGO = new GameObject("SettingsPanel", typeof(Image));
settingsPanelGO.transform.SetParent(canvasGO.transform, false);
var spImg = settingsPanelGO.GetComponent<Image>();
spImg.color = new Color(0.054f, 0.066f, 0.086f, 0.95f);
var sprt = spImg.rectTransform;
sprt.anchorMin = Vector2.zero; sprt.anchorMax = Vector2.one;
sprt.offsetMin = sprt.offsetMax = Vector2.zero;

// Add title
var settingsTitle = new GameObject("Title").AddComponent<Text>();
settingsTitle.transform.SetParent(settingsPanelGO.transform, false);
settingsTitle.text = "SETTINGS";
settingsTitle.font = /* existing font ref */;
settingsTitle.fontSize = 48; settingsTitle.color = AccentCyan;
settingsTitle.alignment = TextAnchor.MiddleCenter;
var strt = settingsTitle.rectTransform;
strt.anchorMin = new Vector2(0.5f, 0.85f); strt.anchorMax = new Vector2(0.5f, 0.85f);
strt.sizeDelta = new Vector2(400, 80); strt.anchoredPosition = Vector2.zero;

// Add close button
var closeSettingsBtn = MakeBtn("CLOSE", new Vector2(0.5f, 0.15f));
closeSettingsBtn.transform.SetParent(settingsPanelGO.transform, false);

settingsPanelGO.SetActive(false);

// --- Credits Panel ---
var creditsPanelGO = new GameObject("CreditsPanel", typeof(Image));
creditsPanelGO.transform.SetParent(canvasGO.transform, false);
var cpImg = creditsPanelGO.GetComponent<Image>();
cpImg.color = new Color(0.054f, 0.066f, 0.086f, 0.95f);
var cprt = cpImg.rectTransform;
cprt.anchorMin = Vector2.zero; cprt.anchorMax = Vector2.one;
cprt.offsetMin = cprt.offsetMax = Vector2.zero;

var creditsBody = new GameObject("Body").AddComponent<Text>();
creditsBody.transform.SetParent(creditsPanelGO.transform, false);
creditsBody.text = "HCI Trilogy\n\nDesign & Code: Omar Ammar\n\nAssets: CC0 / Public Domain\nSee CREDITS.md for full attribution.";
creditsBody.font = /* existing font ref */;
creditsBody.fontSize = 24; creditsBody.color = Color.white;
creditsBody.alignment = TextAnchor.MiddleCenter;
var cbrt = creditsBody.rectTransform;
cbrt.anchorMin = new Vector2(0.1f, 0.2f); cbrt.anchorMax = new Vector2(0.9f, 0.85f);
cbrt.offsetMin = cbrt.offsetMax = Vector2.zero;

var closeCreditsBtn = MakeBtn("CLOSE", new Vector2(0.5f, 0.1f));
closeCreditsBtn.transform.SetParent(creditsPanelGO.transform, false);

creditsPanelGO.SetActive(false);

// Wire into MainMenuController
var mso = new SerializedObject(menu);
mso.FindProperty("settingsPanel").objectReferenceValue = settingsPanelGO;
mso.FindProperty("creditsPanel").objectReferenceValue = creditsPanelGO;
mso.ApplyModifiedPropertiesWithoutUndo();
```

**For Signal-2D specifically:** Use the existing `SettingsPanel.cs` component instead of creating raw sliders. Add it to the settings panel GameObject.

### Task 1.4 — Add EnsureEventSystem to Containment-VR Setup (P1)

**File:** `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`

Add the same `EnsureEventSystem()` method used by Lockdown-3D and Signal-2D:

```csharp
private static void EnsureEventSystem()
{
    if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
    {
        var es = new GameObject("EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
    }
}
```

Call it at the end of `CreateMainMenuScene()` and `CreateResultsScene()`.

### Task 1.5 — Add TrackedDeviceGraphicRaycaster (Containment-VR, P1)

**File:** `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`

For each world-space canvas created in the setup menu, add:

```csharp
// After creating a world-space Canvas:
canvasGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
```

---

## Phase 2: Code Quality & Deduplication

### Task 2.1 — Consolidate ServiceLocator in Core Package (P2)

**Target:** `Packages/com.hcitrilogy.core/Runtime/ServiceLocator.cs`

Use Signal-2D's extended version (with `TryGet`, `Unregister`, `Clear`) as the canonical implementation. Remove the copies from each project.

### Task 2.2 — Create Base SettingsManager in Core Package (P2)

**Target:** `Packages/com.hcitrilogy.core/Runtime/SettingsManager.cs`

Extract common logic (volume control, AudioMixer, PlayerPrefs persistence) into a base class. Each project extends with game-specific settings.

### Task 2.3 — Create Base SceneFlow in Core Package (P2)

**Target:** `Packages/com.hcitrilogy.core/Runtime/SceneFlow.cs`

Extract common logic (singleton, async scene loading, fade timing) into a base class with virtual fade methods. Each project overrides fade implementation.

### Task 2.4 — Create Base Bootstrapper in Core Package (P2)

**Target:** `Packages/com.hcitrilogy.core/Runtime/Bootstrapper.cs`

Single implementation that loads "MainMenu" via SceneFlow. Remove project copies.

### Task 2.5 — Fix CalibrationController Audio Scheduling (P2)

**File:** `Signal-2D/Assets/_Project/Scripts/UI/CalibrationController.cs`

Replace `AudioSource.PlayClipAtPoint` with `PlayScheduled` on a single AudioSource for sample-accurate timing.

---

## Phase 3: Feature Completeness

### Task 3.1 — Add Functional Settings Panel Controls (P2)

Each game's settings panel needs working sliders/toggles connected to SettingsManager.

**Lockdown-3D:** Volume × 3, sensitivity, invert Y, head bob
**Signal-2D:** Use existing SettingsPanel.cs (volume × 3, audio offset, colorblind, note speed)
**Containment-VR:** Volume × 3, locomotion, turn mode, vignette, dominant hand

### Task 3.2 — Add AudioMixer Asset (P2)

Create `.mixer` asset with Master/SFX/Music groups and exposed parameters. Wire into SettingsManager.

### Task 3.3 — Add Audio Clip Placeholders (P3)

Add CC0 audio clips for SFX and music. Update CREDITS.md.

### Task 3.4 — Add Assembly Definition Files (P3)

Create `.asmdef` files for proper namespace isolation:
- `HCITrilogy.Core.asmdef`
- `HCITrilogy.Containment.asmdef`
- `HCITrilogy.Lockdown.asmdef`
- `HCITrilogy.Signal.asmdef`
