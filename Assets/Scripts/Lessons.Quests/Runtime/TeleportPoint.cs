using UnityEngine;

namespace Lessons.Quests
{
    public class TeleportPoint : MonoBehaviour
    {
        [SerializeField]
        private Transform targetTransform;

        [Space]
        [SerializeField]
        public bool executeOnInitialize = false;
        [SerializeField]
        private bool _usePosition = true;
        [SerializeField]
        private bool _useRotation = true;
        [SerializeField]
        private bool _useScale = false;

        [Space]
        [SerializeField]
        private bool _resetVelocity = true;


        public void Execute()
        {
            Execute(transform);
        }
        public void Execute(Transform transform)
        {
            if (_usePosition)
                targetTransform.position = transform.position;

            if (_useRotation)
                targetTransform.rotation = transform.rotation;

            if (_useScale)
                targetTransform.localScale = transform.localScale;

            if (_resetVelocity && transform.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Physics.SyncTransforms();
        }

        private void OnDrawGizmos()
        {
            OnValidate();
        }
        private void OnValidate()
        {
            if (isActiveAndEnabled && !Application.isPlaying)
            {
                Execute(transform);
            }
        }

        private void OnEnable()
        {
            if (executeOnInitialize)
            {
                Execute();
            }
        }
    }
}
