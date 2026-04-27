using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HCITrilogy.Signal.Core
{
    /// <summary>
    /// Scene loader with a fade-to-black transition. Place one on a persistent
    /// GameObject in the Boot scene. Call LoadAsync("SceneName").
    /// </summary>
    public class SceneFlow : MonoBehaviour
    {
        public static SceneFlow Instance { get; private set; }

        [SerializeField] private float fadeSeconds = 0.35f;
        [SerializeField] private Color fadeColor = Color.black;

        private Canvas _canvas;
        private Image _fader;
        private bool _loading;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildFader();
        }

        private void BuildFader()
        {
            var canvasGO = new GameObject("SceneFlow Canvas");
            canvasGO.transform.SetParent(transform, false);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var imgGO = new GameObject("Fader");
            imgGO.transform.SetParent(canvasGO.transform, false);
            _fader = imgGO.AddComponent<Image>();
            _fader.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            _fader.raycastTarget = false;
            var rt = _fader.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        public void LoadAsync(string sceneName)
        {
            if (_loading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _loading = true;
            yield return Fade(0f, 1f);
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (op is { isDone: false }) yield return null;
            yield return Fade(1f, 0f);
            _loading = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float t = 0f;
            var c = _fader.color;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(from, to, t / fadeSeconds);
                _fader.color = c;
                yield return null;
            }
            c.a = to;
            _fader.color = c;
        }
    }
}
