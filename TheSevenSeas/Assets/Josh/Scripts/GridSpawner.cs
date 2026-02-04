using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

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
    
    [Header("Auto Spawn Settings")]
    [Tooltip("Automatically spawn objects on Start")]
    public bool spawnOnStart = true;
    
    [Tooltip("Spawn lights on start")]
    public bool spawnLightsOnStart = true;
    
    [Tooltip("Spawn walls on start")]
    public bool spawnWallsOnStart = true;
    
    [Tooltip("Spawn from center/player outward (closest first)")]
    public bool spawnFromCenter = true;
    
    [Tooltip("Reference to player/camera for spawn center (uses world origin if null)")]
    public Transform spawnCenterReference;
    
    [Tooltip("Number of objects to instantiate per frame (lower = smoother but slower)")]
    [Range(1, 100)]
    public int objectsPerFrame = 10;
    
    [Header("Progress Info")]
    [Tooltip("Show spawning progress in console")]
    public bool showProgress = true;
    
    [SerializeField, ReadOnly] private bool isSpawning = false;
    [SerializeField, ReadOnly] private int totalLightsToSpawn = 0;
    [SerializeField, ReadOnly] private int lightsSpawned = 0;
    [SerializeField, ReadOnly] private int totalWallsToSpawn = 0;
    [SerializeField, ReadOnly] private int wallsSpawned = 0;
    
    private int cullingLayer = -1;
    
    // Queues for spawning on the main thread
    private Queue<SpawnData> lightsToSpawn = new Queue<SpawnData>();
    private Queue<SpawnData> wallsToSpawn = new Queue<SpawnData>();
    
    private struct SpawnData
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isLight;
    }
    
    async void Start()
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
        
        if (lightParent == null)
            lightParent = transform;
        
        if (wallParent == null)
            wallParent = transform;
        
        // Auto spawn if enabled
        if (spawnOnStart)
        {
            if (showProgress)
                Debug.Log("Starting background object generation...");
            
            if (spawnLightsOnStart)
                await SpawnLightsAsync();
            
            if (spawnWallsOnStart)
                await SpawnWallsAsync();
            
            if (showProgress)
                Debug.Log("All objects spawned! Player can move freely during this process.");
        }
    }
    
    void Update()
    {
        // Process spawn queues on the main thread (Unity objects can only be created on main thread)
        if (lightsToSpawn.Count > 0)
        {
            int spawnedThisFrame = 0;
            while (lightsToSpawn.Count > 0 && spawnedThisFrame < objectsPerFrame)
            {
                SpawnData data = lightsToSpawn.Dequeue();
                InstantiateLight(data.position, data.rotation);
                lightsSpawned++;
                spawnedThisFrame++;
            }
            
            if (showProgress && lightsSpawned % 100 == 0)
            {
                float progress = (float)lightsSpawned / totalLightsToSpawn * 100f;
                Debug.Log($"Spawning lights: {lightsSpawned}/{totalLightsToSpawn} ({progress:F1}%)");
            }
        }
        
        if (wallsToSpawn.Count > 0)
        {
            int spawnedThisFrame = 0;
            while (wallsToSpawn.Count > 0 && spawnedThisFrame < objectsPerFrame)
            {
                SpawnData data = wallsToSpawn.Dequeue();
                InstantiateWall(data.position, data.rotation);
                wallsSpawned++;
                spawnedThisFrame++;
            }
            
            if (showProgress && wallsSpawned % 100 == 0)
            {
                float progress = (float)wallsSpawned / totalWallsToSpawn * 100f;
                Debug.Log($"Spawning walls: {wallsSpawned}/{totalWallsToSpawn} ({progress:F1}%)");
            }
        }
        
        // Check if spawning is complete
        if (isSpawning && lightsToSpawn.Count == 0 && wallsToSpawn.Count == 0)
        {
            isSpawning = false;
            
            // Notify the distance renderer to refresh its tracked objects
            if (distanceRenderer != null)
            {
                distanceRenderer.RefreshTrackedObjects();
            }
            
            if (showProgress)
            {
                Debug.Log($"Spawning complete! Total lights: {lightsSpawned}, Total walls: {wallsSpawned}");
            }
        }
    }
    
    // ---------------- ASYNC LIGHTS ----------------
    public async Task SpawnLightsAsync()
    {
        if (lightPrefab == null)
        {
            Debug.LogError("No light prefab assigned!");
            return;
        }
        
        isSpawning = true;
        lightsSpawned = 0;
        lightsToSpawn.Clear();
        
        // CRITICAL: Cache the center position on the MAIN THREAD before going to background thread
        // Unity objects can't be accessed from background threads
        Vector3 centerPosition = Vector3.zero;
        if (spawnCenterReference != null)
        {
            centerPosition = spawnCenterReference.position;
        }
        
        // Calculate positions on background thread, passing the cached position
        await Task.Run(() => CalculateLightPositions(centerPosition));
        
        if (showProgress)
            Debug.Log($"Calculated {totalLightsToSpawn} light positions. Spawning...");
    }
    
    private void CalculateLightPositions(Vector3 center)
    {
        List<SpawnData> tempList = new List<SpawnData>();
        
        for (float x = -lightRange; x <= lightRange; x += lightSpacing)
        {
            for (float z = -lightRange; z <= lightRange; z += lightSpacing)
            {
                SpawnData data = new SpawnData
                {
                    position = new Vector3(x, lightHeight, z),
                    rotation = Quaternion.identity,
                    isLight = true
                };
                tempList.Add(data);
            }
        }
        
        // Sort by distance from center if enabled
        if (spawnFromCenter)
        {
            tempList.Sort((a, b) =>
            {
                float distA = Vector3.Distance(new Vector3(a.position.x, 0, a.position.z), 
                                              new Vector3(center.x, 0, center.z));
                float distB = Vector3.Distance(new Vector3(b.position.x, 0, b.position.z), 
                                              new Vector3(center.x, 0, center.z));
                return distA.CompareTo(distB);
            });
        }
        
        totalLightsToSpawn = tempList.Count;
        
        // Transfer to main thread queue (lock for thread safety)
        lock (lightsToSpawn)
        {
            foreach (var data in tempList)
            {
                lightsToSpawn.Enqueue(data);
            }
        }
    }
    
    private void InstantiateLight(Vector3 position, Quaternion rotation)
    {
        GameObject spawned = Instantiate(lightPrefab, position, rotation, lightParent);
        
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
    }
    
    // ---------------- ASYNC WALLS ----------------
    public async Task SpawnWallsAsync()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("No wall prefab assigned!");
            return;
        }
        
        isSpawning = true;
        wallsSpawned = 0;
        wallsToSpawn.Clear();
        
        // CRITICAL: Cache the center position on the MAIN THREAD before going to background thread
        // Unity objects can't be accessed from background threads
        Vector3 centerPosition = Vector3.zero;
        if (spawnCenterReference != null)
        {
            centerPosition = spawnCenterReference.position;
        }
        
        // Calculate positions on background thread, passing the cached position
        await Task.Run(() => CalculateWallPositions(centerPosition));
        
        if (showProgress)
            Debug.Log($"Calculated {totalWallsToSpawn} wall positions. Spawning...");
    }
    
    private void CalculateWallPositions(Vector3 center)
    {
        List<SpawnData> tempList = new List<SpawnData>();
        
        // Use separate Random for thread safety
        System.Random random = new System.Random();
        
        float x = -wallRange;
        while (x <= wallRange)
        {
            float z = -wallRange;
            while (z <= wallRange)
            {
                // Random 45-degree rotation
                float randomYRotation = 45f * random.Next(0, 8);
                
                SpawnData data = new SpawnData
                {
                    position = new Vector3(x, wallHeight, z),
                    rotation = Quaternion.Euler(0f, randomYRotation, 0f),
                    isLight = false
                };
                tempList.Add(data);
                
                // Step forward by random spacing
                z += (float)(random.NextDouble() * (wallMaxSpacing - wallMinSpacing) + wallMinSpacing);
            }
            
            x += (float)(random.NextDouble() * (wallMaxSpacing - wallMinSpacing) + wallMinSpacing);
        }
        
        // Sort by distance from center if enabled
        if (spawnFromCenter)
        {
            tempList.Sort((a, b) =>
            {
                float distA = Vector3.Distance(new Vector3(a.position.x, 0, a.position.z), 
                                              new Vector3(center.x, 0, center.z));
                float distB = Vector3.Distance(new Vector3(b.position.x, 0, b.position.z), 
                                              new Vector3(center.x, 0, center.z));
                return distA.CompareTo(distB);
            });
        }
        
        totalWallsToSpawn = tempList.Count;
        
        // Transfer to main thread queue (lock for thread safety)
        lock (wallsToSpawn)
        {
            foreach (var data in tempList)
            {
                wallsToSpawn.Enqueue(data);
            }
        }
    }
    
    private void InstantiateWall(Vector3 position, Quaternion rotation)
    {
        GameObject spawned = Instantiate(wallPrefab, position, rotation, wallParent);
        
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
    }
    
    // ---------------- MANUAL SPAWN (for editor/testing) ----------------
    public async void SpawnLights()
    {
        await SpawnLightsAsync();
    }
    
    public async void SpawnWalls()
    {
        await SpawnWallsAsync();
    }
    
    // ---------------- CLEAR ----------------
    public void ClearLights()
    {
        if (lightParent == null)
            lightParent = transform;
        
        ClearChildren(lightParent);
        lightsSpawned = 0;
        totalLightsToSpawn = 0;
        lightsToSpawn.Clear();
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
        wallsSpawned = 0;
        totalWallsToSpawn = 0;
        wallsToSpawn.Clear();
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
    
    // Get spawning progress (0-1)
    public float GetSpawningProgress()
    {
        int totalToSpawn = totalLightsToSpawn + totalWallsToSpawn;
        int totalSpawned = lightsSpawned + wallsSpawned;
        
        if (totalToSpawn == 0)
            return 1f;
        
        return (float)totalSpawned / totalToSpawn;
    }
    
    public bool IsSpawning()
    {
        return isSpawning;
    }
}

// Custom attribute for read-only fields in inspector
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}

// ---------------- CUSTOM INSPECTOR ----------------
[CustomEditor(typeof(GridSpawner))]
public class GridSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GridSpawner spawner = (GridSpawner)target;
        
        GUILayout.Space(10);
        
        // Progress bar
        if (spawner.IsSpawning())
        {
            float progress = spawner.GetSpawningProgress();
            Rect rect = EditorGUILayout.BeginVertical();
            EditorGUI.ProgressBar(new Rect(3, rect.y, rect.width - 6, 20), progress, $"Spawning: {progress * 100:F1}%");
            GUILayout.Space(25);
            EditorGUILayout.EndVertical();
        }
        
        GUILayout.Space(5);
        
        // Light buttons
        GUILayout.Label("Manual Controls (Editor Only)", EditorStyles.boldLabel);
        
        EditorGUI.BeginDisabledGroup(spawner.IsSpawning());
        
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
        
        EditorGUI.EndDisabledGroup();
        
        // Repaint to update progress bar
        if (spawner.IsSpawning())
        {
            Repaint();
        }
    }
}
#endif