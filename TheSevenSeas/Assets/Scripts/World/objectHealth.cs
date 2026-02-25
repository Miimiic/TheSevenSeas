using UnityEngine;

public class objectHealth : MonoBehaviour
{
    public bool metal;
    public bool wood;
    public bool brick;
    public float Health;

    private void Update()
    {
        if (Health <= 0f)
        {
            Destroy(gameObject);

        }
    }
   public void takeDamage(float damage)
    {
        Health -= damage;
    }
}
