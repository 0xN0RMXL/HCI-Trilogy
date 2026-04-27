using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HCITrilogy.Lockdown.Core
{
    public class SceneFlow : MonoBehaviour
    {
        public static SceneFlow Instance { get; private set; }
        [SerializeField] private float fadeSeconds = 0.4f;

        private Image _fader; private bool _loading;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildFader();
        }

        private void BuildFader()
        {
            var c = new GameObject("SceneFlow Canvas").AddComponent<Canvas>();
            c.transform.SetParent(transform, false);
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 9999;
            c.gameObject.AddComponent<CanvasScaler>();
            c.gameObject.AddComponent<GraphicRaycaster>();
            var img = new GameObject("Fader").AddComponent<Image>();
            img.transform.SetParent(c.transform, false);
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            _fader = img;
        }

        public void LoadAsync(string scene) { if (!_loading) StartCoroutine(Run(scene)); }

        private IEnumerator Run(string scene)
        {
            _loading = true;
            yield return Fade(0, 1);
            var op = SceneManager.LoadSceneAsync(scene);
            while (op is { isDone: false }) yield return null;
            yield return Fade(1, 0);
            _loading = false;
        }

        private IEnumerator Fade(float a, float b)
        {
            float t = 0; var c = _fader.color;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(a, b, t / fadeSeconds);
                _fader.color = c;
                yield return null;
            }
            c.a = b; _fader.color = c;
        }
    }
}
