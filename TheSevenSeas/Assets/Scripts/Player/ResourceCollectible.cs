using UnityEngine;

// This ensures the object always has a collider to detect the player
[RequireComponent(typeof(Collider))]
public class ResourceCollectible : MonoBehaviour
{
    [Header("Resource Settings")]
    [Tooltip("Select what kind of resource this object is.")]
    public ResourceType resourceType;

    [Tooltip("How much of this resource you get per pickup.")]
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding with this is the Player
        if (other.CompareTag("Player"))
        {
            // Add the resource to the manager
            ResourceManager.Instance.AddResource(resourceType, amount);

            // Destroy the object from the map
            Destroy(gameObject);
        }
    }
}