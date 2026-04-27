using UnityEngine;
using HCITrilogy.Containment.Core;
using HCITrilogy.Containment.Puzzles;

namespace HCITrilogy.Containment.UI
{
    public class LabSceneController : MonoBehaviour
    {
        [SerializeField] private VRDoor door;
        [SerializeField] private VRPuzzleStateMachine state;
        [SerializeField] private OxygenTimer timer;

        private float _runStarted;
        private bool _ended;

        private void Start()
        {
            _runStarted = Time.unscaledTime;
            if (door != null)  door.OnOpened += OnEscape;
            if (timer != null) timer.OnExpired += OnFail;
        }

        private void OnDestroy()
        {
            if (door != null)  door.OnOpened -= OnEscape;
            if (timer != null) timer.OnExpired -= OnFail;
        }

        private void OnEscape()
        {
            if (_ended) return;
            _ended = true;
            ResultsScreen.LastEscapeSeconds = Time.unscaledTime - _runStarted;
            ResultsScreen.LastSuccess = true;
            Invoke(nameof(GoResults), 2.0f);
        }

        private void OnFail()
        {
            if (_ended) return;
            _ended = true;
            ResultsScreen.LastEscapeSeconds = Time.unscaledTime - _runStarted;
            ResultsScreen.LastSuccess = false;
            Invoke(nameof(GoResults), 1.0f);
        }

        private void GoResults() => SceneFlow.Instance?.LoadAsync("Results");
    }
}
