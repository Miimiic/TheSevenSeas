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
    
    [Header("Spawn Settings")]
    [Tooltip("Spawn from center/player outward (closest first)")]
    public bool spawnFromCenter = true;
    
    [Tooltip("Reference to player/camera for spawn center (uses world origin if null)")]
    public Transform spawnCenterReference;
    
    [Tooltip("Number of objects to instantiate per frame (lower = smoother but slower)")]
    [Range(1, 100)]
    public int objectsPerFrame = 100;
    
    [Header("Progress Info")]
    [Tooltip("Show spawning progress in console")]
    public bool showProgress = true;
    
    [SerializeField, ReadOnly] private bool isSpawning = false;
    [SerializeField, ReadOnly] private int totalLightsToSpawn = 0;
    [SerializeField, ReadOnly] private int lightsSpawned = 0;
    [SerializeField, ReadOnly] private int totalWallsToSpawn = 0;
    [SerializeField, ReadOnly] private int wallsSpawned = 0;

    [Header("UI")]
    [Tooltip("Loading/progress panel to hide once all spawning is complete")]
    public GameObject loadingPanel;

    [Header("World Seed")]
    public int worldSeed = 0;
    public bool useRandomSeed = true;
    
    private int cullingLayer = -1;
    
    private struct SpawnData 
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isLight;
    }
    
    void Start()
    {
        if (assignCullingLayer)
        {
            cullingLayer = LayerMask.NameToLayer(cullingLayerName);
            if (cullingLayer == -1)
                Debug.LogWarning($"Layer '{cullingLayerName}' not found!");
        }

        if (lightParent == null) lightParent = transform;
        if (wallParent == null) wallParent = transform;

        // Spawning is triggered by GameStateController via NewGame() or LoadGame()
        // Never spawns automatically on Start
    }
    
    // ---------------- SYNCHRONOUS SPAWNING FOR EDIT MODE ----------------
    public void SpawnLightsImmediate()
    {
        if (lightPrefab == null) { Debug.LogError("No light prefab assigned!"); return; }
        if (lightParent == null) lightParent = transform;
        if (assignCullingLayer && cullingLayer == -1)
            cullingLayer = LayerMask.NameToLayer(cullingLayerName);
        
        isSpawning = true;
        lightsSpawned = 0;
        
        Vector3 centerPosition = spawnCenterReference != null ? spawnCenterReference.position : Vector3.zero;
        List<SpawnData> positions = CalculateLightPositionsSync(centerPosition);
        totalLightsToSpawn = positions.Count;
        
        if (showProgress)
            Debug.Log($"Spawning {totalLightsToSpawn} lights immediately...");
        
        foreach (var data in positions)
        {
            InstantiateLight(data.position, data.rotation);
            lightsSpawned++;
            
            if (showProgress && lightsSpawned % 100 == 0)
                Debug.Log($"Spawning lights: {lightsSpawned}/{totalLightsToSpawn} ({(float)lightsSpawned/totalLightsToSpawn*100f:F1}%)");
        }
        
        isSpawning = false;
        distanceRenderer?.RefreshTrackedObjects();
        
        if (showProgress)
            Debug.Log($"Light spawning complete! Total: {lightsSpawned}");
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        #endif
    }
    
    public void SpawnWallsImmediate()
    {
        if (wallPrefab == null) { Debug.LogError("No wall prefab assigned!"); return; }
        if (wallParent == null) wallParent = transform;
        if (assignCullingLayer && cullingLayer == -1)
            cullingLayer = LayerMask.NameToLayer(cullingLayerName);
        
        isSpawning = true;
        wallsSpawned = 0;
        
        Vector3 centerPosition = spawnCenterReference != null ? spawnCenterReference.position : Vector3.zero;
        List<SpawnData> positions = CalculateWallPositionsSync(centerPosition);
        totalWallsToSpawn = positions.Count;
        
        if (showProgress)
            Debug.Log($"Spawning {totalWallsToSpawn} walls immediately...");
        
        foreach (var data in positions)
        {
            InstantiateWall(data.position, data.rotation);
            wallsSpawned++;
            
            if (showProgress && wallsSpawned % 100 == 0)
                Debug.Log($"Spawning walls: {wallsSpawned}/{totalWallsToSpawn} ({(float)wallsSpawned/totalWallsToSpawn*100f:F1}%)");
        }
        
        isSpawning = false;
        distanceRenderer?.RefreshTrackedObjects();
        
        if (showProgress)
            Debug.Log($"Wall spawning complete! Total: {wallsSpawned}");
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        #endif
    }
    
    private List<SpawnData> CalculateLightPositionsSync(Vector3 center)
    {
        List<SpawnData> positions = new List<SpawnData>();
        
        for (float x = -lightRange; x <= lightRange; x += lightSpacing)
        {
            for (float z = -lightRange; z <= lightRange; z += lightSpacing)
            {
                positions.Add(new SpawnData
                {
                    position = new Vector3(x, lightHeight, z),
                    rotation = Quaternion.identity,
                    isLight = true
                });
            }
        }
        
        if (spawnFromCenter)
        {
            positions.Sort((a, b) =>
            {
                float distA = Vector3.Distance(new Vector3(a.position.x, 0, a.position.z), new Vector3(center.x, 0, center.z));
                float distB = Vector3.Distance(new Vector3(b.position.x, 0, b.position.z), new Vector3(center.x, 0, center.z));
                return distA.CompareTo(distB);
            });
        }
        
        return positions;
    }
    
    private List<SpawnData> CalculateWallPositionsSync(Vector3 center)
    {
        List<SpawnData> positions = new List<SpawnData>();
        System.Random random = new System.Random(worldSeed);
        
        float x = -wallRange;
        while (x <= wallRange)
        {
            float z = -wallRange;
            while (z <= wallRange)
            {
                positions.Add(new SpawnData
                {
                    position = new Vector3(x, wallHeight, z),
                    rotation = Quaternion.Euler(0f, 45f * random.Next(0, 8), 0f),
                    isLight = false
                });
                z += (float)(random.NextDouble() * (wallMaxSpacing - wallMinSpacing) + wallMinSpacing);
            }
            x += (float)(random.NextDouble() * (wallMaxSpacing - wallMinSpacing) + wallMinSpacing);
        }
        
        if (spawnFromCenter)
        {
            positions.Sort((a, b) =>
            {
                float distA = Vector3.Distance(new Vector3(a.position.x, 0, a.position.z), new Vector3(center.x, 0, center.z));
                float distB = Vector3.Distance(new Vector3(b.position.x, 0, b.position.z), new Vector3(center.x, 0, center.z));
                return distA.CompareTo(distB);
            });
        }
        
        return positions;
    }
    
    // ---------------- ASYNC SPAWNING FOR RUNTIME ----------------
    public async Task SpawnLightsAsync()
    {
        if (lightPrefab == null) { Debug.LogError("No light prefab assigned!"); return; }
        
        #if UNITY_EDITOR
        if (!Application.isPlaying) { SpawnLightsImmediate(); return; }
        #endif
        
        isSpawning = true;
        lightsSpawned = 0;
        
        Vector3 centerPosition = spawnCenterReference != null ? spawnCenterReference.position : Vector3.zero;
        List<SpawnData> positions = await Task.Run(() => CalculateLightPositionsSync(centerPosition));
        totalLightsToSpawn = positions.Count;
        
        if (showProgress)
            Debug.Log($"Spawning {totalLightsToSpawn} lights...");
        
        int spawnedThisFrame = 0;
        foreach (var data in positions)
        {
            InstantiateLight(data.position, data.rotation);
            lightsSpawned++;
            spawnedThisFrame++;
            
            if (showProgress && lightsSpawned % 100 == 0)
                Debug.Log($"Spawning lights: {lightsSpawned}/{totalLightsToSpawn} ({(float)lightsSpawned/totalLightsToSpawn*100f:F1}%)");
            
            if (spawnedThisFrame >= objectsPerFrame)
            {
                spawnedThisFrame = 0;
                await Task.Yield();
            }
        }
        
        isSpawning = false;
        distanceRenderer?.RefreshTrackedObjects();
        
        if (showProgress)
            Debug.Log($"Light spawning complete! Total: {lightsSpawned}");
    }

    public async Task SpawnWallsAsync()
    {
        if (wallPrefab == null) { Debug.LogError("No wall prefab assigned!"); return; }
        
        #if UNITY_EDITOR
        if (!Application.isPlaying) { SpawnWallsImmediate(); return; }
        #endif
        
        isSpawning = true;
        wallsSpawned = 0;
        
        Vector3 centerPosition = spawnCenterReference != null ? spawnCenterReference.position : Vector3.zero;
        List<SpawnData> positions = await Task.Run(() => CalculateWallPositionsSync(centerPosition));
        totalWallsToSpawn = positions.Count;
        
        if (showProgress)
            Debug.Log($"Spawning {totalWallsToSpawn} walls...");
        
        int spawnedThisFrame = 0;
        foreach (var data in positions)
        {
            InstantiateWall(data.position, data.rotation);
            wallsSpawned++;
            spawnedThisFrame++;
            
            if (showProgress && wallsSpawned % 100 == 0)
                Debug.Log($"Spawning walls: {wallsSpawned}/{totalWallsToSpawn} ({(float)wallsSpawned/totalWallsToSpawn*100f:F1}%)");
            
            if (spawnedThisFrame >= objectsPerFrame)
            {
                spawnedThisFrame = 0;
                await Task.Yield();
            }
        }
        
        isSpawning = false;
        distanceRenderer?.RefreshTrackedObjects();
        
        if (showProgress)
            Debug.Log($"Wall spawning complete! Total: {wallsSpawned}");
    }
    
    // ---------------- RUN SPAWNING (called by GameStateController) ----------------
    public async Task RunSpawning()
    {
        await SpawnLightsAsync();
        await SpawnWallsAsync();

    }

    public async void SpawnOnDemand()
    {
        if (isSpawning) return;
        await RunSpawning();
    }

    // ---------------- MANUAL SPAWN (for editor/testing) ----------------
    public async void SpawnLights() { await SpawnLightsAsync(); }
    public async void SpawnWalls() { await SpawnWallsAsync(); }
    
    // ---------------- INSTANTIATE ----------------
    private void InstantiateLight(Vector3 position, Quaternion rotation)
    {
        GameObject spawned = Instantiate(lightPrefab, position, rotation, lightParent);
        
        if (assignCullingLayer && cullingLayer != -1)
            spawned.layer = cullingLayer;
        
        if (spawnDisabled)
        {
            Renderer rend = spawned.GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;
            Light light = spawned.GetComponent<Light>();
            if (light != null) light.enabled = false;
        }
    }
    
    private void InstantiateWall(Vector3 position, Quaternion rotation)
    {
        GameObject spawned = Instantiate(wallPrefab, position, rotation, wallParent);
        
        if (assignCullingLayer && cullingLayer != -1)
            spawned.layer = cullingLayer;
        
        if (spawnDisabled)
        {
            Renderer rend = spawned.GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;
        }
    }

    // ---------------- CLEAR ----------------
    public void ClearLights()
    {
        if (lightParent == null) lightParent = transform;
        ClearChildren(lightParent);
        lightsSpawned = 0;
        totalLightsToSpawn = 0;
        Debug.Log("Lights cleared!");
        distanceRenderer?.RefreshTrackedObjects();
        
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
        #endif
    }
    
    public void ClearWalls()
    {
        if (wallParent == null) wallParent = transform;
        ClearChildren(wallParent);
        wallsSpawned = 0;
        totalWallsToSpawn = 0;
        Debug.Log("Walls cleared!");
        distanceRenderer?.RefreshTrackedObjects();
        
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
        #endif
    }
    
    void ClearChildren(Transform parentTransform)
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = parentTransform.childCount - 1; i >= 0; i--)
                DestroyImmediate(parentTransform.GetChild(i).gameObject);
        }
        else
        #endif
        {
            for (int i = parentTransform.childCount - 1; i >= 0; i--)
                Destroy(parentTransform.GetChild(i).gameObject);
        }
    }
    
    // ---------------- PROGRESS ----------------
    public float GetSpawningProgress()
    {
        int totalToSpawn = totalLightsToSpawn + totalWallsToSpawn;
        int totalSpawned = lightsSpawned + wallsSpawned;
        if (totalToSpawn == 0) return 0f;
        return (float)totalSpawned / totalToSpawn;
    }
    
    public bool IsSpawning() => isSpawning;
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

[CustomEditor(typeof(GridSpawner))]
public class GridSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GridSpawner spawner = (GridSpawner)target;
        
        GUILayout.Space(10);
        
        if (spawner.IsSpawning())
        {
            float progress = spawner.GetSpawningProgress();
            Rect rect = EditorGUILayout.BeginVertical();
            EditorGUI.ProgressBar(new Rect(3, rect.y, rect.width - 6, 20), progress, $"Spawning: {progress * 100:F1}%");
            GUILayout.Space(25);
            EditorGUILayout.EndVertical();
        }
        
        GUILayout.Space(5);
        GUILayout.Label("Manual Controls", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(spawner.IsSpawning());
        
        if (GUILayout.Button("Spawn Lights", GUILayout.Height(30)))
        {
            if (!Application.isPlaying) spawner.SpawnLightsImmediate();
            else spawner.SpawnLights();
        }
        
        if (GUILayout.Button("Clear Lights", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Lights", "Are you sure you want to delete all lights?", "Yes", "Cancel"))
                spawner.ClearLights();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Spawn Walls", GUILayout.Height(30)))
        {
            if (!Application.isPlaying) spawner.SpawnWallsImmediate();
            else spawner.SpawnWalls();
        }
        
        if (GUILayout.Button("Clear Walls", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Walls", "Are you sure you want to delete all walls?", "Yes", "Cancel"))
                spawner.ClearWalls();
        }
        
        EditorGUI.EndDisabledGroup();
        
        if (!Application.isPlaying)
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Edit Mode: Objects will spawn immediately. Remember to save the scene after spawning!", MessageType.Info);
        }
        
        if (spawner.IsSpawning()) Repaint();
    }
}
#endif