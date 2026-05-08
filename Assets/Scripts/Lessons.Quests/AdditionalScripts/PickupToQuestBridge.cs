using UnityEngine;
using InteractiveMuseum.Player;

namespace Lessons.Quests
{
    public class PickupToQuestBridge : MonoBehaviour
    {
        [Header("Quest Marker")]
        [SerializeField]
        private GameObject questMarkerToActivate;

        [Header("Settings")]
        [SerializeField]
        private bool activateEvenIfInventoryFull = false;

        private PickableObject _pickableObject;
        private PlayerInventory _playerInventory;
        private bool _wasPickedUp = false;
        private bool _isInitialized = false;

        private void Awake()
        {
            _pickableObject = GetComponent<PickableObject>();
            if (_pickableObject == null)
            {
                Debug.LogError($"PickupToQuestBridge на {gameObject.name}: не найден компонент PickableObject!", this);
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            PlayerMovementController player = FindFirstObjectByType<PlayerMovementController>();
            if (player != null && player.Inventory != null)
            {
                _playerInventory = player.Inventory;
                _playerInventory.OnInventoryChanged += OnInventoryChanged;
                Debug.Log($"PickupToQuestBridge на {gameObject.name}: подписан на изменения инвентаря.");
            }
            else
            {
                Debug.LogWarning($"PickupToQuestBridge на {gameObject.name}: PlayerInventory не найден. " +
                               "Предмет будет отслеживаться через Interactable.", this);
            }

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_playerInventory != null)
            {
                _playerInventory.OnInventoryChanged -= OnInventoryChanged;
            }

            Interactable interactable = GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.onInteract.RemoveListener(OnInteractedSimple);
            }
        }

        private void Update()
        {
            if (!_isInitialized || _playerInventory != null)
                return;

            PlayerMovementController player = FindFirstObjectByType<PlayerMovementController>();
            if (player != null && player.Inventory != null)
            {
                _playerInventory = player.Inventory;
                _playerInventory.OnInventoryChanged += OnInventoryChanged;
                Debug.Log($"PickupToQuestBridge на {gameObject.name}: инвентарь найден (отложенная инициализация).");
            }
        }

        private void OnInventoryChanged()
        {
            if (_wasPickedUp || _playerInventory == null || _pickableObject == null)
                return;

             if (_playerInventory.HasItem(_pickableObject.ItemId))
            {
                ActivateMarker();
            }
        }

        public void OnInteractedSimple()
        {
            if (!_wasPickedUp)
            {
                Debug.Log($"PickupToQuestBridge на {gameObject.name}: взаимодействие через Interactable.");
                
                if (_playerInventory == null && activateEvenIfInventoryFull)
                {
                    ActivateMarker();
                }
            }
        }

        private void ActivateMarker()
        {
            if (_wasPickedUp)
                return;

            _wasPickedUp = true;

            if (questMarkerToActivate != null)
            {
                questMarkerToActivate.SetActive(true);
                Debug.Log($"PickupToQuestBridge: предмет '{_pickableObject.ItemDisplayName}' подобран, " +
                         $"маркер '{questMarkerToActivate.name}' активирован.");
            }
            else
            {
                Debug.LogWarning($"PickupToQuestBridge на {gameObject.name}: questMarkerToActivate не назначен!", this);
            }
        }

        public void ActivateMarkerManually()
        {
            ActivateMarker();
        }

        public bool WasPickedUp()
        {
            return _wasPickedUp;
        }
    }
}