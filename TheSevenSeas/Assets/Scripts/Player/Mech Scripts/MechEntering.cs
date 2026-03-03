using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class MechEntering : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera mainPlayerCamera;
    [SerializeField] private GameObject playerHealthUIObject;

    [Header("Mech Components")]
    [SerializeField] private GameObject mechModel;
    [SerializeField] private MechMovementController mechMovement;
    [SerializeField] private MechAbilityController mechAbility;
    [SerializeField] private MechHealthController mechHealth;
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
            ToggleMechState();
        }
    }




    // Switches the player from Player to Mech control and Vice Versa
    void ToggleMechState()
    {
        if (isInMech)
        {
            // Exit Mech
            // Disable all mech movement and cameras, enabling players

            mechCamera.enabled = false;
            mechMovement.enabled = false;
            headlight.enabled = false;
            mechHealth.enabled = false;
            mechAbility.enabled = false;
            mechModel.gameObject.SetActive(true);

            playerHealthUIObject.SetActive(true);
            mechUIObject.SetActive(false);

            foreach (GameObject screen in screenList)
            {
                screen.SetActive(true);
            }

            cockpitOverlay.SetActive(false);

            playerController.enabled = true;
            mainPlayerCamera.enabled = true;

            isInMech = false;

            player.SetActive(true);
            player.transform.position = mechExitArea.transform.position;
            player.transform.rotation = mechExitArea.transform.rotation;

            Debug.Log("Exiting Mech");
        }

        else if (isInMech == false)
        {
            // Enter Mech
            // Disable all player movement and cameras, enabling mechs

            mechCamera.enabled = true;
            mechMovement.enabled = true;
            headlight.enabled = true;
            mechHealth.enabled = true;
            mechAbility.enabled = true;
            mechModel.gameObject.SetActive(false);

            playerHealthUIObject.SetActive(false);
            mechUIObject.gameObject.SetActive(true);

            cockpitOverlay.SetActive(true);

            playerController.enabled = false;
            mainPlayerCamera.enabled = false;

            player.SetActive(false);
            isInMech = true;
            Debug.Log("Entering Mech");

            StartCoroutine(BeginBootUpSequence());
        }
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


