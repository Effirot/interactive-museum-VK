using UnityEngine;
using InteractiveMuseum.Player;

namespace Lessons.Quests
{
    public class InventoryConditionChecker : QuestCondition
    {
        [Tooltip("ID предмета, который нужно проверить в инвентаре")]
        [SerializeField]
        private string requiredItemId = "";

        [SerializeField]
        private bool consumeItemOnComplete = false;

        [SerializeField]
        private bool invert = false;

        private PlayerInventory _playerInventory;
        private bool _wasCompleted = false;

        public override void Active()
        {
            base.Active();
            FindInventory();
        }

        public override void Refresh()
        {
            if (!conditionActive || _wasCompleted || _playerInventory == null)
                return;

            bool hasItem = _playerInventory.HasItem(requiredItemId);

            bool conditionMet = invert ? !hasItem : hasItem;

            if (conditionMet)
            {
                _wasCompleted = true;

                if (consumeItemOnComplete && hasItem)
                {
                    _playerInventory.ConsumeItem(requiredItemId);
                }

                isCompleted = true;
            }
        }

        private void FindInventory()
        {
            if (_playerInventory != null)
                return;

            PlayerMovementController player = FindFirstObjectByType<PlayerMovementController>();
            if (player != null)
            {
                _playerInventory = player.Inventory;
            }

            if (_playerInventory == null)
            {
                Debug.LogWarning($"InventoryConditionChecker на {gameObject.name}: " +
                               "PlayerInventory не найден в сцене!", this);
            }
        }

        private void OnEnable()
        {
            _wasCompleted = false;
        }

        public override void Deactive()
        {
            base.Deactive();
            _wasCompleted = false;
        }
    }
}