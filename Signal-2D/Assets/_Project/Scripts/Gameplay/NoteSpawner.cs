using UnityEngine;
using HCITrilogy.Signal.Conductor;
using HCITrilogy.Signal.Core;

namespace HCITrilogy.Signal.Gameplay
{
    /// <summary>
    /// Reads the chart and instantiates notes a couple of seconds before
    /// they reach the hit zone. Lookahead is time-based so it scales with
    /// scroll speed.
    /// </summary>
    public class NoteSpawner : MonoBehaviour
    {
        [SerializeField] private Lane[] lanes = new Lane[4];
        [SerializeField] private Note notePrefab;
        [SerializeField] private SongData song;
        [SerializeField] private float lookAheadSeconds = 2.5f;

        private Chart _chart;
        private int _cursor;
        private Conductor.Conductor _conductor;

        public bool HasFinished => _chart != null && _cursor >= _chart.notes.Length;

        private void Start()
        {
            _conductor = ServiceLocator.Get<Conductor.Conductor>();
            if (song != null && song.chartJson != null)
                _chart = JsonUtility.FromJson<Chart>(song.chartJson.text);
            else
                Debug.LogError("NoteSpawner: missing song or chart JSON.");
        }

        private void Update()
        {
            if (_chart == null || _conductor == null) return;
            while (_cursor < _chart.notes.Length)
            {
                var n = _chart.notes[_cursor];
                double secsUntilHit = (n.beat - _conductor.SongPositionBeats) * _conductor.SecondsPerBeat;
                if (secsUntilHit > lookAheadSeconds) break;
                Spawn(n);
                _cursor++;
            }
        }

        private void Spawn(NoteData n)
        {
            if (n.lane < 0 || n.lane >= lanes.Length) return;
            var lane = lanes[n.lane];
            if (lane == null) return;
            var note = Instantiate(notePrefab, lane.SpawnPos, Quaternion.identity, transform);
            note.Init(n, lane, song, _conductor);
        }
    }
}
