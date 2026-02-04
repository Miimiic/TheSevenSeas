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

    [Header("Passive Heal Values")]
    [SerializeField] private bool isPassiveHealing;
    [SerializeField] private bool isInPassiveHealZone;

    [Header("Other Values")]
    [SerializeField] private float timeSinceDamage;
    [SerializeField] private bool invincibility;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCurrentHealth = 50;
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
            StartCoroutine(PassiveHeal());
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

    // ------------------------------- Getters and Setters -------------------------------

    public int GetCurrentHealth()
    {
        return playerCurrentHealth;
    }

    public int GetMaxHealth()
    {
        return playerMaxHealth;
    }

    public void Damage(int damageNumber)
    {
        // Check for I-Frames
        if (!invincibility)
        {
            StartCoroutine(InvincibiltyTimer(0.5f));

            playerCurrentHealth -= damageNumber;
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

    public void SetCurrentHealth(int newHealth)
    {
        playerCurrentHealth = newHealth;

        CheckForDeath();
        HandleHealthText();
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        playerMaxHealth = newMaxHealth;

        CheckForDeath();
        HandleHealthText();
    }

    public void SetPassiveHealZone(bool newZone)
    {
        isInPassiveHealZone = newZone;
    }

    // ------------------------------- Invincibility Coroutine ------------------------------- 

    // Could also use this for an invincibilty powerup ???
    IEnumerator InvincibiltyTimer(float invincibilitySeconds)
    {
        Debug.Log("Invincible for "+invincibilitySeconds);
        invincibility = true;
        yield return new WaitForSeconds(invincibilitySeconds);
        invincibility = false;
        yield break;
    }

    IEnumerator PassiveHeal()
    {
        isPassiveHealing = true;

        // Check timeSinceDamage here aswell to make sure it can stop if taking damage mid-heal
        while (playerCurrentHealth < playerMaxHealth && isInPassiveHealZone && timeSinceDamage >= 7f)
        {
            Debug.Log("Passive Healing");
            // Heal 10% of max health every half second
            Heal(1);
            yield return new WaitForSeconds(0.5f);
        }

        isPassiveHealing = false;
        yield break;
    }
}
