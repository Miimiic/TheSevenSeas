using UnityEngine;

public class SplashDamageController : MonoBehaviour
{
    [SerializeField] private GameObject bloodSpray;
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
            if(other.GetComponent<EnemyHealth>().health-15<=0)
            {
                Instantiate(bloodSpray,other.transform.position, Quaternion.identity);
            }
            other.GetComponent<EnemyHealth>().TakeDamage(15);
        }
        if(other.gameObject.tag=="Mech")
        {
            other.GetComponent<MechHealthController>().Damage(20);
        }
    }

}
