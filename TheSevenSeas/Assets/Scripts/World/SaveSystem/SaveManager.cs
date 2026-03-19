// SaveManager.cs
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to {SavePath}");
    }

    public SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found, starting fresh.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public bool HasSave() => File.Exists(SavePath);

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}