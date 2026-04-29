using UnityEditor;
using UnityEngine;

namespace Lessons.Quests.Editor
{
    [CustomEditor(typeof(QuestCondition), true, isFallback = true)]
    public class ConditionEditor : UnityEditor.Editor
    {
        private new QuestCondition target => base.target as QuestCondition;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(target.isCompleted ? "Incomplete" : "Complete"))
            {
                target.isCompleted ^= true;
            }

            base.OnInspectorGUI();
        }
    }
}
