# Final steps in Unity (Lockdown)

Estimated time: ~5 minutes.

## 1. Open the project
Unity Hub → Open → select `Lockdown-3D/`. First import takes 1–3 minutes.

## 2. Project Settings
Edit → Project Settings → Player → Other Settings:
- **Color Space**: Linear
- **Active Input Handling**: Input System Package (New)

Unity may prompt to restart. Let it.

## 3. Run the setup menu
Top menu → **HCI Lockdown → Setup → Initialize Project**.
This generates all scenes, builds the lab room from primitives, places puzzles,
wires every reference, and adds scenes to Build Settings.

You should see a "Project initialized" dialog when done.

## 4. Press Play
Open `Assets/_Project/Scenes/Boot.unity` (will auto-open) and press Play.

You should:
- See the Main Menu with **ENTER LAB**, **SETTINGS**, **CREDITS**, **QUIT**
- Click **ENTER LAB** → drop into the lab in first-person
- WASD to move, mouse to look, **E** to read the desk note (clue: code 1485)
- Find the keypad on the wall, enter **1485** with the keys, **E** on each
- Pick up the cyan cable, plug it into the cyan socket
- Pick up the amber cable, plug it into the amber socket
- Use the wall dial — **E** to engage, mouse-X to rotate to position 7, **E** to confirm
- Door unlocks → **E** on door → ESCAPED screen

## 5. (Optional) Polish
- Drop ambient sounds into `Assets/_Project/Audio/SFX/` and assign them in the
  scene. The setup leaves these empty by default — game still works silently.
- Replace cube primitives with proper 3D models from Sketchfab CC0 / Poly Haven.

## 6. Build (Windows x86_64)
File → Build Settings →
- Target: Windows
- Architecture: x86_64
- Click **Build** → output to `Builds/Windows/Lockdown/Lockdown.exe`.

## Troubleshooting

- **Crosshair prompt doesn't show**: the Interactor needs the crosshair
  reference; the setup menu wires this. If you re-create scenes manually,
  ensure the HUD canvas's CrosshairPrompt is referenced by the Interactor.
- **Door won't open after solving puzzles**: open the PuzzleState GameObject
  in the Lab scene; verify it has references to keypad, dial, and both
  sockets.
- **Door says "Locked" but all four indicators are green**: rerun the setup
  menu — the DoorLock's `state` field may not be wired.
