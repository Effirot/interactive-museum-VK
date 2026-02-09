using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;
using InteractiveMuseum.Camera;
using InteractiveMuseum.PipeSystem;
using InteractiveMuseum.Player;
using InteractiveMuseum.Interaction;

public class CheckPipeSystemSetup : EditorWindow
{
    [MenuItem("Tools/Pipe System/Check Setup")]
    static void Init()
    {
        CheckPipeSystemSetup window = (CheckPipeSystemSetup)EditorWindow.GetWindow(typeof(CheckPipeSystemSetup));
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Pipe System Setup Checker", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Check All Settings"))
        {
            CheckAllSettings();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("This will check:", EditorStyles.boldLabel);
        GUILayout.Label("• CameraManager setup");
        GUILayout.Label("• PlayerMovementController");
        GUILayout.Label("• PipeGridSystem");
        GUILayout.Label("• FocusableInteractable trigger");
        GUILayout.Label("• Camera references");
        GUILayout.Label("• Layer masks");
    }
    
    static void CheckAllSettings()
    {
        bool allGood = true;
        
        // Check CameraManager
        CameraManager manager = FindObjectOfType<CameraManager>();
        if (manager == null)
        {
            Debug.LogError("❌ CameraManager not found in scene!");
            allGood = false;
        }
        else
        {
            Debug.Log("✓ CameraManager found");
            
            if (manager.playerCamera == null)
            {
                Debug.LogError("❌ CameraManager: Player Camera not assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ CameraManager: Player Camera assigned");
            }
            
            if (manager.miniGameCamera == null)
            {
                Debug.LogError("❌ CameraManager: Pipe Camera not assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ CameraManager: Pipe Camera assigned");
            }
            
            if (manager.player == null)
            {
                Debug.LogError("❌ CameraManager: Player not assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ CameraManager: Player assigned");
            }
        }
        
        // Check PlayerMovementController
        PlayerMovementController player = FindObjectOfType<PlayerMovementController>();
        if (player == null)
        {
            Debug.LogError("❌ PlayerMovementController not found in scene!");
            allGood = false;
        }
        else
        {
            Debug.Log("✓ PlayerMovementController found");
            
            var headCamera = player.GetComponentInChildren<CinemachineCamera>();
            if (headCamera == null)
            {
                Debug.LogError("❌ PlayerMovementController: Head Camera (CinemachineCamera) not found!");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ PlayerMovementController: Head Camera found");
            }
            
            SerializedObject so = new SerializedObject(player);
            var rightHand = so.FindProperty("rightHand");
            var leftHand = so.FindProperty("leftHand");
            
            if (rightHand.objectReferenceValue == null)
            {
                Debug.LogWarning("⚠ PlayerMovementController: Right Hand not assigned (optional)");
            }
            else
            {
                Debug.Log("✓ PlayerMovementController: Right Hand assigned");
            }
            
            if (leftHand.objectReferenceValue == null)
            {
                Debug.LogWarning("⚠ PlayerMovementController: Left Hand not assigned (optional)");
            }
            else
            {
                Debug.Log("✓ PlayerMovementController: Left Hand assigned");
            }
        }
        
        // Check PipeGridSystem
        PipeGridSystem system = FindObjectOfType<PipeGridSystem>();
        if (system == null)
        {
            Debug.LogWarning("⚠ PipeGridSystem not found (optional if using manual setup)");
        }
        else
        {
            Debug.Log("✓ PipeGridSystem found");
            
            // Check if any pipe prefabs are assigned
            System.Reflection.FieldInfo straightField = typeof(PipeGridSystem).GetField("_straightPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo cornerField = typeof(PipeGridSystem).GetField("_cornerPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo tJunctionField = typeof(PipeGridSystem).GetField("_tJunctionPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo crossField = typeof(PipeGridSystem).GetField("_crossPipePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            bool hasAnyPrefab = (straightField != null && straightField.GetValue(system) != null) ||
                               (cornerField != null && cornerField.GetValue(system) != null) ||
                               (tJunctionField != null && tJunctionField.GetValue(system) != null) ||
                               (crossField != null && crossField.GetValue(system) != null);
            
            if (!hasAnyPrefab)
            {
                Debug.LogWarning("⚠ PipeGridSystem: No pipe prefabs assigned");
            }
            else
            {
                int prefabCount = 0;
                if (straightField != null && straightField.GetValue(system) != null) prefabCount++;
                if (cornerField != null && cornerField.GetValue(system) != null) prefabCount++;
                if (tJunctionField != null && tJunctionField.GetValue(system) != null) prefabCount++;
                if (crossField != null && crossField.GetValue(system) != null) prefabCount++;
                Debug.Log($"✓ PipeGridSystem: {prefabCount} pipe prefab(s) assigned");
            }
        }
        
        // Check FocusableInteractable trigger
        FocusableInteractable focusable = FindObjectOfType<FocusableInteractable>();
        if (focusable == null)
        {
            Debug.LogError("❌ FocusableInteractable trigger not found in scene!");
            allGood = false;
        }
        else
        {
            Debug.Log("✓ FocusableInteractable found");
            
            if (focusable.targetCamera == null)
            {
                Debug.LogError("❌ FocusableInteractable: Target Camera not assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ FocusableInteractable: Target Camera assigned");
            }
            
            if (focusable.pipeSystem == null)
            {
                Debug.LogError("❌ FocusableInteractable: Pipe System not assigned!");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ FocusableInteractable: Pipe System assigned");
            }
            
            // Check collider
            Collider col = focusable.GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogError("❌ FocusableInteractable: No Collider component found!");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ FocusableInteractable: Collider found");
                
                if (!col.isTrigger)
                {
                    Debug.LogWarning("⚠ FocusableInteractable: Collider should be a Trigger!");
                }
                else
                {
                    Debug.Log("✓ FocusableInteractable: Collider is a Trigger");
                }
                
                // Check layer
                int layer = focusable.gameObject.layer;
                int defaultLayer = 0;
                int pickableLayer = 6;
                
                if (layer != defaultLayer && layer != pickableLayer)
                {
                    Debug.LogWarning($"⚠ FocusableInteractable: Object is on layer {layer}. Make sure PlayerMovementController's interactionLayer includes this layer!");
                }
                else
                {
                    Debug.Log($"✓ FocusableInteractable: Object is on layer {layer}");
                }
            }
            
            // Check Interactable component
            Interactable interactable = focusable.GetComponent<Interactable>();
            if (interactable == null)
            {
                Debug.LogError("❌ FocusableInteractable: Interactable component missing!");
                allGood = false;
            }
            else
            {
                Debug.Log("✓ FocusableInteractable: Interactable component found");
            }
        }
        
        if (allGood)
        {
            Debug.Log("=== All checks passed! ===");
        }
        else
        {
            Debug.LogError("=== Some issues found. Please fix them above. ===");
        }
    }
}
