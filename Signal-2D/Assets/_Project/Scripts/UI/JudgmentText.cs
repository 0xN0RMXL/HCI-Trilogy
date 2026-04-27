using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Signal.Core;
using HCITrilogy.Signal.Gameplay;

namespace HCITrilogy.Signal.UI
{
    /// <summary>
    /// "PERFECT/GOOD/MISS" pop text near the hit zone. Spawns from a pool.
    /// </summary>
    public class JudgmentText : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private float lifetime = 0.45f;
        [SerializeField] private float floatDistance = 30f;

        public void Show(Judgment j)
        {
            (string txt, Color col) = j switch
            {
                Judgment.Perfect => (LocalizationStrings.JudgmentPerfect, new Color(0.36f, 0.88f, 1.0f)),
                Judgment.Good    => (LocalizationStrings.JudgmentGood,    new Color(0.30f, 0.88f, 0.63f)),
                _                => (LocalizationStrings.JudgmentMiss,    new Color(1.00f, 0.36f, 0.36f))
            };
            label.text = txt;
            label.color = col;
            StopAllCoroutines();
            StartCoroutine(Anim());
        }

        private IEnumerator Anim()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = start + Vector3.up * floatDistance;
            Color start_c = label.color;
            float t = 0f;
            while (t < lifetime)
            {
                t += Time.deltaTime;
                float a = t / lifetime;
                transform.localPosition = Vector3.Lerp(start, end, a);
                Color c = start_c; c.a = 1f - a;
                label.color = c;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
