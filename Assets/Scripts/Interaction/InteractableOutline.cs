using UnityEngine;
using System.Collections;

namespace InteractiveMuseum.Interaction
{
    /// <summary>
    /// Manages outline highlighting for interactable objects.
    /// Automatically adds and controls QuickOutline component.
    /// </summary>
    public class InteractableOutline : MonoBehaviour
    {
        [Header("Outline Settings")]
        [Tooltip("Color of the outline when hovering")]
        [SerializeField][ColorUsage(false, true)]
        private Color _outlineColor = Color.yellow;
        
        [Tooltip("Width of the outline")]
        [SerializeField, Range(0f, 10f)]
        private float _outlineWidth = 2f;
        
        [Tooltip("Outline mode")]
        [SerializeField]
        private Outline.Mode _outlineMode = Outline.Mode.OutlineVisible;
        
        private Outline _outline;
        private bool _isHighlighted = false;
        private bool _isInitialized = false;
        
        private void Awake()
        {
            InitializeOutline();
        }
        
        private void Start()
        {
            // Ensure settings are applied even if Awake was called before serialization
            if (!_isInitialized)
            {
                InitializeOutline();
            }
            else
            {
                // Re-apply settings in Start to ensure they're applied after Outline's Awake
                ApplySettings();
            }
        }
        
        private void InitializeOutline()
        {
            // Get or add Outline component
            _outline = GetComponent<Outline>();
            if (_outline == null)
            {
                _outline = gameObject.AddComponent<Outline>();
            }

            _outline.enabled = false;

            _isInitialized = true;
            
            // Apply settings - if Outline's Awake hasn't been called yet, 
            // settings will be applied when it's enabled
            if (Application.isPlaying)
            {
                // In play mode, wait a frame for Outline to initialize
                StartCoroutine(ApplySettingsDelayed());
            }
            else
            {
                // In editor, apply immediately
                ApplySettings();
            }
        }
        
        private IEnumerator ApplySettingsDelayed()
        {
            // Wait one frame to ensure Outline's Awake has been called
            yield return null;
            ApplySettings();
        }
        
        private void ApplySettings()
        {
            if (_outline == null)
                return;
            
            // Use immediate application to ensure settings are applied
            ApplySettingsImmediate();
        }
        
        private void OnValidate()
        {
            // Apply settings when changed in inspector (works in both editor and runtime)
            if (_outline != null)
            {
                // Force immediate application of settings
                ApplySettingsImmediate();
            }
        }
        
        /// <summary>
        /// Applies settings immediately, ensuring Outline is enabled temporarily if needed.
        /// </summary>
        private void ApplySettingsImmediate()
        {
            if (_outline == null)
                return;
            
            bool wasEnabled = _outline.enabled;
            bool shouldBeEnabled = _isHighlighted;
            
            // Temporarily enable to ensure materials are created/updated
            if (!wasEnabled)
            {
                _outline.enabled = true;
            }
            
            // Apply settings
            _outline.OutlineColor = _outlineColor;
            _outline.OutlineWidth = _outlineWidth;
            _outline.OutlineMode = _outlineMode;
            
            // Force update by accessing Update method through reflection or by waiting
            // Since Outline updates in Update(), we need to ensure it runs at least once
            if (Application.isPlaying && !shouldBeEnabled && !wasEnabled)
            {
                // Schedule disable after update
                StartCoroutine(DisableAfterUpdate());
            }
            else if (!shouldBeEnabled && !wasEnabled)
            {
                _outline.enabled = false;
            }
        }
        
        private IEnumerator DisableAfterUpdate()
        {
            // Wait for Outline's Update to process the needsUpdate flag
            yield return null;
            yield return null; // Wait one more frame to ensure UpdateMaterialProperties was called
            if (_outline != null && !_isHighlighted)
            {
                _outline.enabled = false;
            }
        }
        
        /// <summary>
        /// Enables outline highlighting.
        /// </summary>
        public void EnableHighlight()
        {
            // Ensure outline is initialized
            if (_outline == null)
            {
                InitializeOutline();
            }
            
            if (!_isHighlighted)
            {
                _isHighlighted = true;
                if (_outline != null)
                {
                    // Apply settings before enabling (in case they were changed)
                    ApplySettings();
                    _outline.enabled = true;
                }
            }
        }
        
        /// <summary>
        /// Disables outline highlighting.
        /// </summary>
        public void DisableHighlight()
        {
            if (_isHighlighted)
            {
                _isHighlighted = false;
                if (_outline != null)
                {
                    _outline.enabled = false;
                }
            }
        }
        
        /// <summary>
        /// Returns whether the outline is currently highlighted.
        /// </summary>
        public bool IsHighlighted()
        {
            return _isHighlighted;
        }
        
        /// <summary>
        /// Sets the outline color.
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            _outlineColor = color;
            if (_outline != null)
            {
                _outline.OutlineColor = color;
            }
        }
        
        /// <summary>
        /// Sets the outline width.
        /// </summary>
        public void SetOutlineWidth(float width)
        {
            _outlineWidth = width;
            if (_outline != null)
            {
                _outline.OutlineWidth = width;
            }
        }
        
        /// <summary>
        /// Forces reapplication of settings. Useful when component is added dynamically.
        /// </summary>
        public void RefreshSettings()
        {
            if (_outline == null)
            {
                InitializeOutline();
            }
            else
            {
                ApplySettings();
            }
        }
    }
}
