using UnityEngine;
using UnityEditor;
using InteractiveMuseum.Player;

public class SetupPlayerHands : EditorWindow
{
    [MenuItem("Tools/Player/Setup Hands")]
    static void Init()
    {
        SetupPlayerHands window = (SetupPlayerHands)EditorWindow.GetWindow(typeof(SetupPlayerHands));
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Setup Player Hands", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Hands for PlayerMovementController"))
        {
            CreateHands();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Select PlayerMovementController in hierarchy");
        GUILayout.Label("2. Click 'Create Hands' button");
        GUILayout.Label("3. Hands will be created as child objects");
        GUILayout.Label("4. Adjust positions in scene view");
    }
    
    static void CreateHands()
    {
        PlayerMovementController player = Selection.activeGameObject?.GetComponent<PlayerMovementController>();
        
        if (player == null)
        {
            player = FindObjectOfType<PlayerMovementController>();
        }
        
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "PlayerMovementController not found!\n\nPlease select a GameObject with PlayerMovementController component in the hierarchy, or make sure there's one in the scene.", "OK");
            return;
        }
        
        // Create Right Hand
        Transform rightHand = player.transform.Find("RightHand");
        if (rightHand == null)
        {
            GameObject rightHandObj = new GameObject("RightHand");
            rightHandObj.transform.SetParent(player.transform);
            rightHandObj.transform.localPosition = new Vector3(0.3f, 0.5f, 0.5f); // Typical right hand position
            rightHandObj.transform.localRotation = Quaternion.identity;
            rightHand = rightHandObj.transform;
            Undo.RegisterCreatedObjectUndo(rightHandObj, "Create Right Hand");
        }
        
        // Create Left Hand
        Transform leftHand = player.transform.Find("LeftHand");
        if (leftHand == null)
        {
            GameObject leftHandObj = new GameObject("LeftHand");
            leftHandObj.transform.SetParent(player.transform);
            leftHandObj.transform.localPosition = new Vector3(-0.3f, 0.5f, 0.5f); // Typical left hand position
            leftHandObj.transform.localRotation = Quaternion.identity;
            leftHand = leftHandObj.transform;
            Undo.RegisterCreatedObjectUndo(leftHandObj, "Create Left Hand");
        }
        
        // Assign to PlayerMovementController
        SerializedObject serializedPlayer = new SerializedObject(player);
        SerializedProperty rightHandProp = serializedPlayer.FindProperty("rightHand");
        SerializedProperty leftHandProp = serializedPlayer.FindProperty("leftHand");
        
        if (rightHandProp != null)
        {
            rightHandProp.objectReferenceValue = rightHand;
        }
        if (leftHandProp != null)
        {
            leftHandProp.objectReferenceValue = leftHand;
        }
        
        serializedPlayer.ApplyModifiedProperties();
        
        // Select the player to show the changes
        Selection.activeGameObject = player.gameObject;
        
        EditorUtility.DisplayDialog("Success", 
            "Hands created successfully!\n\n" +
            "Right Hand: " + rightHand.name + "\n" +
            "Left Hand: " + leftHand.name + "\n\n" +
            "You can now adjust their positions in the scene view.", "OK");
            
        Debug.Log("✓ Created and assigned hands to PlayerMovementController");
    }
    
    [MenuItem("Tools/Player/Setup Hands", true)]
    static bool ValidateCreateHands()
    {
        return true;
    }
}
