using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingSystem : MonoBehaviour
{

    //Specific for each button
    public List<TextMeshProUGUI> textList;
    public Transform player;
    private PlayerInventoryController craftingCode;
    public string type;
    public int damage;
    public float speed;
    public int wood;
    public int brick;
    public int metal;
    public int nails;
    public int pipes;
    public int manuals;

    private void Start() {
        craftingCode = player.GetComponent<PlayerInventoryController>();
    }

    public void ShowInfo () {
        //Update the text-Not necessarily the final stat types/values
        textList[0].text = "Name: "+ type;
        textList[1].text = "Damage: "+damage;
        textList[2].text = "Speed: "+speed;
        textList[3].text = "-" + wood+ " wood";
        textList[4].text = "-" + brick+ " brick";
        textList[5].text = "-" + metal+ " metal";
        textList[6].text = "-" + nails+ " nails";
        textList[7].text = "-" + pipes+ " pipes";
        textList[8].text = "-" + manuals+ " manuals";
    }
    public void Craft()
    {
        if (craftingCode.GetWoodAmount() >= wood) {
            craftingCode.AddWood(-1 * wood);
        }
        if (craftingCode.GetBrickAmount() >= brick)
        {
            craftingCode.AddBrick(-1 * brick);
        }
        if (craftingCode.GetMetalAmount() >= metal)
        {
            craftingCode.AddMetal(-1 * metal);
        }
        if (craftingCode.GetNailAmount() >= nails)
        {
            craftingCode.AddNail(-1 * nails);
        }

        }
}

