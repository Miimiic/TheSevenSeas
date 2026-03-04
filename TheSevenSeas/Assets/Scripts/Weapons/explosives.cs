using Unity.VisualScripting;
using UnityEngine;

public class explosives : MonoBehaviour
{
    public float damage;
    public Animator animator;
    public bool explodeWithTimer;
    public float explosionTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
       if (explodeWithTimer)
        {
            Invoke("explode", explosionTimer);
        }
        
    }

    void explode() 
    {

        animator.Play("explosion");
        
    
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "enemy") 
        { 
        
            
        
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        explode();
    }
    public void destroyAfterAnim()
    {
        Destroy(gameObject);
    }
}
