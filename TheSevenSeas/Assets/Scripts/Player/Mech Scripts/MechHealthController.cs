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




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mechMaxHealth = 1000;
        shieldMaxHealth = 500;

        mechCurrentHealth = mechMaxHealth;
        shieldCurrentHealth = shieldMaxHealth;

        shieldsRegenning = false;
        shieldRegenLockoutTime = 5;
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

                Debug.Log("Mech is dead :(");
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

}
