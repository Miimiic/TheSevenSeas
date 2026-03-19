using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Info")]
    public float maxHealth;
    public float health;
    public bool tookDamage = false;

    private void Start()
    {
        health = maxHealth;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            float damage = other.gameObject.GetComponent<bulletCode>().bulletDamage;
            tookDamage = true;
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

  
