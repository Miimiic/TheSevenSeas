using System.Linq.Expressions;
using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

// The guiHealthText TMPro text must be manually assigned in the inspector.
// Damaging the player will solely be handled by enemies or hazards. Functions are available for these objects to use.

public class PlayerHealthController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TMP_Text guiHealthText;

    [Header("Health Values")]
    [SerializeField] private int playerCurrentHealth;
    [SerializeField] private int playerMaxHealth;

    [Header("Armour Values")]
    [SerializeField] int playerArmourLevel;
    [SerializeField] int playerArmourDamageReduction;
    [SerializeField] int armourDamageReductionScalar;
    // This is what will be multiplied by the armour level to get the damage reduction

    [Header("Passive Heal Values")]
    [SerializeField] private bool isPassiveHealing;
    [SerializeField] private bool isInPassiveHealZone;

    [Header("Other Values")]
    [SerializeField] private float timeSinceDamage;
    [SerializeField] private bool invincibility;



    public static PlayerHealthController Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Incase the instance was not set to this, Set it to this script, if it is already set to something, KILL THIS OBJECT, IT SHOULD NOT BE ABLE TO EXIST
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.Log("Destroying gameObject due to multiple instances of the singleton PlayerHealthController class. Please make sure you havent spawned multiple players.");
            Destroy(gameObject);
        }

        armourDamageReductionScalar = 30;
        playerCurrentHealth = 100;
        playerMaxHealth = 100;
        invincibility = false;
        isPassiveHealing = false;


        // Handle it once on start
        HandleHealthText();
    }

    private void FixedUpdate()
    {
        if (timeSinceDamage < 7)
        {
            timeSinceDamage += Time.deltaTime;
        }

        // Only do the passive heal if In zone, not at full health, isnt currently mid passive heal (Prevents a tonne of calls every frame withing the zone), and the havent taken damage for long enough
        if (isInPassiveHealZone&&playerCurrentHealth<playerMaxHealth&&!isPassiveHealing&& timeSinceDamage >= 7f)
        {
            StartCoroutine(PassiveHeal(15));
        }
    }
    // ------------------------------- Private Functions -------------------------------

    private void HandleHealthText()
    {
        // Only handle it when needed to save frames
        guiHealthText.text = "Health : "+ playerCurrentHealth +" / " + playerMaxHealth;
    }

    private void CheckForDeath()
    {
        if (playerCurrentHealth <= 0)
        {
            // Die
            Debug.Log("Player Died");
        }
    }

    // ------------------------------- Damage and Heal -------------------------------

    public void Damage(int damageNumber)
    {
        // Check for I-Frames
        if (!invincibility)
        {
            // Make sure the armour is up to date before doing any damage calculations
            HandlePlayerArmour();

            StartCoroutine(InvincibiltyTimer(0.5f));
            if (damageNumber-playerArmourDamageReduction>0)
            {
                playerCurrentHealth -= (damageNumber - playerArmourDamageReduction);
                Debug.Log("Damaging player for " + (damageNumber - playerArmourDamageReduction));
            }
            else
            {
                Debug.Log("Player Armour nullified the attack");
            }

            timeSinceDamage = 0;

            CheckForDeath();
            HandleHealthText();
        }

        else
        {
            Debug.Log("No Damage due to Invincibility");
        }
    }

    public void Heal(int healNumber)
    {
        playerCurrentHealth += healNumber;
        if (playerCurrentHealth > playerMaxHealth)
        {
            playerCurrentHealth=playerMaxHealth;
        }

        CheckForDeath();
        HandleHealthText();
    }

    // ------------------------------- Getters and Setters -------------------------------

    public int GetCurrentHealth()
    {
        return playerCurrentHealth;
    }

    public int GetMaxHealth()
    {
        return playerMaxHealth;
    }

    public int GetArmourLevel()
    {
        return playerArmourLevel;
    }

    public int GetArmourReduction()
    {
        return playerArmourDamageReduction;
    }

    public void SetCurrentHealth(int newHealth)
    {
        playerCurrentHealth = newHealth;

        HandleHealthText();
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        playerMaxHealth = newMaxHealth;

        CheckForDeath();
        HandleHealthText();
    }

    public void SetArmourLevel(int newArmourLevel)
    {
        playerArmourLevel = newArmourLevel;
        HandlePlayerArmour();
    }

    public void SetPassiveHealZone(bool newZone)
    {
        isInPassiveHealZone = newZone;
    }

    public void HandlePlayerArmour()
    {
        playerArmourDamageReduction = playerArmourLevel * armourDamageReductionScalar;
    }

    public void TriggerInvincibility(float seconds)
    {
        // UI Feedback for Invincibility
        StartCoroutine(InvincibiltyTimer(seconds));
    }

    // ------------------------------- Coroutines ------------------------------- 

    // Could also use this for an invincibilty powerup ???
    IEnumerator InvincibiltyTimer(float invincibilitySeconds)
    {
        Debug.Log("Invincible for "+invincibilitySeconds);
        invincibility = true;
        yield return new WaitForSeconds(invincibilitySeconds);
        invincibility = false;
        yield break;
    }

    IEnumerator PassiveHeal(int passiveHealAmount)
    {
        isPassiveHealing = true;

        // Check timeSinceDamage here aswell to make sure it can stop if taking damage mid-heal
        while (playerCurrentHealth < playerMaxHealth && isInPassiveHealZone && timeSinceDamage >= 7f)
        {
            Debug.Log("Passive Healing");
            Heal(passiveHealAmount);
            yield return new WaitForSeconds(0.75f);
        }

        isPassiveHealing = false;
        yield break;
    }
}
