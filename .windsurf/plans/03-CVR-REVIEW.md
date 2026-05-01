# Project Structure Review — Containment-VR

## Folder Hierarchy

```
Containment-VR/
├── Assets/
│   ├── XR/                          (XR interaction defaults)
│   ├── XRI/                         (XRI starter assets)
│   ├── _Project/
│   │   └── Scripts/                 (ALL source code)
│   │       ├── Core/                (5 files — duplicated from core pkg)
│   │       ├── Editor/              (1 file — ContainmentSetupMenu.cs)
│   │       ├── Player/              (1 file — HapticBus.cs)
│   │       ├── Puzzles/             (8 files — VR-specific puzzles)
│   │       └── UI/                  (4 files — scene controllers)
│   ├── DefaultVolumeProfile.asset
│   └── UniversalRenderPipelineGlobalSettings.asset
├── Packages/
│   ├── manifest.json                (references com.hcitrilogy.core + XR packages)
│   └── packages-lock.json
└── ProjectSettings/                 (20+ .asset files)
```

## Missing Assets

| Asset | Status | Impact |
|---|---|---|
| `Assets/_Project/Scenes/` | **MISSING** | No scenes to play |
| `Assets/_Project/Audio/` | **MISSING** | No sound effects or music |
| `Assets/_Project/Art/` | **MISSING** | No materials (setup menu creates them) |
| `Assets/_Project/Prefabs/` | **MISSING** | No prefabs |
| `Assets/_Project/Settings/*.mixer` | **MISSING** | SettingsManager can't control volume |
| Settings Panel GameObject | **MISSING** | Settings button does nothing |
| Credits Panel GameObject | **MISSING** | Credits button does nothing |
| AudioMixer asset | **MISSING** | Volume sliders have no effect |

## Script Inventory (18 scripts, ~1,257 lines)

| # | Script | Lines | Issues |
|---|---|---|---|
| 1 | `Core/Bootstrapper.cs` | 15 | Duplicate of core package |
| 2 | `Core/LocalizationStrings.cs` | 15 | Duplicate; Containment-specific strings |
| 3 | `Core/SceneFlow.cs` | 41 | Duplicate; VR-specific fade |
| 4 | `Core/ServiceLocator.cs` | 24 | Exact duplicate |
| 5 | `Core/SettingsManager.cs` | 68 | Duplicate; VR-specific settings |
| 6 | `Editor/ContainmentSetupMenu.cs` | 664 | Missing settings/credits panels; missing EventSystem; uses deprecated API |
| 7 | `Player/HapticBus.cs` | 56 | **Uses deprecated XRBaseInteractable/XRBaseInputInteractor** |
| 8 | `Puzzles/VRCable.cs` | 18 | Clean |
| 9 | `Puzzles/VRCableSocket.cs` | 73 | Clean |
| 10 | `Puzzles/VRDial.cs` | 82 | Clean |
| 11 | `Puzzles/VRDoor.cs` | 64 | Clean |
| 12 | `Puzzles/VRDrawer.cs` | 81 | Clean |
| 13 | `Puzzles/VRKeypad.cs` | 63 | Clean |
| 14 | `Puzzles/VRKeypadButton.cs` | 72 | Clean |
| 15 | `Puzzles/VRPuzzleStateMachine.cs` | 42 | Clean |
| 16 | `UI/LabSceneController.cs` | 53 | Clean |
| 17 | `UI/MainMenuController.cs` | 28 | Missing settings/credits panel references |
| 18 | `UI/OxygenTimer.cs` | 37 | Clean |
| 19 | `UI/ResultsScreen.cs` | 39 | Clean |

## Detailed Code Review

### HapticBus.cs — CRITICAL (Deprecated API)

```csharp
// Line 18: XRBaseInteractable removed in XRI 3.0
private XRBaseInteractable _last;
// Line 24: XRBaseInputInteractor removed in XRI 3.0
XRBaseInputInteractor interactor = ...
```

Must migrate to `XRGrabInteractable` / `XRSimpleInteractable` and `XRBaseControllerInteractor`.

### ContainmentSetupMenu.cs — Multiple Issues

1. **No settings/credits panels created** — MainMenuController's `settingsPanel` and `creditsPanel` fields are null
2. **No EnsureEventSystem() call** — UI buttons won't respond without EventSystem
3. **No TrackedDeviceGraphicRaycaster** on world-space canvases — VR controllers can't interact with UI
4. **Uses deprecated XR API** in some helper methods

### MainMenuController.cs — Missing Panel References

```csharp
[SerializeField] private GameObject settingsPanel;  // null at runtime
[SerializeField] private GameObject creditsPanel;   // null at runtime
```

Toggle method silently no-ops on null:
```csharp
private static void Toggle(GameObject g) { if (g != null) g.SetActive(!g.activeSelf); }
```
