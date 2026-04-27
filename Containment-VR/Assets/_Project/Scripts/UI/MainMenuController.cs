using UnityEngine;
using UnityEngine.UI;
using HCITrilogy.Containment.Core;

namespace HCITrilogy.Containment.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            if (playButton) playButton.onClick.AddListener(() => SceneFlow.Instance?.LoadAsync("Lab"));
            if (quitButton) quitButton.onClick.AddListener(OnQuit);
        }

        private static void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
