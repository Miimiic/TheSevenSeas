using UnityEngine;

public class SplashDamageController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Enemy")
        {
            other.GetComponent<EnemyHealth>().TakeDamage(15);
        }
        if(other.gameObject.tag=="Mech")
        {
            other.GetComponent<MechHealthController>().Damage(20);
        }
    }

}
