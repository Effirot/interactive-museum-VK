using UnityEngine;
using UnityEditor;
using InteractiveMuseum.PipeSystem;

public class PipePrefabCreator : EditorWindow
{
    [MenuItem("Tools/Pipe System/Create Pipe Prefab")]
    static void Init()
    {
        PipePrefabCreator window = (PipePrefabCreator)EditorWindow.GetWindow(typeof(PipePrefabCreator));
        window.Show();
    }
    
    PipeType selectedType = PipeType.Straight;
    
    void OnGUI()
    {
        GUILayout.Label("Create Pipe Prefab", EditorStyles.boldLabel);
        
        selectedType = (PipeType)EditorGUILayout.EnumPopup("Pipe Type:", selectedType);
        
        if (GUILayout.Button("Create Straight Pipe Prefab"))
        {
            CreatePipePrefab(PipeType.Straight);
        }
        
        if (GUILayout.Button("Create Corner Pipe Prefab"))
        {
            CreatePipePrefab(PipeType.Corner);
        }
        
        if (GUILayout.Button("Create T-Junction Pipe Prefab"))
        {
            CreatePipePrefab(PipeType.TJunction);
        }
        
        if (GUILayout.Button("Create Cross Pipe Prefab"))
        {
            CreatePipePrefab(PipeType.Cross);
        }
    }
    
    static void CreatePipePrefab(PipeType pipeType)
    {
        // Create base GameObject
        GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pipe.name = $"Pipe_{pipeType}";
        pipe.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        
        // Add PipeSegment component
        PipeSegment segment = pipe.AddComponent<PipeSegment>();
        segment.pipeType = pipeType;
        
        // Add PipeSegmentInteractable
        pipe.AddComponent<PipeSegmentInteractable>();
        
        // Configure collider
        Collider col = pipe.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }
        
        // Create prefab
        string prefabPath = $"Assets/Prefabs/Pipe_{pipeType}.prefab";
        string prefabDir = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(prefabDir))
        {
            System.IO.Directory.CreateDirectory(prefabDir);
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(pipe, prefabPath);
        DestroyImmediate(pipe);
        
        Debug.Log($"Created prefab: {prefabPath}");
        
        // Select the prefab
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
}
