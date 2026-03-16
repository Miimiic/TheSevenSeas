using UnityEngine;
using TMPro;

public class Melee_Handler : MonoBehaviour
{
    public BoxCollider MeleeHitbox; //WhatIsHitting the enemy
    public float DamageOnHit;//How much damage it will deal
    public float SwingSpeed;// how quickly it will swing possibly may be broken
    public float WaitTimeBetweenSwings;//How long between swings
    private bool isSwinging = false;
    private bool isReadyToSwing = true;
    private bool WaitingToSwing;
    public string AttackAnimation;
    public Animator Animator;
    public int durability;
    public TextMeshProUGUI durabilityText;
    
 
    

    


    // Update is called once per frame

    void Update()
    {
        Animator.speed = SwingSpeed;
        if (Input.GetMouseButtonDown(0))
        {
            canWeAttack();
            WaitingToSwing = true;
        }
        if (durability <= 0) 
        {

            Destroy(gameObject);

        
        }
        if(durabilityText == null)
        {
            durabilityText = GameObject.FindGameObjectWithTag("AmmoCounter").GetComponent<TextMeshProUGUI>();
        }
        durabilityText.SetText(durability.ToString());
    }
    void canWeAttack()
    {
        if (!isSwinging && isReadyToSwing)
        {

            attack();


        }

    }
    void attack()
    {
        isSwinging = true;
        Animator.Play(AttackAnimation);
        Debug.Log("shouldSwing");
        attackCooldown();

        
       
    }
    void endCooldown()
    {
        isSwinging = false;
        WaitingToSwing = false;
    }
    void attackCooldown()
    {
        Invoke("endCooldown", WaitTimeBetweenSwings);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            Debug.Log("Hit");
            durability -= 1;
            var enemyHealth = other.GetComponent<EnemyHealth>();
            enemyHealth.TakeDamage(DamageOnHit);
        }

        if(other.tag == "Breakable") 
        {

            Debug.Log("Hit Obj");

            durability -= 1;
          var ObjHealth = other.GetComponent<objectHealth>();

            ObjHealth.takeDamage(DamageOnHit);




        }
       
    }
    
}
