using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HCITrilogy.Core
{
    /// <summary>
    /// Base scene loader with fade-to-black transition. Place one on a
    /// persistent GameObject in the Boot scene. Call LoadAsync("SceneName").
    ///
    /// Subclasses override FadeOut/FadeIn for project-specific fade
    /// implementations (UI Image, VR vignette, etc.).
    /// </summary>
    public class SceneFlow : MonoBehaviour
    {
        public static SceneFlow Instance { get; private set; }

        [SerializeField] protected float fadeSeconds = 0.4f;

        private bool _loading;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadAsync(string sceneName)
        {
            if (_loading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _loading = true;
            // Defensive: ensure timeScale isn't left at 0 from a prior pause.
            Time.timeScale = 1f;
            yield return FadeOut();
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (op is { isDone: false }) yield return null;
            yield return FadeIn();
            _loading = false;
        }

        /// <summary>Override for project-specific fade-out (e.g. UI Image, VR vignette).</summary>
        protected virtual IEnumerator FadeOut() => null;

        /// <summary>Override for project-specific fade-in (e.g. UI Image, VR vignette).</summary>
        protected virtual IEnumerator FadeIn() => null;
    }
}
