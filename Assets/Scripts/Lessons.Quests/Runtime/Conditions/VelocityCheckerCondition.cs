using UnityEngine;

namespace Lessons.Quests
{
    public class VelocityCheckerCondition : QuestCondition
    {
        public enum WorldSpaceOrientation
        {
            Local,
            World,
        }

        [SerializeField]
        private new Rigidbody rigidbody;
        [SerializeField]
        private WorldSpaceOrientation orientation = WorldSpaceOrientation.Local;

        [Space]
        [SerializeField]
        private Vector3 targetVelocity;
        [SerializeField, Range(0, 40)]
        private float nearDistance = 1;
        [SerializeField]
        private ComparisonType compare = ComparisonType.Less;

        private bool isValid
        {
            get
            {
                var velocity = rigidbody.linearVelocity;

                if (orientation == WorldSpaceOrientation.Local)
                {
                    velocity = Quaternion.Inverse(rigidbody.transform.rotation) * velocity;
                }

                var distance = Vector3.Distance(targetVelocity, velocity);

                return Compare(compare, nearDistance, distance);
            }
        }

        public override void Refresh()
        {
            if (conditionActive)
            {
                isCompleted = isValid;
            }
        }
        private void OnDrawGizmos()
        {
            if (conditionActive)
            {
                OnDrawGizmosSelected();
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (rigidbody == null)
                return;

            var originPoint = rigidbody.transform.position + rigidbody.centerOfMass;
            var direction = targetVelocity;
            if (orientation == WorldSpaceOrientation.Local)
            {
                direction = rigidbody.transform.rotation * direction;
            }

            var targetPoint = originPoint + direction;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPoint, 0.3f);

            Gizmos.color = isValid ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(originPoint + rigidbody.linearVelocity, 0.3f);
            Gizmos.DrawLine(originPoint + rigidbody.linearVelocity, targetPoint);
        }
    }
}
