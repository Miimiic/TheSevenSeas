using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GameStateController : MonoBehaviour
{
    [Header("References")]
    public GridSpawner gridSpawner;
    public Transform playerTransform;
    public PlayerController playerController;
    //public Transform buildingParent;
    //public GameObject[] buildingPrefabs;
    public BuildingSaveSystem buildingSaveSystem; 

    public async void NewGame()
    {
        // Generate the seed here explicitly rather than relying on Start()
        gridSpawner.worldSeed = UnityEngine.Random.Range(0, int.MaxValue);
        gridSpawner.useRandomSeed = false; // seed is now set, don't re-randomise

        await gridSpawner.RunSpawning();

        // Auto-save so the seed is immediately persisted
        SaveGame();
    }

    public async void LoadGame()
{
    SaveData data = SaveManager.Instance.Load();
    if (data == null)
    {
        Debug.LogWarning("No save data found! Starting new game instead.");
        NewGame();
        return;
    }

    gridSpawner.worldSeed = data.world.worldSeed;
    gridSpawner.useRandomSeed = false;

    await gridSpawner.RunSpawning();
    await Task.Yield();

    ApplyPlayerData(data.player);

    // Load buildings via BuildingSaveSystem
    if (buildingSaveSystem != null)
        buildingSaveSystem.LoadBuilding();
}

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.world.worldSeed = gridSpawner.worldSeed;
        CollectPlayerData(data);
        SaveManager.Instance.Save(data);

        // Also save buildings via BuildingSaveSystem
        if (buildingSaveSystem != null)
            buildingSaveSystem.SaveBuilding();
    }

    void CollectPlayerData(SaveData data)
    {
        Vector3 pos = playerTransform.position;
        data.player.posX = pos.x;
        data.player.posY = pos.y;
        data.player.posZ = pos.z;
        data.player.rotY = playerTransform.eulerAngles.y;

        var health = PlayerHealthController.Instance;
        data.player.currentHealth = health.GetCurrentHealth();
        data.player.maxHealth     = health.GetMaxHealth();
        data.player.armourLevel   = health.GetArmourLevel();

        data.player.currentStamina = playerController.GetStamina();

        var inv = PlayerInventoryController.Instance;
        data.player.wood               = inv.GetWoodAmount();
        data.player.brick              = inv.GetBrickAmount();
        data.player.metal              = inv.GetMetalAmount();
        data.player.nails              = inv.GetNailAmount();
        data.player.metalPipes         = inv.GetMetalPipeAmount();
        data.player.instructionManuals = inv.GetInstructionManualAmount();
        data.player.workerFlesh        = inv.GetWorkerFleshAmount();
        data.player.eldritchHearts     = inv.GetHeldritchHeartAmount();
    }

    void ApplyPlayerData(PlayerData p)
    {
        playerTransform.position    = new Vector3(p.posX, p.posY, p.posZ);
        playerTransform.eulerAngles = new Vector3(0, p.rotY, 0);

        var health = PlayerHealthController.Instance;
        health.SetMaxHealth(p.maxHealth);
        health.SetCurrentHealth(p.currentHealth);
        health.SetArmourLevel(p.armourLevel);

        playerController.SetStamina(p.currentStamina);

        var inv = PlayerInventoryController.Instance;
        inv.SetWood(p.wood);
        inv.SetBrick(p.brick);
        inv.SetMetal(p.metal);
        inv.SetNails(p.nails);
        inv.SetMetalPipes(p.metalPipes);
        inv.SetInstructionManuals(p.instructionManuals);
        inv.SetWorkerFlesh(p.workerFlesh);
        inv.SetEldritchHearts(p.eldritchHearts);
    }

    // void CollectBuildingData(SaveData data)
    // {
    //     if (buildingParent == null) return;
    //     foreach (Transform building in buildingParent)
    //     {
    //         BuildingIdentifier id = building.GetComponent<BuildingIdentifier>();
    //         if (id == null) continue;

    //         data.buildings.Add(new BuildingData
    //         {
    //             prefabId = id.prefabId,
    //             posX = building.position.x,
    //             posY = building.position.y,
    //             posZ = building.position.z,
    //             rotX = building.eulerAngles.x,
    //             rotY = building.eulerAngles.y,
    //             rotZ = building.eulerAngles.z,
    //             health = id.health
    //         });
    //     }
    // }

    // void RestoreBuildings(List<BuildingData> buildings)
    // {
    //     if (buildingParent == null) return;
    //     foreach (var b in buildings)
    //     {
    //         GameObject prefab = System.Array.Find(buildingPrefabs, p => p.name == b.prefabId);
    //         if (prefab == null) { Debug.LogWarning($"Prefab '{b.prefabId}' not found!"); continue; }

    //         Vector3 pos    = new Vector3(b.posX, b.posY, b.posZ);
    //         Quaternion rot = Quaternion.Euler(b.rotX, b.rotY, b.rotZ);
    //         GameObject spawned = Instantiate(prefab, pos, rot, buildingParent);

    //         BuildingIdentifier bid = spawned.GetComponent<BuildingIdentifier>();
    //         if (bid != null) bid.health = b.health;
    //     }
    // }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}