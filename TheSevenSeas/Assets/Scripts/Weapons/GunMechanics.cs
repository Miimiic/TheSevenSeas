using TMPro;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine.UI;

public class GunMechanics : MonoBehaviour
{
    public Rigidbody AmmoType;//Change this based on what you want to shoot
    public GameObject Barrel,muzzleFlash,bulletHole;//Change this based on where you want to shoot from
    [SerializeField] private int BulletSpeed;//How quick And far you want the bullet to go
    [SerializeField] private bool FullAuto, raycast;//Self explanitory
    [SerializeField] private float timeBetweenShooting,spread,reloadTime,timeBetweenShots,range;//Self explanitory
     bool shooting,readyToShoot,reloading, playedEmptySound;
    [SerializeField] private int magSize , bulletsPerTap;
    int bulletsLeft, bulletsShot;
    public TextMeshProUGUI text;
    public AudioClip reloadAudio,shootAudio,emptyMagAudio;
    public AudioSource gunSource;
    public Camera fpsCam;
    public Transform muzzleLocation;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;
    public int Raycastdamage;

    private void Update()
    {

        if (text == null)
        {
            text = GameObject.FindGameObjectWithTag("AmmoCounter").GetComponent<TextMeshProUGUI>();

        }
        if (fpsCam == null)
        {
            fpsCam = Camera.main;

        }
        if (gunSource == null)
        {
            gunSource = gameObject.GetComponent<AudioSource>();
        }
        takeInput();
        text.SetText(bulletsLeft + " / " + magSize);
        
    }
    private void Start()
    {
        bulletsLeft = magSize;
        readyToShoot = true;
    }
    
    void takeInput()
    {
        if (!FullAuto)
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0); 
           
        }

        else
        {
           shooting = Input.GetKey(KeyCode.Mouse0);
        }
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magSize && !reloading)
        {
            reload();
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0) 
        {
            bulletsShot = bulletsPerTap;
            shoot();
        
        
        }
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0 &&!FullAuto )
        {
           
            gunSource.PlayOneShot(emptyMagAudio);
            


        }
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0 && FullAuto && !playedEmptySound)
        {
            gunSource.PlayOneShot(emptyMagAudio);
            playedEmptySound = true;

        }


    }
    
    void reload()
    {
        gunSource.PlayOneShot(reloadAudio);
        reloading = true;
        Invoke("reloadFinished",reloadTime);
        playedEmptySound = false;

    }

    void  shoot()
    {
        readyToShoot = false;
        
        float x = Random.Range(-spread,spread);
        float y = Random.Range(-spread, spread);
        if (!raycast)
        {
            Rigidbody justShotRB;
            justShotRB = Instantiate(AmmoType, Barrel.transform.position, Quaternion.identity);
            Vector3 Force = fpsCam.transform.forward * BulletSpeed ;
            justShotRB.AddForce(Force, ForceMode.Impulse);
        }
        if (raycast)
        {
            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out rayHit, range, whatIsEnemy))
            {
                Debug.Log("Hit: " + rayHit.transform.name);

                if (rayHit.transform.CompareTag("Enemy"))
                {
                    EnemyHealth enemyHealth = rayHit.transform.GetComponentInParent<EnemyHealth>();

                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(Raycastdamage);
                    }
                    else
                    {
                        Debug.LogError("EnemyHealth script not found on hit object!");
                    }
                }
                Instantiate(bulletHole, rayHit.point, Quaternion.Euler(0, 180, 0));
            }
        }
        Instantiate(muzzleFlash, muzzleLocation.position,Quaternion.identity);
        bulletsLeft--;
        bulletsShot--;
        Invoke("resetReady", timeBetweenShooting);
        
        if (bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("shoot", timeBetweenShots);
        }
        gunSource.PlayOneShot(shootAudio);

    }
    void resetReady() 
    {  
        readyToShoot = true; 
    }
    void reloadFinished()
    {
        bulletsLeft = magSize;
        reloading = false;
    }
}
