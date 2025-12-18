using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;
using InteractiveMuseum.Camera;
using InteractiveMuseum.PipeSystem;
using InteractiveMuseum.Player;
using InteractiveMuseum.Interaction;

public class PipeSystemSetup : EditorWindow
{
    [MenuItem("Tools/Pipe System/Setup Scene")]
    static void Init()
    {
        PipeSystemSetup window = (PipeSystemSetup)EditorWindow.GetWindow(typeof(PipeSystemSetup));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Pipe System Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup Camera Manager"))
        {
            SetupCameraManager();
        }
        
        if (GUILayout.Button("Setup Pipe Grid System"))
        {
            SetupPipeGridSystem();
        }
        
        if (GUILayout.Button("Create Pipe Camera"))
        {
            CreatePipeCamera();
        }
        
        if (GUILayout.Button("Setup Complete Scene"))
        {
            SetupCompleteScene();
        }
    }
    
    static void SetupCameraManager()
    {
        CameraManager manager = FindObjectOfType<CameraManager>();
        if (manager == null)
        {
            GameObject go = new GameObject("CameraManager");
            manager = go.AddComponent<CameraManager>();
            Debug.Log("Created CameraManager");
        }
        
        // Try to find player camera
        if (manager.playerCamera == null)
        {
            PlayerMovementController player = FindObjectOfType<PlayerMovementController>();
            if (player != null)
            {
                CinemachineCamera playerCam = player.GetComponentInChildren<CinemachineCamera>();
                if (playerCam != null)
                {
                    manager.playerCamera = playerCam;
                    Debug.Log("Assigned player camera");
                }
            }
        }
        
        // Try to find player
        if (manager.player == null)
        {
            PlayerMovementController player = FindObjectOfType<PlayerMovementController>();
            if (player != null)
            {
                manager.player = player;
                Debug.Log("Assigned player");
            }
        }
        
        EditorUtility.SetDirty(manager);
    }
    
    static void SetupPipeGridSystem()
    {
        PipeGridSystem system = FindObjectOfType<PipeGridSystem>();
        if (system == null)
        {
            GameObject go = new GameObject("PipeGridSystem");
            system = go.AddComponent<PipeGridSystem>();
            Debug.Log("Created PipeGridSystem");
        }
        
        EditorUtility.SetDirty(system);
    }
    
    static void CreatePipeCamera()
    {
        GameObject pipeCameraObj = GameObject.Find("PipeCameraHolder");
        if (pipeCameraObj == null)
        {
            pipeCameraObj = new GameObject("PipeCameraHolder");
            pipeCameraObj.transform.position = new Vector3(0, 5, -10);
            pipeCameraObj.transform.rotation = Quaternion.Euler(15, 0, 0);
            
            CinemachineCamera cam = pipeCameraObj.AddComponent<CinemachineCamera>();
            cam.Priority = 0;
            
            Debug.Log("Created Pipe Camera at position: " + pipeCameraObj.transform.position);
        }
        
        // Assign to CameraManager if exists
        CameraManager manager = FindObjectOfType<CameraManager>();
        if (manager != null && manager.pipeCamera == null)
        {
            manager.pipeCamera = pipeCameraObj.GetComponent<CinemachineCamera>();
            EditorUtility.SetDirty(manager);
        }
    }
    
    static void SetupCompleteScene()
    {
        Debug.Log("=== Starting Complete Pipe System Setup ===");
        
        // 1. Setup CameraManager
        SetupCameraManager();
        
        // 2. Setup Pipe Camera
        CreatePipeCamera();
        
        // 3. Create pipe prefab if needed
        CreatePipePrefabIfNeeded();
        
        // 4. Setup Pipe Grid System
        SetupPipeGridSystem();
        
        // 5. Link all references
        LinkAllReferences();
        
        // 6. Create interaction trigger
        CreateInteractionTrigger();
        
        Debug.Log("=== Complete Setup Finished! ===");
        Debug.Log("All components are set up and linked.");
        Debug.Log("You can adjust positions and settings in the inspector.");
    }
    
    static void CreatePipePrefabIfNeeded()
    {
        string prefabPath = "Assets/Prefabs/Pipe_Straight.prefab";
        
        if (!System.IO.File.Exists(prefabPath))
        {
            Debug.Log("Creating pipe prefab...");
            
            // Create base GameObject
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "Pipe_Straight";
            pipe.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
            
            // Add PipeSegment component
            PipeSegment segment = pipe.AddComponent<PipeSegment>();
            segment.pipeType = PipeType.Straight;
            
            // Add PipeSegmentInteractable
            pipe.AddComponent<PipeSegmentInteractable>();
            
            // Configure collider
            Collider col = pipe.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = false;
            }
            
            // Ensure Prefabs directory exists
            string prefabDir = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(prefabDir))
            {
                System.IO.Directory.CreateDirectory(prefabDir);
                AssetDatabase.Refresh();
            }
            
            // Create prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(pipe, prefabPath);
            DestroyImmediate(pipe);
            
            Debug.Log("✓ Created pipe prefab at: " + prefabPath);
        }
        else
        {
            Debug.Log("✓ Pipe prefab already exists");
        }
    }
    
    static void LinkAllReferences()
    {
        CameraManager manager = FindObjectOfType<CameraManager>();
        PipeGridSystem system = FindObjectOfType<PipeGridSystem>();
        GameObject pipeCam = GameObject.Find("PipeCameraHolder");
        
        if (manager != null)
        {
            // Link pipe camera
            if (pipeCam != null && manager.pipeCamera == null)
            {
                manager.pipeCamera = pipeCam.GetComponent<CinemachineCamera>();
                EditorUtility.SetDirty(manager);
                Debug.Log("✓ Linked pipe camera to CameraManager");
            }
            
            // Link player
            if (manager.player == null)
            {
                PlayerMovementController player = FindObjectOfType<PlayerMovementController>();
                if (player != null)
                {
                    manager.player = player;
                    EditorUtility.SetDirty(manager);
                    Debug.Log("✓ Linked player to CameraManager");
                }
            }
            
            // Link player camera
            if (manager.playerCamera == null)
            {
                PlayerMovementController player = FindObjectOfType<PlayerMovementController>();
                if (player != null)
                {
                    CinemachineCamera playerCam = player.GetComponentInChildren<CinemachineCamera>();
                    if (playerCam != null)
                    {
                        manager.playerCamera = playerCam;
                        EditorUtility.SetDirty(manager);
                        Debug.Log("✓ Linked player camera to CameraManager");
                    }
                }
            }
        }
        
        // Assign prefabs to grid system using reflection
        if (system != null)
        {
            System.Reflection.FieldInfo straightField = typeof(PipeGridSystem).GetField("_straightPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo cornerField = typeof(PipeGridSystem).GetField("_cornerPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo tJunctionField = typeof(PipeGridSystem).GetField("_tJunctionPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo crossField = typeof(PipeGridSystem).GetField("_crossPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            bool assignedAny = false;
            
            // Try to assign straight prefab
            if (straightField != null && straightField.GetValue(system) == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe_Straight.prefab");
                if (prefab != null)
                {
                    straightField.SetValue(system, prefab);
                    assignedAny = true;
                }
            }
            
            // Try to assign corner prefab
            if (cornerField != null && cornerField.GetValue(system) == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe_Corner.prefab");
                if (prefab != null)
                {
                    cornerField.SetValue(system, prefab);
                    assignedAny = true;
                }
            }
            
            // Try to assign T-junction prefab
            if (tJunctionField != null && tJunctionField.GetValue(system) == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe_TJunction.prefab");
                if (prefab != null)
                {
                    tJunctionField.SetValue(system, prefab);
                    assignedAny = true;
                }
            }
            
            // Try to assign cross prefab
            if (crossField != null && crossField.GetValue(system) == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pipe_Cross.prefab");
                if (prefab != null)
                {
                    crossField.SetValue(system, prefab);
                    assignedAny = true;
                }
            }
            
            if (assignedAny)
            {
                EditorUtility.SetDirty(system);
                Debug.Log("✓ Assigned pipe prefabs to PipeGridSystem");
            }
        }
        
        // Configure grid system
        if (system != null)
        {
            system.gridWidth = 3;
            system.gridHeight = 3;
            system.pipeSpacing = 2f;
            system.gridOrigin = new Vector3(0, 0, 10);
            
            // Configure puzzle settings (start at top-left, end at bottom-right)
            system.startPosition = new Vector2Int(0, 0);
            system.endPosition = new Vector2Int(2, 2);
            system.randomizeInitialRotations = false;
            
            EditorUtility.SetDirty(system);
            Debug.Log("✓ Configured PipeGridSystem default values");
        }
    }
    
    static void CreateInteractionTrigger()
    {
        GameObject trigger = GameObject.Find("PipeInteractionTrigger");
        
        if (trigger == null)
        {
            trigger = new GameObject("PipeInteractionTrigger");
            trigger.transform.position = new Vector3(0, 1, 5);
            
            BoxCollider collider = trigger.AddComponent<BoxCollider>();
            collider.size = new Vector3(3, 2, 3);
            collider.isTrigger = true;
            
            FocusableInteractable focusable = trigger.AddComponent<FocusableInteractable>();
            Interactable interactable = trigger.AddComponent<Interactable>();
            interactable.onLookText = "Interact with pipes";
            
            // Link references
            CameraManager manager = FindObjectOfType<CameraManager>();
            PipeGridSystem system = FindObjectOfType<PipeGridSystem>();
            GameObject pipeCam = GameObject.Find("PipeCameraHolder");
            
            if (manager != null)
            {
                if (pipeCam != null)
                {
                    focusable.targetCamera = pipeCam.GetComponent<CinemachineCamera>();
                }
                focusable.playerCamera = manager.playerCamera;
            }
            
            if (system != null)
            {
                focusable.pipeSystem = system;
            }
            
            EditorUtility.SetDirty(trigger);
            Debug.Log("✓ Created PipeInteractionTrigger");
        }
        else
        {
            Debug.Log("✓ PipeInteractionTrigger already exists");
        }
    }
}
