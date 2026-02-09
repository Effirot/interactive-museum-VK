using UnityEngine;
using InteractiveMuseum.Interaction;

namespace InteractiveMuseum.PipeSystem
{
    /// <summary>
    /// Makes a pipe segment interactable for clicking/rotating.
    /// </summary>
    public class PipeSegmentInteractable : MonoBehaviour
    {
        private PipeSegment _pipeSegment;
        private PipeGridSystem _pipeGridSystem;
        
        private void Awake()
        {
            _pipeSegment = GetComponent<PipeSegment>();
            if (_pipeSegment == null)
            {
                Debug.LogWarning("PipeSegmentInteractable requires PipeSegment component!");
            }
            
            // Find PipeGridSystem in parent hierarchy
            _pipeGridSystem = GetComponentInParent<PipeGridSystem>();
            if (_pipeGridSystem == null)
            {
                Debug.LogWarning("PipeSegmentInteractable could not find PipeGridSystem in parent!");
            }
            
            // Ensure InteractableOutline exists for highlighting
            InteractableOutline outline = GetComponent<InteractableOutline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<InteractableOutline>();
            }
        }
        
        /// <summary>
        /// Called by raycast from PlayerMovementController or other interaction system.
        /// </summary>
        public void Interact(GameObject interactor)
        {
            Debug.Log($"[PipeSegmentInteractable] Interact called. pipeSegment: {_pipeSegment != null}, pipeGridSystem: {_pipeGridSystem != null}");
            
            if (_pipeSegment == null || _pipeGridSystem == null)
            {
                Debug.LogWarning("[PipeSegmentInteractable] Missing components!");
                return;
            }
                
            // Check if pipe mode is active
            if (!_pipeGridSystem.IsActive())
            {
                Debug.LogWarning("[PipeSegmentInteractable] Pipe mode is not active!");
                return;
            }
                
            Debug.Log("[PipeSegmentInteractable] Rotating pipe...");
            // Rotate the pipe
            _pipeGridSystem.OnPipeClicked(_pipeSegment);
        }
    }
}
