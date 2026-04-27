using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HCITrilogy.Signal.Core;

namespace HCITrilogy.Signal.Gameplay
{
    /// <summary>
    /// A single vertical lane. Listens for its key; forwards press events to Judge.
    /// Also handles its own visual feedback (flash + particles).
    ///
    /// HCI principles:
    /// - Fitts's Law: wide target strip aligned with the home-row finger.
    /// - Multimodal feedback: flash + particles on every press, independent of
    ///   whether the press was successful (positive affordance for the input).
    /// </summary>
    public class Lane : MonoBehaviour
    {
        [SerializeField] private int laneIndex;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform hitZone;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMap = "Gameplay";
        [SerializeField] private string actionName = "Lane0";
        [SerializeField] private SpriteRenderer hitFlashSprite;
        [SerializeField] private ParticleSystem hitParticles;
        [SerializeField] private Color accentColor = new(0.36f, 0.88f, 1.0f, 1.0f);
        [SerializeField] private float flashSeconds = 0.08f;

        private readonly List<Note> _active = new();
        private float _flashT;
        private InputAction _action;

        public int LaneIndex => laneIndex;
        public Vector3 SpawnPos => spawnPoint.position;
        public Vector3 HitPos   => hitZone.position;
        public IReadOnlyList<Note> ActiveNotes => _active;

        private void OnEnable()
        {
            ResolveAction();
            if (_action != null)
            {
                _action.Enable();
                _action.performed += OnPress;
            }
            FeedbackBus.OnFeedback += OnJudged;
        }

        private void OnDisable()
        {
            if (_action != null) _action.performed -= OnPress;
            FeedbackBus.OnFeedback -= OnJudged;
        }

        private void ResolveAction()
        {
            if (inputActions == null) return;
            var map = inputActions.FindActionMap(actionMap, false);
            _action = map != null ? map.FindAction(actionName, false) : null;
        }

        private void Update()
        {
            // Lane flash fade.
            if (_flashT > 0f)
            {
                _flashT -= Time.deltaTime;
                float a = Mathf.Clamp01(_flashT / flashSeconds);
                if (hitFlashSprite != null)
                {
                    var c = hitFlashSprite.color;
                    c.a = a * 0.8f;
                    hitFlashSprite.color = c;
                }
            }

            // Hold-to-play assist: auto-judge first unjudged note when it reaches
            // the hit zone while player is holding the key.
            if (_action != null && _action.IsPressed())
            {
                var s = ServiceLocator.Get<SettingsManager>();
                if (s != null && s.HoldToPlay)
                {
                    Judge.Instance?.JudgePress(this, silent: true);
                }
            }
        }

        public void Register(Note n) => _active.Add(n);
        public void Unregister(Note n) => _active.Remove(n);

        public Note PeekFirstUnjudged()
        {
            for (int i = 0; i < _active.Count; i++)
                if (!_active[i].Judged) return _active[i];
            return null;
        }

        private void OnPress(InputAction.CallbackContext _)
        {
            Judge.Instance?.JudgePress(this);
            Flash(0.4f);
        }

        private void OnJudged(Judgment j, int lane)
        {
            if (lane != laneIndex) return;
            if (j == Judgment.Miss)
            {
                Flash(0.9f, tint: new Color(1f, 0.36f, 0.36f));
                return;
            }
            Flash(1f);
            if (hitParticles != null) hitParticles.Play();
        }

        private void Flash(float strength, Color? tint = null)
        {
            if (hitFlashSprite == null) return;
            var c = tint ?? accentColor;
            c.a = strength;
            hitFlashSprite.color = c;
            _flashT = flashSeconds;
        }

        // Public setter used by the editor setup menu so it can wire references
        // without forcing the user to drag fields manually.
        public void EditorBindAction(InputActionAsset asset, string map, string action)
        {
            inputActions = asset;
            actionMap = map;
            actionName = action;
        }
    }
}
