using UnityEngine;
using HCITrilogy.Lockdown.Core;
using HCITrilogy.Lockdown.Puzzles;

namespace HCITrilogy.Lockdown.UI
{
    public class LabSceneController : MonoBehaviour
    {
        [SerializeField] private OxygenTimer timer;
        [SerializeField] private DoorLock door;
        [SerializeField] private PuzzleStateMachine state;

        private float _runStarted;
        private bool _ended;

        private void Start()
        {
            // Time.time pauses with timeScale=0, matching OxygenTimer's
            // Time.deltaTime accounting so escape time doesn't inflate during pause.
            _runStarted = Time.time;
            if (door != null) door.OnUnlocked += OnEscape;
            if (timer != null) timer.OnExpired += OnFail;
        }

        private void OnDestroy()
        {
            if (door != null) door.OnUnlocked -= OnEscape;
            if (timer != null) timer.OnExpired -= OnFail;
        }

        private void OnEscape()
        {
            if (_ended) return;
            _ended = true;
            ResultsScreen.LastEscapeSeconds = Time.time - _runStarted;
            ResultsScreen.LastSuccess = true;
            Invoke(nameof(GoResults), 1.6f);
        }

        private void OnFail()
        {
            if (_ended) return;
            _ended = true;
            ResultsScreen.LastEscapeSeconds = Time.time - _runStarted;
            ResultsScreen.LastSuccess = false;
            Invoke(nameof(GoResults), 1.0f);
        }

        private void GoResults() => SceneFlow.Instance?.LoadAsync("Results");
    }
}
