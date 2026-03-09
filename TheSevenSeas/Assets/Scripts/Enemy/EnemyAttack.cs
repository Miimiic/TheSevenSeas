using UnityEngine;
using TMPro;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider attackHitbox;

    [Header("Attack Values")]
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackDuration;
    [SerializeField] private float attackDelay;

    private bool canAttack = true;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerHealthController>().Damage(attackDamage);
        }
        if (other.gameObject.tag == "Mech")
        {
            other.gameObject.GetComponent<MechHealthController>().Damage(attackDamage);
        }
    }

    public void Attack()
    {
        if (!canAttack) return;

        StartCoroutine(IsAttacking());
    }

    IEnumerator IsAttacking()
    {
        canAttack = false;
        attackHitbox.enabled = true;

        yield return new WaitForSeconds(attackDuration);
        attackHitbox.enabled = false;

        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
    }

}
