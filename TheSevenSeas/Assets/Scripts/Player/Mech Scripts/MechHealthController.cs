 using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MechHealthController : MonoBehaviour
{
    [Header("Health Values")]
    [SerializeField] private float mechCurrentHealth;
    [SerializeField] private float mechMaxHealth;

    [SerializeField] private float shieldCurrentHealth;
    [SerializeField] private float shieldMaxHealth;

    [Header("Components")]
    [SerializeField] private RawImage mechHealthUISquare;
    [SerializeField] private RawImage mechShieldUISquare;

    [Header("Other Values")]
    [SerializeField] private float timeSinceDamage;
    [SerializeField] private bool invincibility;
    [SerializeField] private bool shieldsRegenning;
    [SerializeField] private float shieldRegenLockoutTime;
    [SerializeField] private bool isTitanDead;

    [Header("Mech Variables For Death")]
    [SerializeField] MechEntering mechEntering;
    [SerializeField] MechAbilityController mechAbilityController;
    [SerializeField] MechMovementController mechMovementController;
    [SerializeField] private GameObject mechScriptObject;
    [SerializeField] private GameObject mechModel;
    [SerializeField] private CapsuleCollider mainCollider;
    [SerializeField] private Rigidbody mainRigidbody;

    [Header("Titanfall Variables")]
    [SerializeField] private int titanfallSpeed;
    [SerializeField] private Camera mainPlayerCamera;
    [SerializeField] private GameObject titanfallMarker;
    [SerializeField] private GameObject titanfallDustImpact;
    [SerializeField] private float maxTitanfallCooldown;
    [SerializeField] private float titanfallCooldown;
    [SerializeField] private bool blockTitanCooldown;
    [SerializeField] private bool canCallTitan;
    [SerializeField] private bool titanIsFalling;
    [SerializeField] private bool hasSmoked;
    [SerializeField] LayerMask titanfallCanLandOnMask;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxTitanfallCooldown = 120f;

        mechMaxHealth = 1000;
        shieldMaxHealth = 500;

        mechCurrentHealth = mechMaxHealth;
        shieldCurrentHealth = shieldMaxHealth;

        shieldsRegenning = false;
        shieldRegenLockoutTime = 5;

        isTitanDead = true;
        blockTitanCooldown = true;
        hasSmoked = false;

        canCallTitan = true;

        KillMech();
        titanfallCooldown = maxTitanfallCooldown+1;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSinceDamage <= shieldRegenLockoutTime)
        {
            timeSinceDamage += Time.deltaTime;
        }

        if(timeSinceDamage>=shieldRegenLockoutTime && !shieldsRegenning && shieldCurrentHealth!=shieldMaxHealth)
        {
            StartCoroutine(PassiveHeal(5));
        }

        if (!blockTitanCooldown)
        {
            titanfallCooldown += Time.deltaTime;
            if (titanfallCooldown >= maxTitanfallCooldown)
            {
                blockTitanCooldown = true;
                // Allow the player to call the titan
                canCallTitan = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            if (canCallTitan)
            {
                RaycastHit hit = new RaycastHit();
                Physics.Raycast(mainPlayerCamera.transform.position, mainPlayerCamera.transform.forward, out hit, Mathf.Infinity,titanfallCanLandOnMask);
                Debug.Log("Standby for titanfall...");
                Titanfall(hit);
                // Do the fucking coolest shit ever
            }
        }
    }


    void HandleUIElements()
    {
        mechShieldUISquare.rectTransform.localPosition = new Vector3(ReturnPercent(shieldCurrentHealth,shieldMaxHealth)-100, 0, 0);
        mechHealthUISquare.rectTransform.localPosition = new Vector3 (ReturnPercent(mechCurrentHealth,mechMaxHealth) - 100,0,0);
    }

    public void Damage(int damage)
    {
        if (!invincibility)
        {
            StartCoroutine(InvincibiltyTimer(0.5f));
            timeSinceDamage = 0;
            // Shield has to be broken before the health can be damaged. There is no chip through the shield

            if (shieldCurrentHealth > 0)
            {
                shieldCurrentHealth -= damage;
                if(shieldCurrentHealth < 0)
                {
                    shieldCurrentHealth = 0;
                }
                HandleUIElements();
            }
            else
            {
                mechCurrentHealth -= damage;
                HandleUIElements();
            }

            if (mechCurrentHealth <= 0)
            {
                // Kill The mech, Eject Pilot ?
                // NUCLEAR EJECTION ????!?!??!?!??!?!

                // Leave the mech
                if(mechEntering.GetMechState())
                {
                    mechEntering.ToggleMechState();
                }

                KillMech();
            }
        }
    }

    public void Heal(int healing)
    {
        mechCurrentHealth += healing;
        if (mechCurrentHealth > mechMaxHealth)
        {
            mechMaxHealth = mechCurrentHealth;
        }
        HandleUIElements();

    }

    private float ReturnPercent(float part, float whole)
    {
        // Seriously... Do not ask why, but C# has gaslit me into thinking these need to be seperate lines, you cant use brackets to make them work as one statement or it will always return 0 for some reason ???
        // Also the 2 input variables must be floats aswell for some reason despite that only the variable holding the result should need to be a float

        float temp = part/whole;
        return temp*100;
    }




    IEnumerator InvincibiltyTimer(float invincibilitySeconds)
    {
        Debug.Log("Invincible for " + invincibilitySeconds);
        invincibility = true;
        yield return new WaitForSeconds(invincibilitySeconds);
        invincibility = false;
        yield break;
    }

    IEnumerator PassiveHeal(int passiveHealAmount)
    {
        shieldsRegenning = true;

        // Check timeSinceDamage here aswell to make sure it can stop if taking damage mid-heal
        while (shieldCurrentHealth < shieldMaxHealth && timeSinceDamage >= shieldRegenLockoutTime)
        {
            shieldCurrentHealth += passiveHealAmount;
            if(shieldCurrentHealth > shieldMaxHealth)
            {
                shieldCurrentHealth = shieldMaxHealth;
            }
            yield return new WaitForSeconds(0.05f);
            HandleUIElements();
        }

        shieldsRegenning = false;
        yield break;
    }

    public void ResetHealth()
    {
        mechCurrentHealth = mechMaxHealth;
        shieldCurrentHealth = shieldMaxHealth;

        HandleUIElements();
    }

    public void KillMech()
    {
        isTitanDead = true;

        mechModel.gameObject.SetActive(false);
        mainRigidbody.useGravity = false;
        mainCollider.enabled = false;

        titanfallCooldown = 0;

        blockTitanCooldown = false;

        Debug.Log("Titan is down :(");
    }

    public bool IsTitanDead()
    {
        return isTitanDead;
    }

    public void Respawn()
    {
        isTitanDead = false;

        ResetHealth();

        mechModel.gameObject.SetActive(true);
        mainCollider.enabled = true;
    }

    private void Titanfall(RaycastHit rayHit)
    {
        Debug.Log("Titan Call recieved, Initiating Titanfall");

        canCallTitan = false;
        titanIsFalling = true;

        // Spawn in the air
        Vector3 realSpawnLocation = rayHit.point + new Vector3(0, 200, 0);
        mechScriptObject.transform.position = realSpawnLocation;

        Respawn();
        Instantiate(titanfallMarker, rayHit.point, new Quaternion(0, 0, 0, 0));

        hasSmoked = false;
    }

    private void FixedUpdate()
    {
        if(titanIsFalling)
        {
            HandleFalling();
        }
    }

    private void HandleFalling()
    {
        mechScriptObject.transform.position -= new Vector3(0, titanfallSpeed, 0);

        // Check to stop falling
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(mechScriptObject.transform.position, -Vector3.up, out hit);
        if (hit.distance < 6 && !hasSmoked)
        {
            hasSmoked = true;
            Instantiate(titanfallDustImpact, hit.point, transform.rotation);
        }
        if (hit.distance < 2)
        {
            titanIsFalling = false;
            mainRigidbody.useGravity = true;
            Debug.Log("Titan Landed");
        }
    }


}
