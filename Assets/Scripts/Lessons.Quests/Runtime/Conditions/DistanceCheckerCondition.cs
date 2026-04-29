using UnityEngine;
using UnityEngine.Search;
using UnityEngine.SearchService;

namespace Lessons.Quests
{
    public class DistanceCheckerCondition : QuestCondition
    {
        [Space]
        [Header("Distance")]
        [SerializeField]
        private bool invert = false;
        [SerializeField]
        private Transform target;
        [SerializeField, Range(0, 100)]
        private float distance = 10;

        public override void Refresh()
        {
            isCompleted = (Vector3.Distance(target.position, transform.position) < distance) ^ invert;
        }
    }
}
