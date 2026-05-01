using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HCITrilogy.Signal.Core
{
    /// <summary>
    /// Signal-2D scene transitions. Inherits from the shared core SceneFlow.
    /// Builds a screen-overlay fader Canvas at runtime for fade-to-black.
    /// </summary>
    public class SceneFlow : HCITrilogy.Core.SceneFlow
    {
        [SerializeField] private Color fadeColor = Color.black;

        private Image _fader;

        protected override void Awake()
        {
            base.Awake();
            BuildFader();
        }

        private void BuildFader()
        {
            var canvasGO = new GameObject("SceneFlow Canvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
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

        protected override IEnumerator FadeOut() => Fade(0f, 1f);
        protected override IEnumerator FadeIn()  => Fade(1f, 0f);

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
