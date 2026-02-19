using UnityEngine;

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


    // Update is called once per frame

    void Update()
    {
        Animator.speed = SwingSpeed;
        if (Input.GetMouseButtonDown(0))
        {
            canWeAttack();
            WaitingToSwing = true;
        }
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
        }
    }
}
