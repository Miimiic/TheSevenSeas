using System.Collections;
using UnityEngine;

public class MechWeaponController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Camera titanCamera;
    [SerializeField] private GameObject explosionEffect, muzzleFlashLocation, muzzleFlashEffect, bloodSplatter;


    [Header("UI Components")]
    [SerializeField] private GameObject ammoCounterParent;

    [Header("Variables")]
    [SerializeField] private float fireRate;
    [SerializeField] private int ammoCount, maxAmmoCapacity, reloadTime, range;
    [SerializeField] private int damage;
    // Splash Damage is handled by the actual explosion effect, It was the easiest way to make it work without breaking 90+ other things
    [SerializeField] private bool canFire, isReloading, canReload;
    [SerializeField] private AudioSource cannonSource;
    [SerializeField] private AudioClip cannonSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ammoCount = maxAmmoCapacity;
        canFire = true;
        isReloading = false;
        canReload = false;
        cannonSource = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();
    }

    private void HandleInputs()
    {
        if (Input.GetKey(KeyCode.R))
        {
            if (!isReloading && ammoCount != maxAmmoCapacity&& canReload)
            {
                StartCoroutine(Reload());
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (canFire&&ammoCount>0)
            {
                Fire();
                StartCoroutine(FireRateCoroutine());
            }
            else if (canFire&&ammoCount==0)
            {
                StartCoroutine(Reload());
            }
        }
    }


    private void Fire()
    {
        cannonSource.PlayOneShot(cannonSound);
        ammoCount--;
        RaycastHit hit;
        // Also play the sound
        if(muzzleFlashEffect!=null)
        {
            Instantiate(muzzleFlashEffect, muzzleFlashLocation.transform.position, muzzleFlashLocation.transform.rotation);
        }
        if (Physics.Raycast(titanCamera.transform.position, titanCamera.transform.forward, out hit, range))
        {
            Debug.Log("Titan hit: " + hit.transform.name);
            if (hit.transform.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.transform.GetComponentInParent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    if (enemyHealth.GetComponent<EnemyHealth>().health-damage <= 0)
                    {
                        Instantiate(bloodSplatter,hit.point, hit.transform.rotation);
                    }
                    enemyHealth.TakeDamage(damage);
                }
                else
                {
                    Debug.LogError("EnemyHealth script not found on hit object!");
                }
            }
            Instantiate(explosionEffect, hit.point, hit.transform.rotation);
        }
    }

    private void HandleAmmoCounter()
    {

    }

    IEnumerator FireRateCoroutine()
    {
        if (canFire&&!isReloading)
        {
            canReload = false;
            canFire = false;
            // Divide a minute by the fireRate to find how long between shots of the gun, prevent it from firing until the time has elapsed
            float waitTimeBetweenShots = 60 / fireRate;
            yield return new WaitForSeconds(waitTimeBetweenShots);

            canFire = true;
            canReload = true;
            yield break;
        }
        else
        {
            yield break;
        }
    }

    IEnumerator Reload()
    {
        canFire = false;
        isReloading = true;
        yield return new WaitForSeconds (reloadTime);
        ammoCount = maxAmmoCapacity;
        isReloading = false;
        canFire = true;
        yield break;
    }
}
