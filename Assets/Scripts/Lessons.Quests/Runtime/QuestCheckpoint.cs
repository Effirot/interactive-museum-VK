using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Lessons.Quests
{
    [DisallowMultipleComponent]
    public sealed class QuestCheckpoint : MonoBehaviour,
        IEnumerable<QuestCondition>
    {
        [SerializeField, TextArea(1, 2)]
        public string checkpointName;
        [SerializeField, TextArea(3, 10)]
        public string checkpointDiscription;

        [Space]
        [SerializeField]
        private QuestCondition[] conditions;
        [SerializeField]
        public UnityEvent<bool> onSelectionChanged = new();


        public Quest ParentQuest { get; internal set; } = null;

        public bool Active => ParentQuest != null;
        public float Progress => conditions.Where(condition => condition.isCompleted).Count() / (conditions.Length - 1);


        public void Select()
        {
            SetCheckpointForConditions(this);
            onSelectionChanged.Invoke(true);

            Checkout();
        }
        public void Deselect()
        {
            SetCheckpointForConditions(null);
            onSelectionChanged.Invoke(false);
        }
        public void Skip()
        {
            if (Active)
                ParentQuest.SkipCheckPoint();
        }


        public IEnumerator<QuestCondition> GetEnumerator() => (IEnumerator<QuestCondition>)conditions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void Checkout()
        {
            if (conditions.Any() && conditions.All(condition => condition.isCompleted))
            {
                Skip();
            }
        }

        private void Awake()
        {
            ClearFromEmpties();
        }
        private void Update()
        {
            if (Active)
            {
                foreach (var condition in conditions)
                {
                    if (condition != null)
                    {
                        condition.Refresh();
                    }
                }
            }
        }
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ClearFromEmpties();
            }
        }

        private void SetCheckpointForConditions(QuestCheckpoint checkpoint)
        {
            foreach (var condition in conditions)
            {
                condition.parentCheckpoint = checkpoint;
                condition.SetComplitionState(condition.isCompleted);
            }
        }
        private void ClearFromEmpties()
        {
            conditions = conditions.Where(condition => condition != null).ToArray();
        }
    }
}
