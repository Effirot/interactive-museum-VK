using UnityEngine;
using Unity.Cinemachine;
using InteractiveMuseum.Camera;
using InteractiveMuseum.Player;
using InteractiveMuseum.Interaction;
using InteractiveMuseum.PipeSystem;

namespace InteractiveMuseum.PipeSystem
{
    /// <summary>
    /// Automatically sets up the pipe system components in the scene.
    /// </summary>
    [System.Serializable]
    public class PipeSystemAutoSetup : MonoBehaviour
    {
        [Header("Auto Setup Settings")]
        [Tooltip("Automatically setup on Start")]
        [SerializeField]
        private bool _autoSetupOnStart = true;
        
        [Tooltip("Create pipe camera if missing")]
        [SerializeField]
        private bool _createCameraIfMissing = true;
        
        [Tooltip("Create grid system if missing")]
        [SerializeField]
        private bool _createGridSystemIfMissing = true;
        
        [Header("Grid Settings")]
        [Tooltip("Width of the pipe grid")]
        [SerializeField]
        private int _gridWidth = 3;
        
        [Tooltip("Height of the pipe grid")]
        [SerializeField]
        private int _gridHeight = 3;
        
        [Tooltip("Spacing between pipes")]
        [SerializeField]
        private float _pipeSpacing = 2f;
        
        [Tooltip("Position of the grid origin")]
        [SerializeField]
        private Vector3 _gridPosition = new Vector3(0, 0, 10);
        
        [Header("Puzzle Settings")]
        [Tooltip("Start position of the puzzle (grid coordinates)")]
        [SerializeField]
        private Vector2Int _startPosition = new Vector2Int(0, 0);
        
        [Tooltip("End position of the puzzle (grid coordinates)")]
        [SerializeField]
        private Vector2Int _endPosition = new Vector2Int(2, 2);
        
        [Tooltip("Randomize initial pipe rotations")]
        [SerializeField]
        private bool _randomizeInitialRotations = false;
        
        [Header("Camera Settings")]
        [Tooltip("Position for the pipe camera")]
        [SerializeField]
        private Vector3 _pipeCameraPosition = new Vector3(0, 5, -10);
        
        [Tooltip("Rotation for the pipe camera")]
        [SerializeField]
        private Vector3 _pipeCameraRotation = new Vector3(15, 0, 0);
        
        [Header("Interaction Trigger")]
        [Tooltip("Position of the interaction trigger")]
        [SerializeField]
        private Vector3 _triggerPosition = new Vector3(0, 1, 5);
        
        [Tooltip("Size of the interaction trigger")]
        [SerializeField]
        private Vector3 _triggerSize = new Vector3(3, 2, 3);
        
        private void Start()
        {
            if (_autoSetupOnStart)
            {
                SetupEverything();
            }
        }
        
        [ContextMenu("Setup Everything Now")]
        public void SetupEverything()
        {
            Debug.Log("Starting Pipe System Auto Setup...");
            
            // 1. Setup CameraManager
            SetupCameraManager();
            
            // 2. Setup Pipe Camera
            if (_createCameraIfMissing)
            {
                SetupPipeCamera();
            }
            
            // 3. Setup Pipe Grid System
            if (_createGridSystemIfMissing)
            {
                SetupPipeGridSystem();
            }
            
            // 4. Create pipe prefab if needed
            CreatePipePrefabsIfNeeded();
            
            // 5. Setup Interaction Trigger
            SetupInteractionTrigger();
            
            Debug.Log("Pipe System Auto Setup Complete!");
        }
        
        private void SetupCameraManager()
        {
            CameraManager manager = FindObjectOfType<CameraManager>();
            if (manager == null)
            {
                GameObject go = new GameObject("CameraManager");
                manager = go.AddComponent<CameraManager>();
                Debug.Log("✓ Created CameraManager");
            }
            
            // Find and assign player camera
            if (manager.playerCamera == null)
            {
                PlayerMovementController player = FindObjectOfType<PlayerMovementController>();
                if (player != null)
                {
                    CinemachineCamera playerCam = player.GetComponentInChildren<CinemachineCamera>();
                    if (playerCam != null)
                    {
                        manager.playerCamera = playerCam;
                        Debug.Log("✓ Assigned player camera to CameraManager");
                    }
                    else
                    {
                        // Try to find any CinemachineCamera in scene
                        CinemachineCamera[] cameras = FindObjectsOfType<CinemachineCamera>();
                        if (cameras.Length > 0)
                        {
                            manager.playerCamera = cameras[0];
                            Debug.Log("✓ Assigned first found camera to CameraManager");
                        }
                    }
                }
            }
            
            // Find and assign player
            if (manager.player == null)
            {
                PlayerMovementController player = FindObjectOfType<PlayerMovementController>();
                if (player != null)
                {
                    manager.player = player;
                    Debug.Log("✓ Assigned player to CameraManager");
                }
            }
        }
        
        private void SetupPipeCamera()
        {
            CameraManager manager = FindObjectOfType<CameraManager>();
            if (manager == null)
            {
                Debug.LogError("CameraManager not found! Setup it first.");
                return;
            }
            
            // Check if pipe camera already exists
            if (manager.miniGameCamera != null)
            {
                Debug.Log("✓ Pipe camera already assigned");
                return;
            }
            
            // Look for existing pipe camera
            GameObject pipeCameraObj = GameObject.Find("PipeCameraHolder");
            CinemachineCamera pipeCam = null;
            
            if (pipeCameraObj != null)
            {
                pipeCam = pipeCameraObj.GetComponent<CinemachineCamera>();
            }
            
            // Create if not found
            if (pipeCam == null)
            {
                pipeCameraObj = new GameObject("PipeCameraHolder");
                pipeCameraObj.transform.position = _pipeCameraPosition;
                pipeCameraObj.transform.rotation = Quaternion.Euler(_pipeCameraRotation);
                
                pipeCam = pipeCameraObj.AddComponent<CinemachineCamera>();
                pipeCam.Priority = 0;
                
                Debug.Log("✓ Created Pipe Camera");
            }
            
            manager.miniGameCamera = pipeCam;
            Debug.Log("✓ Assigned pipe camera to CameraManager");
        }
        
        private void SetupPipeGridSystem()
        {
            PipeGridSystem system = FindObjectOfType<PipeGridSystem>();
            if (system == null)
            {
                GameObject go = new GameObject("PipeGridSystem");
                system = go.AddComponent<PipeGridSystem>();
                Debug.Log("✓ Created PipeGridSystem");
            }
            
            // Configure grid settings
            system.gridWidth = _gridWidth;
            system.gridHeight = _gridHeight;
            system.pipeSpacing = _pipeSpacing;
            system.gridOrigin = _gridPosition;
            
            // Configure puzzle settings
            system.startPosition = _startPosition;
            system.endPosition = _endPosition;
            system.randomizeInitialRotations = _randomizeInitialRotations;
            
            // Try to find and assign pipe prefabs using reflection to access private fields
            #if UNITY_EDITOR
            System.Reflection.FieldInfo straightField = typeof(PipeGridSystem).GetField("_straightPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo cornerField = typeof(PipeGridSystem).GetField("_cornerPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo tJunctionField = typeof(PipeGridSystem).GetField("_tJunctionPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo crossField = typeof(PipeGridSystem).GetField("_crossPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Try to find prefabs
            GameObject straightPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe_Straight.prefab");
            GameObject cornerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe_Corner.prefab");
            GameObject tJunctionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe_TJunction.prefab");
            GameObject crossPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe_Cross.prefab");
            
            // Assign found prefabs
            if (straightField != null && straightPrefab != null && straightField.GetValue(system) == null)
            {
                straightField.SetValue(system, straightPrefab);
                Debug.Log("✓ Found and assigned straight pipe prefab");
            }
            
            if (cornerField != null && cornerPrefab != null && cornerField.GetValue(system) == null)
            {
                cornerField.SetValue(system, cornerPrefab);
                Debug.Log("✓ Found and assigned corner pipe prefab");
            }
            
            if (tJunctionField != null && tJunctionPrefab != null && tJunctionField.GetValue(system) == null)
            {
                tJunctionField.SetValue(system, tJunctionPrefab);
                Debug.Log("✓ Found and assigned T-junction pipe prefab");
            }
            
            if (crossField != null && crossPrefab != null && crossField.GetValue(system) == null)
            {
                crossField.SetValue(system, crossPrefab);
                Debug.Log("✓ Found and assigned cross pipe prefab");
            }
            
            // If no prefabs found, try Resources folder
            if (straightPrefab == null && cornerPrefab == null && tJunctionPrefab == null && crossPrefab == null)
            {
                GameObject resourcePrefab = Resources.Load<GameObject>("Pipe_Straight");
                if (resourcePrefab != null && straightField != null && straightField.GetValue(system) == null)
                {
                    straightField.SetValue(system, resourcePrefab);
                    Debug.Log("✓ Found and assigned straight pipe prefab from Resources");
                }
            }
            
            // Check if at least one prefab is assigned
            bool hasAnyPrefab = (straightField != null && straightField.GetValue(system) != null) ||
                               (cornerField != null && cornerField.GetValue(system) != null) ||
                               (tJunctionField != null && tJunctionField.GetValue(system) != null) ||
                               (crossField != null && crossField.GetValue(system) != null);
            
            if (!hasAnyPrefab)
            {
                Debug.LogWarning("⚠ No pipe prefabs found. Please create them using Tools/Pipe System/Create Pipe Prefab");
            }
            #else
            // Runtime: try Resources folder
            GameObject resourcePrefab = Resources.Load<GameObject>("Pipe_Straight");
            if (resourcePrefab != null)
            {
                Debug.Log("✓ Found pipe prefab in Resources (assign manually in Inspector for full control)");
            }
            else
            {
                Debug.LogWarning("⚠ Pipe prefabs not found in Resources. Please assign them manually in Inspector.");
            }
            #endif
            
            Debug.Log("✓ Configured PipeGridSystem");
        }
        
        private void CreatePipePrefabsIfNeeded()
        {
            // This would be called from Editor script, but we can check if prefabs exist
            #if UNITY_EDITOR
            string prefabPath = "Assets/Prefabs/Pipe_Straight.prefab";
            if (!System.IO.File.Exists(prefabPath))
            {
                Debug.Log("⚠ Pipe prefabs not found. Use Tools/Pipe System/Create Pipe Prefab to create them.");
            }
            #endif
        }
        
        private void SetupInteractionTrigger()
        {
            // Look for existing trigger
            GameObject trigger = GameObject.Find("PipeInteractionTrigger");
            FocusableInteractable focusable = null;
            
            if (trigger != null)
            {
                focusable = trigger.GetComponent<FocusableInteractable>();
            }
            
            // Create if not found
            if (focusable == null)
            {
                if (trigger == null)
                {
                    trigger = new GameObject("PipeInteractionTrigger");
                }
                
                trigger.transform.position = _triggerPosition;
                
                // Add collider
                BoxCollider collider = trigger.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = trigger.AddComponent<BoxCollider>();
                }
                collider.size = _triggerSize;
                collider.isTrigger = true;
                
                // Add FocusableInteractable
                focusable = trigger.GetComponent<FocusableInteractable>();
                if (focusable == null)
                {
                    focusable = trigger.AddComponent<FocusableInteractable>();
                }
                
                // Add Interactable component
                Interactable interactable = trigger.GetComponent<Interactable>();
                if (interactable == null)
                {
                    interactable = trigger.AddComponent<Interactable>();
                }
                interactable.onLookText = "Interact with pipes";
                
                // Add InteractableOutline for highlighting
                InteractableOutline outline = trigger.GetComponent<InteractableOutline>();
                if (outline == null)
                {
                    outline = trigger.AddComponent<InteractableOutline>();
                }
                
                // Link references
                CameraManager manager = FindObjectOfType<CameraManager>();
                PipeGridSystem system = FindObjectOfType<PipeGridSystem>();
                
                if (manager != null)
                {
                    focusable.targetCamera = manager.miniGameCamera;
                    focusable.playerCamera = manager.playerCamera;
                }
                
                if (system != null)
                {
                    focusable.pipeSystem = system;
                }
                
                Debug.Log("✓ Created and configured PipeInteractionTrigger");
            }
            else
            {
                Debug.Log("✓ PipeInteractionTrigger already exists");
            }
        }
    }
}
