using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingSystem : MonoBehaviour
{

    //Specific for each button
    public List<TextMeshProUGUI> textList;
    public Transform player;
    public string type;
    public int damage;
    public float speed;
    public int wood;
    public int brick;
    public int metal;
    public int nails;
    public int pipes;
    public int manuals;
    public int gunType;
    public Transform craftButton;
    private CraftingButtonCode craftButtonCode;
    [SerializeField] private GameObject theCanvas;
    [SerializeField] private TMP_Text mainHealthText;


    private void Start()
    {
        //theCanvas.gameObject.SetActive(false);
        craftButtonCode = craftButton.GetComponent<CraftingButtonCode>();

    }

    public void ShowGunInfo()
    {
        //Update the text-Not necessarily the final stat types/values
        textList[0].text = "Name: " + type;
        textList[1].text = "Damage: " + damage;
        textList[2].text = "Speed: " + speed;
        textList[3].text = "-" + wood + " wood";
        textList[4].text = "-" + brick + " brick";
        textList[5].text = "-" + metal + " metal";
        textList[6].text = "-" + nails + " nails";
        textList[7].text = "-" + pipes + " pipes";
        textList[8].text = "-" + manuals + " manuals";
        //Add the values to the craft button so they can be used when crafting
        craftButtonCode.wood = wood;
        craftButtonCode.brick = brick;
        craftButtonCode.metal = metal;
        craftButtonCode.nails = nails;

    }
}