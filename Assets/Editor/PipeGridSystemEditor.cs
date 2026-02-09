using UnityEngine;
using UnityEditor;
using InteractiveMuseum.PipeSystem;

[CustomEditor(typeof(PipeGridSystem))]
[CanEditMultipleObjects]
public class PipeGridSystemEditor : Editor
{
    private SerializedProperty _enableEntryExit;
    private SerializedProperty _entrySide;
    private SerializedProperty _exitSide;
    
    private void OnEnable()
    {
        _enableEntryExit = serializedObject.FindProperty("_enableEntryExit");
        _entrySide = serializedObject.FindProperty("_entrySide");
        _exitSide = serializedObject.FindProperty("_exitSide");
    }
    
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        serializedObject.Update();
        
        PipeGridSystem pipeSystem = (PipeGridSystem)target;
        
        // Entry/Exit Configuration Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Entry/Exit Configuration", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(_enableEntryExit, new GUIContent("Enable Entry/Exit Pipes"));
        
        if (_enableEntryExit.boolValue)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(_entrySide, new GUIContent("Entry Side"));
            EditorGUILayout.PropertyField(_exitSide, new GUIContent("Exit Side"));
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Button to generate entry/exit pipes
            if (GUILayout.Button("Generate Entry/Exit Pipes", GUILayout.Height(30)))
            {
                if (pipeSystem != null)
                {
                    Undo.RecordObject(pipeSystem, "Generate Entry/Exit Pipes");
                    pipeSystem.SpawnEntryPipe();
                    pipeSystem.SpawnExitPipe();
                    EditorUtility.SetDirty(pipeSystem);
                    SceneView.RepaintAll();
                }
            }
            
            // Button to remove entry/exit pipes
            if (GUILayout.Button("Remove Entry/Exit Pipes", GUILayout.Height(30)))
            {
                if (pipeSystem != null)
                {
                    Undo.RecordObject(pipeSystem, "Remove Entry/Exit Pipes");
                    pipeSystem.RemoveEntryExitPipes();
                    EditorUtility.SetDirty(pipeSystem);
                    SceneView.RepaintAll();
                }
            }
            
            // Show status information
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            // Check if entry/exit pipes exist using reflection
            var entryPipeField = typeof(PipeGridSystem).GetField("_entryPipe", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var exitPipeField = typeof(PipeGridSystem).GetField("_exitPipe", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            bool entryExists = false;
            bool exitExists = false;
            
            if (entryPipeField != null)
            {
                var entryPipe = entryPipeField.GetValue(pipeSystem) as PipeSegment;
                entryExists = entryPipe != null;
            }
            
            if (exitPipeField != null)
            {
                var exitPipe = exitPipeField.GetValue(pipeSystem) as PipeSegment;
                exitExists = exitPipe != null;
            }
            
            EditorGUILayout.LabelField("Entry Pipe:", entryExists ? "Spawned" : "Not spawned");
            EditorGUILayout.LabelField("Exit Pipe:", exitExists ? "Spawned" : "Not spawned");
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void OnSceneGUI()
    {
        PipeGridSystem pipeSystem = (PipeGridSystem)target;
        
        if (pipeSystem == null || !pipeSystem.enabled)
            return;
        
        // Draw gizmos for entry/exit positions
        if (pipeSystem.enableEntryExit)
        {
            DrawEntryExitGizmos(pipeSystem);
        }
    }
    
    private void DrawEntryExitGizmos(PipeGridSystem pipeSystem)
    {
        // Get values using properties and reflection for private fields
        EntryExitSide entrySide = pipeSystem.entrySide;
        EntryExitSide exitSide = pipeSystem.exitSide;
        Vector2Int startPos = pipeSystem.startPosition;
        Vector2Int endPos = pipeSystem.endPosition;
        int gridWidth = pipeSystem.gridWidth;
        int gridHeight = pipeSystem.gridHeight;
        float pipeSpacing = pipeSystem.pipeSpacing;
        Vector3 gridOrigin = pipeSystem.gridOrigin;
        
        // Calculate entry position
        Vector3 entryWorldPos = GetEntryExitWorldPosition(gridOrigin, gridWidth, gridHeight, pipeSpacing, entrySide, startPos.x, startPos.y);
        
        // Calculate exit position
        Vector3 exitWorldPos = GetEntryExitWorldPosition(gridOrigin, gridWidth, gridHeight, pipeSpacing, exitSide, endPos.x, endPos.y);
        
        // Draw entry pipe gizmo
        Handles.color = Color.green;
        Handles.DrawWireCube(entryWorldPos, Vector3.one * pipeSpacing * 0.8f);
        Handles.Label(entryWorldPos + Vector3.up * 0.5f, "Entry");
        
        // Draw exit pipe gizmo
        Handles.color = Color.red;
        Handles.DrawWireCube(exitWorldPos, Vector3.one * pipeSpacing * 0.8f);
        Handles.Label(exitWorldPos + Vector3.up * 0.5f, "Exit");
        
        // Draw lines connecting to grid
        Handles.color = Color.yellow;
        Vector3 startWorldPos = gridOrigin + new Vector3(
            startPos.x * pipeSpacing - (gridWidth - 1) * pipeSpacing * 0.5f,
            startPos.y * pipeSpacing - (gridHeight - 1) * pipeSpacing * 0.5f,
            0
        );
        Vector3 endWorldPos = gridOrigin + new Vector3(
            endPos.x * pipeSpacing - (gridWidth - 1) * pipeSpacing * 0.5f,
            endPos.y * pipeSpacing - (gridHeight - 1) * pipeSpacing * 0.5f,
            0
        );
        
        Handles.DrawDottedLine(entryWorldPos, startWorldPos, 3f);
        Handles.DrawDottedLine(exitWorldPos, endWorldPos, 3f);
    }
    
    private Vector3 GetEntryExitWorldPosition(Vector3 gridOrigin, int gridWidth, int gridHeight, float pipeSpacing, EntryExitSide side, int gridX, int gridY)
    {
        Vector3 gridPos = gridOrigin + new Vector3(
            gridX * pipeSpacing - (gridWidth - 1) * pipeSpacing * 0.5f,
            gridY * pipeSpacing - (gridHeight - 1) * pipeSpacing * 0.5f,
            0
        );
        
        float offset = pipeSpacing;
        
        switch (side)
        {
            case EntryExitSide.North:
                return gridPos + new Vector3(0, offset, 0);
            case EntryExitSide.South:
                return gridPos + new Vector3(0, -offset, 0);
            case EntryExitSide.East:
                return gridPos + new Vector3(offset, 0, 0);
            case EntryExitSide.West:
                return gridPos + new Vector3(-offset, 0, 0);
            default:
                return gridPos;
        }
    }
}
