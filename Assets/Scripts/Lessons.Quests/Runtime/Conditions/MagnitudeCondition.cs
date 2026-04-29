

using UnityEngine;

namespace Lessons.Quests
{
    public class MagnitudeCondition : QuestCondition
    {
        [SerializeField]
        private new Rigidbody rigidbody;
        [SerializeField]
        private ComparisonType comparisonType = ComparisonType.LessOrEqual;
        [SerializeField]
        private float targetMagnitude = 2;

        public override void Refresh()
        {
            isCompleted = Compare(comparisonType, targetMagnitude, rigidbody.linearVelocity.magnitude);
        }
    }
}