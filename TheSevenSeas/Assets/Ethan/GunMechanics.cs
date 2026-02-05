using TMPro;
using UnityEngine;


public class GunMechanics : MonoBehaviour
{
    public Rigidbody AmmoType;//Change this based on what you want to shoot
    public GameObject Barrel;//Change this based on where you want to shoot from
    [SerializeField] private int BulletSpeed;//How quick And far you want the bullet to go
    [SerializeField] private bool FullAuto;//Self explanitory
    [SerializeField] private float timeBetweenShooting,spread,reloadTime,timeBetweenShots;//Self explanitory
     bool shooting,readyToShoot,reloading;
    [SerializeField] private int magSize, bulletsPerTap;
    int bulletsLeft, bulletsShot;
    public TextMeshProUGUI text;
    public AudioClip reloadAudio,shootAudio,emptyMagAudio;
    public AudioSource gunSource;
   
    private void Update()
    {
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
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0)
        {
            gunSource.PlayOneShot(emptyMagAudio);

        }


    }
    void reload()
    {
        gunSource.PlayOneShot(reloadAudio);
        reloading = true;
        Invoke("reloadFinished",reloadTime);

    }

    void  shoot()
    {
        readyToShoot = false;
        
        float x = Random.Range(-spread,spread);
        float y = Random.Range(-spread, spread);

        Rigidbody justShotRB;
        justShotRB = Instantiate(AmmoType, Barrel.transform.position, Quaternion.identity);
        justShotRB.linearVelocity = transform.TransformDirection(Vector3.forward * BulletSpeed);
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
