using UnityEngine;

namespace Lessons.Quests
{
    public class AllMarkersActiveCondition : QuestCondition
    {
        [Header("Markers to Check")]
        [SerializeField]
        private GameObject[] markersToCheck;

        [Header("Action on Complete")]
        [SerializeField]
        private GameObject obstacleToDisable;

        private bool _wasCompleted = false;

        public override void Refresh()
        {
            if (!conditionActive || _wasCompleted || markersToCheck == null || markersToCheck.Length == 0)
                return;

            bool allActive = true;
            foreach (GameObject marker in markersToCheck)
            {
                if (marker == null || !marker.activeSelf)
                {
                    allActive = false;
                    break;
                }
            }

            if (allActive)
            {
                _wasCompleted = true;

                if (obstacleToDisable != null)
                {
                    obstacleToDisable.SetActive(false);
                }

                isCompleted = true;
            }
        }

        public override void Deactive()
        {
            base.Deactive();
            _wasCompleted = false;
        }
    }
}