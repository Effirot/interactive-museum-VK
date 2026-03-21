using System.Collections.Generic;
using InteractiveMuseum.Player;
using UnityEngine;
using UnityEngine.UI;

namespace InteractiveMuseum.UI
{
    /// <summary>
    /// Draws inventory as a horizontal list of slot squares.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField]
        private PlayerInventory targetInventory;

        [SerializeField]
        private Transform slotsRoot;

        [SerializeField]
        private GameObject slotPrefab;

        [SerializeField]
        private Color emptySlotColor = new Color(1f, 1f, 1f, 0.2f);

        private readonly List<Image> _slotImages = new List<Image>();

        private void Start()
        {
            if (targetInventory == null)
            {
                targetInventory = FindFirstObjectByType<PlayerInventory>();
            }

            if (targetInventory != null)
            {
                targetInventory.OnInventoryChanged += Refresh;
            }

            BuildSlots();
            Refresh();
        }

        private void OnDestroy()
        {
            if (targetInventory != null)
            {
                targetInventory.OnInventoryChanged -= Refresh;
            }
        }

        private void BuildSlots()
        {
            _slotImages.Clear();
            if (slotsRoot == null || slotPrefab == null || targetInventory == null)
                return;

            for (int i = slotsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(slotsRoot.GetChild(i).gameObject);
            }

            for (int i = 0; i < targetInventory.MaxSlots; i++)
            {
                GameObject slot = Instantiate(slotPrefab, slotsRoot);
                Image image = slot.GetComponent<Image>();
                if (image == null)
                {
                    image = slot.GetComponentInChildren<Image>();
                }

                if (image != null)
                {
                    _slotImages.Add(image);
                }
            }
        }

        public void Refresh()
        {
            if (targetInventory == null)
                return;

            if (_slotImages.Count != targetInventory.MaxSlots)
            {
                BuildSlots();
            }

            IReadOnlyList<InventoryItemData> items = targetInventory.Items;
            for (int i = 0; i < _slotImages.Count; i++)
            {
                Image slotImage = _slotImages[i];
                if (slotImage == null)
                    continue;

                if (i < items.Count && items[i] != null && items[i].icon != null)
                {
                    slotImage.sprite = items[i].icon;
                    slotImage.color = Color.white;
                }
                else
                {
                    slotImage.sprite = null;
                    slotImage.color = emptySlotColor;
                }
            }
        }
    }
}
