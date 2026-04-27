#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HCITrilogy.Lockdown.Audio;
using HCITrilogy.Lockdown.Core;
using HCITrilogy.Lockdown.Interaction;
using HCITrilogy.Lockdown.Player;
using HCITrilogy.Lockdown.Puzzles;
using HCITrilogy.Lockdown.UI;

namespace HCITrilogy.Lockdown.EditorTools
{
    /// <summary>
    /// One-click project setup for Lockdown. Generates Boot/MainMenu/Lab/Results
    /// scenes, builds the lab room out of primitives, places puzzles and
    /// wires their references, and adds scenes to Build Settings.
    ///
    /// Usage: Top menu → HCI Lockdown → Setup → Initialize Project.
    /// Idempotent: rerunning overwrites scenes from scratch.
    /// </summary>
    public static class LockdownSetupMenu
    {
        private const string ScenesFolder   = "Assets/_Project/Scenes";
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string MaterialsFolder = "Assets/_Project/Art/Materials";
        private const string InputAssetPath = SettingsFolder + "/PlayerInput.inputactions";

        private static readonly Color FloorTone   = new(0.18f, 0.21f, 0.25f);
        private static readonly Color WallTone    = new(0.25f, 0.28f, 0.32f);
        private static readonly Color AccentCyan  = new(0.36f, 0.88f, 1.00f);
        private static readonly Color AccentAmber = new(1.00f, 0.71f, 0.33f);
        private static readonly Color AccentRed   = new(1.00f, 0.36f, 0.36f);
        private static readonly Color AccentGreen = new(0.30f, 0.88f, 0.63f);

        [MenuItem("HCI Lockdown/Setup/Initialize Project")]
        public static void InitializeProject()
        {
            EnsureFolders();
            SetPlayerSettings();
            CreateBootScene();
            CreateMainMenuScene();
            CreateLabScene();
            CreateResultsScene();
            AddScenesToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("HCI Lockdown",
                "Project initialized.\n\n" +
                "Scenes created in Assets/_Project/Scenes and added to Build Settings.\n\n" +
                "Required: Edit > Project Settings > Player > Other Settings:\n" +
                "  • Color Space: Linear\n" +
                "  • Active Input Handling: 'Input System Package (New)'\n" +
                "Unity may prompt to restart.\n\n" +
                "Then open Boot.unity and press Play.",
                "OK");
        }

        private static void EnsureFolders()
        {
            string[] p = {
                "Assets/_Project", ScenesFolder, SettingsFolder,
                "Assets/_Project/Art", MaterialsFolder,
                "Assets/_Project/Audio", "Assets/_Project/Audio/SFX",
                "Assets/_Project/Audio/Music", "Assets/_Project/Prefabs"
            };
            foreach (var f in p)
                if (!AssetDatabase.IsValidFolder(f))
                {
                    var parent = Path.GetDirectoryName(f).Replace('\\', '/');
                    var leaf = Path.GetFileName(f);
                    AssetDatabase.CreateFolder(parent, leaf);
                }
        }

        private static void SetPlayerSettings()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.companyName = "HCI-Trilogy";
            PlayerSettings.productName = "Lockdown";
            PlayerSettings.runInBackground = true;
        }

        // --------- shared builders ---------

        private static Material MakeMat(string name, Color c, float smoothness = 0.1f)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else m.color = c;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            return m;
        }

        private static GameObject MakeBox(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            if (mat != null) g.GetComponent<Renderer>().sharedMaterial = mat;
            return g;
        }

        private static GameObject MakeButton(string name, Vector3 pos, Color col, Transform parent, out Highlightable h)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = new Vector3(0.12f, 0.12f, 0.06f);
            g.GetComponent<Renderer>().sharedMaterial = MakeMat($"Btn_{name}", col, 0.4f);
            h = g.AddComponent<Highlightable>();
            return g;
        }

        // --------- Boot ---------

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var managers = new GameObject("Managers");
            managers.AddComponent<SettingsManager>();
            managers.AddComponent<PauseController>();
            managers.AddComponent<SceneFlow>();
            var bs = new GameObject("Bootstrapper").AddComponent<Bootstrapper>();
            bs.transform.SetParent(managers.transform, false);

            var actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(InputAssetPath);
            if (actions != null)
            {
                var pc = managers.GetComponent<PauseController>();
                var so = new SerializedObject(pc);
                so.FindProperty("inputActions").objectReferenceValue = actions;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.SaveScene(scene, ScenesFolder + "/Boot.unity");
        }

        // --------- MainMenu ---------

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var canvasGO = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var bg = new GameObject("BG").AddComponent<Image>();
            bg.transform.SetParent(canvasGO.transform, false);
            bg.color = new Color(0.054f, 0.066f, 0.086f);
            var rt = bg.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var title = new GameObject("Title").AddComponent<Text>();
            title.transform.SetParent(canvasGO.transform, false);
            title.text = LocalizationStrings.Title;
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 96; title.color = AccentCyan;
            title.alignment = TextAnchor.MiddleCenter;
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0.5f, 0.7f); trt.anchorMax = new Vector2(0.5f, 0.7f);
            trt.sizeDelta = new Vector2(900, 140); trt.anchoredPosition = Vector2.zero;

            var sub = new GameObject("Subtitle").AddComponent<Text>();
            sub.transform.SetParent(canvasGO.transform, false);
            sub.text = LocalizationStrings.Subtitle;
            sub.font = title.font; sub.fontSize = 28; sub.color = new Color(0.6f, 0.6f, 0.6f);
            sub.alignment = TextAnchor.MiddleCenter;
            var srt = sub.rectTransform;
            srt.anchorMin = new Vector2(0.5f, 0.62f); srt.anchorMax = new Vector2(0.5f, 0.62f);
            srt.sizeDelta = new Vector2(700, 60); srt.anchoredPosition = Vector2.zero;

            Button MakeBtn(string label, Vector2 anchor)
            {
                var bGO = new GameObject("Btn_" + label, typeof(Image), typeof(Button));
                bGO.transform.SetParent(canvasGO.transform, false);
                var img = bGO.GetComponent<Image>();
                img.color = new Color(0.10f, 0.12f, 0.15f);
                var brt = (RectTransform)bGO.transform;
                brt.anchorMin = anchor; brt.anchorMax = anchor;
                brt.sizeDelta = new Vector2(360, 64);
                brt.anchoredPosition = Vector2.zero;
                var tGO = new GameObject("Text").AddComponent<Text>();
                tGO.transform.SetParent(bGO.transform, false);
                tGO.text = label; tGO.font = title.font; tGO.fontSize = 28;
                tGO.alignment = TextAnchor.MiddleCenter; tGO.color = Color.white;
                var trt2 = tGO.rectTransform; trt2.anchorMin = Vector2.zero; trt2.anchorMax = Vector2.one;
                trt2.offsetMin = trt2.offsetMax = Vector2.zero;
                return bGO.GetComponent<Button>();
            }

            var play     = MakeBtn(LocalizationStrings.BtnPlay,     new Vector2(0.5f, 0.45f));
            var settings = MakeBtn(LocalizationStrings.BtnSettings, new Vector2(0.5f, 0.36f));
            var credits  = MakeBtn(LocalizationStrings.BtnCredits,  new Vector2(0.5f, 0.27f));
            var quit     = MakeBtn(LocalizationStrings.BtnQuit,     new Vector2(0.5f, 0.18f));

            var menu = new GameObject("MainMenu").AddComponent<MainMenuController>();
            var mso = new SerializedObject(menu);
            mso.FindProperty("playButton").objectReferenceValue     = play;
            mso.FindProperty("settingsButton").objectReferenceValue = settings;
            mso.FindProperty("creditsButton").objectReferenceValue  = credits;
            mso.FindProperty("quitButton").objectReferenceValue     = quit;
            mso.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, ScenesFolder + "/MainMenu.unity");
        }

        // --------- Lab ---------

        private static void CreateLabScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Adjust the default sun.
            var sunGO = GameObject.Find("Directional Light");
            if (sunGO != null)
            {
                var sun = sunGO.GetComponent<Light>();
                sun.intensity = 0.6f;
                sun.color = new Color(0.85f, 0.92f, 1f);
                sunGO.transform.rotation = Quaternion.Euler(50f, 30f, 0f);
            }
            RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.16f);
            RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.08f);

            var matFloor   = MakeMat("Floor",  FloorTone, 0.05f);
            var matWall    = MakeMat("Wall",   WallTone,  0.10f);
            var matAccent  = MakeMat("Accent", AccentCyan, 0.6f);
            var matRed     = MakeMat("Red",    AccentRed,  0.4f);
            var matGreen   = MakeMat("Green",  AccentGreen,0.4f);
            var matAmber   = MakeMat("Amber",  AccentAmber,0.5f);

            // Room dimensions: 8 (X) x 3 (Y) x 6 (Z)
            const float W = 8f, H = 3f, D = 6f;
            var room = new GameObject("Lab").transform;

            MakeBox("Floor",    new Vector3(0, 0, 0),     new Vector3(W, 0.2f, D), matFloor, room);
            MakeBox("Ceiling",  new Vector3(0, H, 0),     new Vector3(W, 0.2f, D), matWall,  room);
            MakeBox("WallN",    new Vector3(0, H/2, D/2), new Vector3(W, H,   0.2f), matWall, room);
            MakeBox("WallS",    new Vector3(0, H/2,-D/2), new Vector3(W, H,   0.2f), matWall, room);
            MakeBox("WallE",    new Vector3(W/2, H/2, 0), new Vector3(0.2f, H, D),  matWall, room);
            MakeBox("WallW",    new Vector3(-W/2,H/2, 0), new Vector3(0.2f, H, D),  matWall, room);

            // --- Player rig ---
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, 0.95f, -D/2 + 1.0f);
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.34f; cc.center = new Vector3(0, 0.9f, 0);

            var camPivot = new GameObject("CameraPivot").transform;
            camPivot.SetParent(player.transform, false);
            camPivot.localPosition = new Vector3(0, 1.65f, 0);

            var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGO.tag = "MainCamera";
            camGO.transform.SetParent(camPivot, false);
            var cam = camGO.GetComponent<Camera>();
            cam.fieldOfView = 70; cam.nearClipPlane = 0.05f; cam.farClipPlane = 80f;

            // Remove the default scene camera (DefaultGameObjects creates one).
            foreach (var dc in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if (dc != cam) Object.DestroyImmediate(dc.gameObject);

            var fpc = player.AddComponent<FirstPersonController>();
            var fso = new SerializedObject(fpc);
            var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(InputAssetPath);
            fso.FindProperty("controller").objectReferenceValue = cc;
            fso.FindProperty("cameraPivot").objectReferenceValue = camPivot;
            fso.FindProperty("inputActions").objectReferenceValue = inputAsset;
            fso.ApplyModifiedPropertiesWithoutUndo();

            // Footstep system
            var footAudio = player.AddComponent<AudioSource>();
            footAudio.playOnAwake = false; footAudio.spatialBlend = 0f;
            var fs = player.AddComponent<FootstepSystem>();
            var fsso = new SerializedObject(fs);
            fsso.FindProperty("controller").objectReferenceValue = cc;
            fsso.FindProperty("source").objectReferenceValue = footAudio;
            fsso.ApplyModifiedPropertiesWithoutUndo();

            // --- HUD canvas ---
            var hudGO = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var hud = hudGO.GetComponent<Canvas>();
            hud.renderMode = RenderMode.ScreenSpaceOverlay;

            var dot = new GameObject("Crosshair").AddComponent<Image>();
            dot.transform.SetParent(hudGO.transform, false);
            dot.color = new Color(1, 1, 1, 0.55f);
            var drt = dot.rectTransform;
            drt.sizeDelta = new Vector2(8, 8);
            drt.anchoredPosition = Vector2.zero;

            var prompt = new GameObject("Prompt").AddComponent<Text>();
            prompt.transform.SetParent(hudGO.transform, false);
            prompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            prompt.fontSize = 24; prompt.alignment = TextAnchor.MiddleCenter;
            prompt.color = Color.white; prompt.text = "";
            var prt = prompt.rectTransform;
            prt.anchorMin = new Vector2(0.5f, 0.5f); prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = new Vector2(0, -28);
            prt.sizeDelta = new Vector2(600, 32);

            var crosshair = hudGO.AddComponent<CrosshairPrompt>();
            var co = new SerializedObject(crosshair);
            co.FindProperty("dot").objectReferenceValue = dot;
            co.FindProperty("promptText").objectReferenceValue = prompt;
            co.ApplyModifiedPropertiesWithoutUndo();

            var interactor = camGO.AddComponent<Interactor>();
            var io = new SerializedObject(interactor);
            io.FindProperty("viewCamera").objectReferenceValue = cam;
            io.FindProperty("inputActions").objectReferenceValue = inputAsset;
            io.FindProperty("crosshair").objectReferenceValue = crosshair;
            io.ApplyModifiedPropertiesWithoutUndo();

            // --- Note Reader overlay ---
            var nrPanel = new GameObject("NoteReader_Panel", typeof(Image));
            nrPanel.transform.SetParent(hudGO.transform, false);
            var nrImg = nrPanel.GetComponent<Image>();
            nrImg.color = new Color(0.04f, 0.05f, 0.07f, 0.92f);
            var nrt = nrImg.rectTransform;
            nrt.anchorMin = Vector2.zero; nrt.anchorMax = Vector2.one;
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            var nrTitle = new GameObject("Title").AddComponent<Text>();
            nrTitle.transform.SetParent(nrPanel.transform, false);
            nrTitle.font = prompt.font; nrTitle.fontSize = 42; nrTitle.color = AccentCyan;
            nrTitle.alignment = TextAnchor.UpperCenter;
            var ntrt = nrTitle.rectTransform;
            ntrt.anchorMin = new Vector2(0, 0.85f); ntrt.anchorMax = new Vector2(1, 0.95f);
            ntrt.offsetMin = ntrt.offsetMax = Vector2.zero;
            var nrBody = new GameObject("Body").AddComponent<Text>();
            nrBody.transform.SetParent(nrPanel.transform, false);
            nrBody.font = prompt.font; nrBody.fontSize = 26; nrBody.color = Color.white;
            nrBody.alignment = TextAnchor.UpperLeft;
            var nbrt = nrBody.rectTransform;
            nbrt.anchorMin = new Vector2(0.18f, 0.15f); nbrt.anchorMax = new Vector2(0.82f, 0.83f);
            nbrt.offsetMin = nbrt.offsetMax = Vector2.zero;
            var nrFooter = new GameObject("Footer").AddComponent<Text>();
            nrFooter.transform.SetParent(nrPanel.transform, false);
            nrFooter.font = prompt.font; nrFooter.fontSize = 18; nrFooter.color = new Color(0.7f, 0.7f, 0.7f);
            nrFooter.alignment = TextAnchor.LowerCenter;
            nrFooter.text = "Press [E] / [Esc] to close";
            var nfrt = nrFooter.rectTransform;
            nfrt.anchorMin = new Vector2(0, 0.05f); nfrt.anchorMax = new Vector2(1, 0.10f);
            nfrt.offsetMin = nfrt.offsetMax = Vector2.zero;

            var nr = hudGO.AddComponent<NoteReader>();
            var nro = new SerializedObject(nr);
            nro.FindProperty("panel").objectReferenceValue = nrPanel;
            nro.FindProperty("titleText").objectReferenceValue = nrTitle;
            nro.FindProperty("bodyText").objectReferenceValue = nrBody;
            nro.ApplyModifiedPropertiesWithoutUndo();

            // --- Pause panel ---
            var pausePanel = new GameObject("Pause_Panel", typeof(Image));
            pausePanel.transform.SetParent(hudGO.transform, false);
            var pImg = pausePanel.GetComponent<Image>();
            pImg.color = new Color(0, 0, 0, 0.65f);
            var prt2 = pImg.rectTransform;
            prt2.anchorMin = Vector2.zero; prt2.anchorMax = Vector2.one;
            prt2.offsetMin = prt2.offsetMax = Vector2.zero;
            Button MakePauseBtn(string label, float yAnchor)
            {
                var bGO = new GameObject("Btn_" + label, typeof(Image), typeof(Button));
                bGO.transform.SetParent(pausePanel.transform, false);
                bGO.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.15f);
                var brt = (RectTransform)bGO.transform;
                brt.anchorMin = new Vector2(0.5f, yAnchor); brt.anchorMax = new Vector2(0.5f, yAnchor);
                brt.sizeDelta = new Vector2(280, 56); brt.anchoredPosition = Vector2.zero;
                var t = new GameObject("Text").AddComponent<Text>();
                t.transform.SetParent(bGO.transform, false);
                t.text = label; t.font = prompt.font; t.fontSize = 24;
                t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
                var trt2 = t.rectTransform; trt2.anchorMin = Vector2.zero; trt2.anchorMax = Vector2.one;
                trt2.offsetMin = trt2.offsetMax = Vector2.zero;
                return bGO.GetComponent<Button>();
            }
            var resumeBtn = MakePauseBtn(LocalizationStrings.BtnResume, 0.55f);
            var menuBtn   = MakePauseBtn(LocalizationStrings.BtnMenu,   0.45f);

            var pause = hudGO.AddComponent<PauseMenu>();
            var pso = new SerializedObject(pause);
            pso.FindProperty("panel").objectReferenceValue = pausePanel;
            pso.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
            pso.FindProperty("menuButton").objectReferenceValue = menuBtn;
            pso.ApplyModifiedPropertiesWithoutUndo();

            // --- Puzzles ---

            // Note on desk (clue: code 1485)
            var deskMat = MakeMat("Desk", new Color(0.30f, 0.32f, 0.38f), 0.2f);
            var desk = MakeBox("Desk", new Vector3(-W/2 + 1.4f, 0.5f, D/2 - 1.2f), new Vector3(1.4f, 1.0f, 0.7f), deskMat, room);
            var noteGO = MakeBox("Memo", new Vector3(-W/2 + 1.4f, 1.05f, D/2 - 1.2f), new Vector3(0.32f, 0.02f, 0.42f), MakeMat("Paper", new Color(0.92f, 0.90f, 0.84f), 0.05f), room);
            noteGO.AddComponent<Highlightable>();
            var note = noteGO.AddComponent<NoteItem>();
            var nso = new SerializedObject(note);
            nso.FindProperty("title").stringValue = "Lab notebook";
            nso.FindProperty("contents").stringValue =
                "Lab notebook — entry 14\n\n" +
                "Reactor decay constant for cell A: 1.485\n" +
                "(remember to drop the decimal when entering it on the door panel)\n\n" +
                "Test cycle for sockets:\n" +
                "  • Cyan plug → primary socket\n" +
                "  • Amber plug → secondary socket\n\n" +
                "Calibration dial: position 7 (red mark).";
            nso.ApplyModifiedPropertiesWithoutUndo();

            // Drawer (with key card inside, decorative only)
            var drawerHousing = MakeBox("Cabinet", new Vector3(W/2 - 1.0f, 0.5f, D/2 - 1.0f), new Vector3(1.2f, 1.0f, 1.0f), deskMat, room);
            var drawerBody = MakeBox("DrawerBody", new Vector3(0, 0.05f, 0.15f), new Vector3(0.95f, 0.45f, 0.7f), deskMat, drawerHousing.transform);
            var drawerHandle = MakeBox("Handle", new Vector3(0, 0.05f, 0.36f), new Vector3(0.30f, 0.05f, 0.04f), MakeMat("Handle", new Color(0.8f, 0.8f, 0.8f), 0.6f), drawerBody.transform);
            drawerBody.AddComponent<Highlightable>();
            var drawer = drawerBody.AddComponent<Drawer>();
            var dso = new SerializedObject(drawer);
            dso.FindProperty("body").objectReferenceValue = drawerBody.transform;
            dso.ApplyModifiedPropertiesWithoutUndo();

            // Keypad (on wall east, near door)
            var keypadBack = MakeBox("KeypadHousing", new Vector3(W/2 - 0.15f, 1.4f, -1.5f), new Vector3(0.1f, 0.6f, 0.6f), MakeMat("KeypadBack", new Color(0.10f, 0.12f, 0.15f), 0.3f), room);
            var keypad = keypadBack.AddComponent<Keypad>();
            var keypadDisplayGO = new GameObject("DisplayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            keypadDisplayGO.transform.SetParent(keypadBack.transform, false);
            var disCanvas = keypadDisplayGO.GetComponent<Canvas>();
            disCanvas.renderMode = RenderMode.WorldSpace;
            keypadDisplayGO.transform.localPosition = new Vector3(-0.55f, 0.18f, 0);
            keypadDisplayGO.transform.localRotation = Quaternion.Euler(0, -90, 0);
            keypadDisplayGO.transform.localScale = Vector3.one * 0.005f;
            var disText = new GameObject("Display").AddComponent<Text>();
            disText.transform.SetParent(keypadDisplayGO.transform, false);
            disText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            disText.fontSize = 80; disText.color = AccentCyan;
            disText.alignment = TextAnchor.MiddleCenter;
            disText.text = "____";
            var disrt = disText.rectTransform;
            disrt.sizeDelta = new Vector2(220, 80);
            var keypadLight = new GameObject("StatusLight").AddComponent<Light>();
            keypadLight.transform.SetParent(keypadBack.transform, false);
            keypadLight.transform.localPosition = new Vector3(-0.6f, 0.25f, 0);
            keypadLight.type = LightType.Point; keypadLight.range = 0.6f; keypadLight.intensity = 1.5f;
            keypadLight.color = AccentRed;

            var kso = new SerializedObject(keypad);
            kso.FindProperty("correctCode").stringValue = "1485";
            kso.FindProperty("display").objectReferenceValue = disText;
            kso.FindProperty("statusLight").objectReferenceValue = keypadLight;
            kso.ApplyModifiedPropertiesWithoutUndo();

            // Keypad buttons (3x4 grid with C and 0)
            int[] grid = { 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, 0, -2 }; // -1 = clear, -2 = blank
            for (int i = 0; i < grid.Length; i++)
            {
                int row = i / 3; int col = i % 3;
                int v = grid[i];
                if (v == -2) continue;
                Vector3 lpos = new(-0.55f, -0.18f - row * 0.10f, -0.16f + col * 0.16f);
                var bGO = MakeButton($"Key_{(v >= 0 ? v.ToString() : "C")}", lpos, v == -1 ? AccentRed : AccentCyan, keypadBack.transform, out _);
                var kb = bGO.AddComponent<KeypadButton>();
                var kbo = new SerializedObject(kb);
                kbo.FindProperty("keypad").objectReferenceValue = keypad;
                kbo.FindProperty("digit").intValue = v == -1 ? 0 : v;
                kbo.FindProperty("isClear").boolValue = v == -1;
                kbo.ApplyModifiedPropertiesWithoutUndo();
            }

            // Cable bench: 2 sockets + 2 cables
            var bench = MakeBox("Bench", new Vector3(0, 0.45f, D/2 - 0.4f), new Vector3(2.4f, 0.9f, 0.6f), deskMat, room);

            CableSocket socketA = MakeSocket("SocketA", new Vector3(-0.5f, 0.35f, -0.25f), AccentCyan, matAccent, bench.transform);
            CableSocket socketB = MakeSocket("SocketB", new Vector3(0.5f,  0.35f, -0.25f), AccentAmber, matAmber,  bench.transform);

            var cableA = MakeCable("CableA", new Vector3(-0.7f, 0.5f, 0), AccentCyan,  matAccent, bench.transform);
            var cableB = MakeCable("CableB", new Vector3(0.7f,  0.5f, 0), AccentAmber, matAmber,  bench.transform);

            // Dial console (west wall)
            var dialBase = MakeBox("DialBase", new Vector3(-W/2 + 0.15f, 1.2f, 0), new Vector3(0.1f, 0.8f, 0.8f), MakeMat("DialBase", new Color(0.10f, 0.12f, 0.15f), 0.3f), room);
            var dialDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dialDisc.name = "DialDisc";
            dialDisc.transform.SetParent(dialBase.transform, false);
            dialDisc.transform.localPosition = new Vector3(0.07f, 0, 0);
            dialDisc.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
            dialDisc.transform.localRotation = Quaternion.Euler(0, 0, 90);
            dialDisc.GetComponent<Renderer>().sharedMaterial = MakeMat("Dial", new Color(0.5f, 0.5f, 0.55f), 0.7f);
            var dialPointer = MakeBox("Pointer", new Vector3(0.13f, 0, 0), new Vector3(0.04f, 0.30f, 0.06f), matRed, dialBase.transform);
            dialBase.AddComponent<Highlightable>();
            var dial = dialBase.AddComponent<Dial>();
            var dlo = new SerializedObject(dial);
            dlo.FindProperty("pointer").objectReferenceValue = dialPointer.transform;
            dlo.FindProperty("inputActions").objectReferenceValue = inputAsset;
            dlo.ApplyModifiedPropertiesWithoutUndo();

            // Door (south wall, but exit through it = north wall to be consistent narrative; using north)
            var doorFrame = MakeBox("DoorFrame", new Vector3(0, 1.1f, -D/2 + 0.05f), new Vector3(2.2f, 2.2f, 0.05f), MakeMat("DoorFrame", new Color(0.10f, 0.10f, 0.12f), 0.4f), room);
            var doorPanel = MakeBox("DoorPanel", new Vector3(0, 0, 0), new Vector3(2.0f, 2.0f, 0.06f), MakeMat("DoorPanel", new Color(0.13f, 0.18f, 0.22f), 0.5f), doorFrame.transform);
            doorPanel.AddComponent<Highlightable>();
            var doorLock = doorPanel.AddComponent<DoorLock>();
            // Door slides on local X axis (frame is rotated so X is horizontal across door).
            var dla = new SerializedObject(doorLock);
            dla.FindProperty("door").objectReferenceValue = doorPanel.transform;
            dla.ApplyModifiedPropertiesWithoutUndo();

            // PuzzleStateMachine (wires keypad + dial + sockets to door)
            var stateGO = new GameObject("PuzzleState");
            stateGO.transform.SetParent(room, false);
            var state = stateGO.AddComponent<PuzzleStateMachine>();
            var sso = new SerializedObject(state);
            sso.FindProperty("keypad").objectReferenceValue = keypad;
            sso.FindProperty("dial").objectReferenceValue = dial;
            sso.FindProperty("socketA").objectReferenceValue = socketA;
            sso.FindProperty("socketB").objectReferenceValue = socketB;
            sso.ApplyModifiedPropertiesWithoutUndo();
            var doorSO2 = new SerializedObject(doorLock);
            doorSO2.FindProperty("state").objectReferenceValue = state;
            doorSO2.ApplyModifiedPropertiesWithoutUndo();

            // Oxygen timer (world-space canvas on east wall above keypad)
            var timerGO = new GameObject("OxygenTimer", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            timerGO.transform.SetParent(room, false);
            var timerCanvas = timerGO.GetComponent<Canvas>();
            timerCanvas.renderMode = RenderMode.WorldSpace;
            timerGO.transform.position = new Vector3(W/2 - 0.25f, 2.4f, -1.5f);
            timerGO.transform.rotation = Quaternion.Euler(0, -90, 0);
            timerGO.transform.localScale = Vector3.one * 0.005f;
            var timerLabel = new GameObject("Label").AddComponent<Text>();
            timerLabel.transform.SetParent(timerGO.transform, false);
            timerLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timerLabel.fontSize = 90; timerLabel.alignment = TextAnchor.MiddleCenter;
            timerLabel.color = AccentCyan; timerLabel.text = "8:00";
            timerLabel.rectTransform.sizeDelta = new Vector2(260, 110);

            var oxy = timerGO.AddComponent<OxygenTimer>();
            var oso = new SerializedObject(oxy);
            oso.FindProperty("startSeconds").floatValue = 480f;
            oso.FindProperty("label").objectReferenceValue = timerLabel;
            oso.ApplyModifiedPropertiesWithoutUndo();

            // LabSceneController — top-level coordinator
            var labCtrl = new GameObject("LabController").AddComponent<LabSceneController>();
            labCtrl.transform.SetParent(room, false);
            var lso = new SerializedObject(labCtrl);
            lso.FindProperty("timer").objectReferenceValue = oxy;
            lso.FindProperty("door").objectReferenceValue = doorLock;
            lso.FindProperty("state").objectReferenceValue = state;
            lso.ApplyModifiedPropertiesWithoutUndo();

            // small ceiling lamp
            var lamp = new GameObject("Lamp").AddComponent<Light>();
            lamp.transform.SetParent(room, false);
            lamp.transform.position = new Vector3(0, H - 0.2f, 0);
            lamp.type = LightType.Point; lamp.range = 9f; lamp.intensity = 1.4f;
            lamp.color = new Color(1f, 0.96f, 0.84f);

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, ScenesFolder + "/Lab.unity");
        }

        private static CableSocket MakeSocket(string name, Vector3 lpos, Color expectedColor, Material mat, Transform parent)
        {
            var housing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            housing.name = name;
            housing.transform.SetParent(parent, false);
            housing.transform.localPosition = lpos;
            housing.transform.localScale = new Vector3(0.18f, 0.18f, 0.10f);
            housing.GetComponent<Renderer>().sharedMaterial = MakeMat($"SocketBase_{name}", new Color(0.10f, 0.12f, 0.15f), 0.3f);
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(housing.transform, false);
            ring.transform.localPosition = new Vector3(0, 0, -0.6f);
            ring.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ring.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f);
            ring.GetComponent<Renderer>().sharedMaterial = mat;
            var snap = new GameObject("SnapPoint").transform;
            snap.SetParent(housing.transform, false);
            snap.localPosition = new Vector3(0, 0, -0.06f);
            var indicator = new GameObject("Indicator").AddComponent<Light>();
            indicator.transform.SetParent(housing.transform, false);
            indicator.transform.localPosition = new Vector3(0, 0.28f, 0);
            indicator.type = LightType.Point; indicator.range = 0.4f; indicator.intensity = 1.0f;
            indicator.color = AccentRed;
            housing.AddComponent<Highlightable>();
            var s = housing.AddComponent<CableSocket>();
            var sso = new SerializedObject(s);
            sso.FindProperty("expectedColor").colorValue = expectedColor;
            sso.FindProperty("snapPoint").objectReferenceValue = snap;
            sso.FindProperty("indicator").objectReferenceValue = indicator;
            sso.ApplyModifiedPropertiesWithoutUndo();
            return s;
        }

        private static Cable MakeCable(string name, Vector3 lpos, Color plugColor, Material mat, Transform parent)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = name;
            c.transform.SetParent(parent, false);
            c.transform.localPosition = lpos;
            c.transform.localScale = new Vector3(0.18f, 0.10f, 0.30f);
            c.GetComponent<Renderer>().sharedMaterial = mat;
            var rb = c.AddComponent<Rigidbody>();
            rb.useGravity = true; rb.mass = 0.6f;
            c.AddComponent<Highlightable>();
            var cable = c.AddComponent<Cable>();
            var co = new SerializedObject(cable);
            co.FindProperty("plugColor").colorValue = plugColor;
            co.FindProperty("displayName").stringValue = "cable";
            co.ApplyModifiedPropertiesWithoutUndo();
            return cable;
        }

        // --------- Results ---------

        private static void CreateResultsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var canvasGO = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var bg = new GameObject("BG").AddComponent<Image>();
            bg.transform.SetParent(canvasGO.transform, false);
            bg.color = new Color(0.054f, 0.066f, 0.086f);
            var rt = bg.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var resText = new GameObject("Result").AddComponent<Text>();
            resText.transform.SetParent(canvasGO.transform, false);
            resText.font = font; resText.fontSize = 96; resText.alignment = TextAnchor.MiddleCenter;
            resText.color = AccentCyan; resText.text = "ESCAPED";
            var rtt = resText.rectTransform;
            rtt.anchorMin = new Vector2(0.5f, 0.6f); rtt.anchorMax = new Vector2(0.5f, 0.6f);
            rtt.sizeDelta = new Vector2(900, 140); rtt.anchoredPosition = Vector2.zero;

            var timeText = new GameObject("Time").AddComponent<Text>();
            timeText.transform.SetParent(canvasGO.transform, false);
            timeText.font = font; timeText.fontSize = 64; timeText.alignment = TextAnchor.MiddleCenter;
            timeText.color = Color.white; timeText.text = "00:00";
            var ttrt = timeText.rectTransform;
            ttrt.anchorMin = new Vector2(0.5f, 0.45f); ttrt.anchorMax = new Vector2(0.5f, 0.45f);
            ttrt.sizeDelta = new Vector2(900, 90); ttrt.anchoredPosition = Vector2.zero;

            Button MakeBtn(string label, float y)
            {
                var bGO = new GameObject("Btn_" + label, typeof(Image), typeof(Button));
                bGO.transform.SetParent(canvasGO.transform, false);
                bGO.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.15f);
                var brt = (RectTransform)bGO.transform;
                brt.anchorMin = new Vector2(0.5f, y); brt.anchorMax = new Vector2(0.5f, y);
                brt.sizeDelta = new Vector2(280, 56); brt.anchoredPosition = Vector2.zero;
                var t = new GameObject("Text").AddComponent<Text>();
                t.transform.SetParent(bGO.transform, false);
                t.text = label; t.font = font; t.fontSize = 24;
                t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
                var trt = t.rectTransform; trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                trt.offsetMin = trt.offsetMax = Vector2.zero;
                return bGO.GetComponent<Button>();
            }

            var retry = MakeBtn(LocalizationStrings.BtnRetry, 0.30f);
            var menu  = MakeBtn(LocalizationStrings.BtnMenu,  0.22f);

            var rs = canvasGO.AddComponent<ResultsScreen>();
            var rso = new SerializedObject(rs);
            rso.FindProperty("timeText").objectReferenceValue = timeText;
            rso.FindProperty("resultText").objectReferenceValue = resText;
            rso.FindProperty("retryButton").objectReferenceValue = retry;
            rso.FindProperty("menuButton").objectReferenceValue = menu;
            rso.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, ScenesFolder + "/Results.unity");
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
                es.GetType();
            }
        }

        private static void AddScenesToBuildSettings()
        {
            string[] scenes = { "Boot", "MainMenu", "Lab", "Results" };
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var s in scenes)
            {
                var path = ScenesFolder + "/" + s + ".unity";
                if (File.Exists(path)) list.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
#endif
