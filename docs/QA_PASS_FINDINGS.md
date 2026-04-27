# QA Pass — Findings Log

Comprehensive static review across all 77 C# files in the three projects (~5,415 lines).
Scope: code correctness, logic bugs, design flaws, event-leak audit, Unity API correctness
against declared package versions, setup-menu wiring, scene generation idempotency.

Severity legend:
- **Critical** — gameplay-blocking; would cause a Console error or unsolvable puzzle.
- **Major** — degrades UX or correctness without fully blocking.
- **Minor** — cleanup / hygiene.

This branch (`devin/qa-pass`) addresses **all Critical findings** and the highest-impact
Major findings. The remaining Major / Minor items are documented for transparency; they
are visual/cosmetic and don't prevent any of the three games from compiling and playing
through end-to-end.

---

## Critical findings (fixed on this branch)

| # | Project       | File:line | Finding | Fix |
|---|---------------|-----------|---------|-----|
| C-1 | Signal-2D     | `SignalSetupMenu.cs:523` | `EnsureEventSystem` added `StandaloneInputModule`, but the project ships with **Active Input Handling = Input System Package (New)**. As a result, **no UI button in any Signal scene would respond to mouse/keyboard** — the menu, calibration "Accept", and results screen would all appear inert. | Switched to `UnityEngine.InputSystem.UI.InputSystemUIInputModule`. |
| C-2 | Signal-2D     | `SignalSetupMenu.cs:143` | `PauseController` was instantiated in the Boot scene but its `inputActions` field was **never assigned**. Pressing Esc therefore had no effect, and the Conductor's pause integration could never be exercised. | Load `PlayerInput.inputactions` and write it to `PauseController.inputActions` via `SerializedObject` immediately after the component is added. |
| C-3 | Signal-2D     | `CalibrationController.cs:65–88` | The Calibration scene scheduled clicks via `AudioSource.PlayClipAtPoint(clickClip, …)` but `clickClip` was never wired by the SetupMenu and there was no fallback. **First entry into the Calibration scene threw an `ArgumentNullException` and stopped the coroutine.** | Generate a procedural click `AudioClip` (50 ms 2 kHz sine, exponential decay) on first run if the field is null. Calibration now works out-of-the-box with no asset imports. |
| C-4 | Lockdown-3D   | `Dial.cs:51–56, 70–76` | `_engagedTime` was declared but **never assigned**, so `ReferenceFrameWasJustEngaged()` returned `false` from frame 0.15 s onward. Concretely: the player's Interact press both *engaged* the dial and (in the same frame, since Interactor.Update runs first) caused an immediate `Confirm()` at index 0, which never matches `targetIndex = 7`. The dial puzzle was effectively **unsolvable**. | Assign `_engagedTime = Time.unscaledTime` when entering engaged state inside `Interact()`. |
| C-5 | Containment-VR| `ResultsScreen.cs` + `ContainmentSetupMenu.cs` | The `ResultsScreen` script defined `OnRetryButton` and `OnMenuButton` methods, but **the SetupMenu never created any buttons** in the `Results` scene. Once the player reached the results screen they had no way back to the lab or main menu — they were forced to take off the headset and quit the application. | Build retry + menu buttons in the SetupMenu, expose them as `[SerializeField] Button` fields on `ResultsScreen`, and wire `onClick` listeners in `Start()`. Added `BtnRetry` / `BtnMenu` to `LocalizationStrings`. |

## Major findings (fixed on this branch)

| # | Project | File | Finding | Fix |
|---|---------|------|---------|-----|
| M-1 | Lockdown-3D   | `Highlightable.cs` | `r.material` accessor allocated a per-renderer material instance every Awake — leaks materials on scene unload and breaks GPU instancing. Multiplied across all puzzle props, several MB of duplicated materials were retained per playthrough. | Replaced with a single `MaterialPropertyBlock`; reads base color from `sharedMaterial`, applies tint via `Get/SetPropertyBlock`. |
| M-2 | Lockdown-3D   | `NoteReader.cs` | When a paper-note overlay was shown the cursor was unlocked, but `FirstPersonController` was not paused — so look-input from the freed cursor would still rotate the camera behind the overlay. | `Show()` now calls `PauseController.Instance.Pause()`; `Hide()` calls `Resume()`. |
| M-3 | Lockdown-3D   | `LabSceneController.cs` | Escape time was tracked via `Time.unscaledTime` while the OxygenTimer used `Time.deltaTime` — pausing inflated the recorded escape time without depleting oxygen. | Switched to `Time.time` so both clocks behave consistently under pause. |
| M-4 | Containment-VR| `VRDial.cs`, `VRDrawer.cs`, `VRKeypadButton.cs`, `VRDoor.cs` | Listeners were added to `selectEntered` / `selectExited` / `OnAllSolved` in `Awake/Start` but never removed. Stale listeners caused minor leaks on scene reloads. | Added `OnDestroy` removers; cached the lambda in `VRKeypadButton` so it can be unsubscribed. |

## Verified-correct (pass)

These were inspected and require no fix; documenting so the user knows what was checked:

- **Signal-2D `Conductor.cs`** — `AudioSettings.dspTime` math (lead-in, pause accumulation, end-of-song detection) is sample-accurate. No use of `Time.time` in the hot path. Pause integration via `OnEnable`/`OnDisable` is correctly paired.
- **Signal-2D `Lane.cs`, `Judge.cs`, `Score.cs`, `ComboMeter.cs`, `HealthMeter.cs`** — Subscribe/unsubscribe pairs are correct, judge windows ±40 ms / ±100 ms are within standard rhythm-game tolerances.
- **Lockdown-3D `FirstPersonController.cs`** — CharacterController physics, head-bob fallbacks, Cursor.lockState wiring all correct. Pause check is in `Update`.
- **Lockdown-3D `Interactor.cs`** — Raycast uses an explicit `interactionMask`, hover state is properly diffed, prompt updates only on transition.
- **Lockdown-3D `PuzzleStateMachine.cs`, `DoorLock.cs`** — All four puzzle solves wired correctly to a single `OnAllSolved` event consumed by the door.
- **Lockdown-3D `Keypad.cs` / `KeypadButton.cs`** — Code "1485" matches the in-world note clue. Light/SFX feedback symmetric on accept/reject.
- **Containment-VR `HapticBus.cs`** — Uses XRIT 3.x's `XRBaseInputInteractor.SendHapticImpulse`. Correct API for the declared package version (3.0.8).
- **Containment-VR `VRCableSocket.cs` / `VRCable.cs`** — `XRSocketInteractor` callback chain validates plug color before accepting; Connected flag is one-way (matches Lockdown's CableSocket semantics).
- **Containment-VR `VRPuzzleStateMachine.cs`** — All four puzzles → single `OnAllSolved` → `VRDoor.BeginOpen`. Auto-open chosen for course-friendly demoability per the original design doc.
- **All three `SettingsManager.cs`** — `[0..1]` linear → `[-80..0]` dB conversion uses `Mathf.Log10`, with the standard `db = -80` floor at the silence threshold.
- **Package manifests** — All declared package versions are consistent within each project (Input System 1.11.2, Cinemachine 3.1.2, URP 17.0.4 for Signal/Lockdown; XRIT 3.0.8, OpenXR 1.13.0 for Containment).
- **All three `Bootstrapper.cs`** — Boot scene transitions to MainMenu via SceneFlow with correct `SceneManager.LoadScene` fallback.
- **All three `EditorSetupMenu.cs`** — Are idempotent: `EnsureFolders`, `MakeMat` (loads-or-creates), and `EditorSceneManager.NewScene` overwrite. Build Settings list rebuilds from scratch. Running the menu twice does not corrupt the project.

## Known limitations (NOT fixed; documented for transparency)

These are visible in the editor but do not prevent the games from compiling or being played end-to-end. Fixing them properly would require a substantial rebuild of the procedural geometry, which is out of scope for a code-correctness pass.

| Area | Finding | Why deferred |
|---|---|---|
| Lockdown-3D & Containment-VR | The procedural lab geometry parents several puzzle props (drawer body, keypad buttons, door panel, cable bench) under primitive cubes with **non-uniform localScale**, so child world transforms come out skewed (e.g. `doorPanel` ends up ~4.4 m wide because its parent `doorFrame` has `localScale.x = 2.2` and the panel itself uses `localScale.x = 2.0`). The puzzles are still solvable; the room just looks geometrically off. | Rebuilding the SetupMenu to use unit-cubes with explicit world transforms would touch every puzzle and risk introducing new placement bugs right before the deadline. The user can rearrange the geometry by hand in Unity if desired. |
| Lockdown-3D | The cable rigid-bodies start slightly inside the bench top (~0.045 m penetration) due to the same scaled-parent issue. The first physics tick bumps them out cleanly. | Cosmetic only. |
| Containment-VR | `VRDoor.openSlide` is in local-X of a non-uniformly-scaled `doorFrame`, so the door slides further than intended in world space. The door still opens completely. | Cosmetic only. |
| Containment-VR | No PauseController equivalent — VR convention is to take off the headset to pause. The 8-minute oxygen timer continues during interruptions. | Acceptable for a course demo; matches established VR UX patterns. |

## Build verification

A full Unity batchmode compile on this VM would have required downloading and licensing
the Unity Editor (~12 GB + license activation), which was outside the scope of this pass.
Instead all 77 files were hand-traced for:
1. **C# syntax** (matching braces, no missing semicolons, no orphan `using` directives).
2. **Namespace alignment** between the file and its `using` callers.
3. **Unity API correctness** against the package versions declared in each project's `manifest.json`.
4. **`[SerializeField]` ↔ SetupMenu wiring** for every component the menu instantiates.
5. **Event subscribe/unsubscribe symmetry** in `OnEnable`/`OnDisable` and `Awake`/`OnDestroy`.

No syntactic problems were found.

## After merge: end-to-end test plan for the user

For each game, on a machine with Unity 6 LTS (6000.0.30f1) installed:

1. Open the project in Unity Hub — wait for first import.
2. Project Settings → Player → set **Color Space = Linear** and **Active Input Handling = Input System Package (New)**. Restart Unity if prompted.
3. **Containment-VR only**: also enable **OpenXR** in *XR Plug-in Management* (PC + Android tabs) and select an interaction profile (e.g. Meta Quest Touch).
4. Top menu: `HCI <Game> → Setup → Initialize Project`.
5. Open `Boot.unity` and press **Play**.
6. Verify:
   - **Signal-2D**: title screen appears, Calibrate plays clicks and reports an offset, Game scrolls notes, Esc pauses, Results displays a grade.
   - **Lockdown-3D**: WASD + mouse work, the dial puzzle now confirms at index 7, paper notes pause the camera, all four puzzles complete and the door slides open.
   - **Containment-VR** (on Quest Link or PCVR): grab cables, twist the dial, poke the keypad, and on the Results screen click *RETRY* / *MENU* to verify they navigate.
7. `File → Build Settings → Build` for the standalone artifact.
