using UnityEngine;

// Place this script on any object with a large area trigger, it will use this trigger as an AOE for the player to begin passive healing. Trigger must be manually assigned in the inspector.

[RequireComponent(typeof(Collider))]

public class PassiveHealZone : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider passiveHealTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            other.gameObject.GetComponent<PlayerHealthController>().SetPassiveHealZone(true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerHealthController>().SetPassiveHealZone(false);
        }
    }
}
