using System.Collections.Generic;
using UnityEngine;

// Define the types of resources
public enum ResourceType
{
    Wood,
    Plastic,
    Metal
}

public class ResourceManager : MonoBehaviour
{
    // Creates a Singleton so other scripts can easily access this
    public static ResourceManager Instance;

    // A dictionary to store the current amounts of each resource
    public Dictionary<ResourceType, int> inventory = new Dictionary<ResourceType, int>();

    private void Awake()
    {
        // Set up the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize our inventory starting at 0 for each item
        inventory.Add(ResourceType.Wood, 0);
        inventory.Add(ResourceType.Plastic, 0);
        inventory.Add(ResourceType.Metal, 0);
    }

    // Function to call when we pick something up
    public void AddResource(ResourceType type, int amount)
    {
        inventory[type] += amount;
        Debug.Log($"Picked up {amount} {type}! Total {type}: {inventory[type]}");
    }
}