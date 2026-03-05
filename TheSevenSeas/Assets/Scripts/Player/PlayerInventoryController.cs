using System.Collections;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{

    // !!! WARNING, THIS IS A SINGLETON CLASS !!! //
    // !! PLEASE DEAR GOD, NEVER INSTANTIATE THIS CLASS IN ANY WAY, SHAPE OR FORM !! //

    // Declare a globally accessible singleton instance of the PlayerInventoryController class
    public static PlayerInventoryController Instance;

    [Header("Basic Materials")]
    [SerializeField] private int woodAmount;
    [SerializeField] private int brickAmount;
    [SerializeField] private int metalAmount;

    [Header("Rare Materials")]
    [SerializeField] private int nailAmount;
    [SerializeField] private int metalPipeAmount;
    [SerializeField] private int instructionManualAmount;
    [SerializeField] private int workerFleshAmount;
    [SerializeField] private int eldritchHeartAmount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Incase the instance was not set to this, Set it to this script, if it is already set to something, KILL THIS OBJECT, IT SHOULD NOT BE ABLE TO EXIST
        if(Instance == null)
            Instance = this;
        else
        {
            Debug.Log("Destroying gameObject due to multiple instances of the singleton PlayerInventoryController class. Please make sure you havent spawned multiple players.");
            Destroy(gameObject);
        }
    }

    // I Know there has to be a better way to manage all these functions but I cant think of how for the life of me
    // --- Getters ---

    public int GetWoodAmount() { return woodAmount; }
    public int GetBrickAmount() { return brickAmount; }
    public int GetMetalAmount() { return metalAmount; }


    public int GetNailAmount() {  return nailAmount; }
    public int GetMetalPipeAmount() { return metalPipeAmount; }
    public int GetInstructionManualAmount() { return instructionManualAmount; }
    public int GetWorkerFleshAmount() { return workerFleshAmount; }
    public int GetHeldritchHeartAmount() { return eldritchHeartAmount; }

    // -----------------------

    public void AddWood(int woodToAdd)
    {
        woodAmount += woodToAdd;
    }
    public void AddBrick(int brickToAdd)
    {
        brickAmount += brickToAdd;
    }
    public void AddMetal(int metalToAdd)
    {
        metalAmount += metalToAdd;
    }

    public void AddNail(int nailToAdd)
    {
        nailAmount += nailToAdd;
    }
    public void AddMetalPipe(int metalPipeToAdd)
    {
        metalPipeAmount += metalPipeToAdd;
    }
    public void AddInstructionManual(int instructionManualToAdd)
    {
        instructionManualAmount += instructionManualToAdd;
    }
    public void AddWorkerFlesh(int workerFleshToAdd)
    {
        workerFleshAmount += workerFleshToAdd;
    }
    public void AddEldritchHeart(int eldritchHeartToAdd)
    {
        eldritchHeartAmount += eldritchHeartToAdd;
    }


}

