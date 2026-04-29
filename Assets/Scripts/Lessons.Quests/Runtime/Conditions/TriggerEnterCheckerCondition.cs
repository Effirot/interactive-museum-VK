using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Search;
using UnityEngine.SearchService;

namespace Lessons.Quests
{
    public class TriggerEnterCheckerCondition : QuestCondition
    {
        [Space]
        [Header("Tag")]
        [SerializeField]
        private string includeObjectsWithTag = "Player";

        private List<Collider> includedColliders = new();

        private void OnTriggerEnter(Collider other)
        {
            if (CheckTagOf(other))
            {
                includedColliders.Add(other);
                isCompleted = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (CheckTagOf(other))
            {
                includedColliders.Remove(other);
                isCompleted = includedColliders.Any();
            }
        }

        private bool CheckTagOf(Collider collider)
        {
            return
                collider.tag == includeObjectsWithTag ||
                (collider.attachedRigidbody != null && collider.attachedRigidbody.tag == includeObjectsWithTag) ||
                (collider.attachedArticulationBody != null && collider.attachedArticulationBody.tag == includeObjectsWithTag);
        }
    }
}
