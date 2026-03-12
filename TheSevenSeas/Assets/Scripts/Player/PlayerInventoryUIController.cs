using TMPro;
using UnityEngine;

public class PlayerInventoryUIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Objects")]
    [SerializeField] GameObject parentUiObject;

    [Header("Text Components")]
    [SerializeField] private TMP_Text basicMaterialText;
    [SerializeField] private TMP_Text rareMaterialText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armourText;

    // The in-game health text, Disabled when inventory opens
    [SerializeField] private GameObject mainHealthBar;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventoryUI(); 
        }
    }

    private void HandleInventoryUI()
    {
        basicMaterialText.text = " Wood : " + PlayerInventoryController.Instance.GetWoodAmount() + "\n Brick : " + PlayerInventoryController.Instance.GetBrickAmount() + "\n Metal : " + PlayerInventoryController.Instance.GetMetalAmount();

        rareMaterialText.text = " Nails : " + PlayerInventoryController.Instance.GetNailAmount() + "\n Pipes : " + PlayerInventoryController.Instance.GetMetalPipeAmount() + 
            "\n Manuals : " + PlayerInventoryController.Instance.GetInstructionManualAmount() + "\n Flesh : " + PlayerInventoryController.Instance.GetWorkerFleshAmount() +
            "\n Hearts : " + PlayerInventoryController.Instance.GetHeldritchHeartAmount();

        healthText.text = " Health : " + PlayerHealthController.Instance.GetCurrentHealth() + " / " + PlayerHealthController.Instance.GetMaxHealth();

        armourText.text = " Armour Level : " + PlayerHealthController.Instance.GetArmourLevel() + "\n Armour Damage Reduction - " + PlayerHealthController.Instance.GetArmourReduction();
    }

    public void ToggleInventoryUI()
    {
        if (parentUiObject.gameObject.activeInHierarchy)
        {
            HandleInventoryUI();
            mainHealthBar.SetActive(true);
            parentUiObject.gameObject.SetActive(false);
        }
        else
        {
            HandleInventoryUI();
            mainHealthBar.gameObject.SetActive(false);
            parentUiObject.gameObject.SetActive(true);  
        }
    }
}
