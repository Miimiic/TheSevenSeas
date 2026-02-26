using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public GameObject objectToPlace;
    public int gridSize = 1;
    private GameObject ghostObject;
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
    private int currentRotation = 0;
    private bool isEnabled = true;
    private GridObject currentGridObject;
    public LayerMask placeableLayer;
    
    [Header("Save System (Optional)")]
    public BuildingSaveSystem saveSystem;
    
    private void Start()
    {
        CreateGhostObject();
    }

    private void Update()
    {
        if (!isEnabled) return;
        UpdateGhostPos();
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateGhost();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }
    }

    public void CreateGhostObject()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
        }
        
        if (objectToPlace == null)
        {
            Debug.LogWarning("No object to place assigned!");
            return;
        }
        
        ghostObject = Instantiate(objectToPlace);
        ghostObject.SetActive(false);
        ghostObject.GetComponent<Collider>().enabled = false;
        
        currentGridObject = ghostObject.GetComponent<GridObject>();
        if (currentGridObject == null)
        {
            Debug.LogError("objectToPlace must have a GridObject component!");
        }
        
        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material ghostMaterial = new Material(renderer.material);
            Color color = ghostMaterial.color;
            color.a = 0.5f;
            ghostMaterial.color = color;
            renderer.material = ghostMaterial;
            ghostMaterial.SetFloat("_Mode", 2);
            ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ghostMaterial.SetInt("_ZWrite", 0);
            ghostMaterial.DisableKeyword("_ALPHATEST_ON");
            ghostMaterial.EnableKeyword("_ALPHABLEND_ON");
            ghostMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            ghostMaterial.renderQueue = 3000;
        }
    }

    void RotateGhost()
    {
        currentRotation = (currentRotation + 90) % 360;
        
        if (currentGridObject != null && currentGridObject.originPoint != null)
        {
            ghostObject.transform.RotateAround(
                currentGridObject.originPoint.position,
                Vector3.up,
                90f
            );
        }
        else
        {
            ghostObject.transform.Rotate(Vector3.up, 90f);
        }
    }

    void UpdateGhostPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer))
        {
            if (!ghostObject.activeSelf)
            {
                ghostObject.SetActive(true);
            }
            
            Vector3 point = hit.point;
            
            Vector3 snappedPos = new Vector3(
                Mathf.Round(point.x / gridSize) * gridSize,
                Mathf.Round(point.y / gridSize) * gridSize,
                Mathf.Round(point.z / gridSize) * gridSize
            );
            
            if (currentGridObject != null && currentGridObject.originPoint != null)
            {
                Vector3 offset = ghostObject.transform.position - currentGridObject.originPoint.position;
                ghostObject.transform.position = snappedPos + offset;
            }
            else
            {
                ghostObject.transform.position = snappedPos;
            }
            
            if (IsPlacementValid())
            {
                SetGhostColor(Color.green);
            }
            else
            {
                SetGhostColor(Color.red);
            }
        }
        else
        {
            if (ghostObject.activeSelf)
            {
                ghostObject.SetActive(false);
            }
        }
    }

    bool IsPlacementValid()
    {
        if (currentGridObject == null) return true;
        
        List<Vector3Int> occupiedCells = currentGridObject.GetOccupiedCells(gridSize);
        
        foreach (Vector3Int cell in occupiedCells)
        {
            if (occupiedPositions.Contains(cell))
            {
                return false;
            }
        }
        
        return true;
    }

    void SetGhostColor(Color color)
    {
        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            Color currentColor = mat.color;
            currentColor.r = color.r;
            currentColor.g = color.g;
            currentColor.b = color.b;
            mat.color = currentColor;
        }
    }

    void PlaceObject()
    {
        if (IsPlacementValid())
        {
            GameObject placedObject = Instantiate(
                objectToPlace,
                ghostObject.transform.position,
                ghostObject.transform.rotation
            );
            
            GridObject gridObj = placedObject.GetComponent<GridObject>();
            if (gridObj != null)
            {
                List<Vector3Int> occupiedCells = gridObj.GetOccupiedCells(gridSize);
                foreach (Vector3Int cell in occupiedCells)
                {
                    occupiedPositions.Add(cell);
                }
            }
            
            // Register with save system if available
            if (saveSystem != null)
            {
                saveSystem.RegisterPlacedObject(placedObject);
            }
            
            Debug.Log($"Placed object: {placedObject.name}");
        }
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        
        if (!enabled && ghostObject != null)
        {
            ghostObject.SetActive(false);
        }
    }
    
    // Public method for save system to clear occupied positions
    public void ClearOccupiedPositions()
    {
        occupiedPositions.Clear();
        Debug.Log("Grid occupied positions cleared.");
    }
    
    // Public method to rebuild occupied positions from existing objects (useful after loading)
    public void RebuildOccupiedPositions()
    {
        occupiedPositions.Clear();
        
        GridObject[] allGridObjects = FindObjectsByType<GridObject>(FindObjectsSortMode.None);
        foreach (GridObject gridObj in allGridObjects)
        {
            // Skip ghost object
            if (gridObj.gameObject == ghostObject) continue;
            
            List<Vector3Int> cells = gridObj.GetOccupiedCells(gridSize);
            foreach (Vector3Int cell in cells)
            {
                occupiedPositions.Add(cell);
            }
        }
        
        Debug.Log($"Rebuilt occupied positions: {occupiedPositions.Count} cells occupied.");
    }
}