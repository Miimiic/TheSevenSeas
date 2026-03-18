// BuildingIdentifier.cs
using UnityEngine;

public class BuildingIdentifier : MonoBehaviour
{
    [Tooltip("Must match the prefab's name exactly")]
    public string prefabId;
    public int health = 100;
}