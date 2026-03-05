using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingSystem : MonoBehaviour
{

    //Specific for each button
    public List<TextMeshProUGUI> textList;
    public string type;
    public int damage;
    public int speed;
    public int wood;
    public int brick;
    public int metal;
    public int nails;
    public int pipes;
    public int manuals;



    void Start()
    {

    }

    public void ButtonClick () {
        //Update the text
        textList[0].text = "Name: "+ type;
        textList[1].text = "Damage: "+damage;
        textList[2].text = "Speed: "+speed;
        textList[3].text = "-x " + wood;
        textList[4].text = "-x " + brick;
        textList[5].text = "-x " + metal;
        textList[6].text = "-x " + nails;
        textList[7].text = "-x " + pipes;
        textList[8].text = "-x " + manuals;


    }
}
