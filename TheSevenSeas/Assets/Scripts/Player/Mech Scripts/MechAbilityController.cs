using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MechAbilityController : MonoBehaviour
{

    [Header("Mech Components")]
    [SerializeField] private Rigidbody mechRigidBody;

    [Header("UI Components")]
    // The RawImage needs to be at ( X -230 ) to appear outside of the mask
    [SerializeField] private RawImage dashCooldownMask;

    [Header("Dash Variables")]
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashCooldownMult;

    [SerializeField] private float dashStrength;

    [SerializeField] private int dashNum;
    [SerializeField] private bool canDash;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canDash = true;
        dashCooldown = 0;
        dashNum = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Dash();
        }
        if(!canDash)
        {
            if (dashCooldown < 0)
            {
                dashCooldown += 1 * dashCooldownMult;
                dashCooldownMask.transform.localPosition = new Vector2(dashCooldown, 0);
            }
            else
            {
                dashCooldown = 0;
                canDash = true;
                dashNum++;
            }
        }
    }

    private void Dash()
    {
        if (canDash)
        {
            if (dashNum > 0)
            {
                // Set the mech not to instantly slow
                MechMovementController.Instance.Dash();

                // Succesfull Dash
                mechRigidBody.AddForce(mechRigidBody.linearVelocity*dashStrength);

                dashNum--;
                dashCooldown = -230;
            }
            if (dashNum == 0)
            {
                // No more dashes available, Block dashing and start regenning dash
                canDash = false;
            }
        }
        else
        {
            Debug.Log("Cannot Dash");
        }
    }



    /*
    IEnumerator DashCooldownTick()
    {
        canDash = false;

        while (dashCooldown!=0)
        {
            // Failsafe to ensure the Coroutine will always end after the cooldown has passed

            dashCooldown += 1 * dashCooldownMult;
            if (dashCooldown >= 0)
            {
                dashCooldown = 0;
                canDash = true;
                yield break;
            }
            yield return new WaitForSeconds(0.05f);
        }
        yield break;
    }
    */

}
