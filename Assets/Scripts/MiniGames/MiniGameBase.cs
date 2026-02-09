using UnityEngine;
using InteractiveMuseum.Camera;

namespace InteractiveMuseum.MiniGames
{
    /// <summary>
    /// Base class for all mini-games in the museum.
    /// Provides common functionality for camera switching and game activation/deactivation.
    /// </summary>
    public abstract class MiniGameBase : MonoBehaviour
    {
        [Header("Camera Settings")]
        [Tooltip("Camera used when this mini-game is active")]
        [SerializeField]
        protected Unity.Cinemachine.CinemachineCamera _miniGameCamera;
        
        protected bool _isActive = false;
        protected CameraManager _cameraManager;
        
        /// <summary>
        /// Gets whether the mini-game is currently active.
        /// </summary>
        public bool IsActive()
        {
            return _isActive;
        }
        
        /// <summary>
        /// Gets the camera used for this mini-game.
        /// </summary>
        public Unity.Cinemachine.CinemachineCamera miniGameCamera
        {
            get => _miniGameCamera;
            set => _miniGameCamera = value;
        }
        
        protected virtual void Start()
        {
            _cameraManager = CameraManager.Instance;
            
            if (_cameraManager == null)
            {
                Debug.LogWarning($"[{GetType().Name}] CameraManager not found. Mini-game camera switching may not work.");
            }
        }
        
        /// <summary>
        /// Activates the mini-game and switches to mini-game camera.
        /// </summary>
        public virtual void ActivateMiniGame()
        {
            _isActive = true;
            
            if (_cameraManager != null)
            {
                // Set the mini-game camera in CameraManager if not already set
                if (_miniGameCamera != null && _cameraManager.miniGameCamera == null)
                {
                    _cameraManager.miniGameCamera = _miniGameCamera;
                }
                
                _cameraManager.SwitchToMiniGameCamera();
            }
            
            OnMiniGameActivated();
        }
        
        /// <summary>
        /// Deactivates the mini-game and switches back to player camera.
        /// </summary>
        public virtual void DeactivateMiniGame()
        {
            _isActive = false;
            
            if (_cameraManager != null)
            {
                _cameraManager.SwitchToPlayerCamera();
            }
            
            OnMiniGameDeactivated();
        }
        
        /// <summary>
        /// Called when the mini-game is activated. Override to add custom behavior.
        /// </summary>
        protected virtual void OnMiniGameActivated()
        {
            // Override in derived classes for custom activation logic
        }
        
        /// <summary>
        /// Called when the mini-game is deactivated. Override to add custom behavior.
        /// </summary>
        protected virtual void OnMiniGameDeactivated()
        {
            // Override in derived classes for custom deactivation logic
        }
    }
}
