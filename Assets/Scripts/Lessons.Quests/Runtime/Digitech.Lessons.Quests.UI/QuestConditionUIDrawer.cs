using TMPro;
using UnityEngine;

namespace Lessons.Quests.UI
{
    public class QuestConditionUIDrawer : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text nameLabel;

        private QuestCondition questCondition;

        public void Init(QuestCondition questCondition)
        {
            this.questCondition = questCondition;
            questCondition.onComplitionStateChanged.AddListener(OnConditionChanged);

            OnConditionChanged(questCondition.isCompleted);
        }

        private void OnConditionChanged(bool complited)
        {
            if (nameLabel != null)
            {
                if (complited)
                {
                    nameLabel.text = questCondition.conditionName;
                }
                else
                {
                    nameLabel.text = "<s>" + questCondition.conditionName + "</s>";
                }
            }
        }
    }
}