using UnityEngine;
using HCITrilogy.Signal.Conductor;

namespace HCITrilogy.Signal.Gameplay
{
    /// <summary>
    /// Individual falling note. Position is computed from dsp-driven song time,
    /// not Time.time — stays sample-accurate with the music.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Note : MonoBehaviour
    {
        public NoteData Data;
        public bool Judged { get; private set; }

        private Lane _lane;
        private SongData _song;
        private Conductor.Conductor _c;
        private SpriteRenderer _sprite;
        private const float MissCutoffSeconds = 0.18f;

        private void Awake() => _sprite = GetComponent<SpriteRenderer>();

        public void Init(NoteData data, Lane lane, SongData song, Conductor.Conductor c)
        {
            Data = data; _lane = lane; _song = song; _c = c; Judged = false;
            lane.Register(this);
            transform.position = lane.SpawnPos;
        }

        private void Update()
        {
            if (_c == null || _song == null) return;

            double secsUntilHit = (Data.beat - _c.SongPositionBeats) * _c.SecondsPerBeat;
            var pos = _lane.HitPos + Vector3.up * (float)(secsUntilHit * _song.scrollSpeedUnitsPerSec);
            transform.position = pos;

            if (!Judged && secsUntilHit < -MissCutoffSeconds)
            {
                Judge.Instance?.JudgeMiss(this);
            }
        }

        public void MarkJudged()
        {
            if (Judged) return;
            Judged = true;
            _lane.Unregister(this);
            Destroy(gameObject);
        }
    }
}
