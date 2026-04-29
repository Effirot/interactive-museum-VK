
using UnityEngine;

namespace Lessons.Quests
{
    public class RotationCheckerCondition : QuestCondition
    {
        [SerializeField]
        private Transform targetTransform;

        [Space]
        [SerializeField, Range(0, 180)]
        private float requireAngle;
        [SerializeField]
        private ComparisonType comparisonType = ComparisonType.LessOrEqual;
        [SerializeField]
        private ConditionAxis rotationAxis = (ConditionAxis)byte.MaxValue;

        public override void Refresh()
        {
            if (!conditionActive)
                return;

            var targetVector = targetTransform.forward;

            if (!rotationAxis.HasFlag(ConditionAxis.X))
                targetVector.x = 0;
            if (!rotationAxis.HasFlag(ConditionAxis.Y))
                targetVector.y = 0;
            if (!rotationAxis.HasFlag(ConditionAxis.Z))
                targetVector.z = 0;

            targetVector.Normalize();

            isCompleted = Compare(comparisonType, requireAngle, Vector3.Angle(targetVector, transform.forward));
        }
    }
}