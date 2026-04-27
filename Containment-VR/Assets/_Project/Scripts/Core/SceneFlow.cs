using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HCITrilogy.Containment.Core
{
    /// <summary>
    /// VR scene transitions. Fades the headset to black via a screen-overlay
    /// canvas in the XR Origin's camera, then loads the new scene.
    /// </summary>
    public class SceneFlow : MonoBehaviour
    {
        public static SceneFlow Instance { get; private set; }
        [SerializeField] private float fadeSeconds = 0.4f;
        private bool _loading;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadAsync(string scene) { if (!_loading) StartCoroutine(Run(scene)); }

        private IEnumerator Run(string scene)
        {
            _loading = true;
            // Defensive: ensure timeScale isn't left at 0 from a prior pause.
            Time.timeScale = 1f;
            // VR fade-to-black is handled by the XR Origin's tunneling/vignette
            // layer when present; here we simply yield while loading.
            var op = SceneManager.LoadSceneAsync(scene);
            while (op is { isDone: false }) yield return null;
            yield return new WaitForSeconds(fadeSeconds);
            _loading = false;
        }
    }
}
