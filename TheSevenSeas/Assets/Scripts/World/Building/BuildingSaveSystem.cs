using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlacedObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public class BuildingSaveData
{
    public List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();
}

public class BuildingSaveSystem : MonoBehaviour
{
    [Header("References")]
    public GridSystem gridSystem;
    
    [Header("Prefab Database")]
    [Tooltip("All prefabs that can be placed - must match names in save data")]
    public List<GameObject> buildablePrefabs = new List<GameObject>();
    
    [Header("Save Settings")]
    public string saveFileName = "buildingSave.json";
    
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    private List<GameObject> placedObjectsList = new List<GameObject>();
    
    private void Start()
    {
        // Build prefab dictionary for quick lookup
        foreach (GameObject prefab in buildablePrefabs)
        {
            if (prefab != null)
            {
                prefabDictionary[prefab.name] = prefab;
            }
        }
        
        // Subscribe to placement events if using the modified GridSystem
        // (We'll need to track placed objects)
    }
    
    private void Update()
    {
        // Quick save with F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveBuilding();
        }
        
        // Quick load with F9
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadBuilding();
        }
    }
    
    public void SaveBuilding()
    {
        BuildingSaveData saveData = new BuildingSaveData();
        
        // Find all placed objects in the scene
        GridObject[] allGridObjects = FindObjectsByType<GridObject>(FindObjectsSortMode.None);
        
        foreach (GridObject gridObj in allGridObjects)
        {
            // Skip the ghost object (it won't have a proper prefab name)
            if (gridObj.transform.parent != null) continue;
            
            PlacedObjectData objectData = new PlacedObjectData
            {
                prefabName = gridObj.gameObject.name.Replace("(Clone)", "").Trim(),
                position = gridObj.transform.position,
                rotation = gridObj.transform.rotation
            };
            
            saveData.placedObjects.Add(objectData);
        }
        
        string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        
        Debug.Log($"Building saved! {saveData.placedObjects.Count} objects saved to: {savePath}");
    }
    
    public void LoadBuilding()
    {
        string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        
        if (!File.Exists(savePath))
        {
            Debug.LogWarning($"Save file not found at: {savePath}");
            return;
        }
        
        // Clear existing placed objects
        ClearPlacedObjects();
        
        // Load save data
        string json = File.ReadAllText(savePath);
        BuildingSaveData saveData = JsonUtility.FromJson<BuildingSaveData>(json);
        
        // Recreate placed objects
        int loadedCount = 0;
        foreach (PlacedObjectData objectData in saveData.placedObjects)
        {
            if (prefabDictionary.TryGetValue(objectData.prefabName, out GameObject prefab))
            {
                GameObject placedObject = Instantiate(prefab, objectData.position, objectData.rotation);
                placedObjectsList.Add(placedObject);
                loadedCount++;
            }
            else
            {
                Debug.LogWarning($"Prefab not found in database: {objectData.prefabName}");
            }
        }
        
        // Rebuild grid occupied positions
        if (gridSystem != null)
        {
            gridSystem.RebuildOccupiedPositions();
        }
        
        Debug.Log($"Building loaded! {loadedCount} objects restored.");
    }
    
    public void ClearPlacedObjects()
    {
        // Clear tracked objects
        foreach (GameObject obj in placedObjectsList)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        placedObjectsList.Clear();
        
        // Also clear any objects that weren't tracked (safety measure)
        GridObject[] allGridObjects = FindObjectsByType<GridObject>(FindObjectsSortMode.None);
        foreach (GridObject gridObj in allGridObjects)
        {
            if (gridObj.transform.parent == null) // Not a child object
            {
                Destroy(gridObj.gameObject);
            }
        }
        
        // Reset grid system occupied positions
        if (gridSystem != null)
        {
            gridSystem.ClearOccupiedPositions();
        }
        
        Debug.Log("All placed objects cleared.");
    }
    
    // Method to register a newly placed object (call this when placing)
    public void RegisterPlacedObject(GameObject obj)
    {
        if (!placedObjectsList.Contains(obj))
        {
            placedObjectsList.Add(obj);
        }
    }
    
    // Get the save file path for UI display
    public string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }
}