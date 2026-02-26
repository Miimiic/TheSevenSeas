using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    [Header("Grid Configuration")]
    [Tooltip("Size of the object in grid cells (X and Z)")]
    public Vector2Int size = new Vector2Int(1, 1);
    
    [Header("References")]
    [Tooltip("The origin point transform (bottom-left corner of the object)")]
    public Transform originPoint;
    
    public List<Vector3Int> GetOccupiedCells(float gridSize)
    {
        List<Vector3Int> cells = new List<Vector3Int>();
        
        if (originPoint == null)
        {
            Vector3Int gridPos = Vector3Int.RoundToInt(transform.position / gridSize);
            cells.Add(gridPos);
            return cells;
        }
        
        Vector3Int originGridPos = Vector3Int.RoundToInt(originPoint.position / gridSize);
        Vector3 forward = originPoint.forward;
        Vector3 right = originPoint.right;
        
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector3 worldOffset = right * x * gridSize + forward * z * gridSize;
                Vector3 cellWorldPos = originPoint.position + worldOffset;
                Vector3Int cellGridPos = Vector3Int.RoundToInt(cellWorldPos / gridSize);
                cells.Add(cellGridPos);
            }
        }
        
        return cells;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (originPoint == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(originPoint.position, 0.1f);
        
        float gridSize = 0.5f;
        Vector3 forward = originPoint.forward;
        Vector3 right = originPoint.right;
        
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector3 worldOffset = right * x * gridSize + forward * z * gridSize;
                Vector3 cellCenter = originPoint.position + worldOffset;
                Gizmos.DrawWireCube(cellCenter, Vector3.one * gridSize);
            }
        }
    }
}