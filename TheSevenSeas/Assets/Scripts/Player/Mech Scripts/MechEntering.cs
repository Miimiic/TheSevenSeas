using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class MechEntering : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera mainPlayerCamera;
    [SerializeField] private GameObject playerHealthUIObject;

    [Header("Mech Components")]
    [SerializeField] private Collider mechFeet;
    [SerializeField] private GameObject mechModel;
    [SerializeField] private MechMovementController mechMovement;
    [SerializeField] private MechAbilityController mechAbility;
    [SerializeField] private MechHealthController mechHealth;
    [SerializeField] private MechWeaponController mechWeapon;
    [SerializeField] private Camera mechCamera;
    [SerializeField] private GameObject mechExitArea;
    [SerializeField] private Light headlight;
    [SerializeField] private GameObject cockpitOverlay;

    [Header("Cockpit Components")]
    [SerializeField] private GameObject mechUIObject;
    [SerializeField] private GameObject screenParent;
    [SerializeField] private List<GameObject> screenList = new List<GameObject>();





    [Header("Variables")]
    [SerializeField] bool isInMech;
    [SerializeField] int numOfScreens;



    // Disable the player camera and movement, enabling the mech camera and movement



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isInMech = false;

        // Iterate through each child of the screenParent and add them to the list
        foreach (Transform screenParentChild in screenParent.transform)
        {
            numOfScreens++;
                screenList.Add(screenParentChild.gameObject);
        }
}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            if  (!mechHealth.IsTitanDead())
            {
                if (isInMech)
                {
                    ToggleMechState();
                }

                else if (Vector3.Distance(player.transform.position, mechModel.transform.position) < 7)
                {
                    ToggleMechState();
                }

            }
        }
    }




    // Switches the player from Player to Mech control and Vice Versa
    public void ToggleMechState()
    {
        if (isInMech)
        {
            // Exit Mech
            // Disable all mech movement and cameras, enabling players

            mechWeapon.enabled = false;
            mechCamera.enabled = false;
            mechCamera.GetComponent<AudioListener>().enabled = false;
            mechMovement.enabled = false;
            headlight.enabled = false;
            mechAbility.enabled = false;
            mechModel.gameObject.SetActive(true);

            playerHealthUIObject.SetActive(true);
            mechUIObject.SetActive(false);

            cockpitOverlay.SetActive(false);

            playerController.enabled = true;
            mainPlayerCamera.gameObject.SetActive(true);

            isInMech = false;

            playerModel.gameObject.SetActive(true);
            playerCollider.enabled = true;
            player.transform.position = mechExitArea.transform.position;
            player.transform.rotation = mechExitArea.transform.rotation;

            mechFeet.enabled = false ;

            Debug.Log("Exiting Mech");


            // Re-"Disable" Screens
            foreach (GameObject screen in screenList)
            {
                screen.SetActive(true);
            }
        }

        else if (isInMech == false)
        {
            // Enter Mech
            // Disable all player movement and cameras, enabling mechs

            mechWeapon.enabled = true;
            mechCamera.enabled = true;
            mechCamera.GetComponent<AudioListener>().enabled = true;
            mechMovement.enabled = true;
            headlight.enabled = true;
            mechAbility.enabled = true;
            mechModel.gameObject.SetActive(false);

            playerHealthUIObject.SetActive(false);
            mechUIObject.gameObject.SetActive(true);

            cockpitOverlay.SetActive(true);

            playerController.enabled = false;
            mainPlayerCamera.gameObject.SetActive(false);

            playerModel.gameObject.SetActive(false);
            playerCollider.enabled = false;
            isInMech = true;
            Debug.Log("Entering Mech");

            mechFeet.enabled = true;

            StartCoroutine(BeginBootUpSequence());
        }
    }  

    public bool GetMechState()
    {
        return isInMech;
    }



    IEnumerator BeginBootUpSequence()
    {
        // Wait before starting
        yield return new WaitForSeconds(0.4f);

        // Iterate through each screen section and activate them
        foreach (GameObject screen in screenList)
        {
            screen.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Bootup Finished");
        yield break;
    }

}


