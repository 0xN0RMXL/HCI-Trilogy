# Final steps in Unity (Containment-VR)

Estimated time: ~15 minutes (most of it is the OpenXR + Quest configuration).

## 1. Open the project

Unity Hub → Open → select `Containment-VR/`. First import takes 3–6 minutes
because Unity will resolve XR Interaction Toolkit, OpenXR, and XR Plugin
Management.

## 2. Project Settings — Player

Edit → Project Settings → Player → Other Settings:
- **Color Space**: Linear

## 3. Project Settings — XR Plug-in Management

Edit → Project Settings → **XR Plug-in Management**:
- **PC, Mac & Linux Standalone** tab: enable **OpenXR**.
- **Android** tab: enable **OpenXR** *and* (Quest-specific) **Meta Quest Support**.
- Click into **OpenXR** settings → add an **Interaction Profile** for your
  controller (e.g., *Meta Quest Touch Pro Controller Profile* or *Oculus
  Touch Controller Profile*).

If Unity prompts to install XR Plugin Management, accept.

## 4. Run the setup menu

Top menu → **HCI Containment → Setup → Initialize Project**.

This creates the four scenes, builds the lab room, places puzzles wired with
XR Interaction Toolkit components, and adds an **XR Origin (VR)** to each
scene by invoking the built-in `GameObject → XR → XR Origin (VR)` menu item.

If the dialog warns *"XR Interaction Toolkit not detected"*, wait for
Package Manager to finish, then re-run the menu.

## 5. (One time) install the XRI starter assets

Window → Package Manager → XR Interaction Toolkit → **Samples** tab →
import **Starter Assets**. This gives you preset locomotion + interaction
input action maps used by the XR Origin.

## 6. World-space canvas raycasting

For each World-Space canvas (`VRMenuCanvas`, `VRResultsCanvas`):
- Add Component → **Tracked Device Graphic Raycaster**.

This lets the controllers' ray interactors hit-test uGUI buttons.

## 7. Press Play

Connect your Quest via Link / AirLink (or use the **XR Device Simulator**
Sample if you don't have a headset). Open `Boot.unity` → Play.

You should:
- Auto-load into the **Main Menu** in world space.
- Point at **ENTER LAB** with your right controller, pull trigger.
- Drop into the lab. See your hands.
- Walk to the keypad — physically poke buttons 1, 4, 8, 5.
- Walk to the cable bench, grab the cyan cable, plug into the cyan socket.
- Same for amber.
- Walk to the dial console, grab the disc, twist your wrist, release at index 7.
- Walk to the door — when all four indicators are green, the door slides open.
- Step through → ESCAPED screen.

## 8. Build for Quest (APK)

File → Build Profiles → switch active to **Android**.
- Texture Compression: **ASTC**.
- Plug in Quest via USB. Enable Developer Mode + USB Debugging in the Meta
  Quest mobile app.
- **Build** → output to `Builds/Android/Containment.apk`.
- Or **Build And Run** to install + launch directly on the headset.

## Troubleshooting

- **No hands visible**: XR Origin uses Sample / Starter Assets. Re-import
  the XRI Starter Assets sample (step 5).
- **Can't grab anything**: ensure the controller's **XR Direct Interactor**
  layer mask doesn't exclude the puzzle layer (default Everything is fine).
- **Door doesn't open after solving puzzles**: open `PuzzleState` GameObject
  in the Lab scene and re-assign references to keypad / dial / sockets if
  any are missing. Re-run the setup menu to repair.
- **Cable doesn't snap into socket**: the cable's `XRGrabInteractable` and
  the socket's `XRSocketInteractor` must be on the same Interaction Layer.
  Default *Everything* works.
- **PC build crashes / black screen**: confirm OpenXR runtime is set to
  *Oculus* in the Meta Quest desktop app **and** in OpenXR settings.

## Performance budgets (Quest 2)

- ≤ 750 k tris per frame, ≤ 50 draw calls.
- Bake static lighting where possible.
- Disable post-processing other than tonemap on the Android pipeline asset.
