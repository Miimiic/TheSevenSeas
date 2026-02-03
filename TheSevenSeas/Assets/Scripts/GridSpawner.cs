using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridSpawner : MonoBehaviour
{
    [Header("Light Settings")]
    public GameObject lightPrefab;
    public float lightSpacing = 10f;
    public int lightRange = 740;
    public float lightHeight = 10f;
    
    [Header("Wall Settings")]
    public GameObject wallPrefab;
    public float wallMinSpacing = 5f;
    public float wallMaxSpacing = 15f;
    public int wallRange = 740;
    public float wallHeight = 0f;
    
    [Header("Parenting")]
    public Transform lightParent;
    public Transform wallParent;
    
    [Header("Distance Culling Integration")]
    [Tooltip("Automatically assign spawned objects to this layer for distance culling")]
    public bool assignCullingLayer = true;
    
    [Tooltip("The layer to assign objects to (should match DistanceBasedRenderer culling layer)")]
    public string cullingLayerName = "DistanceCulled";
    
    [Tooltip("Reference to the DistanceBasedRenderer to notify when spawning completes")]
    public DistanceBasedRenderer distanceRenderer;
    
    [Header("Streaming Settings")]
    [Tooltip("Start with renderers and lights disabled for streaming")]
    public bool spawnDisabled = true;
    
    private int cullingLayer = -1;
    
    void Start()
    {
        // Cache the layer index
        if (assignCullingLayer)
        {
            cullingLayer = LayerMask.NameToLayer(cullingLayerName);
            if (cullingLayer == -1)
            {
                Debug.LogWarning($"Layer '{cullingLayerName}' not found! Objects won't be assigned to culling layer.");
            }
        }
    }
    
    // ---------------- LIGHTS ----------------
    public void SpawnLights()
    {
        if (lightPrefab == null)
        {
            Debug.LogError("No light prefab assigned!");
            return;
        }
        
        if (lightParent == null)
            lightParent = transform;
        
        int count = 0;
        for (float x = -lightRange; x <= lightRange; x += lightSpacing)
        {
            for (float z = -lightRange; z <= lightRange; z += lightSpacing)
            {
                Vector3 pos = new Vector3(x, lightHeight, z);
                GameObject spawned = Instantiate(lightPrefab, pos, Quaternion.identity, lightParent);
                
                // Assign to culling layer
                if (assignCullingLayer && cullingLayer != -1)
                {
                    spawned.layer = cullingLayer;
                }
                
                // Optionally disable on spawn if using streaming
                if (spawnDisabled)
                {
                    Renderer rend = spawned.GetComponent<Renderer>();
                    if (rend != null) rend.enabled = false;
                    
                    Light light = spawned.GetComponent<Light>();
                    if (light != null) light.enabled = false;
                }
                
                count++;
            }
        }
        
        Debug.Log($"Spawned {count} lights!");
        
        // Notify the distance renderer to refresh its tracked objects
        if (distanceRenderer != null)
        {
            distanceRenderer.RefreshTrackedObjects();
        }
    }
    
    // ---------------- WALLS ----------------
    public void SpawnWalls()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("No wall prefab assigned!");
            return;
        }
        
        if (wallParent == null)
            wallParent = transform;
        
        int count = 0;
        float x = -wallRange;
        while (x <= wallRange)
        {
            float z = -wallRange;
            while (z <= wallRange)
            {
                Vector3 pos = new Vector3(x, wallHeight, z);
                
                // Random 45-degree rotation
                float randomYRotation = 45f * Random.Range(0, 8);
                Quaternion rot = Quaternion.Euler(0f, randomYRotation, 0f);
                
                GameObject spawned = Instantiate(wallPrefab, pos, rot, wallParent);
                
                // Assign to culling layer
                if (assignCullingLayer && cullingLayer != -1)
                {
                    spawned.layer = cullingLayer;
                }
                
                // Optionally disable on spawn if using streaming
                if (spawnDisabled)
                {
                    Renderer rend = spawned.GetComponent<Renderer>();
                    if (rend != null) rend.enabled = false;
                }
                
                count++;
                
                // Step forward by random spacing
                z += Random.Range(wallMinSpacing, wallMaxSpacing);
            }
            
            x += Random.Range(wallMinSpacing, wallMaxSpacing);
        }
        
        Debug.Log($"Spawned {count} walls!");
        
        // Notify the distance renderer to refresh its tracked objects
        if (distanceRenderer != null)
        {
            distanceRenderer.RefreshTrackedObjects();
        }
    }
    
    // ---------------- CLEAR ----------------
    public void ClearLights()
    {
        if (lightParent == null)
            lightParent = transform;
        
        ClearChildren(lightParent);
        Debug.Log("Lights cleared!");
        
        // Refresh the distance renderer
        if (distanceRenderer != null)
        {
            distanceRenderer.RefreshTrackedObjects();
        }
    }
    
    public void ClearWalls()
    {
        if (wallParent == null)
            wallParent = transform;
        
        ClearChildren(wallParent);
        Debug.Log("Walls cleared!");
        
        // Refresh the distance renderer
        if (distanceRenderer != null)
        {
            distanceRenderer.RefreshTrackedObjects();
        }
    }
    
    void ClearChildren(Transform parentTransform)
    {
        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parentTransform.GetChild(i).gameObject);
        }
    }
}

// ---------------- CUSTOM INSPECTOR ----------------
#if UNITY_EDITOR
[CustomEditor(typeof(GridSpawner))]
public class GridSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GridSpawner spawner = (GridSpawner)target;
        
        GUILayout.Space(10);
        
        // Light buttons
        GUILayout.Label("Light Controls", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Spawn Lights", GUILayout.Height(30)))
        {
            spawner.SpawnLights();
        }
        
        if (GUILayout.Button("Clear Lights", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Lights", 
                "Are you sure you want to delete all lights?", "Yes", "Cancel"))
            {
                spawner.ClearLights();
            }
        }
        
        GUILayout.Space(10);
        
        // Wall buttons
        GUILayout.Label("Wall Controls", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Spawn Walls", GUILayout.Height(30)))
        {
            spawner.SpawnWalls();
        }
        
        if (GUILayout.Button("Clear Walls", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Walls", 
                "Are you sure you want to delete all walls?", "Yes", "Cancel"))
            {
                spawner.ClearWalls();
            }
        }
    }
    
}
#endif