# HCI Trilogy — *The Lab*

Three Unity games sharing one sci-fi-lab theme but built for three different input modalities. Created for an HCI (Human–Computer Interaction) course project.

| # | Game | Modality | HCI principles |
|---|---|---|---|
| 1 | [`Signal-2D/`](./Signal-2D)         | 2D, keyboard rhythm  | Multimodal feedback, Fitts's Law, accessibility |
| 2 | [`Lockdown-3D/`](./Lockdown-3D)     | 3D first-person FPS  | Affordances, signifiers, mapping, diegetic UI, constraints |
| 3 | [`Containment-VR/`](./Containment-VR) | 6-DoF VR             | Natural mapping, direct manipulation, embodiment, locomotion comfort |

The three games share aesthetic, palette, font, and SFX library. All assets used are CC0 / public-domain — see each game's `CREDITS.md`.

---

## Repository layout

```
HCI-Trilogy/
├── README.md                 (this file)
├── .gitignore                (Unity standard)
├── .gitattributes            (Git LFS rules for binary assets)
├── docs/
│   └── HCI_REPORT.md         (the academic write-up — see template)
├── Signal-2D/                (Unity project 1)
│   ├── README.md
│   ├── CREDITS.md
│   ├── FINAL_STEPS_IN_EDITOR.md
│   ├── Packages/
│   ├── ProjectSettings/
│   └── Assets/_Project/
├── Lockdown-3D/              (Unity project 2)
└── Containment-VR/           (Unity project 3)
```

Each Unity project is self-contained — open with Unity Hub by clicking *Add → select project folder*.

---

## Tech stack

- **Unity Editor**: 6000.0 LTS (Unity 6 LTS) — falls back to 2022.3 LTS automatically.
- **IDE**: Visual Studio 2022 Community (or Rider / VS Code with Unity extension).
- **Render pipeline**: Universal Render Pipeline (URP).
- **Input**: New Input System (`com.unity.inputsystem`).
- **VR**: OpenXR + XR Interaction Toolkit. Targets Meta Quest 2/3/Pro (Android) and PCVR (Windows + SteamVR/Meta Link).

---

## Quick start

1. **Clone** with Git LFS:
   ```bash
   git lfs install
   git clone https://github.com/0xN0RMXL/HCI-Trilogy.git
   cd HCI-Trilogy
   ```
2. **Open Unity Hub** → *Add → Existing project* → select `Signal-2D/` (or one of the others).
3. Unity will resolve packages, import assets, and generate `Library/` (this can take a few minutes the first time).
4. Read that project's `FINAL_STEPS_IN_EDITOR.md` — usually 3–5 menu clicks to compose the scenes via the included `HCI > Setup` editor menu.
5. Press **Play** in the Boot scene.

For builds: `File → Build Settings → Build`. Each project's README documents target platform + settings.

---

## HCI report

The accompanying academic write-up lives in [`docs/HCI_REPORT.md`](./docs/HCI_REPORT.md). It compares the same task across the three input modalities and draws on Norman, Shneiderman, Fitts, LaViola, and Slater.

---

## License

Code: MIT. Assets: each from a CC0 / public-domain source — see per-project `CREDITS.md` for full attribution.
