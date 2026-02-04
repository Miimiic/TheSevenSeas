using UnityEngine;

// For testing damage functionality only.

public class TestDamageCube : MonoBehaviour
{
    private Collider itemCollider;
    private int damageNumber;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemCollider = GetComponent<Collider>();
        damageNumber = 25;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerHealthController>().Damage(damageNumber);
        }
    }
}
