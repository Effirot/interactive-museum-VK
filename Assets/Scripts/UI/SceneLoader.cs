using UnityEngine;
using UnityEngine.SceneManagement;

namespace InteractiveMuseum.UI
{
    /// <summary>
    /// Handles scene loading functionality.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// Loads a scene by name.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        public void OpenScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("Scene name is null or empty. Cannot load scene.");
                return;
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError($"Scene '{sceneName}' not found in build settings!");
            }
        }
    }
}
