

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lessons.Quests
{
    [CreateAssetMenu(fileName = "QuestSelectionAsset", menuName = "Digitech/Quests/Quest selection", order = 2)]
    public class QuestSelectionAsset : ScriptableObject
    {
        public static QuestSelectionAsset selected { get; set; }

#if UNITY_EDITOR
        [SerializeField]
        private SceneAsset scene;
#endif
        [SerializeField]
        public Sprite icon;
        
        [Space]
        [SerializeField, TextArea(1, 3)]
        public string questName;
        [SerializeField, TextArea(3, 100)]
        public string questDescription;

        [SerializeField, HideInInspector]
        public string sceneName;

        public void Select()
        {
            selected = this;
        }
        public void LoadScene()
        {
            SceneManager.LoadScene(sceneName);
        }

        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (scene)
            {
                sceneName = scene.name;

                AddScene(AssetDatabase.GetAssetPath(scene));
            }
            else
                sceneName = "";
        }

        public static void AddScene(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();

            if (scenes.Any(s => s.path == scenePath)) 
                return;

            var newScene = new EditorBuildSettingsScene(scenePath, true);
            scenes.Add(newScene);

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"Add scene {scenePath} to build settings");
        }
#endif
    }
}