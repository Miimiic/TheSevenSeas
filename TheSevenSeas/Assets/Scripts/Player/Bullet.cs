using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int timeForBulletToDespawn;
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
