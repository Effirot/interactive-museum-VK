using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Lessons.Quests
{
    [DisallowMultipleComponent]
    public sealed class Quest : MonoBehaviour,
        IEnumerable<QuestCheckpoint>
    {
        public static event Action<Quest> onActiveQuestChanged;

        public static Quest active { get; private set; } = null;


        [SerializeField, TextArea(1, 2)]
        public string questName;
        [SerializeField, TextArea(3, 10)]
        public string questDiscription;
        [SerializeField]
        public bool activateOnInitialize = false;
        [SerializeField]
        private QuestSelectionAsset bindedAsset;

        [Space]
        [Header("Checkpoints")]
        [SerializeField]
        private QuestCheckpoint[] checkPoints = new QuestCheckpoint[0];

        [Space]
        [Header("Event's")]
        [SerializeField]
        public UnityEvent onQuestSelected = new();
        [SerializeField]
        public UnityEvent onQuestAborted = new();
        [SerializeField]
        public UnityEvent onQuestConpleted = new();
        [SerializeField]
        public UnityEvent<QuestCheckpoint> onCheckpointChanged = new();


        public bool isActive => active == this;
        public float progress => currentCheckpointIndex / (checkPoints.Length - 1) + currentCheckpoint.Progress;
        public QuestCheckpoint currentCheckpoint => checkPoints[currentCheckpointIndex];
        public int currentCheckpointIndex
        {
            get => _currentCheckpointIndex;
            set => SelectCheckpoint(value);
        }


        private int _currentCheckpointIndex = 0;

        public void SelectCheckpoint(QuestCheckpoint questCheckpoint)
        {
            if (!isActive)
                throw new InvalidOperationException("Quest is not active");

            var index = Array.IndexOf(checkPoints, questCheckpoint);
            if (index == -1)
                throw new InvalidOperationException("You can't select QuestCheckpoint. It's not contains in checkpoints list");

            SelectCheckpoint(index);
        }
        public void SelectCheckpoint(int index)
        {
            if (!isActive)
                throw new InvalidOperationException("Quest is not active");

            var oldCheckPoint = checkPoints[_currentCheckpointIndex];
            var newCheckPoint = checkPoints[index];
            _currentCheckpointIndex = index;

            oldCheckPoint.ParentQuest = null;
            oldCheckPoint.Deselect();
            newCheckPoint.ParentQuest = this;
            newCheckPoint.Select();

            try
            {
                onCheckpointChanged.Invoke(newCheckPoint);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void SkipCheckPoint()
        {
            if (_currentCheckpointIndex + 1 >= checkPoints.Length)
            {
                try
                {
                    onQuestConpleted.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                Abort();

                return;
            }

            SelectCheckpoint(_currentCheckpointIndex + 1);
        }

        public void Activate(bool reset = true)
        {
            if (active != null && active != this)
            {
                Debug.LogWarning("Another Quest is already active");
                return;
            }
            if (isActive)
            {
                Debug.LogWarning("Quest is already active");
                return;
            }
            if (!checkPoints.Any(point => point != null))
                throw new InvalidOperationException("You can't start empty quest. Add new checkpoint's into CheckPoints list");


            if (reset)
            {
                _currentCheckpointIndex = 0;
            }

            SetQuestAsSingleton(this);
            SelectCheckpoint(_currentCheckpointIndex);

            onQuestSelected.Invoke();
        }
        public void Abort()
        {
            if (!isActive)
            {
                Debug.LogWarning("Quest is already not active");

                return;
            }

            checkPoints[_currentCheckpointIndex].Deselect();

            SetQuestAsSingleton(null);
            onQuestAborted.Invoke();
        }

        public IEnumerator<QuestCheckpoint> GetEnumerator() => (IEnumerator<QuestCheckpoint>)checkPoints.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void Awake()
        {
            ClearFromEmpties();
        }
        private void Start()
        {
            if (activateOnInitialize || (QuestSelectionAsset.selected != null && QuestSelectionAsset.selected == bindedAsset))
            {
                Activate();
            } 
        }
        private void OnDisable()
        {
            if (isActive)
            {
                Abort();
            }
        }
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ClearFromEmpties();
            }
        }

        private void SetQuestAsSingleton(Quest quest)
        {
            active = quest;
            try
            {
                onActiveQuestChanged?.Invoke(quest);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        private void ClearFromEmpties()
        {
            checkPoints = checkPoints.Where(checkpoint => checkpoint != null).ToArray();
        }
    }
}
