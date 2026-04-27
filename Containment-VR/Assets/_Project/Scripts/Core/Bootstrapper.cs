using UnityEngine;

namespace HCITrilogy.Containment.Core
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
