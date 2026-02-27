using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MechHealthController : MonoBehaviour
{
    [Header("Health Values")]
    [SerializeField] private int mechCurrentHealth;
    [SerializeField] private int mechMaxHealth;

    [SerializeField] private int shieldCurrentHealth;
    [SerializeField] private int shieldMaxHealth;

    [Header("Components")]
    [SerializeField] private RawImage mechHealthUISquare;
    [SerializeField] private RawImage mechShieldUISquare;

    [Header("Other Values")]
    [SerializeField] private float timeSinceDamage;
    [SerializeField] private bool invincibility;
    [SerializeField] private bool shieldsRegenning;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mechMaxHealth = 100;
        mechCurrentHealth = 100;
        shieldCurrentHealth = 100;
        shieldMaxHealth = 100;  
        shieldsRegenning = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeSinceDamage < 10)
        {
            timeSinceDamage += Time.deltaTime;
        }
        if(timeSinceDamage>=10 && !shieldsRegenning && shieldCurrentHealth!=shieldMaxHealth)
        {
            StartCoroutine(PassiveHeal(5));
        }

        HandleUIElements();
    }


    void HandleUIElements()
    {
        mechShieldUISquare.rectTransform.localPosition = new Vector3(shieldCurrentHealth - 100, 0, 0);
        mechHealthUISquare.rectTransform.localPosition = new Vector3 (mechCurrentHealth - 100,0,0);
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
        while (shieldCurrentHealth < shieldMaxHealth && timeSinceDamage >= 7f)
        {
            shieldCurrentHealth+= passiveHealAmount;
            yield return new WaitForSeconds(0.3f);
        }

        shieldsRegenning = false;
        yield break;
    }

}
