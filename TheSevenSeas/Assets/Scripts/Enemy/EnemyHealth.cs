using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth;
    public float health;
    public bool tookDamage = false;
    public GameObject blood;
    public GameObject resource;


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
            Instantiate(blood,transform.position, Quaternion.identity); 

            float rand = Random.Range(0, 1);
            if (rand<=0.5)
            {
                Instantiate(resource,transform.position, Quaternion.identity);
            }
            
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}

  
