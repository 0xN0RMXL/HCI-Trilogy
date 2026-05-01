using UnityEngine;
using UnityEngine.Pool;
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

        private ObjectPool<Note> _pool;
        private Chart _chart;
        private int _cursor;
        private Conductor.Conductor _conductor;

        public bool HasFinished => _chart != null && _cursor >= _chart.notes.Length;

        private void Awake()
        {
            _pool = new ObjectPool<Note>(
                createFunc: () => Instantiate(notePrefab, transform),
                actionOnGet: (note) => note.gameObject.SetActive(true),
                actionOnRelease: (note) => note.gameObject.SetActive(false),
                actionOnDestroy: (note) => Destroy(note.gameObject),
                collectionCheck: false,
                defaultCapacity: 50,
                maxSize: 200
            );
        }

        private void Start()
        {
            _conductor = ServiceLocator.Get<Conductor.Conductor>();
            if (song != null && song.chartJson != null)
                _chart = JsonUtility.FromJson<Chart>(song.chartJson.text);
            else
                Debug.LogWarning("NoteSpawner: missing song or chart JSON; no notes will spawn.");
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
            var note = _pool.Get();
            note.transform.position = lane.SpawnPos;
            note.transform.rotation = Quaternion.identity;
            note.Init(n, lane, song, _conductor, _pool);
        }
    }
}
