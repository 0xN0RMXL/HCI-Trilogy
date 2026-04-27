using UnityEngine;

namespace HCITrilogy.Lockdown.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private string nextScene = "MainMenu";
        private void Start()
        {
            if (SceneFlow.Instance != null) SceneFlow.Instance.LoadAsync(nextScene);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }
}
