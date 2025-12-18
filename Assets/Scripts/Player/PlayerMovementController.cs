using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using InteractiveMuseum.Camera;
using InteractiveMuseum.PipeSystem;
using InteractiveMuseum.Interaction;
using System.Linq;

namespace InteractiveMuseum.Player
{
    /// <summary>
    /// Controls player movement, interaction, and object pickup/placement.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovementController : MonoBehaviour
    {
        public static PlayerMovementController Current { get; private set; } = null;

        [Header("Camera")]
        [SerializeField]
        [Tooltip("Head camera for first-person view")]
        private CinemachineCamera headCamera;

        [Space]
        [Header("Interaction")]
        [SerializeField, Range(0, 50)]
        [Tooltip("Maximum distance for interactions")]
        private float interactionDistance = 5;
        
        [SerializeField]
        [Tooltip("Layer mask for interactable objects")]
        private LayerMask interactionLayer = 1 << 0;

        [Space]
        [Header("Movement")]
        [SerializeField, Range(0, 50)]
        [Tooltip("Normal movement speed")]
        private float speed = 10;
        
        [SerializeField, Range(0, 50)]
        [Tooltip("Sprint movement speed")]
        private float sprintSpeed = 10;
        
        [SerializeField, Range(0, 2)]
        [Tooltip("Mouse sensitivity for camera rotation")]
        private float characterSense = 0.2f;

        [Header("Hands")]
        [Tooltip("Transform for right hand position. Objects will follow this transform when picked up with right hand.")]
        [SerializeField]
        private Transform rightHand;
        
        [Tooltip("Transform for left hand position. Objects will follow this transform when picked up with left hand.")]
        [SerializeField]
        private Transform leftHand;

        [Header("Pipe Mode")]
        [HideInInspector]
        [SerializeField]
        private bool _isInPipeMode = false;

        public bool IsGrounded { get; private set; } = false;

        private CharacterController _characterController;
        private PlayerInput _input;
        
        private Vector3 _movementDirection = Vector3.zero;
        private Vector2 _lookDirection = Vector2.zero;

        private Vector3 _localMovementAccelerationVector = Vector3.zero;

        private bool _sprintState = false;
        
        private Vector3 _velocity = Vector3.zero;
        private float _controllerHitResetTimeout = 0;

        private Vector3 _resultMovementDirection = Vector3.zero;

        // Pickup system
        [SerializeField]
        private PickableObject _righthandObject;
        
        [SerializeField]
        private PickableObject _lefthandObject;

        public bool isInPipeMode
        {
            get => _isInPipeMode;
            set => _isInPipeMode = value;
        }

        public PickableObject righthandObject
        {
            get => _righthandObject;
            set => _righthandObject = value;
        }

        public PickableObject lefthandObject
        {
            get => _lefthandObject;
            set => _lefthandObject = value;
        }
        
        private Vector3 _rightPickedLastPos;
        private Vector3 _leftPickedLastPos;
        private float _rightPickSmoothProgress;
        private float _leftPickSmoothProgress;
        
        private RaycastHit _lastLookRaycast;
        private bool _escapeKeyPressedLastFrame = false;
        private InteractableOutline _currentHighlightedOutline = null;

        /// <summary>
        /// Starts player control and locks cursor.
        /// </summary>
        public void StartControlling()
        {
            _input.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        /// <summary>
        /// Stops player control and unlocks cursor.
        /// </summary>
        public void StopControlling()
        {
            _input.enabled = false;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInput>();
        
            StartControlling();
            Current = this;
        }
        
        private void FixedUpdate()
        {
            ResetCollisionData();
            CalculateVelocity(ref _velocity);

            _resultMovementDirection = _velocity * 5f + CalculateMovementDirection();
        }
        
        private void Update()
        {
            // Update picked objects position
            if (_righthandObject != null && rightHand != null)
            {
                Vector3 posTo = Vector3.Lerp(_rightPickedLastPos, rightHand.position, _rightPickSmoothProgress);
                _righthandObject.transform.position = posTo;
                _righthandObject.transform.rotation = rightHand.rotation;
                MyUtils.SetMove(_righthandObject.gameObject, 0, 0, 0);

                if (_rightPickSmoothProgress < 1)
                {
                    _rightPickSmoothProgress += 0.05f;
                }
            }
            if (_lefthandObject != null && leftHand != null)
            {
                Vector3 posTo = Vector3.Lerp(_leftPickedLastPos, leftHand.position, _leftPickSmoothProgress);
                _lefthandObject.transform.position = posTo;
                _lefthandObject.transform.rotation = leftHand.rotation;
                MyUtils.SetMove(_lefthandObject.gameObject, 0, 0, 0);

                if (_leftPickSmoothProgress < 1)
                {
                    _leftPickSmoothProgress += 0.05f;
                }
            }

            // Check ESC key directly (works even when cursor is unlocked)
            bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            if (escapePressed && !_escapeKeyPressedLastFrame && _isInPipeMode)
            {
                Debug.Log("[Update] ESC key pressed - exiting pipe mode");
                ExitPipeModeViaESC();
            }
            _escapeKeyPressedLastFrame = escapePressed;

            // Update interaction raycast
            UpdateInteractionRaycast();
            
            // In pipe mode, also handle mouse clicks directly (as fallback if OnInteract doesn't work)
            if (_isInPipeMode && Mouse.current != null)
            {
                // Check for left mouse button click (clockwise, right)
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    HandlePipeClick(true);
                }
                // Also check right mouse button (counter-clockwise, left)
                else if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    HandlePipeClick(false);
                }
            }
        }

        private void LateUpdate()
        {
            // Block movement if in pipe mode
            if (_isInPipeMode)
            {
                return;
            }

            float timescale = Time.deltaTime * 20f;

            transform.rotation = 
                Quaternion.Lerp(
                    transform.rotation, 
                    Quaternion.Euler(0, _lookDirection.x, 0), 
                    timescale);
            
            headCamera.transform.localRotation = 
                Quaternion.Lerp(
                    headCamera.transform.localRotation, 
                    Quaternion.Euler(-_lookDirection.y, 0, 0), 
                    timescale);

            _characterController.Move(_resultMovementDirection * Time.deltaTime);
        }
        
        private void UpdateInteractionRaycast()
        {
            Ray ray;
            
            if (_isInPipeMode)
            {
                // In pipe mode, use mouse position for raycast (cursor is unlocked)
                UnityEngine.Camera pipeCameraComponent = null;
                
                // Get Camera from CinemachineBrain (Cinemachine controls the main camera)
                CinemachineBrain cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();
                if (cinemachineBrain != null)
                {
                    pipeCameraComponent = cinemachineBrain.OutputCamera;
                }
                
                // Fallback: try to get Camera from main camera
                if (pipeCameraComponent == null)
                {
                    pipeCameraComponent = UnityEngine.Camera.main;
                }
                
                // Final fallback: find any camera
                if (pipeCameraComponent == null)
                {
                    pipeCameraComponent = FindFirstObjectByType<UnityEngine.Camera>();
                }
                
                if (pipeCameraComponent != null)
                {
                    // Use mouse position to create ray from camera (using new Input System)
                    Vector2 mousePos2D = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
                    Vector3 mousePosition = new Vector3(mousePos2D.x, mousePos2D.y, 0);
                    ray = pipeCameraComponent.ScreenPointToRay(mousePosition);
                }
                else
                {
                    // Last resort: use forward direction from pipe camera transform
                    CameraManager cameraManager = CameraManager.Instance;
                    if (cameraManager != null && cameraManager.pipeCamera != null)
                    {
                        ray = new Ray(cameraManager.pipeCamera.transform.position, 
                                     cameraManager.pipeCamera.transform.forward);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                // Normal mode: use head camera forward direction
                if (headCamera == null)
                    return;
                    
                ray = new Ray(headCamera.transform.position, headCamera.transform.forward);
            }
            RaycastHit hit;
            int mask = interactionLayer.value;
            
            // If interactionLayer is 0, use default layers (Default + Pickable)
            if (mask == 0)
            {
                mask = (1 << 6) | (1 << 0);
            }
            
            // Increase distance for pipe mode to make it easier to click
            float currentDistance = _isInPipeMode ? interactionDistance * 3f : interactionDistance;
            
            // Use larger layer mask in pipe mode to ensure we can detect pipes
            int currentMask = mask;
            if (_isInPipeMode)
            {
                // In pipe mode, accept all layers except UI (layer 5)
                currentMask = ~(1 << 5); // All layers except UI layer 5
            }
            
            // Clear previous highlight
            if (_currentHighlightedOutline != null)
            {
                _currentHighlightedOutline.DisableHighlight();
                _currentHighlightedOutline = null;
            }
            
            // Use longer distance in pipe mode since we're using mouse position
            float raycastDistance = _isInPipeMode ? interactionDistance * 10f : currentDistance;
            
            if (Physics.Raycast(ray, out hit, raycastDistance, currentMask))
            {
                _lastLookRaycast = hit;
                
                // Find and highlight interactable object
                InteractableOutline outline = FindInteractableOutline(hit.collider.gameObject);
                if (outline != null && outline != _currentHighlightedOutline)
                {
                    if (_currentHighlightedOutline != null)
                    {
                        _currentHighlightedOutline.DisableHighlight();
                    }
                    _currentHighlightedOutline = outline;
                    _currentHighlightedOutline.EnableHighlight();
                }
                
                // Show interaction info
                if (_isInPipeMode)
                {
                    // In pipe mode, check for pipes first
                    PipeSegmentInteractable pipeInteractable = hit.collider.gameObject.GetComponent<PipeSegmentInteractable>();
                    if (pipeInteractable == null && hit.collider.transform.parent != null)
                    {
                        pipeInteractable = hit.collider.transform.parent.GetComponent<PipeSegmentInteractable>();
                    }
                    
                    if (pipeInteractable != null)
                    {
                        CanvasManager canvasManager = FindFirstObjectByType<CanvasManager>();
                        if (canvasManager != null)
                        {
                            canvasManager.setInteractionInfo("Click to rotate pipe");
                        }
                    }
                    else
                    {
                        // Check for exit trigger
                        FocusableInteractable focusable = hit.collider.gameObject.GetComponent<FocusableInteractable>();
                        if (focusable != null)
                        {
                            CanvasManager canvasManager = FindFirstObjectByType<CanvasManager>();
                            if (canvasManager != null)
                            {
                                canvasManager.setInteractionInfo("Click to exit pipe mode");
                            }
                        }
                        else
                        {
                            CanvasManager canvasManager = FindFirstObjectByType<CanvasManager>();
                            if (canvasManager != null)
                            {
                                canvasManager.setInteractionInfo("");
                            }
                        }
                    }
                }
                else
                {
                    // Normal mode - check for PickableObject first, then Interactable
                    GameObject hitObj = hit.collider.gameObject;
                    
                    // Check for PickableObject
                    PickableObject pickable = hitObj.GetComponent<PickableObject>();
                    if (pickable == null && hitObj.transform.parent != null)
                    {
                        pickable = hitObj.transform.parent.GetComponent<PickableObject>();
                    }
                    if (pickable == null)
                    {
                        pickable = hitObj.GetComponentInChildren<PickableObject>();
                    }
                    
                    if (pickable != null)
                    {
                        // Check if hand is free
                        bool handFree = _righthandObject == null || _lefthandObject == null;
                        CanvasManager canvasManager = FindFirstObjectByType<CanvasManager>();
                        if (canvasManager != null)
                        {
                            if (handFree)
                            {
                                canvasManager.setInteractionInfo("Click to pick up");
                            }
                            else
                            {
                                canvasManager.setInteractionInfo("Hands full - click to place");
                            }
                        }
                    }
                    else
                    {
                        // Check for Interactable
                        Interactable interactable = hitObj.GetComponent<Interactable>();
                        if (interactable != null)
                        {
                            CanvasManager canvasManager = FindFirstObjectByType<CanvasManager>();
                            if (canvasManager != null)
                            {
                                canvasManager.setInteractionInfo(interactable.onLookText);
                            }
                        }
                        else
                        {
                            CanvasManager canvasManager = FindFirstObjectByType<CanvasManager>();
                            if (canvasManager != null)
                            {
                                canvasManager.setInteractionInfo("");
                            }
                        }
                    }
                }
            }
            else
            {
                _lastLookRaycast = new RaycastHit();
                
                // Clear highlight
                if (_currentHighlightedOutline != null)
                {
                    _currentHighlightedOutline.DisableHighlight();
                    _currentHighlightedOutline = null;
                }
                
                // Clear interaction text
                CanvasManager canvasManager = FindFirstObjectByType<CanvasManager>();
                if (canvasManager != null)
                {
                    canvasManager.setInteractionInfo("");
                }
            }
        }
        
        /// <summary>
        /// Finds InteractableOutline component on the hit object or its parents/children.
        /// </summary>
        private InteractableOutline FindInteractableOutline(GameObject hitObject)
        {
            if (hitObject == null)
                return null;
            
            // Check for InteractableOutline on the hit object
            InteractableOutline outline = hitObject.GetComponent<InteractableOutline>();
            if (outline != null)
                return outline;
            
            // Check parent
            if (hitObject.transform.parent != null)
            {
                outline = hitObject.transform.parent.GetComponent<InteractableOutline>();
                if (outline != null)
                    return outline;
            }
            
            // Check children
            outline = hitObject.GetComponentInChildren<InteractableOutline>();
            if (outline != null)
                return outline;
            
            // Check for interactable components and add outline if needed
            // Check for PipeSegmentInteractable
            PipeSegmentInteractable pipeInteractable = hitObject.GetComponent<PipeSegmentInteractable>();
            if (pipeInteractable == null && hitObject.transform.parent != null)
            {
                pipeInteractable = hitObject.transform.parent.GetComponent<PipeSegmentInteractable>();
            }
            if (pipeInteractable == null)
            {
                pipeInteractable = hitObject.GetComponentInChildren<PipeSegmentInteractable>();
            }
            
            if (pipeInteractable != null)
            {
                // Add outline component if it doesn't exist
                outline = pipeInteractable.GetComponent<InteractableOutline>();
                if (outline == null)
                {
                    outline = pipeInteractable.gameObject.AddComponent<InteractableOutline>();
                    // Ensure settings are applied after dynamic addition
                    outline.RefreshSettings();
                }
                return outline;
            }
            
            // Check for FocusableInteractable
            FocusableInteractable focusable = hitObject.GetComponent<FocusableInteractable>();
            if (focusable == null && hitObject.transform.parent != null)
            {
                focusable = hitObject.transform.parent.GetComponent<FocusableInteractable>();
            }
            
            if (focusable != null)
            {
                // Add outline component if it doesn't exist
                outline = focusable.GetComponent<InteractableOutline>();
                if (outline == null)
                {
                    outline = focusable.gameObject.AddComponent<InteractableOutline>();
                    // Ensure settings are applied after dynamic addition
                    outline.RefreshSettings();
                }
                return outline;
            }
            
            // Check for Interactable
            Interactable interactable = hitObject.GetComponent<Interactable>();
            if (interactable == null && hitObject.transform.parent != null)
            {
                interactable = hitObject.transform.parent.GetComponent<Interactable>();
            }
            
            if (interactable != null)
            {
                // Add outline component if it doesn't exist
                outline = interactable.GetComponent<InteractableOutline>();
                if (outline == null)
                {
                    outline = interactable.gameObject.AddComponent<InteractableOutline>();
                    // Ensure settings are applied after dynamic addition
                    outline.RefreshSettings();
                }
                return outline;
            }
            
            // Check for PickableObject
            PickableObject pickable = hitObject.GetComponent<PickableObject>();
            if (pickable == null && hitObject.transform.parent != null)
            {
                pickable = hitObject.transform.parent.GetComponent<PickableObject>();
            }
            if (pickable == null)
            {
                pickable = hitObject.GetComponentInChildren<PickableObject>();
            }
            
            if (pickable != null)
            {
                // Add outline component if it doesn't exist
                outline = pickable.GetComponent<InteractableOutline>();
                if (outline == null)
                {
                    outline = pickable.gameObject.AddComponent<InteractableOutline>();
                    // Ensure settings are applied after dynamic addition
                    outline.RefreshSettings();
                }
                return outline;
            }
            
            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (headCamera == null)
            {
                headCamera = GetComponentInChildren<CinemachineCamera>();
            }
        }
#endif

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _controllerHitResetTimeout = 0.1f;

            IsGrounded = Vector3.Angle(hit.normal, Vector3.up) <= 35;

            Quaternion normalAngle = Quaternion.FromToRotation(hit.normal, Vector3.down);

            Vector3 deltaVelocity = normalAngle * _velocity;
            deltaVelocity.y = Mathf.Min(0, deltaVelocity.y);

            if (IsGrounded)
            {
                deltaVelocity.x = 0;    
                deltaVelocity.z = 0;
            }

            _velocity = Quaternion.Inverse(normalAngle) * deltaVelocity;
        }

        private void OnMove(InputValue inputValue)
        {
            Vector2 input = inputValue.Get<Vector2>();
            _movementDirection = new Vector3(input.x, 0, input.y); 
        } 
        
        private void OnLook(InputValue inputValue)
        {
            // Don't process look input in pipe mode (cursor is unlocked)
            if (_isInPipeMode)
                return;
                
            _lookDirection += inputValue.Get<Vector2>() * characterSense;

            _lookDirection.y = Mathf.Clamp(_lookDirection.y, -89, 89);
        }
        
        private void OnJump(InputValue inputValue)
        {
            if (inputValue.isPressed && IsGrounded)
            {
                _velocity = Vector3.up * 3;
            }
        }
        
        private void OnSprint(InputValue inputValue)
        {
            _sprintState = inputValue.isPressed;
        }
        
        private void OnCancel(InputValue inputValue)
        {
            if (inputValue.isPressed && _isInPipeMode)
            {
                Debug.Log("[OnCancel] Exiting pipe mode via ESC");
                ExitPipeModeViaESC();
            }
        }
    
        private void ExitPipeModeViaESC()
        {
            // Clear highlight when exiting pipe mode
            if (_currentHighlightedOutline != null)
            {
                _currentHighlightedOutline.DisableHighlight();
                _currentHighlightedOutline = null;
            }
            
            // Find PipeGridSystem and deactivate it
            PipeGridSystem pipeSystem = FindFirstObjectByType<PipeGridSystem>();
            if (pipeSystem != null)
            {
                pipeSystem.DeactivatePipeMode();
            }
            
            // CameraManager will be updated by PipeGridSystem.DeactivatePipeMode()
            // But also ensure it's done
            CameraManager cameraManager = CameraManager.Instance;
            if (cameraManager != null && cameraManager.IsPipeModeActive())
            {
                cameraManager.SwitchToPlayerCamera();
            }
        }
        
        private void OnInteract(InputValue inputValue)
        {
            if (!inputValue.isPressed)
                return;
                
            // Handle pipe interaction mode (default to clockwise for Interact action)
            if (_isInPipeMode)
            {
                HandlePipeClick(true);
                return;
            }
            
            // Don't check headCamera.IsLive - just check if we have a valid raycast
            if (_lastLookRaycast.collider != null)
            {
                GameObject hitObject = _lastLookRaycast.collider.gameObject;
                Debug.Log($"[OnInteract] Hit object: {hitObject.name}");
                
                // Priority 1: Try to pickup PickableObject (if hand is free)
                // Check on hit object, parent, and children
                PickableObject pickableObject = hitObject.GetComponent<PickableObject>();
                if (pickableObject == null && hitObject.transform.parent != null)
                {
                    pickableObject = hitObject.transform.parent.GetComponent<PickableObject>();
                }
                if (pickableObject == null)
                {
                    pickableObject = hitObject.GetComponentInChildren<PickableObject>();
                }
                
                if (pickableObject != null)
                {
                    Debug.Log($"[OnInteract] Found PickableObject: {pickableObject.name}");
                    
                    // Check if hand is free
                    if (_righthandObject == null || _lefthandObject == null)
                    {
                        // Use right hand if free, otherwise left
                        bool useRightHand = _righthandObject == null;
                        if (PickupObjectToHand(pickableObject, useRightHand))
                        {
                            Debug.Log($"[OnInteract] Picked up {pickableObject.name} in {(useRightHand ? "right" : "left")} hand");
                            return;
                        }
                    }
                    else
                    {
                        Debug.Log($"[OnInteract] Both hands are full, cannot pick up {pickableObject.name}");
                    }
                }
                
                // Priority 2: If we have an object in hand, try to place it
                if (_righthandObject != null || _lefthandObject != null)
                {
                    bool useRightHand = _righthandObject != null;
                    Debug.Log($"[OnInteract] Placing object from {(useRightHand ? "right" : "left")} hand");
                    PlaceObjectFromHand(useRightHand, _lastLookRaycast);
                    return;
                }
                
                // Priority 3: Try to interact with Interactable (but skip FocusableInteractable if we want to pick up)
                Interactable interactable = hitObject.GetComponent<Interactable>();
                FocusableInteractable focusable = hitObject.GetComponent<FocusableInteractable>();
                
                // If no object in hand and there's an interactable, use it
                if (interactable != null)
                {
                    Debug.Log($"[OnInteract] Found Interactable on {hitObject.name}");
                    interactable.Interact(this, _lastLookRaycast.point);
                    return;
                }
            }
            else
            {
                // Drop object if nothing to interact with
                if (righthandObject != null)
                {
                    DropObjectFromHand(true);
                }
                else if (lefthandObject != null)
                {
                    DropObjectFromHand(false);
                }
            }
        }
    
        private void OnLeftClick(InputValue inputValue)
        {
            if (!inputValue.isPressed)
                return;
                
            // Handle pipe interaction mode - left click rotates pipes clockwise (right)
            if (_isInPipeMode)
            {
                Debug.Log("[OnLeftClick] In pipe mode, handling pipe click (clockwise)");
                HandlePipeClick(true);
                return;
            }
                
            // Normal mode - handle pickup/place
            if (_lastLookRaycast.collider != null)
            {
                GameObject hitObject = _lastLookRaycast.collider.gameObject;
                
                // Check for PickableObject on hit object, parent, or children
                PickableObject pickableObject = hitObject.GetComponent<PickableObject>();
                if (pickableObject == null && hitObject.transform.parent != null)
                {
                    pickableObject = hitObject.transform.parent.GetComponent<PickableObject>();
                }
                if (pickableObject == null)
                {
                    pickableObject = hitObject.GetComponentInChildren<PickableObject>();
                }
                
                // Priority: Pickup if hand is free, otherwise place
                if (pickableObject != null && righthandObject == null)
                {
                    PickupObjectToHand(pickableObject, true);
                }
                else if (righthandObject != null)
                {
                    PlaceObjectFromHand(true, _lastLookRaycast);
                }
            }
            else if (righthandObject != null)
            {
                DropObjectFromHand(true);
            }
        }
    
        private void OnRightClick(InputValue inputValue)
        {
            if (!inputValue.isPressed)
                return;
                
            // Handle pipe interaction mode - right click rotates pipes counter-clockwise (left)
            if (_isInPipeMode)
            {
                Debug.Log("[OnRightClick] In pipe mode, handling pipe click (counter-clockwise)");
                HandlePipeClick(false);
                return;
            }
                
            // Normal mode - handle pickup/place
            if (_lastLookRaycast.collider != null)
            {
                GameObject hitObject = _lastLookRaycast.collider.gameObject;
                
                // Check for PickableObject on hit object, parent, or children
                PickableObject pickableObject = hitObject.GetComponent<PickableObject>();
                if (pickableObject == null && hitObject.transform.parent != null)
                {
                    pickableObject = hitObject.transform.parent.GetComponent<PickableObject>();
                }
                if (pickableObject == null)
                {
                    pickableObject = hitObject.GetComponentInChildren<PickableObject>();
                }
                
                // Priority: Pickup if hand is free, otherwise place
                if (pickableObject != null && lefthandObject == null)
                {
                    PickupObjectToHand(pickableObject, false);
                }
                else if (lefthandObject != null)
                {
                    PlaceObjectFromHand(false, _lastLookRaycast);
                }
            }
            else if (lefthandObject != null)
            {
                DropObjectFromHand(false);
            }
        }
    
        /// <summary>
        /// Picks up an object and assigns it to a hand.
        /// </summary>
        public bool PickupObjectToHand(PickableObject pickableObject, bool isRighthand)
        {
            if (isRighthand && _righthandObject == null && rightHand != null)
            {
                _righthandObject = pickableObject;
                pickableObject.OnPick();
                _rightPickedLastPos = pickableObject.transform.position;
                _rightPickSmoothProgress = 0;
                return true;
            }
            if (!isRighthand && _lefthandObject == null && leftHand != null)
            {
                _lefthandObject = pickableObject;
                pickableObject.OnPick();
                _leftPickedLastPos = pickableObject.transform.position;
                _leftPickSmoothProgress = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Places an object from hand at the specified hit point.
        /// </summary>
        public void PlaceObjectFromHand(bool isRighthand, RaycastHit hit)
        {
            if (isRighthand && _righthandObject != null)
            {
                _righthandObject.OnPlace(hit.point);
                _righthandObject = null;
            }
            if (!isRighthand && _lefthandObject != null)
            {
                _lefthandObject.OnPlace(hit.point);
                _lefthandObject = null;
            }
        }
    
        /// <summary>
        /// Drops an object from hand.
        /// </summary>
        public void DropObjectFromHand(bool isRighthand)
        {
            if (isRighthand && _righthandObject != null)
            {
                if (headCamera != null && headCamera.IsLive)
                {
                    MyUtils.AddMove(_righthandObject.gameObject, headCamera.transform.forward * 3);
                }
                _righthandObject.OnDrop();
                _righthandObject = null;
            }
            if (!isRighthand && _lefthandObject != null)
            {
                if (headCamera != null && headCamera.IsLive)
                {
                    MyUtils.AddMove(_lefthandObject.gameObject, headCamera.transform.forward * 3);
                }
                _lefthandObject.OnDrop();
                _lefthandObject = null;
            }
        }
    
        private void HandlePipeClick(bool clockwise = true)
        {
            // Do a fresh raycast for pipe clicking (more reliable)
            Ray ray;
            
            if (_isInPipeMode)
            {
                // In pipe mode, use mouse position for raycast
                UnityEngine.Camera pipeCameraComponent = null;
                
                // Get Camera from CinemachineBrain (Cinemachine controls the main camera)
                CinemachineBrain cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();
                if (cinemachineBrain != null)
                {
                    pipeCameraComponent = cinemachineBrain.OutputCamera;
                }
                
                // Fallback: try to get Camera from main camera
                if (pipeCameraComponent == null)
                {
                    pipeCameraComponent = UnityEngine.Camera.main;
                }
                
                // Final fallback: find any camera
                if (pipeCameraComponent == null)
                {
                    pipeCameraComponent = FindFirstObjectByType<UnityEngine.Camera>();
                }
                
                if (pipeCameraComponent != null)
                {
                    // Use mouse position to create ray from camera (using new Input System)
                    Vector2 mousePos2D = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
                    Vector3 mousePosition = new Vector3(mousePos2D.x, mousePos2D.y, 0);
                    ray = pipeCameraComponent.ScreenPointToRay(mousePosition);
                }
                else
                {
                    Debug.LogWarning("[HandlePipeClick] No camera available for raycast!");
                    return;
                }
            }
            else
            {
                // Normal mode: use head camera forward direction
                if (headCamera == null)
                    return;
                    
                ray = new Ray(headCamera.transform.position, headCamera.transform.forward);
            }

            // Use wider layer mask for pipes
            int mask = interactionLayer.value;
            if (mask == 0)
            {
                mask = (1 << 6) | (1 << 0);
            }
            // In pipe mode, accept all layers except UI
            mask = ~(1 << 5);

            // Use longer distance in pipe mode
            float raycastDistance = _isInPipeMode ? interactionDistance * 10f : interactionDistance * 3f;
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastDistance, mask))
            {
                GameObject hitObject = hit.collider.gameObject;
                Debug.Log($"[HandlePipeClick] Hit object: {hitObject.name}, Layer: {hitObject.layer}");
                
                // Try to find PipeSegment directly (most reliable method)
                PipeSegment pipeSegment = null;
                
                // Check on hit object
                pipeSegment = hitObject.GetComponent<PipeSegment>();
                if (pipeSegment != null)
                {
                    Debug.Log($"[HandlePipeClick] Found PipeSegment on hit object: {hitObject.name}");
                }
                
                // Check parent
                if (pipeSegment == null && hitObject.transform.parent != null)
                {
                    pipeSegment = hitObject.transform.parent.GetComponent<PipeSegment>();
                    if (pipeSegment != null)
                    {
                        Debug.Log($"[HandlePipeClick] Found PipeSegment on parent: {hitObject.transform.parent.name}");
                    }
                }
                
                // Check children
                if (pipeSegment == null)
                {
                    pipeSegment = hitObject.GetComponentInChildren<PipeSegment>();
                    if (pipeSegment != null)
                    {
                        Debug.Log($"[HandlePipeClick] Found PipeSegment in children of: {hitObject.name}");
                    }
                }
                
                // Check root parent (in case pipe is nested deeper)
                if (pipeSegment == null && hitObject.transform.root != null && hitObject.transform.root != hitObject.transform)
                {
                    pipeSegment = hitObject.transform.root.GetComponentInChildren<PipeSegment>();
                    if (pipeSegment != null)
                    {
                        Debug.Log($"[HandlePipeClick] Found PipeSegment in root hierarchy: {hitObject.transform.root.name}");
                    }
                }
                
                // If we found a pipe segment, rotate it directly
                if (pipeSegment != null)
                {
                    Debug.Log($"[HandlePipeClick] Rotating PipeSegment: {pipeSegment.name}");
                    PipeGridSystem pipeSystem = FindFirstObjectByType<PipeGridSystem>();
                    if (pipeSystem != null)
                    {
                        if (pipeSystem.IsActive())
                        {
                            Debug.Log($"[HandlePipeClick] Calling OnPipeClicked on PipeGridSystem (clockwise: {clockwise})");
                            pipeSystem.OnPipeClicked(pipeSegment, clockwise);
                            return;
                        }
                        else
                        {
                            Debug.LogWarning("[HandlePipeClick] PipeGridSystem found but not active!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[HandlePipeClick] PipeGridSystem not found in scene!");
                    }
                }
                
                // Fallback: try PipeSegmentInteractable
                PipeSegmentInteractable pipeInteractable = hitObject.GetComponent<PipeSegmentInteractable>();
                if (pipeInteractable == null && hitObject.transform.parent != null)
                {
                    pipeInteractable = hitObject.transform.parent.GetComponent<PipeSegmentInteractable>();
                }
                if (pipeInteractable == null)
                {
                    pipeInteractable = hitObject.GetComponentInChildren<PipeSegmentInteractable>();
                }
                
                if (pipeInteractable != null)
                {
                    Debug.Log($"[HandlePipeClick] Found PipeSegmentInteractable on {pipeInteractable.name}");
                    pipeInteractable.Interact(this.gameObject);
                    return;
                }
                
                // Don't process FocusableInteractable in pipe mode - exit only via ESC key
                // This prevents accidental exit when clicking near the trigger
                
                Debug.LogWarning($"[HandlePipeClick] No pipe interactable found on {hitObject.name}. Object has components: {string.Join(", ", hitObject.GetComponents<Component>().Select(c => c.GetType().Name))}");
            }
            else
            {
                Debug.LogWarning($"[HandlePipeClick] Raycast did not hit anything. Ray: origin={ray.origin}, direction={ray.direction}, distance={raycastDistance}, mask={mask}");
            }
        }
    
        /// <summary>
        /// Sets pipe mode active/inactive state.
        /// </summary>
        public void SetPipeMode(bool active)
        {
            _isInPipeMode = active;
            
            // Ensure PlayerInput is enabled in pipe mode (needed for Input System callbacks)
            if (_input != null && active && !_input.enabled)
            {
                _input.enabled = true;
            }
            
            // Unlock cursor in pipe mode for mouse interaction
            if (active)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    
        /// <summary>
        /// Gets the camera holder transform.
        /// </summary>
        public Transform GetCameraHolder()
        {
            if (headCamera != null)
                return headCamera.transform;
            return transform;
        }

        private Vector3 CalculateMovementDirection()
        {
            // Block movement in pipe mode
            if (_isInPipeMode)
            {
                _localMovementAccelerationVector = Vector3.zero;
                return _localMovementAccelerationVector;
            }
            
            if (headCamera.IsLive)
            {
                _localMovementAccelerationVector = Vector3.Lerp(_localMovementAccelerationVector, transform.rotation * _movementDirection * (_sprintState ? sprintSpeed : speed), (IsGrounded ? 10 : 1) * Time.fixedDeltaTime);
            }
            else
            {
                _localMovementAccelerationVector = Vector3.zero;
            }

            return _localMovementAccelerationVector; 
        }
        
        private void CalculateVelocity(ref Vector3 velocity)
        {
            velocity = Vector3.Lerp(velocity, Physics.gravity, Time.fixedDeltaTime);
        }

        private void ResetCollisionData()
        {
            _controllerHitResetTimeout -= Time.fixedDeltaTime;
            
            if (_controllerHitResetTimeout < 0)
            {
                IsGrounded = false;
            }
        }
    }
}
