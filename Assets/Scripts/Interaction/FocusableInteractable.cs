using UnityEngine;
using Unity.Cinemachine;
using InteractiveMuseum.Camera;
using InteractiveMuseum.PipeSystem;

namespace InteractiveMuseum.Interaction
{
    /// <summary>
    /// Allows interaction with objects that can switch camera focus and activate pipe system.
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
        
        [Header("Pipe System")]
        [Tooltip("Pipe grid system to activate")]
        [SerializeField]
        private PipeGridSystem _pipeSystem;
        
        [Header("Interaction Settings")]
        [Tooltip("Text shown when looking at this object")]
        [SerializeField]
        private string _interactionText = "Interact with pipes";

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

        public PipeGridSystem pipeSystem
        {
            get => _pipeSystem;
            set => _pipeSystem = value;
        }

        public string interactionText
        {
            get => _interactionText;
            set => _interactionText = value;
        }
        
        
        private CameraManager _cameraManager;
        private Interactable _interactableComponent;
        private bool _isPipeModeActive = false;
        
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
        }
        
        /// <summary>
        /// Called when the object is interacted with.
        /// </summary>
        public void OnInteract()
        {
            Debug.Log($"[FocusableInteractable] OnInteract called. isPipeModeActive: {_isPipeModeActive}");
            
            if (_isPipeModeActive)
            {
                // Exit pipe mode
                Debug.Log("[FocusableInteractable] Exiting pipe mode");
                ExitPipeMode();
            }
            else
            {
                // Enter pipe mode
                Debug.Log("[FocusableInteractable] Entering pipe mode");
                EnterPipeMode();
            }
        }
        
        private void EnterPipeMode()
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
            
            // Ensure camera manager has references
            if (_cameraManager.pipeCamera == null)
            {
                _cameraManager.pipeCamera = _targetCamera;
            }
            if (_cameraManager.playerCamera == null && _playerCamera != null)
            {
                _cameraManager.playerCamera = _playerCamera;
            }
            
            // Activate pipe system
            if (_pipeSystem != null)
            {
                _pipeSystem.ActivatePipeMode();
            }
            
            _isPipeModeActive = true;
        }
        
        private void ExitPipeMode()
        {
            if (_cameraManager == null)
                return;
                
            // Deactivate pipe system
            if (_pipeSystem != null)
            {
                _pipeSystem.DeactivatePipeMode();
            }
            
            _isPipeModeActive = false;
        }
        
        /// <summary>
        /// Public method to exit pipe mode from external calls (e.g., ESC key).
        /// </summary>
        public void ExitPipeModePublic()
        {
            ExitPipeMode();
        }
        
        // Legacy method for compatibility with PlayerMovementController
        private void Interact(GameObject interactor)
        {
            OnInteract();
        }
    }
}
