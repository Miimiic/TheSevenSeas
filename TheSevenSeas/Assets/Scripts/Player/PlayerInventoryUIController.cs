using TMPro;
using UnityEngine;

public class PlayerInventoryUIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Objects")]
    [SerializeField] GameObject parentUiObject;
    [SerializeField] GameObject craftingUIObject;

    [Header("Text Components")]
    [SerializeField] private TMP_Text basicMaterialText;
    [SerializeField] private TMP_Text rareMaterialText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armourText;

    // The in-game health text, Disabled when inventory opens
    [SerializeField] private TMP_Text mainHealthText;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventoryUI(); 
        }
        //Daniel's code
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleCraftingUI();
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
            mainHealthText.gameObject.SetActive(true);
            parentUiObject.gameObject.SetActive(false);
        }
        else
        {
            HandleInventoryUI();
            mainHealthText.gameObject.SetActive(false);
            parentUiObject.gameObject.SetActive(true);  
        }
    }
    //Daniel's code
    public void ToggleCraftingUI()
    {
        if (craftingUIObject.gameObject.activeInHierarchy)
        {
            Cursor.visible = true;
            mainHealthText.gameObject.SetActive(true);
            craftingUIObject.gameObject.SetActive(false);
        }
        else
        {
            Cursor.visible = false;
            mainHealthText.gameObject.SetActive(false);
            craftingUIObject.gameObject.SetActive(true);
        }
    }
}
