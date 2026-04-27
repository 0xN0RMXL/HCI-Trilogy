using System;
using UnityEngine;
using UnityEngine.UI;

namespace HCITrilogy.Containment.Puzzles
{
    /// <summary>
    /// 4-digit keypad. Identical accept/reject logic to Lockdown's keypad —
    /// only the input pathway changes (VRKeypadButton fires Press() on
    /// XRPokeInteractor poke or controller select).
    /// </summary>
    public class VRKeypad : MonoBehaviour
    {
        [SerializeField] private string correctCode = "1485";
        [SerializeField] private Text  display;
        [SerializeField] private AudioSource sfxClick;
        [SerializeField] private AudioSource sfxAccept;
        [SerializeField] private AudioSource sfxReject;
        [SerializeField] private Light statusLight;

        public event Action OnAccepted;
        public bool Solved { get; private set; }
        private string _entry = "";

        public void Press(int digit)
        {
            if (Solved) return;
            sfxClick?.Play();
            _entry += digit.ToString();
            if (_entry.Length > 4) _entry = _entry.Substring(_entry.Length - 4);
            UpdateDisplay();
            if (_entry.Length == 4)
            {
                if (_entry == correctCode)
                {
                    Solved = true;
                    sfxAccept?.Play();
                    if (statusLight != null) statusLight.color = new Color(0.30f, 0.88f, 0.63f);
                    OnAccepted?.Invoke();
                }
                else
                {
                    sfxReject?.Play();
                    if (statusLight != null) StartCoroutine(FlashRed());
                    _entry = ""; UpdateDisplay();
                }
            }
        }
        public void Clear() { if (Solved) return; _entry = ""; UpdateDisplay(); }
        private void UpdateDisplay() { if (display != null) display.text = _entry.PadRight(4, '_'); }
        private System.Collections.IEnumerator FlashRed()
        {
            var prev = statusLight.color;
            statusLight.color = new Color(1f, 0.36f, 0.36f);
            yield return new WaitForSeconds(0.6f);
            statusLight.color = prev;
        }
    }
}
