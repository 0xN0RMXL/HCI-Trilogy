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
        private bool   _usingSilentMode;
        private double _playStartTime;
        private double _pauseAccumTime;
        private double _pauseStartedAtTime;

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
            if (Song == null) return;
            _usingSilentMode = audioSource.clip == null;
            _dspStartTime = AudioSettings.dspTime + leadInSeconds;
            _playStartTime = Time.unscaledTime + leadInSeconds;
            _pauseAccum = 0;
            _pauseAccumTime = 0;
            if (!_usingSilentMode)
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
                if (!_usingSilentMode) audioSource.Pause();
                _pauseStartedAtDsp = AudioSettings.dspTime;
                _pauseStartedAtTime = Time.unscaledTime;
                IsPaused = true;
            }
            else if (!paused && IsPaused)
            {
                if (_usingSilentMode)
                    _pauseAccumTime += Time.unscaledTime - _pauseStartedAtTime;
                else
                {
                    _pauseAccum += AudioSettings.dspTime - _pauseStartedAtDsp;
                    audioSource.UnPause();
                }
                IsPaused = false;
            }
        }

        private void Update()
        {
            if (!IsPlaying || IsPaused) return;

            if (_usingSilentMode)
            {
                double now = Time.unscaledTime - _pauseAccumTime;
                SongPositionSeconds = now - _playStartTime - _userOffsetSeconds - (Song != null ? Song.offsetSeconds : 0);
                if (SongPositionSeconds > EstimateSongLength())
                    Stop();
            }
            else
            {
                double now = AudioSettings.dspTime - _pauseAccum;
                SongPositionSeconds = now - _dspStartTime - _userOffsetSeconds - (Song != null ? Song.offsetSeconds : 0);
                if (!audioSource.isPlaying
                    && audioSource.clip != null
                    && SongPositionSeconds > audioSource.clip.length)
                {
                    Stop();
                }
            }
        }

        private double EstimateSongLength()
        {
            if (Song == null || Song.notes == null || Song.notes.Length == 0) return 60.0;
            double lastBeat = 0;
            foreach (var n in Song.notes)
                if (n.beat + n.holdBeats > lastBeat) lastBeat = n.beat + n.holdBeats;
            return lastBeat * SecondsPerBeat + 2.0;
        }
    }
}
