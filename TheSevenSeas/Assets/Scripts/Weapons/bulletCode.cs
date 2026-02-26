using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class bulletCode : MonoBehaviour
{
    public float timeForBulletToDespawn;
    public float bulletDamage;
    private void Awake()
    {
        StartCoroutine(waiter());
    }
    IEnumerator waiter() 
    {

        yield return new WaitForSeconds(timeForBulletToDespawn);
        Object.Destroy(this.gameObject);
    
    }
}
