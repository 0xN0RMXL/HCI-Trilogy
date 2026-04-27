using UnityEngine;
using HCITrilogy.Signal.Conductor;
using HCITrilogy.Signal.Core;
using HCITrilogy.Signal.Gameplay;

namespace HCITrilogy.Signal.UI
{
    /// <summary>
    /// Top-level coordinator for the Game scene. Loads the song into the
    /// conductor, kicks off Play, and on song end transitions to Results.
    /// </summary>
    public class GameSceneController : MonoBehaviour
    {
        [SerializeField] private SongData song;
        [SerializeField] private Conductor.Conductor conductor;
        [SerializeField] private NoteSpawner spawner;
        [SerializeField] private Judge judge;
        [SerializeField] private ScoreManager score;
        [SerializeField] private ComboMeter combo;
        [SerializeField] private HealthMeter health;

        private bool _ended;

        private void Start()
        {
            var s = ServiceLocator.Get<SettingsManager>();
            int offset = s != null ? s.AudioOffsetMs : 0;
            if (s != null && song != null)
                song.scrollSpeedUnitsPerSec = s.NoteSpeed;
            conductor.LoadSong(song, offset);
            conductor.OnSongEnded += OnSongEnded;
            if (health != null) health.OnFailed += OnFailed;
            conductor.Play();
        }

        private void OnSongEnded() { if (!_ended) ToResults(); }
        private void OnFailed()    { if (!_ended) ToResults(); }

        private void ToResults()
        {
            _ended = true;
            ResultsScreen.LastScore = score != null ? score.Score : 0;
            ResultsScreen.LastPerfect = judge != null ? judge.PerfectCount : 0;
            ResultsScreen.LastGood = judge != null ? judge.GoodCount : 0;
            ResultsScreen.LastMiss = judge != null ? judge.MissCount : 0;
            ResultsScreen.LastMaxCombo = combo != null ? combo.MaxCombo : 0;
            if (SceneFlow.Instance != null) SceneFlow.Instance.LoadAsync("Results");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Results");
        }
    }
}
