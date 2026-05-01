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
            // Time.time matches OxygenTimer's Time.deltaTime accounting so escape time
            // and remaining-oxygen stay in sync. Containment-VR has no PauseController
            // (VR convention: take the headset off), so timeScale stays at 1 in normal play.
            _runStarted = Time.time;
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
            ResultsScreen.LastEscapeSeconds = Time.time - _runStarted;
            ResultsScreen.LastSuccess = true;
            Invoke(nameof(GoResults), 2.0f);
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
