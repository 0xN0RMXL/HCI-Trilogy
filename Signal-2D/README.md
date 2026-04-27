# Signal — 2D rhythm game

Part of the [HCI Trilogy](../README.md). A 4-lane keyboard rhythm game demonstrating multimodal feedback, Fitts's Law, and accessibility-aware design.

## Controls

| Action | Key |
|---|---|
| Hit Lane 0 | **D** |
| Hit Lane 1 | **F** |
| Hit Lane 2 | **J** |
| Hit Lane 3 | **K** |
| Pause      | **Esc** |
| Calibration | **Space** (during the calibration scene) |

Home-row placement is intentional — keeps target acquisition time near zero (Fitts's Law).

## How to run

1. Open Unity Hub → Add → select this folder (`Signal-2D/`).
2. Open with Unity **6000.0 LTS** (or 2022.3 LTS).
3. After Unity finishes importing, run **HCI Signal → Setup → Initialize Project** from the top menu. This generates all scenes (Boot, MainMenu, Calibration, Game, Results) and adds them to Build Settings.
4. Open `Assets/_Project/Scenes/Boot.unity`.
5. Press Play.

## How to build (Windows)

1. `File → Build Settings → Windows, Mac, Linux → Windows x86_64`.
2. Build to `Builds/Windows/Signal/`.

## HCI principles demonstrated

- **Multimodal feedback** — every hit triggers visual flash, particles, audio, screen-shake, and HUD pop within the same frame (`FeedbackBus`).
- **Fitts's Law** — hit zones placed at the keyboard home-row (D / F / J / K) keep target acquisition time effectively zero.
- **Accessibility** — colorblind palette toggle + shape coding, audio offset calibration, hold-to-play assist, adjustable note speed.
- **Real-time discrete input** — sample-accurate timing via `AudioSettings.dspTime` (not `Time.time`) so judgment windows stay tight even on long sessions.

## Code map

```
Assets/_Project/Scripts/
├── Core/        ServiceLocator, SettingsManager, PauseController, SceneFlow, Bootstrapper, LocalizationStrings
├── Conductor/   NoteData, Chart, SongData, Conductor (dsp-time clock)
├── Gameplay/    Judgment, FeedbackBus, Lane, Note, NoteSpawner, Judge, ScoreManager, ComboMeter, HealthMeter, CameraShaker
├── UI/          HUD, JudgmentText, MainMenuController, CalibrationController, SettingsPanel, ResultsScreen, PauseMenu, GameSceneController
└── Editor/      SignalSetupMenu (one-click scene + asset generator)
```

See [`FINAL_STEPS_IN_EDITOR.md`](./FINAL_STEPS_IN_EDITOR.md) for the post-import checklist (≈ 5 clicks).

## Credits

Code: MIT. Asset attributions in [`CREDITS.md`](./CREDITS.md).
