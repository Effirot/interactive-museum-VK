using UnityEngine;
using UnityEngine.Events;

namespace Lessons.Quests
{
    public class TimerCondition : QuestCondition
    {
        [SerializeField, Range(0, 50)]
        private float targetTime = 5;
        [SerializeField]
        private UnityEvent _onTimerOverEvent = new(); 

        private float time;

        private void OnDisable()
        {
            time = 0;
            isCompleted = false;
        }

        public override void Refresh()
        {
            if (conditionActive)
            {
                time += Time.deltaTime * Time.timeScale;

                var complitionOldValue = isCompleted;
                isCompleted = time > targetTime;

                if (!complitionOldValue && isCompleted)
                {
                    _onTimerOverEvent.Invoke();
                }
            }
            else
            {
                isCompleted = false;
                time = 0;
            }
        }
    }
}
