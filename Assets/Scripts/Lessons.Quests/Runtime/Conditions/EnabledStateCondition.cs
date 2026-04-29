using UnityEngine;
using UnityEngine.Search;
using UnityEngine.SearchService;

namespace Lessons.Quests
{
    public class EnabledStateCondition : QuestCondition
    {
        [Space]
        [Header("Enabled")]
        [SerializeField]
        private bool invert = false;

        private void OnEnable()
        {
            isCompleted = !invert;
        }
        private void OnDisable()
        {
            isCompleted = invert;
        }
    }
}
