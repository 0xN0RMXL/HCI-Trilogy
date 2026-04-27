using System.Collections.Generic;
using UnityEngine;

namespace HCITrilogy.Lockdown.Interaction
{
    /// <summary>
    /// Tint child renderers when hovered.
    /// Uses MaterialPropertyBlock so we don't allocate a per-instance material
    /// copy — this keeps GPU instancing intact and avoids leaking materials on
    /// scene unload.
    /// HCI: visual signifier — the looked-at object brightens to confirm focus.
    /// </summary>
    public class Highlightable : MonoBehaviour
    {
        [SerializeField] private Color hoverColor = new(0.36f, 0.88f, 1.0f, 1f);
        [SerializeField] private float hoverBoost = 0.20f;

        private readonly List<Renderer> _renderers = new();
        private readonly List<Color> _baseColors = new();
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock _mpb;
        private bool _hover;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            GetComponentsInChildren(true, _renderers);
            foreach (var r in _renderers)
            {
                Color baseC = Color.white;
                if (r != null && r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorId))
                    baseC = r.sharedMaterial.GetColor(BaseColorId);
                _baseColors.Add(baseC);
            }
        }

        public void SetHover(bool on)
        {
            if (_hover == on) return;
            _hover = on;
            for (int i = 0; i < _renderers.Count; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;
                Color baseC = _baseColors[i];
                Color c = on ? Color.Lerp(baseC, hoverColor, hoverBoost) : baseC;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(BaseColorId, c);
                r.SetPropertyBlock(_mpb);
            }
        }
    }
}
