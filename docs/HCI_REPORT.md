# HCI-Trilogy: One Lab, Three Modalities

**Course:** Human-Computer Interaction
**Author:** Omar Ammar (omarammar@std.mans.edu.eg)
**Repository:** https://github.com/0xN0RMXL/HCI-Trilogy

---

## 1. Abstract

This project presents three Unity games that share a single fictional setting
— a small sci-fi laboratory — and isolate one variable: the input modality.
**Signal** (2D) uses keyboard input only, **Lockdown** (3D, first-person)
uses keyboard + mouse, and **Containment** (VR) uses tracked controllers in
room-scale 6-DoF VR. Because the *task* (escape the lab) and the *aesthetic*
are kept constant, comparative observations are attributable to the input
modality rather than to incidental design differences. The trilogy is
intended as a small-scale demonstrator of Norman's design principles
(Norman, 2013), Shneiderman's direct-manipulation guidelines (Shneiderman,
1983; Hutchins, Hollan & Norman, 1985), and contemporary VR-comfort
practice (LaViola, 2000; Slater, 2009).

---

## 2. Design rationale

The strongest pedagogical move available to a student trilogy is to hold
the design space constant and vary one axis. Picking three unrelated games
would produce three unrelated reports. We instead pick one verb — "escape"
— and three input vocabularies for it. This makes the comparative table at
the end of this report (§7) the central deliverable.

The lab setting is shared across all three:

- A 4-digit keypad mounted on the east wall (code: **1485**, hinted at by a
  paper note on the desk that mentions "decay constant: 1.485").
- A pair of color-coded cables (cyan, amber) that must be plugged into
  same-colored wall sockets.
- A rotatable dial console that must be turned to position **7** (red mark).
- A door that unlocks once all four sub-puzzles are solved.
- A wall-mounted oxygen timer (8 minutes) — diegetic UI, no HUD.

The aesthetic is dark blue / cyan with amber and red accents; one font
(Liberation Sans); one mixer with three groups (Master, Music, SFX). The
trilogy has only one art style.

---

## 3. Game 1 — *Signal* (2D, keyboard)

### 3.1. Mechanics

Four lanes (D / F / J / K). Notes scroll downward; the player presses the
matching key when each note crosses the hit line. Hits are graded as
*Perfect* (±40 ms) or *Good* (±100 ms); anything later is a *Miss*. Combo
multiplies score (up to ×4). Health bleeds on misses; running out ends the
song.

### 3.2. HCI principles demonstrated

- **Multimodal feedback.** Every press fires lane flash + particle burst +
  per-judgment audio + screen-shake. The player has four redundant channels
  reporting the same event so they're never starved of feedback.
- **Fitts's Law** (MacKenzie, 1992). The four lanes are fixed, large,
  spatially adjacent rectangles; the resting hand position (D-F-J-K) is the
  lowest-Fitts cost configuration on a QWERTY keyboard.
- **Accessibility.** Colorblind mode adds shape signifiers; a hold-to-play
  toggle replaces taps with sustained holds; note-speed and audio offset
  are user-configurable.
- **Latency calibration.** A built-in calibration scene measures the user's
  individual audio + monitor latency by asking them to tap on the beat;
  the offset is stored in `PlayerPrefs` and used by the Conductor.

### 3.3. Notable implementation detail

Rhythm timing uses `AudioSettings.dspTime` rather than `Time.time`, which
prevents drift over the course of a song:

```csharp
_dspStartTime = AudioSettings.dspTime + leadInSeconds;
audioSource.PlayScheduled(_dspStartTime);
SongPositionSeconds = (AudioSettings.dspTime - _pauseAccum)
                     - _dspStartTime
                     - userOffsetSeconds
                     - songOffsetSeconds;
```

`Time.time` is frame-clocked and can drift a frame or two per second under
load; `dspTime` is the audio engine's sample clock and is stable to within
microseconds.

---

## 4. Game 2 — *Lockdown* (3D, first-person)

### 4.1. Mechanics

The same lab as Signal, but the player walks through it in first-person.
WASD + mouse-look + a single interaction key (E). The crosshair is a small
dot; when looking at an interactable, a prompt appears beneath the crosshair
("[E] Pick up cable"). Same four puzzles as the trilogy spec.

### 4.2. HCI principles demonstrated

- **Affordances & signifiers** (Norman, 2013). Each puzzle object has a
  shape that signals its function: handles afford pulling, dials afford
  twisting, sockets afford insertion. There is no text label saying "this
  is a drawer."
- **Mapping.** A single interaction key is used for everything; the *prompt*
  shows the player what that key will do, so the mapping is consistent
  even though the verbs change.
- **Diegetic UI** (Galloway, 2006). The oxygen timer is a wall-mounted
  screen, not a HUD overlay. Notes are paper objects opened with `E`. There
  is no permanent on-screen HUD — every piece of information lives in the
  world.
- **Constraints** (Norman). The dial has 12 detents, converting a continuous
  input (mouse-X) into a discrete state space.
- **Feedback.** Indicator lights on each puzzle (red → green) plus audio
  cues for accept / reject.

### 4.3. Notable implementation detail

Pitch is applied to a child `cameraPivot` while yaw is applied to the
player's root. This decoupling prevents the `CharacterController` collider
from rotating with the camera's pitch, which would cause physics jitter:

```csharp
transform.Rotate(0f, lookDelta.x * sensitivity, 0f);          // root yaw
cameraPivot.localEulerAngles = new Vector3(_pitch, 0f, 0f);   // child pitch
```

---

## 5. Game 3 — *Containment* (VR)

### 5.1. Mechanics

The same lab in room-scale 6-DoF VR. The player physically pokes the
keypad, twists the dial, plugs cables into sockets, and pulls the drawer.
Locomotion defaults to teleport + snap-turn; smooth locomotion is opt-in.
Tunneling vignette is on by default.

### 5.2. HCI principles demonstrated

- **Natural mapping** (Norman). Twisting the wrist *is* the input. The
  `VRDial` script reads the controller's Y rotation while held and applies
  the delta to the dial transform — there is no explicit mapping layer.
- **Direct manipulation** (Shneiderman, 1983; Hutchins, Hollan & Norman,
  1985). The hand replaces the cursor. Objects respond to the hand's pose
  in real time.
- **Embodiment & presence** (Slater, 2009; Bowman et al., 2004). Tracked
  hands matched to the user's real proprioception strongly increase
  task confidence and reduce error rates relative to abstracted input.
- **Multimodal feedback.** Every successful select fires controller
  haptics (`HapticBus`), spatial audio, and a visible state change (light
  color, plug snap). Three channels for each event.
- **Locomotion-comfort tradeoffs** (LaViola, 2000). Defaults are biased
  toward comfort — teleport over smooth, snap-turn over smooth-turn,
  vignette tunneling on — because simulator-sickness susceptibility varies
  significantly between users and a comfort default protects the median
  user. The settings menu lets confident users opt out.

### 5.3. Notable implementation detail

The `VRDrawer` constrains motion to a single local axis. While grabbed,
the drawer's local Z follows the controller's local-Z position relative to
the cabinet, clamped between closed and open positions:

```csharp
float currentZ = transform.parent.InverseTransformPoint(_gripT.position).z;
float delta = currentZ - _gripStartZLocal;
float z = Mathf.Clamp(_bodyStartZLocal + delta, _closedLocal.z, _closedLocal.z + openDistance);
body.localPosition = new Vector3(_closedLocal.x, _closedLocal.y, z);
```

This is cheaper and more reliable than a configurable joint, and gives the
"resistance at the limit" feel that makes a drawer feel like a drawer.

---

## 6. Cross-cutting design decisions

- **One service-locator pattern** per project (`ServiceLocator`,
  `SettingsManager`, `SceneFlow`, `PauseController`). All three games
  follow the same structural skeleton so the comparative observations
  cannot be confounded by code-architecture differences.
- **One color palette and font** across all three games.
- **One puzzle state machine** (`*PuzzleStateMachine`) per game that
  aggregates puzzle completion events and fires `OnAllSolved` so the door
  doesn't need to know about individual puzzles.
- **CC0 assets only.** No login-walled or attribution-required resources.
- **No `Resources.Load`, no `FindObjectsOfType` in `Update`, no `Any`-typed
  reflection.** The codebase is intentionally pedestrian C# so it's easy to
  read by a TA grading the project.

---

## 7. Comparative analysis (the central table)

The same three sub-tasks compared across the three modalities.

| Sub-task | Signal (2D, keyboard) | Lockdown (3D, mouse+kbd) | Containment (VR) |
|---|---|---|---|
| **Hit a target on time** | Press D / F / J / K when note crosses the line | Click on a small button across the room | Reach out, physically poke the button |
| **Cognitive load** | Pattern memorization — convert color/lane to muscle memory | Spatial scanning + symbolic mapping (E key) | None — same as real life |
| **Mapping type** | Symbolic, learned (Norman: "arbitrary") | Symbolic, single-key, but the *prompt* makes it explicit | Natural — the wrist is the input |
| **Feedback channels** | Visual flash + particle + audio + screen-shake + HUD | Indicator light + audio + animation | Light + audio + haptic + animation |
| **Failure mode** | Misses (timing-only) | Wrong code triggers a red flash; player can retry | Plug wrong color rejects; player simply tries another |
| **Accessibility** | Colorblind & hold-to-play modes; latency calibration | Sensitivity, invert-Y, head-bob toggles | Locomotion + comfort presets, dominant-hand swap |
| **Physical fatigue** | Low (resting on keys) | Low (mouse) | Moderate (arms-up reaching) |
| **Onboarding cost** | Tutorial in 30 s | Tutorial in 60 s (must explain the prompt + E) | Effectively zero — the actions ARE the actions |

The most interesting cell of this table is the **Mapping type** row. In
Signal, the player must learn that *this color = that key* — a mapping
that exists nowhere outside the game. In Lockdown, the player must learn
that *crosshair-aim + E = "do the verb shown in the prompt"* — fewer
mappings (one key for everything) but still arbitrary. In Containment, the
mapping disappears: the player's existing physical knowledge of how to
twist a dial transfers directly. This is the textbook definition of
*natural mapping* (Norman, 2013, ch. 3).

The corresponding cost is in the **Physical fatigue** row: VR is the only
modality where the input has a measurable energy cost. A VR session of
more than ~10 minutes of arms-up interaction will fatigue the median user;
consequently, *Containment* is intentionally short.

---

## 8. Limitations & future work

- **No empirical user study.** Sample size of 1 (the author). A proper
  followup would A/B test the same task across the three modalities with
  ≥10 participants per condition and compare time-to-completion + NASA-TLX
  workload scores.
- **No dynamic difficulty in Signal.** A real rhythm game adapts its note
  density to player skill (Yannakakis & Togelius, 2018). Out of scope for
  this submission.
- **Locomotion in Containment is teleport-only by default.** A full study
  would compare smooth + snap-turn + teleport on the same puzzle and report
  simulator-sickness incidence.
- **No hand tracking.** Containment uses controllers only. A hand-tracked
  follow-up would test whether the natural-mapping advantage compounds
  when the player's *bare hand* is the input.

---

## 9. References

- Bowman, D., Kruijff, E., LaViola, J. & Poupyrev, I. (2004).
  *3D User Interfaces: Theory and Practice.* Addison-Wesley.
- Galloway, A. (2006). *Gaming: Essays on Algorithmic Culture.* University
  of Minnesota Press. [diegetic UI]
- Hutchins, E. L., Hollan, J. D. & Norman, D. A. (1985). Direct
  manipulation interfaces. *Human–Computer Interaction*, 1(4), 311–338.
- LaViola, J. J. (2000). A discussion of cybersickness in virtual
  environments. *ACM SIGCHI Bulletin*, 32(1), 47–56.
- MacKenzie, I. S. (1992). Fitts's law as a research and design tool in
  human–computer interaction. *Human–Computer Interaction*, 7(1), 91–139.
- Norman, D. A. (2013). *The Design of Everyday Things* (rev. ed.). Basic
  Books.
- Shneiderman, B. (1983). Direct manipulation: A step beyond programming
  languages. *Computer*, 16(8), 57–69.
- Slater, M. (2009). Place illusion and plausibility can lead to realistic
  behaviour in immersive virtual environments. *Philosophical Transactions
  of the Royal Society B*, 364(1535), 3549–3557.
- Yannakakis, G. N. & Togelius, J. (2018). *Artificial Intelligence and
  Games.* Springer.

---

## Appendix A — Repository layout

```
HCI-Trilogy/
├── Signal-2D/         Game 1 (2D, keyboard)
├── Lockdown-3D/       Game 2 (3D, first-person, kbd+mouse)
├── Containment-VR/    Game 3 (VR, room-scale)
└── docs/
    └── HCI_REPORT.md  (this file)
```

Each game directory is an independent Unity project with its own
`Assets/`, `Packages/`, and `ProjectSettings/`. They share no code at
runtime, by design — each is isolated so it can be opened, played, and
graded independently.
