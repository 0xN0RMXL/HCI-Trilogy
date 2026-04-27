using UnityEngine;
using UnityEngine.UI;

namespace HCITrilogy.Lockdown.Interaction
{
    public class CrosshairPrompt : MonoBehaviour
    {
        [SerializeField] private Image dot;
        [SerializeField] private Text  promptText;

        public void SetPrompt(string text)
        {
            if (promptText != null)
            {
                promptText.text = text ?? "";
                promptText.enabled = !string.IsNullOrEmpty(text);
            }
            if (dot != null)
            {
                var c = dot.color;
                c.a = string.IsNullOrEmpty(text) ? 0.55f : 1f;
                dot.color = c;
            }
        }
    }
}
