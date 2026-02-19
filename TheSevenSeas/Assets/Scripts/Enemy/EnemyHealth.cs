using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth;
    public float health;

    private void Start()
    {
        health = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            float damage = other.GetComponent<Bullet>().damage;
            TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}

  
