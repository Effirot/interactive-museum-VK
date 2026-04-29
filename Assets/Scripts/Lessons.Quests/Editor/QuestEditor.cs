using UnityEditor;
using UnityEngine;

namespace Lessons.Quests.Editor
{
    [CustomEditor(typeof(Quest), true, isFallback = false)]
    public class QuestEditor : UnityEditor.Editor
    {
        private new Quest target => base.target as Quest;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                if (target.isActive)
                {
                    GUI.color = Color.green;
                    GUILayout.Label("Quest is active now!");
                    GUI.color = Color.white;


                    GUI.enabled = false;
                    EditorGUILayout.ObjectField("Current checkpoint", target.currentCheckpoint, typeof(QuestCheckpoint), true);
                    GUI.enabled = true;

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Next checkpoint"))
                    {
                        target.SkipCheckPoint();
                    }
                    if (GUILayout.Button("Stop quest"))
                    {
                        target.Abort();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUI.contentColor = Color.white;

                    if (GUILayout.Button("Activate"))
                    {
                        if (Quest.active != null)
                        {
                            Quest.active.Abort();
                        }
                        target.Activate();
                    }
                }
            }

            base.OnInspectorGUI();
        }
    }
}
