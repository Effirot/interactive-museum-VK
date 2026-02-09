using UnityEngine;
using Unity.Cinemachine;
using InteractiveMuseum.Camera;
using InteractiveMuseum.MiniGames;
using InteractiveMuseum.PipeSystem;

namespace InteractiveMuseum.Interaction
{
    /// <summary>
    /// Allows interaction with objects that can switch camera focus and activate mini-games.
    /// </summary>
    public class FocusableInteractable : MonoBehaviour
    {
        [Header("Camera Settings")]
        [Tooltip("Camera to switch to when interacting with this object")]
        [SerializeField]
        private CinemachineCamera _targetCamera;
        
        [Tooltip("Player camera (will be found automatically if not set)")]
        [SerializeField]
        private CinemachineCamera _playerCamera;
        
        [Header("Mini-Game")]
        [Tooltip("Mini-game to activate when interacting with this object")]
        [SerializeField]
        private MiniGameBase _miniGame;
        
        [Header("Interaction Settings")]
        [Tooltip("Text shown when looking at this object")]
        [SerializeField]
        private string _interactionText = "Interact";

        public CinemachineCamera targetCamera
        {
            get => _targetCamera;
            set => _targetCamera = value;
        }

        public CinemachineCamera playerCamera
        {
            get => _playerCamera;
            set => _playerCamera = value;
        }

        public MiniGameBase miniGame
        {
            get => _miniGame;
            set => _miniGame = value;
        }
        
        /// <summary>
        /// Legacy property for backward compatibility with PipeGridSystem.
        /// </summary>
        [System.Obsolete("Use miniGame instead")]
        public PipeGridSystem pipeSystem
        {
            get => _miniGame as PipeGridSystem;
            set => _miniGame = value;
        }

        public string interactionText
        {
            get => _interactionText;
            set => _interactionText = value;
        }
        
        
        private CameraManager _cameraManager;
        private Interactable _interactableComponent;
        private bool _isMiniGameModeActive = false;
        
        private void Start()
        {
            _cameraManager = CameraManager.Instance;
            
            // Get or add Interactable component for integration with existing system
            _interactableComponent = GetComponent<Interactable>();
            if (_interactableComponent == null)
            {
                _interactableComponent = gameObject.AddComponent<Interactable>();
                _interactableComponent.onLookText = _interactionText;
            }
            
            // Subscribe to interact event
            _interactableComponent.onInteract.AddListener(OnInteract);
            
            // Find player camera if not set
            if (_playerCamera == null && _cameraManager != null)
            {
                _playerCamera = _cameraManager.playerCamera;
            }
            
            // Ensure InteractableOutline exists for highlighting
            InteractableOutline outline = GetComponent<InteractableOutline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<InteractableOutline>();
            }
        }
        
        /// <summary>
        /// Called when the object is interacted with.
        /// </summary>
        public void OnInteract()
        {
            // Check actual mini-game mode state from MiniGameBase instead of local flag
            bool isCurrentlyActive = false;
            if (_miniGame != null)
            {
                isCurrentlyActive = _miniGame.IsActive();
            }
            
            // Also check CameraManager state as fallback
            if (!isCurrentlyActive && _cameraManager != null)
            {
                isCurrentlyActive = _cameraManager.IsMiniGameModeActive();
            }
            
            // If mini-game mode is already active, don't allow interaction with trigger (exit only via ESC)
            if (isCurrentlyActive)
            {
                // Don't exit on trigger click - only allow exit via ESC key
                return;
            }
            
            // Enter mini-game mode
            EnterMiniGameMode();
        }
        
        private void EnterMiniGameMode()
        {
            if (_cameraManager == null)
            {
                Debug.LogError("CameraManager not found! Make sure CameraManager is in the scene.");
                return;
            }
            
            if (_targetCamera == null)
            {
                Debug.LogError("Target camera not set in FocusableInteractable!");
                return;
            }
            
            if (_miniGame == null)
            {
                Debug.LogError("Mini-game not set in FocusableInteractable!");
                return;
            }
            
            // Always set this zone's camera as the active mini-game camera, so multiple
            // mini-games (e.g. pipes vs cockroaches) don't conflict — whichever zone
            // we interact with gets its camera.
            _cameraManager.miniGameCamera = _targetCamera;
            if (_cameraManager.playerCamera == null && _playerCamera != null)
            {
                _cameraManager.playerCamera = _playerCamera;
            }
            
            _miniGame.miniGameCamera = _targetCamera;
            
            // Activate mini-game
            _miniGame.ActivateMiniGame();
            
            // Disable interaction trigger when entering mini-game mode
            DisableInteractionTrigger();
            
            // Update local flag to match actual state
            _isMiniGameModeActive = true;
        }
        
        private void ExitMiniGameMode()
        {
            if (_cameraManager == null)
                return;
                
            // Deactivate mini-game
            if (_miniGame != null)
            {
                _miniGame.DeactivateMiniGame();
            }
            
            // Enable interaction trigger when exiting mini-game mode
            EnableInteractionTrigger();
            
            // Update local flag to match actual state
            _isMiniGameModeActive = false;
        }
        
        /// <summary>
        /// Disables the interaction trigger GameObject.
        /// </summary>
        private void DisableInteractionTrigger()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.enabled = false;
            }
        }
        
        /// <summary>
        /// Enables the interaction trigger GameObject.
        /// </summary>
        private void EnableInteractionTrigger()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.enabled = true;
            }
        }
        
        /// <summary>
        /// Public method to exit mini-game mode from external calls (e.g., ESC key).
        /// </summary>
        public void ExitMiniGameModePublic()
        {
            ExitMiniGameMode();
        }
        
        /// <summary>
        /// Legacy method name for backward compatibility.
        /// </summary>
        [System.Obsolete("Use ExitMiniGameModePublic instead")]
        public void ExitPipeModePublic()
        {
            ExitMiniGameModePublic();
        }
        
        // Legacy method for compatibility with PlayerMovementController
        private void Interact(GameObject interactor)
        {
            OnInteract();
        }
    }
}
