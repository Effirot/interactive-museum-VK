using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lessons.Quests.UI
{
    public class QuestConditionsListUIDrawer : MonoBehaviour
    {
        [SerializeField]
        private GameObject checkpointPrefab;
        [SerializeField]
        private bool drawAllCheckpoints;


        private Dictionary<QuestCheckpoint, QuestCheckpointUIDrawer> checkpointDrawerPare = new();

        private void OnEnable()
        {
            Quest.onActiveQuestChanged += OnActiveQuestChanged_Event;
        }
        private void OnDisable()
        {
            Quest.onActiveQuestChanged -= OnActiveQuestChanged_Event;
            Clear();
        }
        private void OnValidate()
        {
            if (checkpointPrefab != null)
            {
                if (checkpointPrefab.TryGetComponent<QuestCheckpointUIDrawer>(out var drawer))
                {
                    drawer.syncWithCurrent = false;
                }
                else
                {
                    checkpointPrefab = null;
                    Debug.LogError("checkpointPrefab has no QuestCheckpointUIDrawer");
                }
            }
        }

        private void Clear()
        {
            foreach (var drawer in checkpointDrawerPare.Values)
            {
                Destroy(drawer.gameObject);
            }

            checkpointDrawerPare.Clear();
        }

        private void OnActiveQuestChanged_Event(Quest newQuest)
        {
            Clear();

            if (newQuest == null)
                return;

            if (drawAllCheckpoints)
            {
                foreach (var checkpoint in newQuest)
                {
                    CreateCheckpointElement(checkpoint);
                }
            }
            else
            {
                CreateCheckpointElement(newQuest.currentCheckpoint);
            }
        }
        private void CreateCheckpointElement(QuestCheckpoint checkpoint)
        {
            if (checkpointDrawerPare.ContainsKey(checkpoint))
            {
                throw new InvalidOperationException("You are already drawed this Quest Checkpoint");
            }

            var checkpointDrawerObject = Instantiate(checkpointPrefab, transform);
            checkpointDrawerObject.SetActive(true);

            var checkpointDrawer = checkpointDrawerObject.GetComponent<QuestCheckpointUIDrawer>();
            checkpointDrawer.Init(checkpoint);

            checkpointDrawerPare.Add(checkpoint, checkpointDrawer);
        }
    }
}
