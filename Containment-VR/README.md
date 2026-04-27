# Containment — VR Escape Room

Game 3 of the **HCI-Trilogy**. The same lab and puzzles as Lockdown, ported
to room-scale VR. The player physically pokes the keypad, twists the dial,
plugs the cables, and pulls the drawer.

## Hardware

- Meta Quest 2 / 3 / Pro (standalone Quest build), **or**
- Quest via Link / AirLink, Valve Index, or any OpenXR PC headset (PC build).

## Run it (first time)

1. Open `Containment-VR/` in Unity 6 LTS (`6000.0.30f1`).
2. Wait for first import (3–6 minutes — XR Interaction Toolkit pulls a few packages).
3. Edit → Project Settings → Player: **Color Space → Linear**.
4. Edit → Project Settings → **XR Plug-in Management**:
   - Click **Install XR Plugin Management** if prompted.
   - On the **PC** tab → enable **OpenXR**.
   - On the **Android** tab → enable **OpenXR**.
   - In the OpenXR settings panel, add an **Interaction Profile**
     (e.g. *Oculus Touch Controller Profile* / *Meta Quest Touch Pro*).
5. Top menu → **HCI Containment → Setup → Initialize Project**.
6. If the setup dialog warns "XR Interaction Toolkit not detected", let
   Unity finish importing XRIT (Window → Package Manager) and rerun the
   menu. Each scene needs an **XR Origin (VR)** rig — the setup tries to
   add one via the built-in `GameObject → XR → XR Origin (VR)` menu.
7. Open `Boot.unity` and press Play (with headset connected via Link).

## Build for Quest

1. File → Build Profiles → switch active to **Android**.
2. Player Settings → Other Settings → Minimum API Level **29**, target **ARM64**.
3. Plug in your Quest via USB, enable Developer Mode + USB debugging.
4. Build & Run → APK is written to `Builds/Android/Containment.apk`
   and installed on the headset.

## Controls

| Action                         | How                                       |
|--------------------------------|-------------------------------------------|
| Move (smooth or teleport)      | Left thumbstick                           |
| Turn (snap or smooth)          | Right thumbstick                          |
| Grab (cables, drawer, dial)    | Grip                                      |
| Poke (keypad)                  | Move your finger into the button          |
| UI select (menus)              | Trigger while pointing at the button      |

## HCI principles demonstrated

- **Natural mapping** (Norman). Twisting a wrist *is* the input — there is
  no key-binding layer between intention and result.
- **Direct manipulation** (Shneiderman). The hand replaces the cursor; the
  object responds in real time to the hand's pose.
- **Embodiment & presence** (Slater, Bowman). Virtual hands matched to the
  user's real proprioception strongly increase task confidence.
- **Multimodal feedback**. Visual + spatial audio + controller haptics every
  time something happens (button press, plug snap, dial detent click).
- **Locomotion comfort tradeoffs**. Teleport (default) vs smooth move; snap
  turn (default) vs smooth turn. Vignette tunneling on by default. The
  player can opt out — but the report explains why we ship comfort defaults
  in line with simulator-sickness research (LaViola, 2000).

## Code map

```
Assets/_Project/Scripts/
├── Core/        ServiceLocator, SettingsManager (locomotion + comfort),
│                SceneFlow, Bootstrapper, LocalizationStrings
├── Player/      HapticBus (haptics on every select/hover)
├── Puzzles/     VRKeypad + VRKeypadButton (XRSimpleInteractable),
│                VRDial (wrist-rotation tracking), VRCable + VRCableSocket
│                (XRGrabInteractable + XRSocketInteractor color match),
│                VRDrawer (constrained pull along Z), VRDoor,
│                VRPuzzleStateMachine
├── UI/          MainMenuController, ResultsScreen, OxygenTimer (world-space),
│                LabSceneController
└── Editor/      ContainmentSetupMenu — generates Boot/MainMenu/Lab/Results,
                 builds the lab room with XR-grabbable puzzles, and tries to
                 instantiate an XR Origin via GameObject>XR>XR Origin (VR).
```

## License

Code: MIT. Assets: see `CREDITS.md` (all CC0).
