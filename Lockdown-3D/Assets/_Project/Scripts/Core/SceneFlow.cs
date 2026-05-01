using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HCITrilogy.Lockdown.Core
{
    /// <summary>
    /// Lockdown-3D scene transitions. Inherits from the shared core SceneFlow.
    /// Uses a screen-overlay UI Image for fade-to-black.
    /// </summary>
    public class SceneFlow : HCITrilogy.Core.SceneFlow
    {
        private Image _fader;

        protected override void Awake()
        {
            base.Awake();
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

        protected override IEnumerator FadeOut() => Fade(0, 1);
        protected override IEnumerator FadeIn()  => Fade(1, 0);

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
