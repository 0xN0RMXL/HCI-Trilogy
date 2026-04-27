using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HCITrilogy.Containment.Puzzles
{
    /// <summary>
    /// XR Simple Interactable wrapped into a keypad-button. Reports a press
    /// when a poke or controller select first occurs.
    ///
    /// HCI: natural mapping — the player physically pokes the button. No
    /// keymap to memorize.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class VRKeypadButton : MonoBehaviour
    {
        [SerializeField] private VRKeypad keypad;
        [SerializeField] private int digit;
        [SerializeField] private bool isClear;
        [SerializeField] private Transform visual;
        [SerializeField] private float pressDepth = 0.012f;
        [SerializeField] private float pressSeconds = 0.08f;

        private XRSimpleInteractable _i;
        private Vector3 _restPos;
        private UnityEngine.Events.UnityAction<UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs> _onSelect;

        private void Awake()
        {
            _i = GetComponent<XRSimpleInteractable>();
            if (visual == null) visual = transform;
            _restPos = visual.localPosition;
            _onSelect = _ => OnPress();
            _i.selectEntered.AddListener(_onSelect);
        }

        private void OnDestroy()
        {
            if (_i == null || _onSelect == null) return;
            _i.selectEntered.RemoveListener(_onSelect);
        }

        private void OnPress()
        {
            if (keypad == null) return;
            if (isClear) keypad.Clear();
            else keypad.Press(digit);
            StopAllCoroutines();
            StartCoroutine(Bounce());
        }

        private System.Collections.IEnumerator Bounce()
        {
            float t = 0f;
            Vector3 down = _restPos + Vector3.forward * -pressDepth;
            while (t < pressSeconds)
            {
                t += Time.deltaTime;
                visual.localPosition = Vector3.Lerp(_restPos, down, t / pressSeconds);
                yield return null;
            }
            t = 0;
            while (t < pressSeconds * 1.6f)
            {
                t += Time.deltaTime;
                visual.localPosition = Vector3.Lerp(down, _restPos, t / (pressSeconds * 1.6f));
                yield return null;
            }
            visual.localPosition = _restPos;
        }
    }
}
