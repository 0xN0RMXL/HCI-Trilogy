# Signal — Final Steps in Unity Editor

Once Unity finishes importing the project (the first import takes 1–3 minutes — Unity has to compile scripts, fetch packages, and generate `.meta` files for every asset), do the following **once**.

## 1. Switch project to Linear color space + new Input System

`Edit → Project Settings → Player → Other Settings`:
- **Color Space**: change to **Linear**. (Unity will warn that switching may invalidate colors — accept.)
- **Active Input Handling**: change to **Input System Package (New)**. (Unity will prompt to restart — let it.)

## 2. Run the one-click setup

After the editor restarts, top menu: **HCI Signal → Setup → Initialize Project**.

This will:
- Create `Assets/_Project/Scenes/Boot.unity`, `MainMenu.unity`, `Calibration.unity`, `Game.unity`, `Results.unity`.
- Create a `SongData` asset at `Assets/_Project/Settings/Song1.asset` plus the matching chart JSON at `Assets/_Project/Charts/Song1.json` (a 120 BPM demo pattern, 116 notes).
- Add all 5 scenes to **Build Settings** in the right order.

Click OK on the dialog. Save the project (`File → Save`).

## 3. Add a music track (optional but recommended)

The setup creates a SongData asset, but it has no audio clip yet. To add one:
1. Download a CC0 OGG track from https://opengameart.org/art-search-advanced?field_art_type_tid%5B%5D=12&field_art_licenses_tid%5B%5D=4 (filter: Music + CC0). Aim for 100–135 BPM, 1–3 minutes.
2. Drop it into `Assets/_Project/Audio/Music/`.
3. Click `Assets/_Project/Settings/Song1.asset`. In the Inspector, drag your audio clip into the **Track** field and set **BPM** to the track's BPM.
4. Update `Assets/_Project/Charts/Song1.json` if your track is at a different BPM (the demo chart is at 120 BPM).
5. Update `CREDITS.md` with the source + author.

If you skip this step the game still runs — the conductor just won't have audio to play; you'll see notes scrolling but in silence.

## 4. (Optional) Add hit / miss / UI sound effects

Drop CC0 `.wav` files into `Assets/_Project/Audio/SFX/`. Drag them into the matching field on the relevant `Lane` (for hit feedback) or button. The default scene works without them — you'll just have visual feedback without audio.

## 5. Press Play

Open `Assets/_Project/Scenes/Boot.unity`. Press Play. You should see:
1. Boot scene fades to MainMenu.
2. Five buttons: Play / Calibrate / Settings / Credits / Quit.
3. Click Play → Game scene → notes start scrolling at the configured BPM, you press D / F / J / K to hit them.
4. After the song ends (or if you fail by missing too many) → Results scene with grade, accuracy, and Retry / Menu buttons.

## 6. Build (Windows)

`File → Build Settings → Windows x86_64 → Build`. Output to `Builds/Windows/Signal/`. The first build takes 5–10 min; subsequent builds are faster.

## Troubleshooting

| Symptom | Fix |
|---|---|
| "All compiler errors have to be fixed before you can enter playmode!" | Ensure Color Space is Linear AND Input System Package is selected; restart Unity. |
| Notes scroll but pressing keys does nothing | The `Lane` GameObjects need their `laneAction` field set. Open `Game.unity`, select each `Lane0..3`, and drag the corresponding action reference (`Gameplay/Lane0..3`) from `Assets/_Project/Settings/PlayerInput.inputactions` into the `Lane Action` field. (The setup menu does this automatically if the InputActions asset exists — if it didn't, see step 7.) |
| No audio | See step 3 — add a track to `Song1.asset`. |

## 7. Input Actions asset

If the `Lane Action` references on the Lanes are empty:
1. Right-click in `Assets/_Project/Settings/` → Create → Input Actions. Name it `PlayerInput`.
2. Open it. Add Action Map `Gameplay`. Add 5 actions: `Lane0`, `Lane1`, `Lane2`, `Lane3`, `Pause`. Bindings: `<Keyboard>/d`, `<Keyboard>/f`, `<Keyboard>/j`, `<Keyboard>/k`, `<Keyboard>/escape`.
3. Tick **Generate C# Class**. Save.
4. In `Game.unity`, select each Lane and drag the corresponding action from this asset into the `Lane Action` slot.

## What's *not* automated

- TextMeshPro SDF font assets — the project uses Unity's built-in legacy `Text` component for simplicity. If you want the cleaner TMP look, swap each `Text` for `TextMeshProUGUI` and generate SDF font assets via `Window → TextMeshPro → Font Asset Creator`.
- Sprite slicing — the setup uses procedurally-generated white squares. If you import the Kenney UI pack (mentioned in CREDITS.md), you can replace the `Note` prefab's sprite with a nicer one.
- Lighting baking — none needed; the game is 2D / Unlit.

That's it. Total time after Unity finishes importing: ~5 minutes.
