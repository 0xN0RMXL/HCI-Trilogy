using System.Collections.Generic;
using UnityEngine;

namespace HCITrilogy.Lockdown.Interaction
{
    /// <summary>
    /// Optional helper: tint emission/color of all child renderers when hovered.
    /// HCI: visual signifier — the looked-at object brightens to confirm focus.
    /// </summary>
    public class Highlightable : MonoBehaviour
    {
        [SerializeField] private Color hoverColor = new(0.36f, 0.88f, 1.0f, 1f);
        [SerializeField] private float hoverBoost = 0.20f;

        private readonly List<Renderer> _renderers = new();
        private readonly List<Color> _baseColors = new();
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private bool _hover;

        private void Awake()
        {
            GetComponentsInChildren(true, _renderers);
            foreach (var r in _renderers)
                _baseColors.Add(r.material.HasProperty(BaseColorId) ? r.material.GetColor(BaseColorId) : Color.white);
        }

        public void SetHover(bool on)
        {
            if (_hover == on) return;
            _hover = on;
            for (int i = 0; i < _renderers.Count; i++)
            {
                var r = _renderers[i];
                if (r == null || !r.material.HasProperty(BaseColorId)) continue;
                Color baseC = _baseColors[i];
                Color c = on ? Color.Lerp(baseC, hoverColor, hoverBoost) : baseC;
                r.material.SetColor(BaseColorId, c);
            }
        }
    }
}
