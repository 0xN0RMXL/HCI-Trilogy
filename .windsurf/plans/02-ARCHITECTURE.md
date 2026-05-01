# Architecture Overview

## Shared Pattern

All three games follow an identical architectural skeleton:

```
Boot Scene → MainMenu Scene → Game Scene → Results Scene
     ↓              ↓              ↓
 [Managers]    [UI Canvas]    [Game Systems]
  - SceneFlow   - MainMenu    - PuzzleStateMachine / Conductor
  - SettingsMgr   Controller  - Puzzles / Lanes
  - PauseCtrl   - Settings    - Timer / Judge
  - Bootstrapper  Panel       - Door / NoteSpawner
                - Credits
                  Panel
```

## Service Locator Pattern

```
ServiceLocator (static dictionary<Type, object>)
  ├── SettingsManager  (registered in Awake)
  ├── Conductor        (Signal-2D only)
  └── [extensible]
```

## Scene Flow

```
SceneFlow (singleton, DontDestroyOnLoad)
  ├── LoadAsync(sceneName) → fade-to-black → SceneManager.LoadSceneAsync → fade-in
  └── Built-in fader Canvas (sortingOrder 9999)
```

## Dependency Graph — Containment-VR

```
Bootstrapper → SceneFlow
MainMenuController → SceneFlow
LabSceneController → OxygenTimer, VRDoor, VRPuzzleStateMachine
VRPuzzleStateMachine → VRKeypad, VRDial, VRCableSocket×2
VRDoor → VRPuzzleStateMachine.OnAllSolved
VRCableSocket → VRCable (color match)
VRKeypadButton → VRKeypad (digit press)
HapticBus → XRGrabInteractable (FIX NEEDED: currently uses deprecated XRBaseInteractable)
```

## Dependency Graph — Lockdown-3D

```
Bootstrapper → SceneFlow
MainMenuController → SceneFlow
LabSceneController → OxygenTimer, DoorLock, PuzzleStateMachine
PuzzleStateMachine → Keypad, Dial, CableSocket×2
DoorLock → PuzzleStateMachine
Interactor → IInteractable (Cable, CableSocket, Dial, Drawer, KeypadButton, NoteItem, Pickupable, DoorLock)
FirstPersonController → PauseController, SettingsManager, CharacterController
NoteReader → FirstPersonController
PauseController → InputActionAsset
```

## Dependency Graph — Signal-2D

```
Bootstrapper → SceneFlow
MainMenuController → SceneFlow
CalibrationController → SettingsManager, SceneFlow
GameSceneController → Conductor, NoteSpawner, Judge, ScoreManager, ComboMeter, HealthMeter
Conductor → SongData, AudioSource, PauseController
NoteSpawner → SongData, Conductor, Lane[]
Lane → Judge, SettingsManager, InputActionAsset
Judge → Conductor, FeedbackBus
Note → Conductor, SongData, Lane, Judge
ScoreManager → ComboMeter, FeedbackBus
ComboMeter → FeedbackBus
HealthMeter → FeedbackBus
CameraShaker → FeedbackBus
HUD → ScoreManager, ComboMeter, HealthMeter, Conductor
SettingsPanel → SettingsManager
PauseMenu → PauseController, SceneFlow
```

## Code Duplication Map

| Script | Core Pkg | Containment | Lockdown | Signal |
|---|---|---|---|---|
| Bootstrapper | ✅ | ✅ (copy) | ✅ (copy) | ✅ (copy) |
| LocalizationStrings | ✅ | ✅ (copy) | ✅ (copy) | ✅ (copy) |
| SceneFlow | ✅ | ✅ (VR fade) | ✅ (UI fade) | ✅ (runtime Canvas) |
| ServiceLocator | ✅ | ✅ (basic) | ✅ (basic) | ✅ (extended) |
| SettingsManager | ✅ | ✅ (VR) | ✅ (FPS) | ✅ (rhythm) |

**Signal-2D's ServiceLocator** is the most complete version (has `TryGet`, `Unregister`, `Clear`). This should become the canonical version in the shared package.
