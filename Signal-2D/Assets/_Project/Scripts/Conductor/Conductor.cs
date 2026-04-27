using System;
using UnityEngine;
using HCITrilogy.Signal.Core;

namespace HCITrilogy.Signal.Conductor
{
    /// <summary>
    /// The heartbeat of Signal. Drives song-time from AudioSettings.dspTime
    /// (not Time.time — dsp time is audio-hardware-accurate, Time.time drifts
    /// against audio over minutes).
    ///
    /// HCI note: Provides a single source-of-truth clock for every reactive
    /// element (notes, lanes, feedback). Multimodal feedback requires precise
    /// temporal alignment or it feels "off" — dsp time gives that precision.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Conductor : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float leadInSeconds = 1.0f;

        public SongData Song { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused  { get; private set; }

        /// <summary>Seconds since the first beat of the song (negative during lead-in).</summary>
        public double SongPositionSeconds { get; private set; }
        /// <summary>Current position expressed in beats (fractional).</summary>
        public double SongPositionBeats   => SongPositionSeconds / SecondsPerBeat;
        public double SecondsPerBeat      => Song != null && Song.bpm > 0 ? 60.0 / Song.bpm : 0.5;

        public event Action OnSongStarted;
        public event Action OnSongEnded;

        private double _dspStartTime;
        private double _userOffsetSeconds;
        private double _pauseAccum;
        private double _pauseStartedAtDsp;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            ServiceLocator.Register(this);
        }

        private void OnDestroy() => ServiceLocator.Unregister<Conductor>();

        private void OnEnable()
        {
            if (PauseController.Instance != null)
                PauseController.Instance.OnPauseChanged += HandlePause;
        }

        private void OnDisable()
        {
            if (PauseController.Instance != null)
                PauseController.Instance.OnPauseChanged -= HandlePause;
        }

        public void LoadSong(SongData song, int userOffsetMs)
        {
            Song = song;
            _userOffsetSeconds = userOffsetMs / 1000.0;
            audioSource.clip = song != null ? song.track : null;
        }

        public void Play()
        {
            if (Song == null || audioSource.clip == null) return;
            _dspStartTime = AudioSettings.dspTime + leadInSeconds;
            _pauseAccum = 0;
            audioSource.PlayScheduled(_dspStartTime);
            IsPlaying = true;
            IsPaused = false;
            OnSongStarted?.Invoke();
        }

        public void Stop()
        {
            audioSource.Stop();
            IsPlaying = false;
            IsPaused = false;
            OnSongEnded?.Invoke();
        }

        private void HandlePause(bool paused)
        {
            if (!IsPlaying) return;
            if (paused && !IsPaused)
            {
                audioSource.Pause();
                _pauseStartedAtDsp = AudioSettings.dspTime;
                IsPaused = true;
            }
            else if (!paused && IsPaused)
            {
                _pauseAccum += AudioSettings.dspTime - _pauseStartedAtDsp;
                audioSource.UnPause();
                IsPaused = false;
            }
        }

        private void Update()
        {
            if (!IsPlaying || IsPaused) return;

            double now = AudioSettings.dspTime - _pauseAccum;
            SongPositionSeconds = now - _dspStartTime - _userOffsetSeconds - (Song != null ? Song.offsetSeconds : 0);

            // Detect end of song.
            if (!audioSource.isPlaying
                && audioSource.clip != null
                && SongPositionSeconds > audioSource.clip.length)
            {
                Stop();
            }
        }
    }
}
