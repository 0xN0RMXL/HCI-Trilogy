using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HCITrilogy.Signal.Core;

namespace HCITrilogy.Signal.UI
{
    /// <summary>
    /// Plays a metronome at fixed BPM; the player presses Space on each click.
    /// Computes mean (player_time - expected_click_time) → user audio offset.
    ///
    /// HCI principle: corrects for individual perceptual variance + audio
    /// chain latency (display, USB audio, Bluetooth, etc.). Without this,
    /// the player's "perfect" feels off-by-50ms on some setups.
    /// </summary>
    public class CalibrationController : MonoBehaviour
    {
        [SerializeField] private AudioSource clickSource;
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private float bpm = 120f;
        [SerializeField] private int totalClicks = 16;
        [SerializeField] private Text statusText;
        [SerializeField] private Text resultText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button retestButton;
        [SerializeField] private Button backButton;

        private double _firstClickDsp;
        private int _clicksScheduled;
        private int _clicksHeard;
        private readonly List<double> _errors = new();
        private bool _running;

        private void Start()
        {
            if (acceptButton != null) acceptButton.onClick.AddListener(OnAccept);
            if (retestButton != null) retestButton.onClick.AddListener(StartTest);
            if (backButton   != null) backButton.onClick.AddListener(OnBack);
            StartTest();
        }

        private void Update()
        {
            if (!_running) return;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                int expectedIdx = _clicksHeard;
                if (expectedIdx >= totalClicks) return;
                double expected = _firstClickDsp + expectedIdx * (60.0 / bpm);
                double err = AudioSettings.dspTime - expected;
                _errors.Add(err);
                _clicksHeard++;
                if (_clicksHeard >= totalClicks) Finish();
            }
        }

        private void StartTest()
        {
            _errors.Clear();
            _clicksScheduled = 0;
            _clicksHeard = 0;
            _running = true;
            _firstClickDsp = AudioSettings.dspTime + 1.0;
            for (int i = 0; i < totalClicks; i++)
            {
                double when = _firstClickDsp + i * (60.0 / bpm);
                clickSource.clip = clickClip;
                // Schedule each click manually (one source can't schedule all 16
                // discrete plays at once, so we use coroutine-y polling).
            }
            statusText.text = "Press SPACE on every click...";
            resultText.text = "";
            acceptButton.interactable = false;
            // Schedule clicks via a dispatcher.
            StopAllCoroutines();
            StartCoroutine(SchedulerRoutine());
        }

        private System.Collections.IEnumerator SchedulerRoutine()
        {
            while (_clicksScheduled < totalClicks)
            {
                double when = _firstClickDsp + _clicksScheduled * (60.0 / bpm);
                if (AudioSettings.dspTime >= when - 0.05)
                {
                    AudioSource.PlayClipAtPoint(clickClip, Vector3.zero);
                    _clicksScheduled++;
                }
                yield return null;
            }
        }

        private void Finish()
        {
            _running = false;
            double sum = 0; foreach (var e in _errors) sum += e;
            double mean = _errors.Count > 0 ? sum / _errors.Count : 0;
            int ms = (int)System.Math.Round(mean * 1000.0);
            resultText.text = $"Detected offset: {ms} ms\n(positive = you press late)";
            statusText.text = "Done!";
            acceptButton.interactable = true;

            var s = ServiceLocator.Get<SettingsManager>();
            if (s != null) s.SetAudioOffsetMs(ms);
        }

        private void OnAccept() => OnBack();
        private void OnBack()
        {
            if (SceneFlow.Instance != null) SceneFlow.Instance.LoadAsync("MainMenu");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
