using UnityEngine;
using Unity.Cinemachine;
using InteractiveMuseum.Player;

namespace InteractiveMuseum.Camera
{
    /// <summary>
    /// Manages camera switching between player and pipe modes.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("Camera References")]
        [Tooltip("Camera used for normal player gameplay")]
        [SerializeField]
        private CinemachineCamera _playerCamera;
        
        [Tooltip("Camera used when interacting with pipe puzzles")]
        [SerializeField]
        private CinemachineCamera _pipeCamera;

        [Header("Player Reference")]
        [Tooltip("Reference to the player controller")]
        [SerializeField]
        private PlayerMovementController _player;

        public CinemachineCamera playerCamera
        {
            get => _playerCamera;
            set => _playerCamera = value;
        }

        public CinemachineCamera pipeCamera
        {
            get => _pipeCamera;
            set => _pipeCamera = value;
        }

        public PlayerMovementController player
        {
            get => _player;
            set => _player = value;
        }

        private bool _isPipeModeActive = false;

        public static CameraManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple CameraManager instances found. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Ensure player camera is active by default
            if (_playerCamera != null && _pipeCamera != null)
            {
                _playerCamera.Priority = 10;
                _pipeCamera.Priority = 0;
            }
        }

        /// <summary>
        /// Switches to pipe camera and disables player movement.
        /// </summary>
        public void SwitchToPipeCamera()
        {
            if (_pipeCamera == null || _playerCamera == null)
            {
                Debug.LogError("Camera references not set in CameraManager!");
                return;
            }

            _isPipeModeActive = true;
            
            // Switch camera priorities
            _playerCamera.Priority = 0;
            _pipeCamera.Priority = 10;

            // Disable player movement
            if (_player != null)
            {
                _player.SetPipeMode(true);
            }
        }

        /// <summary>
        /// Switches back to player camera and enables player movement.
        /// </summary>
        public void SwitchToPlayerCamera()
        {
            if (_pipeCamera == null || _playerCamera == null)
            {
                Debug.LogError("Camera references not set in CameraManager!");
                return;
            }

            _isPipeModeActive = false;
            
            // Switch camera priorities back
            _playerCamera.Priority = 10;
            _pipeCamera.Priority = 0;

            // Enable player movement
            if (_player != null)
            {
                _player.SetPipeMode(false);
            }
        }

        /// <summary>
        /// Returns whether pipe mode is currently active.
        /// </summary>
        public bool IsPipeModeActive()
        {
            return _isPipeModeActive;
        }
    }
}
