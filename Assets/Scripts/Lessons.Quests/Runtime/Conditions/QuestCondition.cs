using System;
using UnityEngine;
using UnityEngine.Events;

namespace Lessons.Quests
{
    public enum ComparisonType : byte
    {
        Less           = 0x10,
        LessOrEqual    = 0x11,
        Equal          = 0x01,
        GreaterOrEqual = 0x21,
        Greater        = 0x20
    }

    [Flags]
    public enum ConditionAxis : byte
    {
        X = 1,
        Y = 2,
        Z = 4
    }

    public class QuestCondition : MonoBehaviour
    {
        public static bool Compare(ComparisonType comparisonType, float targetValue, float value)
        {
            return (comparisonType.HasFlag(ComparisonType.Equal) && targetValue == value) ||
                    (comparisonType.HasFlag(ComparisonType.Less) && targetValue > value) ||
                    (comparisonType.HasFlag(ComparisonType.Greater) && targetValue < value);
        }

        [SerializeField]
        public bool hidden = false;

        [SerializeField, TextArea(1, 2)]
        public string conditionName;

        [Space]
        [SerializeField]
        private bool _isCompleted = false;
        [SerializeField]
        public UnityEvent<bool> onComplitionStateChanged = new();

        public bool conditionActive { get; private set; } = false;
        public bool isCompleted
        {
            get => _isCompleted;
            set
            {
                if (value != _isCompleted)
                {
                    SetComplitionState(value);
                }
            }
        }
        public QuestCheckpoint parentCheckpoint
        {
            get => _parentCheckpoint;
            internal set
            {
                _parentCheckpoint = value;

                if (conditionActive != (_parentCheckpoint != null))
                {
                    conditionActive = _parentCheckpoint != null;

                    if (conditionActive)
                    {
                        Active();
                    }
                    else
                    {
                        Deactive();
                    }
                }
            }
        }


        private QuestCheckpoint _parentCheckpoint;

        protected virtual void Awake()
        {
            onComplitionStateChanged.Invoke(_isCompleted);
        }

        public virtual void Active() { }
        public virtual void Deactive() { }
        public virtual void Refresh() { }

        public void SetComplitionState(bool state)
        {
            _isCompleted = state;
            onComplitionStateChanged.Invoke(state);

            if (parentCheckpoint != null)
                NotifyCheckpoint();
        }
        protected void NotifyCheckpoint()
        {
            if (parentCheckpoint == null)
            {
                Debug.LogWarning("Chekpoint can't be notified, when QuestCondition in not active");
                return;
            }

            parentCheckpoint.Checkout();
        }
    }
}
