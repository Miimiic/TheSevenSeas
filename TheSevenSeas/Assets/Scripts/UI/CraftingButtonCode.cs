using UnityEngine;

public class CraftingButtonCode : MonoBehaviour
{
    public Transform player;
    private PlayerInventoryController playerCode;
    public int wood;
    public int brick;
    public int metal;
    public int nails;
    public int pipes;
    public int manuals;

    private void Start() {

        playerCode = player.GetComponent<PlayerInventoryController>();
    }

    public void CraftGun()
    {
        if (playerCode.GetWoodAmount() >= wood && playerCode.GetBrickAmount() >= brick && playerCode.GetMetalAmount() >= metal && playerCode.GetNailAmount() >= nails)
        {
            playerCode.AddWood(-1 * wood);
            playerCode.AddBrick(-1 * brick);
            playerCode.AddMetal(-1 * metal);
            playerCode.AddNail(-1 * nails);
            //Spawn gun gunType
        }
        Debug.Log("Lost wood:" + wood + " Lost brick:" + brick + " Lost metal:" + metal + " Lost nails:" + nails + " ");


    }
}
