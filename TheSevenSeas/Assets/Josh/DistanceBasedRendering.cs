using UnityEngine;
using System.Collections.Generic;

public class DistanceBasedRenderer : MonoBehaviour
{
    [Header("Culling Settings")]
    [Tooltip("Maximum distance at which objects are rendered")]
    public float renderDistance = 50f;
    
    [Tooltip("How often to check distances (in seconds). Lower = more responsive but more expensive")]
    public float updateInterval = 0.2f;
    
    [Tooltip("Layer mask for objects that should be culled by distance")]
    public LayerMask cullingLayer;
    
    [Header("Performance Settings")]
    [Tooltip("Use spatial partitioning for better performance with many objects")]
    public bool useSpatialPartitioning = true;
    
    [Tooltip("Grid cell size for spatial partitioning (should be ~2x renderDistance)")]
    public float gridCellSize = 100f;
    
    [Header("Optional Settings")]
    [Tooltip("Fade objects near the render distance instead of popping (requires Fade shader support)")]
    public bool useFading = false;
    
    [Tooltip("Distance to start fading")]
    public float fadeStartDistance = 40f;
    
    private Transform playerTransform;
    private float updateTimer;
    private Dictionary<Vector2Int, List<Renderer>> spatialGrid;
    private List<Renderer> allTrackedRenderers;
    
    void Start()
    {
        playerTransform = transform;
        allTrackedRenderers = new List<Renderer>();
        
        if (useSpatialPartitioning)
        {
            spatialGrid = new Dictionary<Vector2Int, List<Renderer>>();
            BuildSpatialGrid();
        }
        else
        {
            // Find all renderers in the culling layer
            FindAllRenderersInLayer();
        }
        
        // Do initial update
        UpdateVisibility();
    }
    
    void Update()
    {
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateVisibility();
        }
    }
    
    void FindAllRenderersInLayer()
    {
        allTrackedRenderers.Clear();
        
        // Find all renderers in the scene
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        
        foreach (Renderer rend in allRenderers)
        {
            // Check if the renderer's layer is in our culling layer mask
            if (((1 << rend.gameObject.layer) & cullingLayer) != 0)
            {
                allTrackedRenderers.Add(rend);
            }
        }
        
        Debug.Log($"Found {allTrackedRenderers.Count} renderers to track");
    }
    
    void BuildSpatialGrid()
    {
        spatialGrid.Clear();
        
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        
        foreach (Renderer rend in allRenderers)
        {
            if (((1 << rend.gameObject.layer) & cullingLayer) != 0)
            {
                Vector2Int gridPos = GetGridPosition(rend.transform.position);
                
                if (!spatialGrid.ContainsKey(gridPos))
                {
                    spatialGrid[gridPos] = new List<Renderer>();
                }
                
                spatialGrid[gridPos].Add(rend);
                allTrackedRenderers.Add(rend);
            }
        }
        
        Debug.Log($"Built spatial grid with {spatialGrid.Count} cells and {allTrackedRenderers.Count} total renderers");
    }
    
    Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / gridCellSize),
            Mathf.FloorToInt(worldPosition.z / gridCellSize)
        );
    }
    
    void UpdateVisibility()
    {
        if (useSpatialPartitioning)
        {
            UpdateVisibilityWithSpatialPartitioning();
        }
        else
        {
            UpdateVisibilityBruteForce();
        }
    }
    
    void UpdateVisibilityBruteForce()
    {
        Vector3 playerPos = playerTransform.position;
        
        foreach (Renderer rend in allTrackedRenderers)
        {
            if (rend == null) continue;
            
            float distance = Vector3.Distance(playerPos, rend.transform.position);
            
            if (useFading)
            {
                UpdateRendererWithFade(rend, distance);
            }
            else
            {
                rend.enabled = distance <= renderDistance;
            }
        }
    }
    
    void UpdateVisibilityWithSpatialPartitioning()
    {
        Vector3 playerPos = playerTransform.position;
        Vector2Int playerGridPos = GetGridPosition(playerPos);
        
        // Calculate how many grid cells we need to check based on render distance
        int gridRadius = Mathf.CeilToInt(renderDistance / gridCellSize) + 1;
        
        // Check nearby grid cells
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                Vector2Int checkPos = playerGridPos + new Vector2Int(x, z);
                
                if (spatialGrid.ContainsKey(checkPos))
                {
                    foreach (Renderer rend in spatialGrid[checkPos])
                    {
                        if (rend == null) continue;
                        
                        float distance = Vector3.Distance(playerPos, rend.transform.position);
                        
                        if (useFading)
                        {
                            UpdateRendererWithFade(rend, distance);
                        }
                        else
                        {
                            rend.enabled = distance <= renderDistance;
                        }
                    }
                }
            }
        }
    }
    
    void UpdateRendererWithFade(Renderer rend, float distance)
    {
        if (distance > renderDistance)
        {
            rend.enabled = false;
        }
        else
        {
            rend.enabled = true;
            
            // Calculate fade alpha (requires materials that support transparency)
            if (distance > fadeStartDistance)
            {
                float fadeAmount = 1f - ((distance - fadeStartDistance) / (renderDistance - fadeStartDistance));
                
                // Apply fade to material (requires shader with _Color or _Alpha property)
                if (rend.material.HasProperty("_Color"))
                {
                    Color color = rend.material.color;
                    color.a = fadeAmount;
                    rend.material.color = color;
                }
            }
            else
            {
                // Fully opaque
                if (rend.material.HasProperty("_Color"))
                {
                    Color color = rend.material.color;
                    color.a = 1f;
                    rend.material.color = color;
                }
            }
        }
    }
    
    // Call this if you dynamically spawn new objects
    public void RefreshTrackedObjects()
    {
        if (useSpatialPartitioning)
        {
            BuildSpatialGrid();
        }
        else
        {
            FindAllRenderersInLayer();
        }
    }
    
    // Optional: Visualize the render distance in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, renderDistance);
        
        if (useFading)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, fadeStartDistance);
        }
    }
}