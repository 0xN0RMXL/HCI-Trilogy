# QA Pass #2 — Deep Audit Findings

This second pass goes deeper than QA Pass #1: it adds a dotnet/Roslyn-based syntax check across every script, audits compile-level (semantic) errors that hand-trace missed, rebuilds skewed procedural geometry, tightens lifecycle null-guards, fixes pause/scene-load races, and adds VR-specific UI plumbing.

Branch: `devin/qa-pass-2` against `devin/scaffold-signal-2d` (effective main).

## Summary of severity

| Severity | Count | Examples |
|---|---|---|
| Critical (would block builds) | 1 | Auto-property pass-by-ref CS0206 in PuzzleStateMachine + VRPuzzleStateMachine |
| Major (gameplay/UI breaks)    | 4 | NoteReader NRE, dial pointer spinning in place, VR canvas non-targetable, scene-load timeScale stuck at 0 |
| Minor (defensive)             | 4 | LabSceneController event-leak, settings-button NRE, interactor double-fire while note open, geometry skew (Phase B) |
| Verified-clean (no change)    | many | dspTime conductor, FPC controller fallback, ServiceLocator, SettingsManager, FirstPersonController pause guard |

## Findings

### Critical-1 — PuzzleStateMachine + VRPuzzleStateMachine: auto-property passed by ref (CS0206)
**Files:**
- `Lockdown-3D/Assets/_Project/Scripts/Puzzles/PuzzleStateMachine.cs`
- `Containment-VR/Assets/_Project/Scripts/Puzzles/VRPuzzleStateMachine.cs`

**Before:**
```csharp
public bool KeypadSolved { get; private set; }
...
keypad.OnAccepted += () => Set(ref KeypadSolved, "Keypad");
```
A non-ref-returning property cannot be passed by `ref`. The compiler emits `CS0206`. Both projects would have failed to compile in the Unity Editor on first open. This was reproduced with a minimal `dotnet build`:
```
Test.cs(4,26): error CS0206: A non ref-returning property or indexer
may not be used as an out or ref value
```

**After:** explicit backing fields with read-only properties.
```csharp
private bool _keypadSolved;
public bool KeypadSolved => _keypadSolved;
...
keypad.OnAccepted += () => Set(ref _keypadSolved, "Keypad");
```

### Major-1 — NoteReader.Show / Hide NRE on missing serialized refs
**File:** `Lockdown-3D/Assets/_Project/Scripts/UI/NoteReader.cs`

`Show()` previously dereferenced `Instance.titleText.text`, `Instance.bodyText.text`, and `Instance.panel.SetActive(...)` without null-guards. If SetupMenu hadn't wired any of these (or a hand-built scene was missing fields), the static call would NRE. `Update()` similarly checked `panel.activeSelf` without a null check, NREing every frame. All three call sites now guard.

### Major-2 — Dial pointer rotates around its own center, not the disc's
**Files:**
- `Lockdown-3D/Assets/_Project/Scripts/Editor/LockdownSetupMenu.cs`
- `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`

The pointer arm was previously placed at an X offset and then rotated on its own local Y axis — a 12 cm bar spinning in place 12 cm off-center. After the fix, the pointer is parented to a `PointerPivot` GameObject *at the east edge of the disc*; the visual arm extends along the pivot's local +Z; rotating the pivot's local Y now sweeps the arm across the disc face like a real indicator.

### Major-3 — VR canvases lacked TrackedDeviceGraphicRaycaster
**File:** `Containment-VR/Assets/_Project/Scripts/Editor/ContainmentSetupMenu.cs`

Both `VRMenuCanvas` and `VRResultsCanvas` were created with only a regular `GraphicRaycaster`. XR ray interactors cannot hit-test world-space UI through that — the menu and results buttons would have been ignored by VR controllers (mouse-only fallback). Now adds XRIT 3's `TrackedDeviceGraphicRaycaster` via reflection so the editor script still compiles when the XR Interaction Toolkit package is missing, with a clear warning.

### Major-4 — SceneFlow could carry timeScale = 0 across scene loads
**Files:**
- `Lockdown-3D/Assets/_Project/Scripts/Core/SceneFlow.cs`
- `Containment-VR/Assets/_Project/Scripts/Core/SceneFlow.cs`

If the user paused mid-game and triggered a scene transition (e.g., the in-game oxygen timer expiring while the pause menu is still up, or the menu button being clicked), `Time.timeScale = 0` could be inherited by the next scene, freezing all UI animations + Invoke timers. Both `SceneFlow.Run` coroutines now reset `Time.timeScale = 1f` before fading.

### Minor-1 — LabSceneController never unsubscribed events
**Files:**
- `Lockdown-3D/Assets/_Project/Scripts/UI/LabSceneController.cs`
- `Containment-VR/Assets/_Project/Scripts/UI/LabSceneController.cs`

Subscribed to `door.OnUnlocked` / `door.OnOpened` and `timer.OnExpired` in `Start` but never removed handlers. On scene reload, lambdas continued to hold references to a destroyed controller. Added matching `OnDestroy` unsubscribers.

### Minor-2 — Signal PauseMenu settings-button NRE
**File:** `Signal-2D/Assets/_Project/Scripts/UI/PauseMenu.cs`

The settings button's `onClick` lambda referenced `settingsPanel.SetActive(!settingsPanel.activeSelf)` even when `settingsPanel` was null. Now only registers the listener when both `settingsButton` and `settingsPanel` are non-null.

### Minor-3 — Lockdown Interactor stayed live while a paper note was open
**File:** `Lockdown-3D/Assets/_Project/Scripts/Interaction/Interactor.cs`

The note overlay disabled `FirstPersonController.ControlEnabled`, but `Interactor.Update` continued raycasting and accepting Interact presses — meaning a hidden note-trigger or other interactable could fire through the overlay. Now early-returns and clears the prompt when `NoteReader.IsOpen`.

### Minor-4 — Procedural geometry skew (Phase B, see also QA Pass #1 deferral)
Already covered in the QA Pass #1 findings as a known cosmetic issue. Phase B introduced `MakeAnchoredBox()` in both `LockdownSetupMenu.cs` and `ContainmentSetupMenu.cs` — an empty unit-scaled anchor with a child visual cube — so the Drawer/Cabinet/Keypad/Dial/Door no longer skew their child geometry through non-uniform scale propagation. Dimensions were re-tuned to fit the new anchored containers.

## Verified-clean (audited, no change required)

- **Conductor (Signal-2D)** — `dspTime`-driven, pause accumulation correct, events match `Stop()`/`Play()` lifecycle. Lead-in math verified.
- **FirstPersonController (Lockdown-3D)** — pitch on `cameraPivot`, yaw on root; pause guard correct (operator-precedence verified for `!ControlEnabled || (instance != null && IsPaused)`).
- **HapticBus (Containment-VR)** — XRIT 3.0 API: `XRBaseInputInteractor.SendHapticImpulse(amp, dur)` is correct.
- **VRDial / VRDrawer / VRCableSocket** — XRIT 3.0 event signatures (`SelectEnterEventArgs.interactorObject`) verified; `OnDestroy` unsubscribers present.
- **ServiceLocator** — type-keyed dictionary; Awake/Register/OnDestroy/Unregister symmetric on every consumer.
- **SettingsManager** — PlayerPrefs persistence; AudioMixer dB mapping matches the standard log10 formula.
- **Build Settings** — all three SetupMenus enumerate Boot/MainMenu/(Calibration|Lab)/Game/Results and inject them via `EditorBuildSettings.scenes`.
- **Tags** — only the default `MainCamera` tag is used; no custom tag dependency.
- **No use of legacy/deprecated APIs** — no `FindObjectOfType`, no `Resources.Load` runtime, no `Update`-loop allocations.

## Static analysis tooling

A dotnet/Roslyn syntax-check tool was built at `/tmp/csharp-syntax-check` and run against every project. After every fix in this pass:

```
=== Signal-2D ===     Total syntax errors: 0
=== Lockdown-3D ===   Total syntax errors: 0
=== Containment-VR === Total syntax errors: 0
```

The Roslyn-only pass catches **syntactic** errors and a subset of structural problems but does not perform full **semantic** analysis (which would require Unity's reference DLLs to resolve `UnityEngine.*` types). The CS0206 Critical-1 finding above was caught by hand-trace + a focused `dotnet build` reproduction, not the tree-only syntax check.

## Recommended verification by user

After merge:
1. Open each project in Unity 6 LTS. **First-import compile must succeed without errors** (this was previously not true — see Critical-1).
2. Run `HCI <Game> → Setup → Initialize Project` for each project.
3. **Lockdown-3D / Containment-VR**: confirm the dial in the lab scene displays a visible **arm** swinging across the disc face, not a vertical bar spinning in place.
4. **Containment-VR**: confirm MainMenu/Results buttons respond to XR ray clicks once XRIT is imported.
5. Press Play → Game → trigger pause → click "Menu" → verify MainMenu is **not** frozen.
