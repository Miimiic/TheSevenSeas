using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int timeForBulletToDespawn;
    public float damage;
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
