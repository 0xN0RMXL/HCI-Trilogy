#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HCITrilogy.Signal.Conductor;
using HCITrilogy.Signal.Core;
using HCITrilogy.Signal.Gameplay;
using HCITrilogy.Signal.UI;

namespace HCITrilogy.Signal.EditorTools
{
    /// <summary>
    /// One-click project setup. Creates scenes (Boot, MainMenu, Calibration,
    /// Game, Results), adds them to Build Settings, sets project options
    /// (Linear color space, etc.), and creates the SongData asset.
    ///
    /// Usage: Top menu → HCI Signal → Setup → Initialize Project.
    /// Run again any time to rebuild scenes idempotently (will overwrite).
    /// </summary>
    public static class SignalSetupMenu
    {
        private const string ScenesFolder    = "Assets/_Project/Scenes";
        private const string PrefabsFolder   = "Assets/_Project/Prefabs";
        private const string SettingsFolder  = "Assets/_Project/Settings";
        private const string ChartsFolder    = "Assets/_Project/Charts";
        private const string AudioFolder     = "Assets/_Project/Audio";
        private const string MusicFolder     = AudioFolder + "/Music";
        private const string SfxFolder       = AudioFolder + "/SFX";
        private const string MixerPath       = AudioFolder + "/Signal.mixer";
        private const string SongAssetPath   = SettingsFolder + "/Song1.asset";
        private const string ChartJsonPath   = ChartsFolder + "/Song1.json";

        [MenuItem("HCI Signal/Setup/Initialize Project")]
        public static void InitializeProject()
        {
            EnsureFolders();
            SetPlayerSettings();
            CreateOrUpdateChartJson();
            CreateSongAssetIfMissing();
            CreateBootScene();
            CreateMainMenuScene();
            CreateCalibrationScene();
            CreateGameScene();
            CreateResultsScene();
            AddScenesToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("HCI Signal",
                "Project initialized.\n\n" +
                "Scenes have been created in Assets/_Project/Scenes and added to Build Settings.\n\n" +
                "Important next step: in Edit > Project Settings > Player > Other Settings:\n" +
                "  • Set Color Space to Linear\n" +
                "  • Set Active Input Handling to 'Input System Package (New)'\n" +
                "Unity may prompt you to restart.\n\n" +
                "Then open Boot.unity and press Play.",
                "OK");
        }

        // --------------- folders & settings ---------------

        private static void EnsureFolders()
        {
            string[] paths = {
                "Assets/_Project",
                ScenesFolder, PrefabsFolder, SettingsFolder, ChartsFolder,
                AudioFolder, MusicFolder, SfxFolder
            };
            foreach (var p in paths)
                if (!AssetDatabase.IsValidFolder(p))
                {
                    var parent = Path.GetDirectoryName(p).Replace('\\', '/');
                    var leaf = Path.GetFileName(p);
                    AssetDatabase.CreateFolder(parent, leaf);
                }
        }

        private static void SetPlayerSettings()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.companyName = "HCI Trilogy";
            PlayerSettings.productName = "Signal";
            PlayerSettings.runInBackground = true;
        }

        // --------------- assets ---------------

        private static void CreateOrUpdateChartJson()
        {
            if (File.Exists(ChartJsonPath)) return; // do not clobber user edits
            var sample = SampleChart();
            File.WriteAllText(ChartJsonPath, JsonUtility.ToJson(sample, true));
            AssetDatabase.ImportAsset(ChartJsonPath);
        }

        private static Chart SampleChart()
        {
            var c = new Chart { title = "Demo Loop", artist = "HCI Trilogy", bpm = 120f, offsetSeconds = 0f };
            var notes = new List<NoteData>();
            // 1-minute pattern at 120 bpm = 120 beats. Build 4-on-the-floor with
            // alternating lane patterns so all four lanes are exercised.
            for (int beat = 4; beat < 120; beat++)
            {
                int lane = (beat - 4) % 4;
                notes.Add(new NoteData { beat = beat, lane = lane, holdBeats = 0 });
                if (beat % 8 == 0)
                    notes.Add(new NoteData { beat = beat + 0.5f, lane = (lane + 2) % 4 });
            }
            c.notes = notes.ToArray();
            return c;
        }

        private static void CreateSongAssetIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SongData>(SongAssetPath) != null) return;
            var so = ScriptableObject.CreateInstance<SongData>();
            so.title = "Demo Loop";
            so.artist = "HCI Trilogy";
            so.bpm = 120f;
            so.offsetSeconds = 0f;
            so.scrollSpeedUnitsPerSec = 6f;
            so.chartJson = AssetDatabase.LoadAssetAtPath<TextAsset>(ChartJsonPath);
            // Try to find a music clip in MusicFolder.
            var clips = AssetDatabase.FindAssets("t:AudioClip", new[] { MusicFolder });
            if (clips.Length > 0)
                so.track = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(clips[0]));
            AssetDatabase.CreateAsset(so, SongAssetPath);
        }

        // --------------- scenes ---------------

        private static void CreateBootScene()
        {
            var path = ScenesFolder + "/Boot.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Persistent managers root.
            var managers = new GameObject("[Managers]");
            managers.AddComponent<SettingsManager>();
            var pauseCtrl = managers.AddComponent<PauseController>();
            // Wire the InputActionAsset so Esc actually triggers pause.
            var actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                SettingsFolder + "/PlayerInput.inputactions");
            if (actions != null)
            {
                var pcSO = new SerializedObject(pauseCtrl);
                pcSO.FindProperty("inputActions").objectReferenceValue = actions;
                pcSO.ApplyModifiedPropertiesWithoutUndo();
            }
            var sceneFlow = new GameObject("[SceneFlow]");
            sceneFlow.AddComponent<SceneFlow>();
            // Bootstrapper transitions to MainMenu.
            var boot = new GameObject("Bootstrapper");
            boot.AddComponent<Bootstrapper>();
            // A camera (so the editor doesn't complain).
            var cam = new GameObject("Main Camera"); cam.AddComponent<Camera>(); cam.tag = "MainCamera";
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateMainMenuScene()
        {
            var path = ScenesFolder + "/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.054f, 0.067f, 0.086f);
            cam.orthographic = true;

            var canvas = MakeCanvas("Menu Canvas");
            var title = MakeText(canvas.transform, "Title", LocalizationStrings.Title, 96, FontStyle.Bold,
                                 new Vector2(0f, 200f), 800, 120, new Color(0.36f, 0.88f, 1.0f));
            var sub = MakeText(canvas.transform, "Subtitle", LocalizationStrings.Subtitle, 22, FontStyle.Normal,
                                 new Vector2(0f, 130f), 600, 40, new Color(0.54f, 0.58f, 0.63f));

            var play = MakeButton(canvas.transform, "Play",      LocalizationStrings.BtnPlay,      new Vector2(0f, 30f));
            var cal  = MakeButton(canvas.transform, "Calibrate", LocalizationStrings.BtnCalibrate, new Vector2(0f, -30f));
            var setn = MakeButton(canvas.transform, "Settings",  LocalizationStrings.BtnSettings,  new Vector2(0f, -90f));
            var cred = MakeButton(canvas.transform, "Credits",   LocalizationStrings.BtnCredits,   new Vector2(0f, -150f));
            var quit = MakeButton(canvas.transform, "Quit",      LocalizationStrings.BtnQuit,      new Vector2(0f, -210f));

            var ctrl = canvas.AddComponent<MainMenuController>();
            SerializedObject so = new SerializedObject(ctrl);
            so.FindProperty("playButton").objectReferenceValue      = play.GetComponent<Button>();
            so.FindProperty("calibrateButton").objectReferenceValue = cal.GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue  = setn.GetComponent<Button>();
            so.FindProperty("creditsButton").objectReferenceValue   = cred.GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue      = quit.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            // EventSystem so buttons work
            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateCalibrationScene()
        {
            var path = ScenesFolder + "/Calibration.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            MakeBgCamera();
            var canvas = MakeCanvas("Cal Canvas");
            MakeText(canvas.transform, "Header", "CALIBRATION", 64, FontStyle.Bold,
                     new Vector2(0, 220), 800, 80, new Color(0.36f, 0.88f, 1f));
            var status = MakeText(canvas.transform, "Status", LocalizationStrings.CalibrationPrompt, 22,
                     FontStyle.Normal, new Vector2(0, 100), 800, 80, Color.white);
            var result = MakeText(canvas.transform, "Result", "", 28, FontStyle.Normal,
                     new Vector2(0, 0), 800, 60, new Color(0.30f, 0.88f, 0.63f));
            var accept = MakeButton(canvas.transform, "Accept", LocalizationStrings.BtnAccept, new Vector2(-110, -120));
            var retest = MakeButton(canvas.transform, "Retest", LocalizationStrings.BtnRecalibrate, new Vector2(110, -120));
            var back   = MakeButton(canvas.transform, "Back", LocalizationStrings.BtnBack, new Vector2(0, -200));

            var calGO = new GameObject("Calibration");
            var src = calGO.AddComponent<AudioSource>();
            var ctrl = calGO.AddComponent<CalibrationController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("clickSource").objectReferenceValue = src;
            so.FindProperty("statusText").objectReferenceValue = status.GetComponent<Text>();
            so.FindProperty("resultText").objectReferenceValue = result.GetComponent<Text>();
            so.FindProperty("acceptButton").objectReferenceValue = accept.GetComponent<Button>();
            so.FindProperty("retestButton").objectReferenceValue = retest.GetComponent<Button>();
            so.FindProperty("backButton").objectReferenceValue = back.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateGameScene()
        {
            var path = ScenesFolder + "/Game.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.054f, 0.067f, 0.086f);
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.transform.position = new Vector3(0, 0, -10);
            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<CameraShaker>();

            // Conductor
            var conductorGO = new GameObject("Conductor");
            var aSrc = conductorGO.AddComponent<AudioSource>();
            aSrc.playOnAwake = false;
            var conductor = conductorGO.AddComponent<HCITrilogy.Signal.Conductor.Conductor>();

            // Note prefab (created as runtime prefab if missing)
            var notePrefab = EnsureNotePrefab();

            // Lanes parent
            var lanesParent = new GameObject("Lanes");
            var lanes = new Lane[4];
            float[] xs = { -3f, -1f, 1f, 3f };
            for (int i = 0; i < 4; i++)
            {
                var laneGO = new GameObject($"Lane{i}");
                laneGO.transform.SetParent(lanesParent.transform);
                laneGO.transform.position = new Vector3(xs[i], 0, 0);

                // hit zone visual
                var hitZone = new GameObject("HitZone");
                hitZone.transform.SetParent(laneGO.transform, false);
                hitZone.transform.localPosition = new Vector3(0, -3.5f, 0);
                var hitSprite = hitZone.AddComponent<SpriteRenderer>();
                hitSprite.sprite = MakeSquareSprite();
                hitSprite.color = new Color(1, 1, 1, 0.15f);
                hitZone.transform.localScale = new Vector3(1.6f, 0.4f, 1f);

                // flash sprite
                var flashGO = new GameObject("Flash");
                flashGO.transform.SetParent(laneGO.transform, false);
                flashGO.transform.localPosition = new Vector3(0, -3.5f, 0);
                var flash = flashGO.AddComponent<SpriteRenderer>();
                flash.sprite = MakeSquareSprite();
                flash.color = new Color(0.36f, 0.88f, 1.0f, 0f);
                flashGO.transform.localScale = new Vector3(1.6f, 0.4f, 1f);

                // spawn point above camera
                var spawnGO = new GameObject("Spawn");
                spawnGO.transform.SetParent(laneGO.transform, false);
                spawnGO.transform.localPosition = new Vector3(0, 6f, 0);

                var lane = laneGO.AddComponent<Lane>();
                var laneSO = new SerializedObject(lane);
                laneSO.FindProperty("laneIndex").intValue = i;
                laneSO.FindProperty("spawnPoint").objectReferenceValue = spawnGO.transform;
                laneSO.FindProperty("hitZone").objectReferenceValue = hitZone.transform;
                laneSO.FindProperty("hitFlashSprite").objectReferenceValue = flash;
                var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                    SettingsFolder + "/PlayerInput.inputactions");
                if (inputAsset != null)
                {
                    laneSO.FindProperty("inputActions").objectReferenceValue = inputAsset;
                    laneSO.FindProperty("actionMap").stringValue = "Gameplay";
                    laneSO.FindProperty("actionName").stringValue = $"Lane{i}";
                }
                laneSO.ApplyModifiedPropertiesWithoutUndo();

                lanes[i] = lane;
            }

            // Spawner
            var spawnerGO = new GameObject("NoteSpawner");
            var spawner = spawnerGO.AddComponent<NoteSpawner>();
            var song = AssetDatabase.LoadAssetAtPath<SongData>(SongAssetPath);
            var spawnerSO = new SerializedObject(spawner);
            var lanesProp = spawnerSO.FindProperty("lanes");
            lanesProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                lanesProp.GetArrayElementAtIndex(i).objectReferenceValue = lanes[i];
            spawnerSO.FindProperty("notePrefab").objectReferenceValue = notePrefab;
            spawnerSO.FindProperty("song").objectReferenceValue = song;
            spawnerSO.ApplyModifiedPropertiesWithoutUndo();

            // Judge / Score / Combo / Health
            var sysGO = new GameObject("Systems");
            var judge = sysGO.AddComponent<Judge>();
            var combo = sysGO.AddComponent<ComboMeter>();
            var score = sysGO.AddComponent<ScoreManager>();
            var health = sysGO.AddComponent<HealthMeter>();
            var scoreSO = new SerializedObject(score);
            scoreSO.FindProperty("combo").objectReferenceValue = combo;
            scoreSO.ApplyModifiedPropertiesWithoutUndo();

            // HUD
            var canvas = MakeCanvas("HUD Canvas");
            var scoreTxt = MakeText(canvas.transform, "Score", "000000", 36, FontStyle.Bold,
                                    new Vector2(-540, 320), 300, 60, Color.white,
                                    TextAnchor.MiddleLeft);
            var comboTxt = MakeText(canvas.transform, "Combo", "", 30, FontStyle.Bold,
                                    new Vector2(0, 280), 400, 50, new Color(0.36f, 0.88f, 1.0f));
            var healthBg = MakeRect(canvas.transform, "HealthBg", new Vector2(540, 320), 280, 24,
                                    new Color(0.1f, 0.12f, 0.15f, 0.8f));
            var healthFillGO = new GameObject("HealthFill");
            healthFillGO.transform.SetParent(healthBg.transform, false);
            var fill = healthFillGO.AddComponent<Image>();
            fill.color = new Color(0.30f, 0.88f, 0.63f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;
            var fillRT = fill.rectTransform;
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(2, 2); fillRT.offsetMax = new Vector2(-2, -2);

            var hud = canvas.AddComponent<HUD>();
            var hudSO = new SerializedObject(hud);
            hudSO.FindProperty("scoreText").objectReferenceValue = scoreTxt.GetComponent<Text>();
            hudSO.FindProperty("comboText").objectReferenceValue = comboTxt.GetComponent<Text>();
            hudSO.FindProperty("healthFill").objectReferenceValue = fill;
            hudSO.FindProperty("score").objectReferenceValue = score;
            hudSO.FindProperty("combo").objectReferenceValue = combo;
            hudSO.FindProperty("health").objectReferenceValue = health;
            hudSO.ApplyModifiedPropertiesWithoutUndo();

            // Game scene controller
            var ctrl = sysGO.AddComponent<GameSceneController>();
            var ctrlSO = new SerializedObject(ctrl);
            ctrlSO.FindProperty("song").objectReferenceValue = song;
            ctrlSO.FindProperty("conductor").objectReferenceValue = conductor;
            ctrlSO.FindProperty("spawner").objectReferenceValue = spawner;
            ctrlSO.FindProperty("judge").objectReferenceValue = judge;
            ctrlSO.FindProperty("score").objectReferenceValue = score;
            ctrlSO.FindProperty("combo").objectReferenceValue = combo;
            ctrlSO.FindProperty("health").objectReferenceValue = health;
            ctrlSO.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateResultsScene()
        {
            var path = ScenesFolder + "/Results.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            MakeBgCamera();
            var canvas = MakeCanvas("Results Canvas");
            MakeText(canvas.transform, "Header", "RESULTS", 64, FontStyle.Bold,
                     new Vector2(0, 280), 800, 80, new Color(0.36f, 0.88f, 1f));

            var grade = MakeText(canvas.transform, "Grade", "—", 120, FontStyle.Bold,
                     new Vector2(0, 140), 400, 200, new Color(0.30f, 0.88f, 0.63f));
            var acc = MakeText(canvas.transform, "Accuracy", "0%", 30, FontStyle.Normal,
                     new Vector2(0, 50), 400, 60, Color.white);
            var sc = MakeText(canvas.transform, "Score", "000000", 28, FontStyle.Normal,
                     new Vector2(0, 0), 400, 60, Color.white);
            var perf = MakeText(canvas.transform, "Perf", "0", 22, FontStyle.Normal,
                     new Vector2(-300, -60), 200, 40, new Color(0.36f, 0.88f, 1f));
            var good = MakeText(canvas.transform, "Good", "0", 22, FontStyle.Normal,
                     new Vector2(-100, -60), 200, 40, new Color(0.30f, 0.88f, 0.63f));
            var miss = MakeText(canvas.transform, "Miss", "0", 22, FontStyle.Normal,
                     new Vector2(100, -60), 200, 40, new Color(1f, 0.36f, 0.36f));
            var maxc = MakeText(canvas.transform, "MaxCombo", "0", 22, FontStyle.Normal,
                     new Vector2(300, -60), 200, 40, Color.white);

            var retry = MakeButton(canvas.transform, "Retry", LocalizationStrings.BtnRetry, new Vector2(-110, -180));
            var menu  = MakeButton(canvas.transform, "Menu", LocalizationStrings.BtnMenu, new Vector2(110, -180));

            var ctrl = canvas.AddComponent<ResultsScreen>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("perfectText").objectReferenceValue = perf.GetComponent<Text>();
            so.FindProperty("goodText").objectReferenceValue = good.GetComponent<Text>();
            so.FindProperty("missText").objectReferenceValue = miss.GetComponent<Text>();
            so.FindProperty("maxComboText").objectReferenceValue = maxc.GetComponent<Text>();
            so.FindProperty("scoreText").objectReferenceValue = sc.GetComponent<Text>();
            so.FindProperty("accuracyText").objectReferenceValue = acc.GetComponent<Text>();
            so.FindProperty("gradeText").objectReferenceValue = grade.GetComponent<Text>();
            so.FindProperty("retryButton").objectReferenceValue = retry.GetComponent<Button>();
            so.FindProperty("menuButton").objectReferenceValue = menu.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, path);
        }

        // --------------- helpers ---------------

        private static Note EnsureNotePrefab()
        {
            const string path = PrefabsFolder + "/Note.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<Note>(path);
            if (existing != null) return existing;
            var go = new GameObject("Note");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSquareSprite();
            sr.color = new Color(0.9f, 0.9f, 0.9f);
            go.transform.localScale = new Vector3(1.4f, 0.35f, 1f);
            go.AddComponent<Note>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Note>();
        }

        private static Sprite _cachedSquare;
        private static Sprite MakeSquareSprite()
        {
            if (_cachedSquare != null) return _cachedSquare;
            var tex = new Texture2D(1, 1) { filterMode = FilterMode.Bilinear };
            tex.SetPixel(0, 0, Color.white); tex.Apply();
            _cachedSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _cachedSquare;
        }

        private static GameObject MakeBgCamera()
        {
            var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.054f, 0.067f, 0.086f);
            cam.orthographic = true;
            return camGO;
        }

        private static Canvas MakeCanvas(string name)
        {
            var go = new GameObject(name);
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        private static GameObject MakeRect(Transform parent, string name, Vector2 pos, float w, float h, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = img.rectTransform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(w, h);
            return go;
        }

        private static GameObject MakeText(Transform parent, string name, string text, int size,
            FontStyle style, Vector2 pos, float w, float h, Color color,
            TextAnchor align = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = t.rectTransform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(w, h);
            return go;
        }

        private static GameObject MakeButton(Transform parent, string name, string label, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.13f, 0.16f, 0.20f, 0.9f);
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.13f, 0.16f, 0.20f);
            colors.highlightedColor = new Color(0.36f, 0.88f, 1.0f, 0.5f);
            colors.pressedColor = new Color(0.36f, 0.88f, 1.0f);
            btn.colors = colors;
            var rt = img.rectTransform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280, 50);

            var labelGO = MakeText(go.transform, "Label", label, 22, FontStyle.Bold,
                                   Vector2.zero, 280, 50, Color.white);
            var lt = labelGO.GetComponent<RectTransform>();
            lt.anchorMin = Vector2.zero; lt.anchorMax = Vector2.one;
            lt.offsetMin = lt.offsetMax = Vector2.zero;
            return go;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Use the new Input System UI module so menus respond to mouse/keyboard
            // when Active Input Handling = "Input System Package (New)".
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static void AddScenesToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new(ScenesFolder + "/Boot.unity", true),
                new(ScenesFolder + "/MainMenu.unity", true),
                new(ScenesFolder + "/Calibration.unity", true),
                new(ScenesFolder + "/Game.unity", true),
                new(ScenesFolder + "/Results.unity", true),
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
