using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Lessons.Quests.UI
{
    public class QuestCheckpointUIDrawer : MonoBehaviour
    {
        [SerializeField]
        public bool syncWithCurrent = true;
        [SerializeField]
        private TMP_Text checkpointLabel;
        [SerializeField]
        private TMP_Text checkpointDiscriptionLabel;
        [SerializeField]
        private GameObject conditionPrefab;
        [SerializeField]
        private UnityEvent<bool> onSourceSelected = new();

        private Quest _quest;
        private QuestCheckpoint _checkpoint;

        private Dictionary<QuestCondition, QuestConditionUIDrawer> conditionDrawerPare = new();

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        public void LoadScene(int index)
        {
            SceneManager.LoadScene(index);
        }

        public void Init(QuestCheckpoint checkpoint)
        {
            if (this._checkpoint != null)
            {
                this._checkpoint.onSelectionChanged.RemoveListener(UpdateIncludes);
            }

            this._checkpoint = checkpoint;

            if (checkpoint != null)
            {
                checkpoint.onSelectionChanged.AddListener(UpdateIncludes);
                UpdateIncludes(checkpoint.Active);
            }
        }

        private void Start()
        {
            // Убираем автоматическое отключение, полагаемся на события
            if (syncWithCurrent && Quest.active != null)
            {
                OnCurrentQuestChanged_Event(Quest.active);
            }
        }
        private void OnEnable()
        {
            if (syncWithCurrent)
            {
                Quest.onActiveQuestChanged += OnCurrentQuestChanged_Event;

                if (Quest.active != null)
                {
                    _checkpoint = Quest.active.currentCheckpoint;
                }
            }
        }
        private void OnDisable()
        {
            Quest.onActiveQuestChanged -= OnCurrentQuestChanged_Event;
            Init(null);
        }
        private void OnValidate()
        {
            if (conditionPrefab != null && !conditionPrefab.TryGetComponent<QuestConditionUIDrawer>(out _))
            {
                conditionPrefab = null;
                Debug.LogError("conditionPrefab has no QuestConditionUIDrawer");
            }
        }


        private void OnCurrentQuestChanged_Event(Quest newQuest)
        {
            if (_quest != null)
            {
                _quest.onCheckpointChanged.RemoveListener(OnQuestCheckpointChanged);
            }

            _quest = newQuest;
            _checkpoint = Quest.active.currentCheckpoint;

            if (_quest != null)
            {
                _quest.onCheckpointChanged.AddListener(OnQuestCheckpointChanged);
            }

            Init(_checkpoint);
        }
        private void OnQuestCheckpointChanged(QuestCheckpoint questCheckpoint)
        {
            Init(questCheckpoint);
        }

        private void UpdateIncludes(bool selected)
        {
            if (checkpointLabel != null)
            {
                checkpointLabel.text = _checkpoint.checkpointName;
            }
            if (checkpointDiscriptionLabel != null)
            {
                checkpointDiscriptionLabel.text = _checkpoint.checkpointDiscription;
            }

            onSourceSelected.Invoke(selected);

            RemoveConditions();

            if (selected && conditionPrefab != null)
            {
                foreach (var condition in _checkpoint)
                {
                    CreateConditionDrawer(condition);
                }
            }
        }

        private void CreateConditionDrawer(QuestCondition condition)
        {
            var drawerObject = Instantiate(conditionPrefab, transform);
            var drawer = drawerObject.GetComponent<QuestConditionUIDrawer>();

            drawer.Init(condition);
            conditionDrawerPare.Add(condition, drawer);
        }
        private void RemoveConditions()
        {
            foreach (var drawer in conditionDrawerPare.Values)
            {
                Destroy(drawer.gameObject);
            }

            conditionDrawerPare.Clear();
        }
    }
}