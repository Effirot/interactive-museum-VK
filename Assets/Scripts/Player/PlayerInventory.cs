using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractiveMuseum.Player
{
    [Serializable]
    public class InventoryItemData
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
    }

    /// <summary>
    /// Stores collected items and provides API for automatic item checks/consumption.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField]
        private int maxSlots = 8;

        [SerializeField]
        private List<InventoryItemData> items = new List<InventoryItemData>();

        public event Action OnInventoryChanged;

        public IReadOnlyList<InventoryItemData> Items => items;
        public int MaxSlots => maxSlots;

        public bool TryAddPickable(PickableObject pickableObject)
        {
            if (pickableObject == null || !HasFreeSlot())
                return false;

            InventoryItemData item = new InventoryItemData
            {
                itemId = pickableObject.ItemId,
                displayName = pickableObject.ItemDisplayName,
                icon = pickableObject.ItemIcon
            };

            items.Add(item);
            pickableObject.OnPick();
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return false;

            for (int i = 0; i < items.Count; i++)
            {
                if (string.Equals(items[i].itemId, itemId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public bool ConsumeItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return false;

            for (int i = 0; i < items.Count; i++)
            {
                if (!string.Equals(items[i].itemId, itemId, StringComparison.OrdinalIgnoreCase))
                    continue;

                items.RemoveAt(i);
                OnInventoryChanged?.Invoke();
                return true;
            }

            return false;
        }

        public bool HasFreeSlot()
        {
            return items.Count < maxSlots;
        }
    }
}
