using UnityEngine;

// Place this script on any object with a small area trigger, it will use this trigger as an indicator to heal the player by healingAmount. Trigger must be manually assigned in the inspector.

public class HealthItemController : MonoBehaviour
{
    private Collider itemCollider;
    [SerializeField] private int healingAmount;

    // Constructor can set the healing amount for the item, Incase we need to spawn better healing items in certain scenarios
    public HealthItemController(int newHealingAmount)
    {
        healingAmount = newHealingAmount;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            // Check it wont waste the healing item on full health
            PlayerHealthController playerHealthController = other.gameObject.GetComponent<PlayerHealthController>();
            if(playerHealthController.GetCurrentHealth()!=playerHealthController.GetMaxHealth())
            {
                playerHealthController.Heal(healingAmount);
                Destroy(gameObject);
            }
        }
    }
}
