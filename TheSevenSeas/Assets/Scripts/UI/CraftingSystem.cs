using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingSystem : MonoBehaviour
{

    //Specific for each button
    public int buttonNo;
    public List<TextMeshProUGUI> textList;
    public List<string> nameList = new List<string>();
    public List<string> damageList = new List<string>();
    public List<string> speedList = new List<string>();


    void Start()
    {
        textList = new List<TextMeshProUGUI>();

        nameList = new List<string>();
        damageList = new List<string>();
        speedList = new List<string>();

    }

    public void ButtonClick () {
        //Update the text
        textList[0].text = "Name: "+nameList[buttonNo];
        textList[1].text = "Damage: "+damageList[buttonNo];
        textList[2].text = "Speed: "+speedList[buttonNo];

    }
}
