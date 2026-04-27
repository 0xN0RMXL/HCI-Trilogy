using UnityEngine;

namespace HCITrilogy.Signal.Core
{
    /// <summary>
    /// Lives on a GameObject in the Boot scene. Waits one frame so persistent
    /// managers finish their Awake, then loads the MainMenu scene via SceneFlow.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private string nextScene = "MainMenu";

        private void Start()
        {
            if (SceneFlow.Instance != null)
                SceneFlow.Instance.LoadAsync(nextScene);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }
}
