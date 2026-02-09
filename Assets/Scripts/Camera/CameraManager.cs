using UnityEngine;
using Unity.Cinemachine;
using InteractiveMuseum.Player;

namespace InteractiveMuseum.Camera
{
    /// <summary>
    /// Manages camera switching between player and mini-game modes.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("Camera References")]
        [Tooltip("Camera used for normal player gameplay")]
        [SerializeField]
        private CinemachineCamera _playerCamera;
        
        [Tooltip("Camera used when interacting with mini-games")]
        [SerializeField]
        private CinemachineCamera _miniGameCamera;

        [Header("Player Reference")]
        [Tooltip("Reference to the player controller")]
        [SerializeField]
        private PlayerMovementController _player;

        public CinemachineCamera playerCamera
        {
            get => _playerCamera;
            set => _playerCamera = value;
        }

        public CinemachineCamera miniGameCamera
        {
            get => _miniGameCamera;
            set => _miniGameCamera = value;
        }

        public PlayerMovementController player
        {
            get => _player;
            set => _player = value;
        }

        private bool _isMiniGameModeActive = false;

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
            if (_playerCamera != null && _miniGameCamera != null)
            {
                _playerCamera.Priority = 10;
                _miniGameCamera.Priority = 0;
            }
        }

        /// <summary>
        /// Switches to mini-game camera and disables player movement.
        /// </summary>
        public void SwitchToMiniGameCamera()
        {
            if (_miniGameCamera == null || _playerCamera == null)
            {
                Debug.LogError("Camera references not set in CameraManager!");
                return;
            }

            _isMiniGameModeActive = true;
            
            // Switch camera priorities
            _playerCamera.Priority = 0;
            _miniGameCamera.Priority = 10;

            // Disable player movement
            if (_player != null)
            {
                _player.SetMiniGameMode(true);
            }
        }
        
        /// <summary>
        /// Switches to mini-game camera (legacy method name for backward compatibility).
        /// </summary>
        [System.Obsolete("Use SwitchToMiniGameCamera() instead")]
        public void SwitchToPipeCamera()
        {
            SwitchToMiniGameCamera();
        }

        /// <summary>
        /// Switches back to player camera and enables player movement.
        /// </summary>
        public void SwitchToPlayerCamera()
        {
            if (_miniGameCamera == null || _playerCamera == null)
            {
                Debug.LogError("Camera references not set in CameraManager!");
                return;
            }

            _isMiniGameModeActive = false;
            
            // Switch camera priorities back
            _playerCamera.Priority = 10;
            _miniGameCamera.Priority = 0;

            // Enable player movement
            if (_player != null)
            {
                _player.SetMiniGameMode(false);
            }
        }

        /// <summary>
        /// Returns whether mini-game mode is currently active.
        /// </summary>
        public bool IsMiniGameModeActive()
        {
            return _isMiniGameModeActive;
        }
        
        /// <summary>
        /// Returns whether mini-game mode is currently active (legacy method name for backward compatibility).
        /// </summary>
        [System.Obsolete("Use IsMiniGameModeActive() instead")]
        public bool IsPipeModeActive()
        {
            return IsMiniGameModeActive();
        }
    }
}
