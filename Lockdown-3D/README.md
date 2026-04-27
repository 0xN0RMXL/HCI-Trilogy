# Lockdown — 3D First-Person Escape Room

Game 2 of the **HCI-Trilogy**. The same sci-fi lab from Signal, reimagined as
a 3D first-person environment. Read clues, manipulate objects, escape before
the oxygen runs out.

## Controls

| Action          | Key            |
|-----------------|----------------|
| Move            | W / A / S / D  |
| Look            | Mouse          |
| Jump            | Space          |
| Sprint          | Left Shift     |
| Interact        | E              |
| Drop held item  | Q              |
| Pause           | Esc            |

## Run it (first time)

1. Open `Lockdown-3D/` in Unity 6 LTS (`6000.0.30f1`) or Unity 2022.3 LTS.
2. Wait for first import (1–3 minutes).
3. Edit → Project Settings → Player → Other Settings:
   - **Color Space** → **Linear**
   - **Active Input Handling** → **Input System Package (New)**
   - Restart Unity if prompted.
4. Top menu → **HCI Lockdown → Setup → Initialize Project**.
   This generates Boot, MainMenu, Lab, Results scenes and wires every reference.
5. Open `Assets/_Project/Scenes/Boot.unity` and press Play.

## HCI principles demonstrated

- **Affordances & signifiers** (Norman). Each puzzle object is shaped to hint
  at its function — handles look pullable, dials look turnable, sockets look
  insertable.
- **Mapping**. Look-at + [E] is the *single consistent* interaction. The
  prompt text beneath the crosshair always tells you exactly what [E] will do.
- **Diegetic UI**. The oxygen timer is a wall-mounted screen, not a HUD
  overlay. Notes are paper objects in the world, not pop-ups.
- **Constraints** (Norman). The dial has 12 detents — a continuous input
  funneled into discrete states.
- **Feedback**. Audio cues + indicator lights for every successful and failed
  interaction. Wrong code → red flash. Right code → green steady.

## Code map

```
Assets/_Project/Scripts/
├── Core/        ServiceLocator, SettingsManager, PauseController, SceneFlow,
│                Bootstrapper, LocalizationStrings
├── Player/      FirstPersonController (CharacterController + camera pivot)
├── Interaction/ IInteractable, Interactor, CrosshairPrompt, Highlightable
├── Puzzles/     Drawer, Pickupable, Keypad, KeypadButton, Cable, CableSocket,
│                Dial, NoteItem, DoorLock, PuzzleStateMachine
├── UI/          MainMenuController, PauseMenu, NoteReader, ResultsScreen,
│                OxygenTimer, LabSceneController
├── Audio/       FootstepSystem
└── Editor/      LockdownSetupMenu (HCI Lockdown > Setup > Initialize Project)
```

## License

Code: MIT. Assets: see `CREDITS.md` (all CC0).
