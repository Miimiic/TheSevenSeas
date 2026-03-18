// SaveData.cs
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public WorldData world = new WorldData();
    public PlayerData player = new PlayerData();
    public List<BuildingData> buildings = new List<BuildingData>();
    public List<ObjectStateData> objectStates = new List<ObjectStateData>();
}

[Serializable]
public class WorldData
{
    public int worldSeed;
}


[Serializable]
public class PlayerData
{
    // Position / look
    public float posX, posY, posZ;
    public float rotY;

    // Health (from PlayerHealthController)
    public int currentHealth;
    public int maxHealth;
    public int armourLevel;

    // Stamina (from PlayerController)
    public float currentStamina;

    // Basic materials (from PlayerInventoryController)
    public int wood;
    public int brick;
    public int metal;

    // Rare materials
    public int nails;
    public int metalPipes;
    public int instructionManuals;
    public int workerFlesh;
    public int eldritchHearts;
}

[Serializable]
public class BuildingData
{
    public string prefabId;   // matches a key in your prefab registry
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ;
    public int health;
}

[Serializable]
public class ObjectStateData
{
    // For world objects whose state can change (crates, loot spawners, etc.)
    public string uniqueId;   // e.g. "crate_740_0" — grid position as ID
    public bool isDestroyed;
    public int resourceAmount;
}