using UnityEngine;

public class PowerStarItem : MonoBehaviour
{
    private Collider itemCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemCollider = GetComponent<Collider>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerHealthController>().TriggerInvincibility(10);
            Destroy(gameObject);
        }
    }
}
