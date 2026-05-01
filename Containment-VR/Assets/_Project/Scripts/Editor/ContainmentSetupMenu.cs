#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using HCITrilogy.Containment.Core;
using HCITrilogy.Containment.Player;
using HCITrilogy.Containment.Puzzles;
using HCITrilogy.Containment.UI;

namespace HCITrilogy.Containment.EditorTools
{
    /// <summary>
    /// One-click VR project setup. Creates Boot/MainMenu/Lab/Results scenes,
    /// builds the lab room from primitives, places puzzles wired with XR
    /// Interaction Toolkit components, and adds scenes to Build Settings.
    ///
    /// Usage: HCI Containment → Setup → Initialize Project.
    ///
    /// Notes on the XR Origin:
    /// We invoke the built-in `GameObject/XR/XR Origin (VR)` menu item to add
    /// a fully-configured rig. If that menu isn't available (XRIT not yet
    /// imported), we create a minimal placeholder and ask the user to add the
    /// rig manually.
    /// </summary>
    public static class ContainmentSetupMenu
    {
        private const string ScenesFolder    = "Assets/_Project/Scenes";
        private const string SettingsFolder  = "Assets/_Project/Settings";
        private const string MaterialsFolder = "Assets/_Project/Art/Materials";

        private static readonly Color FloorTone   = new(0.18f, 0.21f, 0.25f);
        private static readonly Color WallTone    = new(0.25f, 0.28f, 0.32f);
        private static readonly Color AccentCyan  = new(0.36f, 0.88f, 1.00f);
        private static readonly Color AccentAmber = new(1.00f, 0.71f, 0.33f);
        private static readonly Color AccentRed   = new(1.00f, 0.36f, 0.36f);
        private static readonly Color AccentGreen = new(0.30f, 0.88f, 0.63f);

        [MenuItem("HCI Containment/Setup/Initialize Project")]
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
            EditorUtility.DisplayDialog("HCI Containment",
                "VR project initialized.\n\n" +
                "Scenes created in Assets/_Project/Scenes and added to Build Settings.\n\n" +
                "REQUIRED next steps in Unity:\n" +
                "  1. Project Settings > XR Plug-in Management > install OpenXR (PC + Android tabs).\n" +
                "  2. Enable an OpenXR Interaction Profile (Oculus Touch / Meta Quest Touch Pro).\n" +
                "  3. In each scene, run GameObject > XR > XR Origin (VR) if no rig is present.\n" +
                "  4. Build > Switch Platform to Android for Quest builds.\n\n" +
                "Then open Boot.unity and press Play (with Quest Link or Quest connected).",
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
            PlayerSettings.productName = "Containment";
            PlayerSettings.runInBackground = true;
            // Quest target
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.hcitrilogy.containment");
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

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

        /// <summary>
        /// Returns an empty anchor at `pos` with a unit-scaled child cube of
        /// `scale` as its visual. Children added to the returned object inherit
        /// position/rotation only — no scale-propagation skew. Use this for any
        /// container that will have meaningful child geometry.
        /// </summary>
        private static GameObject MakeAnchoredBox(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
        {
            var anchor = new GameObject(name);
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = pos;
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = name + "_Visual";
            visual.transform.SetParent(anchor.transform, false);
            visual.transform.localScale = scale;
            if (mat != null) visual.GetComponent<Renderer>().sharedMaterial = mat;
            return anchor;
        }

        // ---------- Boot ----------

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var managers = new GameObject("Managers");
            managers.AddComponent<SettingsManager>();
            managers.AddComponent<SceneFlow>();
            var bs = new GameObject("Bootstrapper").AddComponent<Bootstrapper>();
            bs.transform.SetParent(managers.transform, false);
            HCITrilogy.Core.Editor.MixerFactory.CreateOrLoad("Assets/_Project/Settings", managers);
            EditorSceneManager.SaveScene(scene, ScenesFolder + "/Boot.unity");
        }

        // ---------- MainMenu ----------

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // World-space canvas with two big buttons. The XR Ray Interactor
            // (added via the XR Origin) will hit-test these and drive uGUI
            // through the TrackedDeviceGraphicRaycaster added below.
            var canvasGO = new GameObject("VRMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.transform.position = new Vector3(0, 1.5f, 1.5f);
            canvasGO.transform.localScale = Vector3.one * 0.005f;
            canvasGO.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 600);
            AddVRRaycaster(canvasGO);

            var bg = new GameObject("BG").AddComponent<Image>();
            bg.transform.SetParent(canvasGO.transform, false);
            bg.color = new Color(0.054f, 0.066f, 0.086f, 0.95f);
            var bgrt = bg.rectTransform;
            bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = bgrt.offsetMax = Vector2.zero;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var title = new GameObject("Title").AddComponent<Text>();
            title.transform.SetParent(canvasGO.transform, false);
            title.text = LocalizationStrings.Title;
            title.font = font; title.fontSize = 72; title.color = AccentCyan;
            title.alignment = TextAnchor.MiddleCenter;
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0.5f, 0.75f); trt.anchorMax = new Vector2(0.5f, 0.75f);
            trt.sizeDelta = new Vector2(800, 120); trt.anchoredPosition = Vector2.zero;

            Button MakeBtn(string label, Vector2 anchor)
            {
                var bGO = new GameObject("Btn_" + label, typeof(Image), typeof(Button));
                bGO.transform.SetParent(canvasGO.transform, false);
                bGO.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.15f);
                var brt = (RectTransform)bGO.transform;
                brt.anchorMin = anchor; brt.anchorMax = anchor;
                brt.sizeDelta = new Vector2(420, 96);
                brt.anchoredPosition = Vector2.zero;
                var t = new GameObject("Text").AddComponent<Text>();
                t.transform.SetParent(bGO.transform, false);
                t.text = label; t.font = font; t.fontSize = 36;
                t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
                var ttrt = t.rectTransform; ttrt.anchorMin = Vector2.zero; ttrt.anchorMax = Vector2.one;
                ttrt.offsetMin = ttrt.offsetMax = Vector2.zero;
                return bGO.GetComponent<Button>();
            }
            var play     = MakeBtn(LocalizationStrings.BtnPlay,     new Vector2(0.5f, 0.58f));
            var settings = MakeBtn(LocalizationStrings.BtnSettings, new Vector2(0.5f, 0.46f));
            var credits  = MakeBtn(LocalizationStrings.BtnCredits,  new Vector2(0.5f, 0.34f));
            var quit     = MakeBtn(LocalizationStrings.BtnQuit,     new Vector2(0.5f, 0.22f));

            // --- Settings Panel ---
            var settingsPanelGO = new GameObject("SettingsPanel", typeof(Image));
            settingsPanelGO.transform.SetParent(canvasGO.transform, false);
            var spImg = settingsPanelGO.GetComponent<Image>();
            spImg.color = new Color(0.054f, 0.066f, 0.086f, 0.95f);
            var sprt = spImg.rectTransform;
            sprt.anchorMin = Vector2.zero; sprt.anchorMax = Vector2.one;
            sprt.offsetMin = sprt.offsetMax = Vector2.zero;

            var settingsTitle = new GameObject("Title").AddComponent<Text>();
            settingsTitle.transform.SetParent(settingsPanelGO.transform, false);
            settingsTitle.text = "SETTINGS"; settingsTitle.font = font;
            settingsTitle.fontSize = 48; settingsTitle.color = AccentCyan;
            settingsTitle.alignment = TextAnchor.MiddleCenter;
            var strrt = settingsTitle.rectTransform;
            strrt.anchorMin = new Vector2(0.5f, 0.85f); strrt.anchorMax = new Vector2(0.5f, 0.85f);
            strrt.sizeDelta = new Vector2(400, 80); strrt.anchoredPosition = Vector2.zero;

            // Volume sliders
            Slider MakeSlider(string label, float value, Vector2 pos)
            {
                var row = new GameObject("Row_" + label);
                row.transform.SetParent(settingsPanelGO.transform, false);
                var rowRt = row.GetComponent<RectTransform>();
                rowRt.anchorMin = pos; rowRt.anchorMax = pos;
                rowRt.sizeDelta = new Vector2(500, 40); rowRt.anchoredPosition = Vector2.zero;

                var lbl = new GameObject("Label").AddComponent<Text>();
                lbl.transform.SetParent(row.transform, false);
                lbl.text = label; lbl.font = font; lbl.fontSize = 22;
                lbl.color = Color.white; lbl.alignment = TextAnchor.MiddleLeft;
                var lrt = lbl.rectTransform;
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = new Vector2(0.4f, 1f);
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;

                var sliderGO = new GameObject("Slider", typeof(Image), typeof(Slider));
                sliderGO.transform.SetParent(row.transform, false);
                var sl = sliderGO.GetComponent<Slider>();
                sl.minValue = 0f; sl.maxValue = 1f; sl.value = value;
                var slRt = sliderGO.GetComponent<RectTransform>();
                slRt.anchorMin = new Vector2(0.45f, 0f); slRt.anchorMax = new Vector2(1f, 1f);
                slRt.offsetMin = slRt.offsetMax = Vector2.zero;
                return sl;
            }

            MakeSlider("Master Volume", 0.7f, new Vector2(0.5f, 0.72f));
            MakeSlider("SFX Volume",    0.55f, new Vector2(0.5f, 0.66f));
            MakeSlider("Music Volume",  0.4f,  new Vector2(0.5f, 0.60f));

            // VR-specific toggles
            Toggle MakeToggle(string label, bool value, Vector2 pos)
            {
                var row = new GameObject("Row_" + label);
                row.transform.SetParent(settingsPanelGO.transform, false);
                var rowRt = row.GetComponent<RectTransform>();
                rowRt.anchorMin = pos; rowRt.anchorMax = pos;
                rowRt.sizeDelta = new Vector2(500, 36); rowRt.anchoredPosition = Vector2.zero;

                var toggleGO = new GameObject("Toggle", typeof(Image), typeof(Toggle));
                toggleGO.transform.SetParent(row.transform, false);
                var tog = toggleGO.GetComponent<Toggle>();
                tog.isOn = value;
                var tRt = toggleGO.GetComponent<RectTransform>();
                tRt.anchorMin = new Vector2(0f, 0f); tRt.anchorMax = new Vector2(0.15f, 1f);
                tRt.offsetMin = tRt.offsetMax = Vector2.zero;

                var lbl = new GameObject("Label").AddComponent<Text>();
                lbl.transform.SetParent(row.transform, false);
                lbl.text = label; lbl.font = font; lbl.fontSize = 20;
                lbl.color = Color.white; lbl.alignment = TextAnchor.MiddleLeft;
                var lrt = lbl.rectTransform;
                lrt.anchorMin = new Vector2(0.18f, 0f); lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
                return tog;
            }

            MakeToggle("Vignette", true, new Vector2(0.5f, 0.50f));
            MakeToggle("Snap Turn", true, new Vector2(0.5f, 0.44f));
            MakeToggle("Left Hand", false, new Vector2(0.5f, 0.38f));

            var closeSettingsBtn = MakeBtn("CLOSE", new Vector2(0.5f, 0.12f));
            closeSettingsBtn.transform.SetParent(settingsPanelGO.transform, false);
            settingsPanelGO.SetActive(false);

            // --- Credits Panel ---
            var creditsPanelGO = new GameObject("CreditsPanel", typeof(Image));
            creditsPanelGO.transform.SetParent(canvasGO.transform, false);
            var cpImg = creditsPanelGO.GetComponent<Image>();
            cpImg.color = new Color(0.054f, 0.066f, 0.086f, 0.95f);
            var cprt = cpImg.rectTransform;
            cprt.anchorMin = Vector2.zero; cprt.anchorMax = Vector2.one;
            cprt.offsetMin = cprt.offsetMax = Vector2.zero;

            var creditsBody = new GameObject("Body").AddComponent<Text>();
            creditsBody.transform.SetParent(creditsPanelGO.transform, false);
            creditsBody.text = "HCI Trilogy\n\nDesign & Code: Omar Ammar\n\nAssets: CC0 / Public Domain\nSee CREDITS.md for full attribution.";
            creditsBody.font = font; creditsBody.fontSize = 28;
            creditsBody.color = Color.white; creditsBody.alignment = TextAnchor.MiddleCenter;
            var cbrt = creditsBody.rectTransform;
            cbrt.anchorMin = new Vector2(0.1f, 0.2f); cbrt.anchorMax = new Vector2(0.9f, 0.85f);
            cbrt.offsetMin = cbrt.offsetMax = Vector2.zero;

            var closeCreditsBtn = MakeBtn("CLOSE", new Vector2(0.5f, 0.08f));
            closeCreditsBtn.transform.SetParent(creditsPanelGO.transform, false);
            creditsPanelGO.SetActive(false);

            var menu = canvasGO.AddComponent<MainMenuController>();
            var so = new SerializedObject(menu);
            so.FindProperty("playButton").objectReferenceValue = play;
            so.FindProperty("settingsButton").objectReferenceValue = settings;
            so.FindProperty("creditsButton").objectReferenceValue = credits;
            so.FindProperty("quitButton").objectReferenceValue = quit;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanelGO;
            so.FindProperty("creditsPanel").objectReferenceValue = creditsPanelGO;
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            TryAddXROrigin();
            EditorSceneManager.SaveScene(scene, ScenesFolder + "/MainMenu.unity");
        }

        // ---------- Lab ----------

        private static void CreateLabScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Lighting tweaks.
            var sunGO = GameObject.Find("Directional Light");
            if (sunGO != null)
            {
                var sun = sunGO.GetComponent<Light>();
                sun.intensity = 0.6f;
                sun.color = new Color(0.85f, 0.92f, 1f);
                sunGO.transform.rotation = Quaternion.Euler(50f, 30f, 0f);
            }
            RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.16f);

            var matFloor   = MakeMat("Floor",  FloorTone, 0.05f);
            var matWall    = MakeMat("Wall",   WallTone,  0.10f);
            var matAccent  = MakeMat("Accent", AccentCyan,  0.6f);
            var matAmber   = MakeMat("Amber",  AccentAmber, 0.5f);
            var matRed     = MakeMat("Red",    AccentRed,   0.4f);
            var matGreen   = MakeMat("Green",  AccentGreen, 0.4f);

            const float W = 8f, H = 3f, D = 6f;
            var room = new GameObject("Lab").transform;

            MakeBox("Floor",   new Vector3(0, 0, 0),     new Vector3(W, 0.2f, D), matFloor, room);
            MakeBox("Ceiling", new Vector3(0, H, 0),     new Vector3(W, 0.2f, D), matWall,  room);
            MakeBox("WallN",   new Vector3(0, H/2, D/2), new Vector3(W, H,    0.2f), matWall, room);
            MakeBox("WallS",   new Vector3(0, H/2,-D/2), new Vector3(W, H,    0.2f), matWall, room);
            MakeBox("WallE",   new Vector3(W/2, H/2, 0), new Vector3(0.2f, H, D),    matWall, room);
            MakeBox("WallW",   new Vector3(-W/2,H/2, 0), new Vector3(0.2f, H, D),    matWall, room);

            // Desk
            var deskMat = MakeMat("Desk", new Color(0.30f, 0.32f, 0.38f), 0.2f);
            MakeBox("Desk", new Vector3(-W/2 + 1.4f, 0.5f, D/2 - 1.2f),
                new Vector3(1.4f, 1.0f, 0.7f), deskMat, room);

            // Drawer (grabbable). Anchored boxes so child geometry isn't
            // distorted by non-uniform parent scales.
            var drawerHousing = MakeAnchoredBox("Cabinet",
                new Vector3(W/2 - 1.0f, 0.5f, D/2 - 1.0f),
                new Vector3(1.2f, 1.0f, 1.0f), deskMat, room);
            var drawerBody = MakeAnchoredBox("DrawerBody", new Vector3(0, 0.05f, 0.15f),
                new Vector3(0.95f, 0.45f, 0.7f), deskMat, drawerHousing.transform);
            MakeBox("Handle", new Vector3(0, 0.05f, 0.36f),
                new Vector3(0.30f, 0.05f, 0.04f),
                MakeMat("Handle", new Color(0.8f, 0.8f, 0.8f), 0.6f), drawerBody.transform);
            var drawerRb = drawerBody.AddComponent<Rigidbody>();
            drawerRb.isKinematic = true; drawerRb.useGravity = false;
            drawerBody.AddComponent<XRGrabInteractable>();
            var vrDrawer = drawerBody.AddComponent<VRDrawer>();
            var vdSO = new SerializedObject(vrDrawer);
            vdSO.FindProperty("body").objectReferenceValue = drawerBody.transform;
            vdSO.ApplyModifiedPropertiesWithoutUndo();

            // Keypad (wall, east). Anchored housing so the display canvas,
            // status light, and 12 keys don't inherit the (0.1, 0.6, 0.6)
            // non-uniform scale of the housing.
            var keypadBack = MakeAnchoredBox("KeypadHousing",
                new Vector3(W/2 - 0.15f, 1.4f, -1.5f),
                new Vector3(0.1f, 0.6f, 0.6f),
                MakeMat("KeypadBack", new Color(0.10f, 0.12f, 0.15f), 0.3f), room);

            var keypad = keypadBack.AddComponent<VRKeypad>();
            // Housing extents: x in [-0.05, 0.05], y in [-0.30, 0.30], z in [-0.30, 0.30].
            // Front (west) face is at x = -0.05.
            var displayCanvas = new GameObject("DisplayCanvas", typeof(Canvas), typeof(CanvasScaler));
            displayCanvas.transform.SetParent(keypadBack.transform, false);
            var dc = displayCanvas.GetComponent<Canvas>();
            dc.renderMode = RenderMode.WorldSpace;
            displayCanvas.transform.localPosition = new Vector3(-0.06f, 0.20f, 0);
            displayCanvas.transform.localRotation = Quaternion.Euler(0, -90, 0);
            displayCanvas.transform.localScale = Vector3.one * 0.001f;
            var disText = new GameObject("Display").AddComponent<Text>();
            disText.transform.SetParent(displayCanvas.transform, false);
            disText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            disText.fontSize = 80; disText.color = AccentCyan;
            disText.alignment = TextAnchor.MiddleCenter;
            disText.text = "____";
            disText.rectTransform.sizeDelta = new Vector2(220, 80);

            var keypadLight = new GameObject("StatusLight").AddComponent<Light>();
            keypadLight.transform.SetParent(keypadBack.transform, false);
            keypadLight.transform.localPosition = new Vector3(-0.08f, 0.25f, 0);
            keypadLight.type = LightType.Point; keypadLight.range = 0.6f; keypadLight.intensity = 1.5f;
            keypadLight.color = AccentRed;

            var kSO = new SerializedObject(keypad);
            kSO.FindProperty("correctCode").stringValue = "1485";
            kSO.FindProperty("display").objectReferenceValue = disText;
            kSO.FindProperty("statusLight").objectReferenceValue = keypadLight;
            kSO.ApplyModifiedPropertiesWithoutUndo();

            // Buttons (3x4 grid: 1..9, C, 0, blank). 4 cm tile keys laid out
            // on the front face of the housing.
            int[] grid = { 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, 0, -2 };
            for (int i = 0; i < grid.Length; i++)
            {
                int row = i / 3; int col = i % 3;
                int v = grid[i];
                if (v == -2) continue;
                Vector3 lpos = new(-0.06f, 0.04f - row * 0.08f, -0.12f + col * 0.12f);
                var bGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bGO.name = v == -1 ? "Key_C" : $"Key_{v}";
                bGO.transform.SetParent(keypadBack.transform, false);
                bGO.transform.localPosition = lpos;
                bGO.transform.localScale = new Vector3(0.04f, 0.06f, 0.06f);
                bGO.GetComponent<Renderer>().sharedMaterial = MakeMat($"Btn_{bGO.name}",
                    v == -1 ? AccentRed : AccentCyan, 0.4f);
                bGO.AddComponent<XRSimpleInteractable>();
                var btn = bGO.AddComponent<VRKeypadButton>();
                bGO.AddComponent<HapticBus>();
                var bSO = new SerializedObject(btn);
                bSO.FindProperty("keypad").objectReferenceValue = keypad;
                bSO.FindProperty("digit").intValue = v == -1 ? 0 : v;
                bSO.FindProperty("isClear").boolValue = v == -1;
                bSO.FindProperty("visual").objectReferenceValue = bGO.transform;
                bSO.ApplyModifiedPropertiesWithoutUndo();
            }

            // Cable bench. Anchored so socket housings and cable plugs sit at
            // their authored sizes.
            var bench = MakeAnchoredBox("Bench", new Vector3(0, 0.45f, D/2 - 0.4f),
                new Vector3(2.4f, 0.9f, 0.6f), deskMat, room);

            VRCableSocket socketA = MakeSocket("SocketA",
                new Vector3(-0.5f, 0.35f, -0.25f), AccentCyan,  matAccent, bench.transform);
            VRCableSocket socketB = MakeSocket("SocketB",
                new Vector3(0.5f,  0.35f, -0.25f), AccentAmber, matAmber, bench.transform);
            VRCable cableA = MakeCable("CableA",
                new Vector3(-0.7f, 0.5f, 0.10f), AccentCyan,  matAccent, bench.transform);
            VRCable cableB = MakeCable("CableB",
                new Vector3(0.7f,  0.5f, 0.10f), AccentAmber, matAmber, bench.transform);

            // Dial console (west wall). Anchored so the disc and pointer
            // aren't compressed along X by the (0.1, 0.8, 0.8) console scale.
            var dialBase = MakeAnchoredBox("DialBase",
                new Vector3(-W/2 + 0.15f, 1.2f, 0),
                new Vector3(0.1f, 0.8f, 0.8f),
                MakeMat("DialBase", new Color(0.10f, 0.12f, 0.15f), 0.3f), room);
            var dialDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dialDisc.name = "DialDisc";
            dialDisc.transform.SetParent(dialBase.transform, false);
            dialDisc.transform.localPosition = new Vector3(0.06f, 0, 0);
            dialDisc.transform.localScale = new Vector3(0.30f, 0.04f, 0.30f);
            dialDisc.transform.localRotation = Quaternion.Euler(0, 0, 90);
            dialDisc.GetComponent<Renderer>().sharedMaterial = MakeMat("Dial",
                new Color(0.5f, 0.5f, 0.55f), 0.7f);
            // Two-level pivot hierarchy (see Lockdown's matching block for the
            // full rationale): outer PointerPivot bakes the Z=90 disc-axis
            // alignment, inner PointerYaw is the transform VRDial.Update
            // overwrites with `localEulerAngles = (0, angle, 0)` each frame.
            var pointerPivot = new GameObject("PointerPivot");
            pointerPivot.transform.SetParent(dialBase.transform, false);
            pointerPivot.transform.localPosition = new Vector3(0.10f, 0, 0);
            pointerPivot.transform.localRotation = Quaternion.Euler(0, 0, 90);
            var pointerYaw = new GameObject("PointerYaw");
            pointerYaw.transform.SetParent(pointerPivot.transform, false);
            var dialPointer = MakeBox("Pointer", new Vector3(0, 0, 0.06f),
                new Vector3(0.02f, 0.02f, 0.12f), matRed, pointerYaw.transform);
            // Make the disc grabbable.
            var dialRb = dialDisc.AddComponent<Rigidbody>();
            dialRb.isKinematic = true; dialRb.useGravity = false;
            dialDisc.AddComponent<XRGrabInteractable>();
            var dial = dialDisc.AddComponent<VRDial>();
            dialDisc.AddComponent<HapticBus>();
            var dlo = new SerializedObject(dial);
            dlo.FindProperty("pointer").objectReferenceValue = pointerYaw.transform;
            dlo.ApplyModifiedPropertiesWithoutUndo();

            // Door (north wall). Frame + panel both anchored so VRDoor's slide
            // animation translates the panel cleanly along its local +X axis,
            // and the panel doesn't inherit (2.2 * 2.0)m = 4.4 m world width.
            var doorFrame = MakeAnchoredBox("DoorFrame", new Vector3(0, 1.1f, -D/2 + 0.05f),
                new Vector3(2.2f, 2.2f, 0.05f),
                MakeMat("DoorFrame", new Color(0.10f, 0.10f, 0.12f), 0.4f), room);
            var doorPanel = MakeAnchoredBox("DoorPanel", new Vector3(0, 0, 0),
                new Vector3(2.0f, 2.0f, 0.06f),
                MakeMat("DoorPanel", new Color(0.13f, 0.18f, 0.22f), 0.5f), doorFrame.transform);
            var doorScript = doorPanel.AddComponent<VRDoor>();

            // PuzzleStateMachine
            var stateGO = new GameObject("PuzzleState");
            stateGO.transform.SetParent(room, false);
            var state = stateGO.AddComponent<VRPuzzleStateMachine>();
            var sso = new SerializedObject(state);
            sso.FindProperty("keypad").objectReferenceValue = keypad;
            sso.FindProperty("dial").objectReferenceValue = dial;
            sso.FindProperty("socketA").objectReferenceValue = socketA;
            sso.FindProperty("socketB").objectReferenceValue = socketB;
            sso.ApplyModifiedPropertiesWithoutUndo();

            var doorSO = new SerializedObject(doorScript);
            doorSO.FindProperty("door").objectReferenceValue = doorPanel.transform;
            doorSO.FindProperty("state").objectReferenceValue = state;
            doorSO.ApplyModifiedPropertiesWithoutUndo();

            // Oxygen timer (world-space canvas on east wall)
            var timerGO = new GameObject("OxygenTimer", typeof(Canvas), typeof(CanvasScaler));
            timerGO.transform.SetParent(room, false);
            var tCanvas = timerGO.GetComponent<Canvas>();
            tCanvas.renderMode = RenderMode.WorldSpace;
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
            var oxoSO = new SerializedObject(oxy);
            oxoSO.FindProperty("startSeconds").floatValue = 480f;
            oxoSO.FindProperty("label").objectReferenceValue = timerLabel;
            oxoSO.ApplyModifiedPropertiesWithoutUndo();

            // Lab controller
            var labCtrl = new GameObject("LabController").AddComponent<LabSceneController>();
            labCtrl.transform.SetParent(room, false);
            var lso = new SerializedObject(labCtrl);
            lso.FindProperty("door").objectReferenceValue = doorScript;
            lso.FindProperty("state").objectReferenceValue = state;
            lso.FindProperty("timer").objectReferenceValue = oxy;
            lso.ApplyModifiedPropertiesWithoutUndo();

            // Ceiling lamp
            var lamp = new GameObject("Lamp").AddComponent<Light>();
            lamp.transform.SetParent(room, false);
            lamp.transform.position = new Vector3(0, H - 0.2f, 0);
            lamp.type = LightType.Point; lamp.range = 9f; lamp.intensity = 1.4f;
            lamp.color = new Color(1f, 0.96f, 0.84f);

            // Try to add the XR Origin via the built-in menu item.
            TryAddXROrigin();

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, ScenesFolder + "/Lab.unity");
        }

        private static VRCableSocket MakeSocket(string name, Vector3 lpos, Color expectedColor, Material ringMat, Transform parent)
        {
            var housing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            housing.name = name;
            housing.transform.SetParent(parent, false);
            housing.transform.localPosition = lpos;
            housing.transform.localScale = new Vector3(0.20f, 0.20f, 0.10f);
            housing.GetComponent<Renderer>().sharedMaterial = MakeMat($"SocketBase_{name}", new Color(0.10f, 0.12f, 0.15f), 0.3f);
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(housing.transform, false);
            ring.transform.localPosition = new Vector3(0, 0, -0.55f);
            ring.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ring.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f);
            ring.GetComponent<Renderer>().sharedMaterial = ringMat;
            var attachPoint = new GameObject("Attach").transform;
            attachPoint.SetParent(housing.transform, false);
            attachPoint.localPosition = new Vector3(0, 0, -0.06f);
            var indicator = new GameObject("Indicator").AddComponent<Light>();
            indicator.transform.SetParent(housing.transform, false);
            indicator.transform.localPosition = new Vector3(0, 0.28f, 0);
            indicator.type = LightType.Point; indicator.range = 0.4f; indicator.intensity = 1.0f;
            indicator.color = AccentRed;
            // XR socket interactor
            var socket = housing.AddComponent<XRSocketInteractor>();
            socket.attachTransform = attachPoint;
            var s = housing.AddComponent<VRCableSocket>();
            var sso = new SerializedObject(s);
            sso.FindProperty("expectedColor").colorValue = expectedColor;
            sso.FindProperty("indicator").objectReferenceValue = indicator;
            sso.ApplyModifiedPropertiesWithoutUndo();
            return s;
        }

        private static VRCable MakeCable(string name, Vector3 lpos, Color plugColor, Material mat, Transform parent)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = name;
            c.transform.SetParent(parent, false);
            c.transform.localPosition = lpos;
            c.transform.localScale = new Vector3(0.18f, 0.10f, 0.30f);
            c.GetComponent<Renderer>().sharedMaterial = mat;
            var rb = c.AddComponent<Rigidbody>();
            rb.useGravity = true; rb.mass = 0.6f;
            c.AddComponent<XRGrabInteractable>();
            var cable = c.AddComponent<VRCable>();
            c.AddComponent<HapticBus>();
            var co = new SerializedObject(cable);
            co.FindProperty("plugColor").colorValue = plugColor;
            co.ApplyModifiedPropertiesWithoutUndo();
            return cable;
        }

        // ---------- Results ----------

        private static void CreateResultsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var canvasGO = new GameObject("VRResultsCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.transform.position = new Vector3(0, 1.5f, 1.5f);
            canvasGO.transform.localScale = Vector3.one * 0.005f;
            ((RectTransform)canvasGO.transform).sizeDelta = new Vector2(900, 600);
            AddVRRaycaster(canvasGO);

            var bg = new GameObject("BG").AddComponent<Image>();
            bg.transform.SetParent(canvasGO.transform, false);
            bg.color = new Color(0.054f, 0.066f, 0.086f, 0.95f);
            var bgrt = bg.rectTransform;
            bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = bgrt.offsetMax = Vector2.zero;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var resText = new GameObject("Result").AddComponent<Text>();
            resText.transform.SetParent(canvasGO.transform, false);
            resText.font = font; resText.fontSize = 96; resText.alignment = TextAnchor.MiddleCenter;
            resText.color = AccentCyan; resText.text = "ESCAPED";
            var rrt = resText.rectTransform;
            rrt.anchorMin = new Vector2(0.5f, 0.65f); rrt.anchorMax = new Vector2(0.5f, 0.65f);
            rrt.sizeDelta = new Vector2(800, 140); rrt.anchoredPosition = Vector2.zero;

            var timeText = new GameObject("Time").AddComponent<Text>();
            timeText.transform.SetParent(canvasGO.transform, false);
            timeText.font = font; timeText.fontSize = 64; timeText.alignment = TextAnchor.MiddleCenter;
            timeText.color = Color.white; timeText.text = "00:00";
            var trt = timeText.rectTransform;
            trt.anchorMin = new Vector2(0.5f, 0.50f); trt.anchorMax = new Vector2(0.5f, 0.50f);
            trt.sizeDelta = new Vector2(800, 90); trt.anchoredPosition = Vector2.zero;

            // Retry / Menu buttons (driven by VR ray interactor + uGUI Button).
            Button MakeBtn(string label, Vector2 anchor)
            {
                var bGO = new GameObject("Btn_" + label, typeof(Image), typeof(Button));
                bGO.transform.SetParent(canvasGO.transform, false);
                bGO.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.15f);
                var brt = (RectTransform)bGO.transform;
                brt.anchorMin = anchor; brt.anchorMax = anchor;
                brt.sizeDelta = new Vector2(360, 96);
                brt.anchoredPosition = Vector2.zero;
                var t = new GameObject("Text").AddComponent<Text>();
                t.transform.SetParent(bGO.transform, false);
                t.text = label; t.font = font; t.fontSize = 32;
                t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
                var ttrt = t.rectTransform;
                ttrt.anchorMin = Vector2.zero; ttrt.anchorMax = Vector2.one;
                ttrt.offsetMin = ttrt.offsetMax = Vector2.zero;
                return bGO.GetComponent<Button>();
            }
            var retryBtn = MakeBtn(LocalizationStrings.BtnRetry, new Vector2(0.32f, 0.30f));
            var menuBtn  = MakeBtn(LocalizationStrings.BtnMenu,  new Vector2(0.68f, 0.30f));

            var rs = canvasGO.AddComponent<ResultsScreen>();
            var rso = new SerializedObject(rs);
            rso.FindProperty("timeText").objectReferenceValue = timeText;
            rso.FindProperty("resultText").objectReferenceValue = resText;
            rso.FindProperty("retryButton").objectReferenceValue = retryBtn;
            rso.FindProperty("menuButton").objectReferenceValue = menuBtn;
            rso.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            TryAddXROrigin();
            EditorSceneManager.SaveScene(scene, ScenesFolder + "/Results.unity");
        }

        // ---------- Common helpers ----------

        /// <summary>
        /// Reflectively adds XRIT's TrackedDeviceGraphicRaycaster to the canvas
        /// so XR ray interactors can drive uGUI buttons. Uses reflection so
        /// this Editor script still compiles even if XRIT is absent — in that
        /// case the regular GraphicRaycaster suffices for desktop/editor mouse
        /// hit-testing and a warning is printed.
        /// </summary>
        private static void AddVRRaycaster(GameObject canvasGO)
        {
            var t = System.Type.GetType(
                "UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit",
                throwOnError: false);
            if (t != null)
            {
                canvasGO.AddComponent(t);
            }
            else
            {
                Debug.LogWarning("[HCI Containment] TrackedDeviceGraphicRaycaster not found — XR rays won't hit UI. " +
                                 "Install the XR Interaction Toolkit package, then add it to '" + canvasGO.name + "' in the Inspector.");
            }
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

        /// <summary>
        /// Invoke the built-in `GameObject/XR/XR Origin (VR)` menu item to add
        /// a fully-configured rig. If unavailable, fall back to a placeholder
        /// camera so the scene still has a viewpoint in the editor.
        /// </summary>
        private static void TryAddXROrigin()
        {
            // Drop a default camera if present (the XR rig brings its own).
            var existingCam = Object.FindFirstObjectByType<Camera>();
            if (existingCam != null) Object.DestroyImmediate(existingCam.gameObject);

            bool added = EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (VR)");
            if (!added)
            {
                // Placeholder camera so the scene saves with a viewpoint.
                var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camGO.tag = "MainCamera";
                camGO.transform.position = new Vector3(0, 1.6f, 0);
                camGO.GetComponent<Camera>().fieldOfView = 70;
                Debug.LogWarning("[HCI Containment] XR Interaction Toolkit not detected — added a placeholder camera. " +
                                 "Please use GameObject > XR > XR Origin (VR) once XRIT is imported.");
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
